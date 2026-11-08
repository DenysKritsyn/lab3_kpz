using lab3_kpz.Core.WebApi.DTOs.Common;
using lab3_kpz.Core.WebApi.DTOs.Symptoms;
using lab3_kpz.Core.WebApi.Exceptions;
using lab3_kpz.Core.WebApi.Mappings;
using lab3_kpz.Core.WebApi.Services.Interfaces;
using lab3_kpz.Data;
using Microsoft.EntityFrameworkCore;

namespace lab3_kpz.Core.WebApi.Services;

public sealed class SymptomService(
    SymptomDiaryDbContext db,
    SymptomDiaryMapper mapper) : ISymptomService
{
    public async Task<PagedResultDto<SymptomResponseDto>> GetAllAsync(SymptomQueryDto query, CancellationToken ct = default)
    {
        var dbQuery = db.Symptoms.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            dbQuery = dbQuery.Where(s => s.SymptomName.ToLower().Contains(search));
        }

        dbQuery = (query.SortBy?.ToLower(), query.SortDirection?.ToLower()) switch
        {
            ("name", "desc") => dbQuery.OrderByDescending(s => s.SymptomName),
            ("name", _) => dbQuery.OrderBy(s => s.SymptomName),
            ("id", "desc") => dbQuery.OrderByDescending(s => s.SymptomId),
            _ => dbQuery.OrderBy(s => s.SymptomId)
        };

        var totalCount = await dbQuery.CountAsync(ct);

        var items = await dbQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return PagedResultDto<SymptomResponseDto>.Create(
            mapper.ToDto(items),
            totalCount,
            query.Page,
            query.PageSize);
    }

    public async Task<SymptomResponseDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var symptom = await db.Symptoms.AsNoTracking().FirstOrDefaultAsync(s => s.SymptomId == id, ct)
            ?? throw ResourceNotFoundException.ForEntity("Symptom", id);

        return mapper.ToDto(symptom);
    }

    public async Task<SymptomResponseDto> CreateAsync(CreateSymptomDto dto, CancellationToken ct = default)
    {
        var trimmedName = dto.SymptomName.Trim();
        var exists = await db.Symptoms.AnyAsync(s => s.SymptomName.ToLower() == trimmedName.ToLower(), ct);
        if (exists)
        {
            throw new ConflictException($"A symptom with the name '{trimmedName}' already exists.");
        }

        var entity = mapper.ToEntity(dto);
        entity.SymptomName = trimmedName;

        db.Symptoms.Add(entity);
        await db.SaveChangesAsync(ct);

        return mapper.ToDto(entity);
    }

    public async Task<SymptomResponseDto> UpdateAsync(int id, UpdateSymptomDto dto, CancellationToken ct = default)
    {
        var entity = await db.Symptoms.FirstOrDefaultAsync(s => s.SymptomId == id, ct)
            ?? throw ResourceNotFoundException.ForEntity("Symptom", id);

        var trimmedName = dto.SymptomName.Trim();
        var exists = await db.Symptoms.AnyAsync(s => s.SymptomId != id && s.SymptomName.ToLower() == trimmedName.ToLower(), ct);
        if (exists)
        {
            throw new ConflictException($"Another symptom with the name '{trimmedName}' already exists.");
        }

        entity.SymptomName = trimmedName;
        await db.SaveChangesAsync(ct);

        return mapper.ToDto(entity);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await db.Symptoms.FirstOrDefaultAsync(s => s.SymptomId == id, ct)
            ?? throw ResourceNotFoundException.ForEntity("Symptom", id);

        var hasUsages = await db.IntervalSymptoms.AnyAsync(i => i.SymptomId == id, ct);
        if (hasUsages)
        {
            throw new ConflictException("Cannot delete symptom because it is referenced in patient diary entries.");
        }

        db.Symptoms.Remove(entity);
        await db.SaveChangesAsync(ct);
    }
}

