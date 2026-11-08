using lab3_kpz.Data;
using lab3_kpz.Entities;
using Microsoft.EntityFrameworkCore;

namespace lab3_kpz.Core.EntityFramework;

public sealed class LinqQueries(SymptomDiaryDbContext context)
{
    private readonly SymptomDiaryDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    // Filtering by multiple parameters (Read-only + AsNoTracking #1)
    public async Task<List<DayEvent>> FilterDayEventsAsync(
        int eventTypeId,
        TimeOnly fromTime,
        TimeOnly toTime,
        CancellationToken ct = default)
    {
        return await _context.DayEvents
            .AsNoTracking()
            .Where(e => e.EventTypeId == eventTypeId
                     && e.EventStartTime >= fromTime
                     && e.EventEndTime <= toTime)
            .ToListAsync(ct);
    }

    // Sorting by EntryDate and primary key DiaryEntryId (Read-only + AsNoTracking #2)
    public IQueryable<DiaryEntry> GetSortedDiaryEntries()
    {
        return _context.DiaryEntries
            .AsNoTracking()
            .OrderByDescending(d => d.EntryDate)
            .ThenBy(d => d.DiaryEntryId);
    }

    public async Task<List<DiaryEntry>> GetSortedDiaryEntriesAsync(CancellationToken ct = default)
    {
        return await GetSortedDiaryEntries().ToListAsync(ct);
    }

    // DTO projection (UserId, UserName, DiaryEntries)
    public async Task<List<UserDiaryDto>> GetUserDiaryDtosAsync(CancellationToken ct = default)
    {
        return await _context.Users
            .AsNoTracking()
            .Select(u => new UserDiaryDto(
                u.UserId,
                u.UserName,
                u.DiaryEntries
                    .OrderByDescending(d => d.EntryDate)
                    .Select(d => new DiaryEntrySummaryDto(
                        d.DiaryEntryId,
                        d.EntryDate,
                        d.EntryNote))
                    .ToList()))
            .ToListAsync(ct);
    }

    // Query with related data (DiaryEntry -> DayIntervals -> ConditionState & IntervalSymptoms -> Symptom & SeverityLevel)
    public async Task<DiaryEntry?> GetDiaryEntryWithRelatedDataAsync(
        int diaryEntryId,
        CancellationToken ct = default)
    {
        return await _context.DiaryEntries
            .Include(d => d.DayIntervals)
                .ThenInclude(i => i.ConditionState)
            .Include(d => d.DayIntervals)
                .ThenInclude(i => i.IntervalSymptoms)
                    .ThenInclude(s => s.Symptom)
            .Include(d => d.DayIntervals)
                .ThenInclude(i => i.IntervalSymptoms)
                    .ThenInclude(s => s.SeverityLevel)
            .AsSplitQuery()
            .FirstOrDefaultAsync(d => d.DiaryEntryId == diaryEntryId, ct);
    }

    // GroupBy with aggregates (Count, Average, Min, Max of symptom severity)
    public IQueryable<SymptomStatisticsDto> GetSymptomStatistics()
    {
        return _context.IntervalSymptoms
            .GroupBy(s => new { s.SymptomId, s.Symptom.SymptomName })
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key.SymptomName)
            .Select(g => new SymptomStatisticsDto(
                g.Key.SymptomId,
                g.Key.SymptomName,
                g.Count(),
                Math.Round(g.Average(s => (double)s.SeverityLevelId), 2),
                g.Min(s => s.SeverityLevelId),
                g.Max(s => s.SeverityLevelId)));
    }

    public async Task<List<SymptomStatisticsDto>> GetSymptomStatisticsAsync(CancellationToken ct = default)
    {
        return await GetSymptomStatistics().ToListAsync(ct);
    }

    // Pagination of diary entries
    public async Task<PagedResult<DiaryEntry>> GetPagedDiaryEntriesAsync(
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        IQueryable<DiaryEntry> sortedQuery = GetSortedDiaryEntries();

        int totalCount = await sortedQuery.CountAsync(ct);

        List<DiaryEntry> items = await sortedQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResult<DiaryEntry>(items, totalCount, pageNumber, pageSize, totalPages);
    }
}

public sealed record DiaryEntrySummaryDto(
    int DiaryEntryId,
    DateOnly EntryDate,
    string? EntryNote);

public sealed record UserDiaryDto(
    int UserId,
    string UserName,
    IReadOnlyList<DiaryEntrySummaryDto> DiaryEntries);

public sealed record SymptomStatisticsDto(
    int SymptomId,
    string SymptomName,
    int OccurrencesCount,
    double AverageSeverity,
    int MinSeverity,
    int MaxSeverity);

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);

