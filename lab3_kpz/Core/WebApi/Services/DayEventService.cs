using System.Security.Claims;
using lab3_kpz.Core.WebApi.Authorization;
using lab3_kpz.Core.WebApi.DTOs.Common;
using lab3_kpz.Core.WebApi.DTOs.DayEvents;
using lab3_kpz.Core.WebApi.Exceptions;
using lab3_kpz.Core.WebApi.Mappings;
using lab3_kpz.Core.WebApi.Services.Interfaces;
using lab3_kpz.Data;
using lab3_kpz.Entities;
using Microsoft.EntityFrameworkCore;

namespace lab3_kpz.Core.WebApi.Services;

public sealed class DayEventService(
    SymptomDiaryDbContext db,
    SymptomDiaryMapper mapper,
    IUserService userService) : IDayEventService
{
    public async Task<PagedResultDto<DayEventResponseDto>> GetAllAsync(DayEventQueryDto query, ClaimsPrincipal principal, CancellationToken ct = default)
    {
        var currentUser = await userService.GetOrCreateCurrentUserAsync(principal, ct);
        var isAdminOrDoctor = principal.IsInRole(AppRoles.Admin) || principal.IsInRole(AppRoles.Doctor);

        var dbQuery = db.DayEvents
            .AsNoTracking()
            .AsQueryable();

        if (!isAdminOrDoctor)
        {
            dbQuery = dbQuery.Where(e => e.DiaryEntry.UserId == currentUser.UserId);
        }

        if (query.DiaryEntryId.HasValue)
        {
            dbQuery = dbQuery.Where(e => e.DiaryEntryId == query.DiaryEntryId.Value);
        }

        if (query.EventTypeId.HasValue)
        {
            dbQuery = dbQuery.Where(e => e.EventTypeId == query.EventTypeId.Value);
        }

        dbQuery = (query.SortBy?.ToLower(), query.SortDirection?.ToLower()) switch
        {
            ("time", "desc") => dbQuery.OrderByDescending(e => e.EventStartTime),
            ("time", _) => dbQuery.OrderBy(e => e.EventStartTime),
            _ => dbQuery.OrderByDescending(e => e.DayEventId)
        };

        var totalCount = await dbQuery.CountAsync(ct);

        var events = await dbQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(e => new DayEventResponseDto(
                e.DayEventId,
                e.DiaryEntryId,
                e.EventTypeId,
                e.EventType.EventTypeName,
                e.EventStartTime,
                e.EventEndTime,
                e.IsPreciseTime,
                e.EventNote))
            .ToListAsync(ct);

        return PagedResultDto<DayEventResponseDto>.Create(
            events,
            totalCount,
            query.Page,
            query.PageSize);
    }

    public async Task<DayEventResponseDto> GetByIdAsync(int id, ClaimsPrincipal principal, CancellationToken ct = default)
    {
        var dayEvent = await db.DayEvents
            .AsNoTracking()
            .Include(e => e.EventType)
            .FirstOrDefaultAsync(e => e.DayEventId == id, ct)
            ?? throw ResourceNotFoundException.ForEntity("DayEvent", id);

        return mapper.ToDto(dayEvent);
    }

    public async Task<string?> GetOwnerIdAsync(int id, CancellationToken ct = default)
    {
        var owner = await db.DayEvents
            .AsNoTracking()
            .Where(e => e.DayEventId == id)
            .Select(e => e.DiaryEntry.User.UserEmail)
            .FirstOrDefaultAsync(ct);

        return owner;
    }

    public async Task<DayEventResponseDto> CreateAsync(CreateDayEventDto dto, ClaimsPrincipal principal, CancellationToken ct = default)
    {
        var diaryEntry = await db.DiaryEntries
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.DiaryEntryId == dto.DiaryEntryId, ct)
            ?? throw ResourceNotFoundException.ForEntity("DiaryEntry", dto.DiaryEntryId);

        var eventTypeExists = await db.EventTypes.AnyAsync(t => t.EventTypeId == dto.EventTypeId, ct);
        if (!eventTypeExists)
        {
            throw ResourceNotFoundException.ForEntity("EventType", dto.EventTypeId);
        }

        var dayEvent = mapper.ToEntity(dto);
        dayEvent.EventNote = dto.EventNote?.Trim();

        db.DayEvents.Add(dayEvent);
        await db.SaveChangesAsync(ct);

        return await GetByIdAsync(dayEvent.DayEventId, principal, ct);
    }

    public async Task<DayEventResponseDto> UpdateAsync(int id, UpdateDayEventDto dto, ClaimsPrincipal principal, CancellationToken ct = default)
    {
        var dayEvent = await db.DayEvents.FirstOrDefaultAsync(e => e.DayEventId == id, ct)
            ?? throw ResourceNotFoundException.ForEntity("DayEvent", id);

        var eventTypeExists = await db.EventTypes.AnyAsync(t => t.EventTypeId == dto.EventTypeId, ct);
        if (!eventTypeExists)
        {
            throw ResourceNotFoundException.ForEntity("EventType", dto.EventTypeId);
        }

        dayEvent.EventTypeId = dto.EventTypeId;
        dayEvent.EventStartTime = dto.EventStartTime;
        dayEvent.EventEndTime = dto.EventEndTime;
        dayEvent.IsPreciseTime = dto.IsPreciseTime;
        dayEvent.EventNote = dto.EventNote?.Trim();

        await db.SaveChangesAsync(ct);

        return await GetByIdAsync(id, principal, ct);
    }

    public async Task DeleteAsync(int id, ClaimsPrincipal principal, CancellationToken ct = default)
    {
        var dayEvent = await db.DayEvents.FirstOrDefaultAsync(e => e.DayEventId == id, ct)
            ?? throw ResourceNotFoundException.ForEntity("DayEvent", id);

        db.DayEvents.Remove(dayEvent);
        await db.SaveChangesAsync(ct);
    }
}

