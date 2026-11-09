using System.Security.Claims;
using lab3_kpz.Core.WebApi.Authorization;
using lab3_kpz.Core.WebApi.DTOs.Common;
using lab3_kpz.Core.WebApi.DTOs.DiaryEntries;
using lab3_kpz.Core.WebApi.Exceptions;
using lab3_kpz.Core.WebApi.Mappings;
using lab3_kpz.Core.WebApi.Services.Interfaces;
using lab3_kpz.Data;
using lab3_kpz.Entities;
using Microsoft.EntityFrameworkCore;

namespace lab3_kpz.Core.WebApi.Services;

public sealed class DiaryEntryService(
    SymptomDiaryDbContext db,
    SymptomDiaryMapper mapper,
    IUserService userService) : IDiaryEntryService
{
    public async Task<PagedResultDto<DiaryEntrySummaryDto>> GetAllAsync(DiaryEntryQueryDto query, ClaimsPrincipal principal, CancellationToken ct = default)
    {
        var currentUser = await userService.GetOrCreateCurrentUserAsync(principal, ct);
        var isAdminOrDoctor = principal.IsInRole(AppRoles.Admin) || principal.IsInRole(AppRoles.Doctor);

        var dbQuery = db.DiaryEntries
            .AsNoTracking()
            .AsQueryable();

        // Patients can only see their own diary entries
        if (!isAdminOrDoctor)
        {
            dbQuery = dbQuery.Where(d => d.UserId == currentUser.UserId);
        }

        if (query.FromDate.HasValue)
        {
            dbQuery = dbQuery.Where(d => d.EntryDate >= query.FromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            dbQuery = dbQuery.Where(d => d.EntryDate <= query.ToDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchNote))
        {
            var note = query.SearchNote.Trim().ToLower();
            dbQuery = dbQuery.Where(d => d.EntryNote != null && d.EntryNote.ToLower().Contains(note));
        }

        if (query.SymptomId.HasValue)
        {
            dbQuery = dbQuery.Where(d => d.DayIntervals
                .Any(i => i.IntervalSymptoms.Any(s => s.SymptomId == query.SymptomId.Value)));
        }

        dbQuery = (query.SortBy?.ToLower(), query.SortDirection?.ToLower()) switch
        {
            ("date", "asc") => dbQuery.OrderBy(d => d.EntryDate),
            ("date", _) => dbQuery.OrderByDescending(d => d.EntryDate),
            ("createdat", "asc") => dbQuery.OrderBy(d => d.EntryCreatedAt),
            ("createdat", _) => dbQuery.OrderByDescending(d => d.EntryCreatedAt),
            _ => dbQuery.OrderByDescending(d => d.EntryDate)
        };

        var totalCount = await dbQuery.CountAsync(ct);

        var summaries = await dbQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(d => new DiaryEntrySummaryDto(
                d.DiaryEntryId,
                d.UserId,
                d.EntryDate,
                d.EntryNote,
                d.DayIntervals.Count,
                d.DayEvents.Count,
                d.Measurements.Count,
                d.EntryCreatedAt,
                d.EntryUpdatedAt))
            .ToListAsync(ct);

        return PagedResultDto<DiaryEntrySummaryDto>.Create(
            summaries,
            totalCount,
            query.Page,
            query.PageSize);
    }

    public async Task<DiaryEntryResponseDto> GetByIdAsync(int id, ClaimsPrincipal principal, CancellationToken ct = default)
    {
        var entry = await db.DiaryEntries
            .AsNoTracking()
            .Include(d => d.DayIntervals)
                .ThenInclude(i => i.ConditionState)
            .Include(d => d.DayIntervals)
                .ThenInclude(i => i.IntervalSymptoms)
                    .ThenInclude(s => s.Symptom)
            .Include(d => d.DayIntervals)
                .ThenInclude(i => i.IntervalSymptoms)
                    .ThenInclude(s => s.SeverityLevel)
            .Include(d => d.DayEvents)
                .ThenInclude(e => e.EventType)
            .Include(d => d.Measurements)
                .ThenInclude(m => m.Metrics)
                    .ThenInclude(met => met.MetricUnit)
            .FirstOrDefaultAsync(d => d.DiaryEntryId == id, ct)
            ?? throw ResourceNotFoundException.ForEntity("DiaryEntry", id);

        return mapper.ToDto(entry);
    }

    public async Task<string?> GetOwnerIdAsync(int id, CancellationToken ct = default)
    {
        var owner = await db.DiaryEntries
            .AsNoTracking()
            .Where(d => d.DiaryEntryId == id)
            .Select(d => d.User.UserEmail)
            .FirstOrDefaultAsync(ct);

        return owner;
    }

    public async Task<DiaryEntryResponseDto> CreateAsync(CreateDiaryEntryDto dto, ClaimsPrincipal principal, CancellationToken ct = default)
    {
        var currentUser = await userService.GetOrCreateCurrentUserAsync(principal, ct);

        var exists = await db.DiaryEntries.AnyAsync(d => d.UserId == currentUser.UserId && d.EntryDate == dto.EntryDate, ct);
        if (exists)
        {
            throw new ConflictException($"A diary entry for date '{dto.EntryDate}' already exists for this user.");
        }

        var now = DateTime.UtcNow;
        var entry = new DiaryEntry
        {
            UserId = currentUser.UserId,
            EntryDate = dto.EntryDate,
            EntryNote = dto.EntryNote?.Trim(),
            EntryCreatedAt = now,
            EntryUpdatedAt = now
        };

        if (dto.DayIntervals is { Count: > 0 })
        {
            foreach (var intervalDto in dto.DayIntervals)
            {
                var interval = new DayInterval
                {
                    IntervalStartTime = intervalDto.IntervalStartTime,
                    IntervalEndTime = intervalDto.IntervalEndTime,
                    ConditionStateId = intervalDto.ConditionStateId,
                    IntervalNote = intervalDto.IntervalNote?.Trim()
                };

                if (intervalDto.Symptoms is { Count: > 0 })
                {
                    foreach (var sDto in intervalDto.Symptoms)
                    {
                        interval.IntervalSymptoms.Add(new IntervalSymptom
                        {
                            SymptomId = sDto.SymptomId,
                            SeverityLevelId = sDto.SeverityLevelId,
                            SymptomNote = sDto.SymptomNote?.Trim()
                        });
                    }
                }

                entry.DayIntervals.Add(interval);
            }
        }

        if (dto.DayEvents is { Count: > 0 })
        {
            foreach (var eventDto in dto.DayEvents)
            {
                entry.DayEvents.Add(new DayEvent
                {
                    EventTypeId = eventDto.EventTypeId,
                    EventStartTime = eventDto.EventStartTime,
                    EventEndTime = eventDto.EventEndTime,
                    IsPreciseTime = eventDto.IsPreciseTime,
                    EventNote = eventDto.EventNote?.Trim()
                });
            }
        }

        if (dto.Measurements is { Count: > 0 })
        {
            foreach (var mDto in dto.Measurements)
            {
                var measurement = new Measurement
                {
                    MeasuringTime = mDto.MeasuringTime
                };

                if (mDto.Metrics is { Count: > 0 })
                {
                    foreach (var metDto in mDto.Metrics)
                    {
                        measurement.Metrics.Add(new Metric
                        {
                            MetricUnitId = metDto.MetricUnitId,
                            MetricName = metDto.MetricName.Trim(),
                            MetricValue = metDto.MetricValue
                        });
                    }
                }

                entry.Measurements.Add(measurement);
            }
        }

        db.DiaryEntries.Add(entry);
        await db.SaveChangesAsync(ct);

        return await GetByIdAsync(entry.DiaryEntryId, principal, ct);
    }

    public async Task<DiaryEntryResponseDto> UpdateAsync(int id, UpdateDiaryEntryDto dto, ClaimsPrincipal principal, CancellationToken ct = default)
    {
        var entry = await db.DiaryEntries
            .Include(d => d.DayIntervals)
                .ThenInclude(i => i.IntervalSymptoms)
            .Include(d => d.DayEvents)
            .Include(d => d.Measurements)
                .ThenInclude(m => m.Metrics)
            .FirstOrDefaultAsync(d => d.DiaryEntryId == id, ct)
            ?? throw ResourceNotFoundException.ForEntity("DiaryEntry", id);

        entry.EntryNote = dto.EntryNote?.Trim();
        entry.EntryUpdatedAt = DateTime.UtcNow;

        // Replace intervals if provided
        if (dto.DayIntervals is not null)
        {
            db.DayIntervals.RemoveRange(entry.DayIntervals);
            entry.DayIntervals.Clear();

            foreach (var intervalDto in dto.DayIntervals)
            {
                var interval = new DayInterval
                {
                    DiaryEntryId = id,
                    IntervalStartTime = intervalDto.IntervalStartTime,
                    IntervalEndTime = intervalDto.IntervalEndTime,
                    ConditionStateId = intervalDto.ConditionStateId,
                    IntervalNote = intervalDto.IntervalNote?.Trim()
                };

                if (intervalDto.Symptoms is { Count: > 0 })
                {
                    foreach (var sDto in intervalDto.Symptoms)
                    {
                        interval.IntervalSymptoms.Add(new IntervalSymptom
                        {
                            SymptomId = sDto.SymptomId,
                            SeverityLevelId = sDto.SeverityLevelId,
                            SymptomNote = sDto.SymptomNote?.Trim()
                        });
                    }
                }

                entry.DayIntervals.Add(interval);
            }
        }

        // Replace events if provided
        if (dto.DayEvents is not null)
        {
            db.DayEvents.RemoveRange(entry.DayEvents);
            entry.DayEvents.Clear();

            foreach (var eventDto in dto.DayEvents)
            {
                entry.DayEvents.Add(new DayEvent
                {
                    DiaryEntryId = id,
                    EventTypeId = eventDto.EventTypeId,
                    EventStartTime = eventDto.EventStartTime,
                    EventEndTime = eventDto.EventEndTime,
                    IsPreciseTime = eventDto.IsPreciseTime,
                    EventNote = eventDto.EventNote?.Trim()
                });
            }
        }

        // Replace measurements if provided
        if (dto.Measurements is not null)
        {
            db.Measurements.RemoveRange(entry.Measurements);
            entry.Measurements.Clear();

            foreach (var mDto in dto.Measurements)
            {
                var measurement = new Measurement
                {
                    DiaryEntryId = id,
                    MeasuringTime = mDto.MeasuringTime
                };

                if (mDto.Metrics is { Count: > 0 })
                {
                    foreach (var metDto in mDto.Metrics)
                    {
                        measurement.Metrics.Add(new Metric
                        {
                            MetricUnitId = metDto.MetricUnitId,
                            MetricName = metDto.MetricName.Trim(),
                            MetricValue = metDto.MetricValue
                        });
                    }
                }

                entry.Measurements.Add(measurement);
            }
        }

        await db.SaveChangesAsync(ct);

        return await GetByIdAsync(id, principal, ct);
    }

    public async Task DeleteAsync(int id, ClaimsPrincipal principal, CancellationToken ct = default)
    {
        var entry = await db.DiaryEntries.FirstOrDefaultAsync(d => d.DiaryEntryId == id, ct)
            ?? throw ResourceNotFoundException.ForEntity("DiaryEntry", id);

        db.DiaryEntries.Remove(entry);
        await db.SaveChangesAsync(ct);
    }
}

