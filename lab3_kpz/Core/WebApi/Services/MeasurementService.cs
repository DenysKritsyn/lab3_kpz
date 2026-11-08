using System.Security.Claims;
using lab3_kpz.Core.WebApi.Authorization;
using lab3_kpz.Core.WebApi.DTOs.Common;
using lab3_kpz.Core.WebApi.DTOs.Measurements;
using lab3_kpz.Core.WebApi.Exceptions;
using lab3_kpz.Core.WebApi.Mappings;
using lab3_kpz.Core.WebApi.Services.Interfaces;
using lab3_kpz.Data;
using lab3_kpz.Entities;
using Microsoft.EntityFrameworkCore;

namespace lab3_kpz.Core.WebApi.Services;

public sealed class MeasurementService(
    SymptomDiaryDbContext db,
    SymptomDiaryMapper mapper,
    IUserService userService) : IMeasurementService
{
    public async Task<PagedResultDto<MeasurementResponseDto>> GetAllAsync(MeasurementQueryDto query, ClaimsPrincipal principal, CancellationToken ct = default)
    {
        var currentUser = await userService.GetOrCreateCurrentUserAsync(principal, ct);
        var isAdminOrDoctor = principal.IsInRole(AppRoles.Admin) || principal.IsInRole(AppRoles.Doctor);

        var dbQuery = db.Measurements
            .AsNoTracking()
            .Include(m => m.DiaryEntry)
            .Include(m => m.Metrics)
                .ThenInclude(met => met.MetricUnit)
            .AsQueryable();

        if (!isAdminOrDoctor)
        {
            dbQuery = dbQuery.Where(m => m.DiaryEntry.UserId == currentUser.UserId);
        }

        if (query.DiaryEntryId.HasValue)
        {
            dbQuery = dbQuery.Where(m => m.DiaryEntryId == query.DiaryEntryId.Value);
        }

        if (query.FromDate.HasValue)
        {
            dbQuery = dbQuery.Where(m => m.DiaryEntry.EntryDate >= query.FromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            dbQuery = dbQuery.Where(m => m.DiaryEntry.EntryDate <= query.ToDate.Value);
        }

        if (query.MetricUnitId.HasValue)
        {
            dbQuery = dbQuery.Where(m => m.Metrics.Any(met => met.MetricUnitId == query.MetricUnitId.Value));
        }

        if (!string.IsNullOrWhiteSpace(query.MetricName))
        {
            var metricName = query.MetricName.Trim().ToLower();
            dbQuery = dbQuery.Where(m => m.Metrics.Any(met => met.MetricName.ToLower().Contains(metricName)));
        }

        dbQuery = dbQuery.OrderByDescending(m => m.MeasurementId);

        var totalCount = await dbQuery.CountAsync(ct);

        var measurements = await dbQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return PagedResultDto<MeasurementResponseDto>.Create(
            mapper.ToDto(measurements),
            totalCount,
            query.Page,
            query.PageSize);
    }

    public async Task<MeasurementResponseDto> GetByIdAsync(int id, ClaimsPrincipal principal, CancellationToken ct = default)
    {
        var measurement = await db.Measurements
            .AsNoTracking()
            .Include(m => m.Metrics)
                .ThenInclude(met => met.MetricUnit)
            .FirstOrDefaultAsync(m => m.MeasurementId == id, ct)
            ?? throw ResourceNotFoundException.ForEntity("Measurement", id);

        return mapper.ToDto(measurement);
    }

    public async Task<string?> GetOwnerIdAsync(int id, CancellationToken ct = default)
    {
        var owner = await db.Measurements
            .AsNoTracking()
            .Where(m => m.MeasurementId == id)
            .Select(m => m.DiaryEntry.User.UserEmail)
            .FirstOrDefaultAsync(ct);

        return owner;
    }

    public async Task<MeasurementResponseDto> CreateAsync(CreateMeasurementDto dto, ClaimsPrincipal principal, CancellationToken ct = default)
    {
        var diaryEntryExists = await db.DiaryEntries.AnyAsync(d => d.DiaryEntryId == dto.DiaryEntryId, ct);
        if (!diaryEntryExists)
        {
            throw ResourceNotFoundException.ForEntity("DiaryEntry", dto.DiaryEntryId);
        }

        var measurement = new Measurement
        {
            DiaryEntryId = dto.DiaryEntryId,
            MeasuringTime = dto.MeasuringTime
        };

        foreach (var metricDto in dto.Metrics)
        {
            var unitExists = await db.MetricUnits.AnyAsync(u => u.MetricUnitId == metricDto.MetricUnitId, ct);
            if (!unitExists)
            {
                throw ResourceNotFoundException.ForEntity("MetricUnit", metricDto.MetricUnitId);
            }

            measurement.Metrics.Add(new Metric
            {
                MetricUnitId = metricDto.MetricUnitId,
                MetricName = metricDto.MetricName.Trim(),
                MetricValue = metricDto.MetricValue
            });
        }

        db.Measurements.Add(measurement);
        await db.SaveChangesAsync(ct);

        return await GetByIdAsync(measurement.MeasurementId, principal, ct);
    }

    public async Task DeleteAsync(int id, ClaimsPrincipal principal, CancellationToken ct = default)
    {
        var measurement = await db.Measurements.FirstOrDefaultAsync(m => m.MeasurementId == id, ct)
            ?? throw ResourceNotFoundException.ForEntity("Measurement", id);

        db.Measurements.Remove(measurement);
        await db.SaveChangesAsync(ct);
    }
}