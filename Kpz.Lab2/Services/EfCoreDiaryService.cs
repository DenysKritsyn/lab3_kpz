using System.Text.Json;
using Kpz.Lab2.Data;
using Kpz.Lab2.Dtos;
using Kpz.Lab2.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kpz.Lab2.Services;

public sealed class EfCoreDiaryService(SymptomDiaryDbContext db)
{
    // =========================================================================
    // Task 8. EF Core CRUD for at least three entities:
    //         1) User
    //         2) DiaryEntry
    //         3) DayEvent
    //         + Complex business operation working with an entity graph in a transaction
    // =========================================================================

    // --- 1) CRUD for User ---
    public async Task<User> CreateUserAsync(
        string externalAuthId,
        string userEmail,
        string userName,
        DateOnly? userBirthDate,
        string userRole,
        CancellationToken ct)
    {
        var user = new User
        {
            ExternalAuthId = externalAuthId,
            UserEmail = userEmail,
            UserName = userName,
            UserBirthDate = userBirthDate,
            UserRole = userRole,
            UserCreatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        return user;
    }

    public async Task<User?> GetUserByIdAsNoTrackingAsync(int userId, CancellationToken ct)
    {
        // Read-only entity query using AsNoTracking (Task 9 requirement)
        return await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId, ct);
    }

    public async Task<bool> UpdateUserNameAsync(int userId, string newName, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.UserId == userId, ct);
        if (user is null)
            return false;

        user.UserName = newName;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteUserAsync(int userId, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.UserId == userId, ct);
        if (user is null)
            return false;

        db.Users.Remove(user);
        await db.SaveChangesAsync(ct);
        return true;
    }

    // --- 2) CRUD for DiaryEntry ---
    public async Task<DiaryEntry> CreateDiaryEntryAsync(
        int userId,
        DateOnly entryDate,
        string? entryNote,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var entry = new DiaryEntry
        {
            UserId = userId,
            EntryDate = entryDate,
            EntryNote = entryNote,
            EntryCreatedAt = now,
            EntryUpdatedAt = now
        };

        db.DiaryEntries.Add(entry);
        await db.SaveChangesAsync(ct);
        return entry;
    }

    public async Task<DiaryEntry?> GetDiaryEntryWithGraphAsync(int diaryEntryId, CancellationToken ct)
    {
        // Eager loading of related data via Include / ThenInclude + AsNoTracking (Task 9 requirement)
        return await db.DiaryEntries
            .AsNoTracking()
            .Include(d => d.User)
            .Include(d => d.DayIntervals.OrderBy(i => i.IntervalStartHour))
                .ThenInclude(i => i.WellbeingLevel)
            .Include(d => d.DayIntervals)
                .ThenInclude(i => i.IntervalSymptoms)
                    .ThenInclude(s => s.Symptom)
            .Include(d => d.DayIntervals)
                .ThenInclude(i => i.IntervalSymptoms)
                    .ThenInclude(s => s.SeverityLevel)
            .Include(d => d.DayEvents)
                .ThenInclude(e => e.EventType)
            .Include(d => d.Measurements)
                .ThenInclude(m => m.MetricType)
            .FirstOrDefaultAsync(d => d.DiaryEntryId == diaryEntryId, ct);
    }

    public async Task<bool> UpdateDiaryEntryNoteAsync(int diaryEntryId, string? newNote, CancellationToken ct)
    {
        var entry = await db.DiaryEntries.FirstOrDefaultAsync(d => d.DiaryEntryId == diaryEntryId, ct);
        if (entry is null)
            return false;

        entry.EntryNote = newNote;
        entry.EntryUpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteDiaryEntryAsync(int diaryEntryId, CancellationToken ct)
    {
        var entry = await db.DiaryEntries.FirstOrDefaultAsync(d => d.DiaryEntryId == diaryEntryId, ct);
        if (entry is null)
            return false;

        db.DiaryEntries.Remove(entry);
        await db.SaveChangesAsync(ct);
        return true;
    }

    // --- 3) CRUD for DayEvent ---
    public async Task<DayEvent> CreateDayEventAsync(
        int diaryEntryId,
        int eventTypeId,
        byte? eventHour,
        string? eventNote,
        CancellationToken ct)
    {
        var dayEvent = new DayEvent
        {
            DiaryEntryId = diaryEntryId,
            EventTypeId = eventTypeId,
            EventHour = eventHour,
            EventNote = eventNote
        };

        db.DayEvents.Add(dayEvent);

        // Update EntryUpdatedAt on the parent DiaryEntry to track report freshness
        await db.DiaryEntries
            .Where(d => d.DiaryEntryId == diaryEntryId)
            .ExecuteUpdateAsync(s => s.SetProperty(d => d.EntryUpdatedAt, DateTime.UtcNow), ct);

        await db.SaveChangesAsync(ct);
        return dayEvent;
    }

    public async Task<IReadOnlyList<DayEvent>> GetDayEventsByEntryAsNoTrackingAsync(int diaryEntryId, CancellationToken ct)
    {
        return await db.DayEvents
            .AsNoTracking()
            .Include(e => e.EventType)
            .Where(e => e.DiaryEntryId == diaryEntryId)
            .OrderBy(e => e.EventHour)
            .ToListAsync(ct);
    }

    public async Task<bool> UpdateDayEventNoteAsync(int dayEventId, byte? newHour, string? newNote, CancellationToken ct)
    {
        var ev = await db.DayEvents.FirstOrDefaultAsync(e => e.DayEventId == dayEventId, ct);
        if (ev is null)
            return false;

        ev.EventHour = newHour;
        ev.EventNote = newNote;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteDayEventAsync(int dayEventId, CancellationToken ct)
    {
        var ev = await db.DayEvents.FirstOrDefaultAsync(e => e.DayEventId == dayEventId, ct);
        if (ev is null)
            return false;

        db.DayEvents.Remove(ev);
        await db.SaveChangesAsync(ct);
        return true;
    }

    // --- Complex business operation (Task 8): Persist full day entry graph + generate report snapshot in a transaction ---
    public async Task<(int DiaryEntryId, int ReportId)> CreateFullEntryGraphAndGenerateReportAsync(
        int userId,
        DateOnly entryDate,
        string? entryNote,
        IReadOnlyList<IntervalInputDto> intervals,
        IReadOnlyList<MeasurementInputDto> measurements,
        IReadOnlyList<(int EventTypeId, byte? Hour, string? Note)> events,
        DateOnly reportFrom,
        DateOnly reportTo,
        CancellationToken ct)
    {
        // Validate non-overlapping intervals before opening the database transaction
        var ordered = intervals.OrderBy(x => x.IntervalStartHour).ToList();
        for (int i = 0; i < ordered.Count; i++)
        {
            if (ordered[i].IntervalEndHour <= ordered[i].IntervalStartHour || ordered[i].IntervalEndHour > 24)
                throw new InvalidOperationException("Invalid interval hour bounds.");

            if (i > 0 && ordered[i].IntervalStartHour < ordered[i - 1].IntervalEndHour)
                throw new InvalidOperationException("Day intervals overlap with each other.");
        }

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        try
        {
            var now = DateTime.UtcNow;

            var entry = new DiaryEntry
            {
                UserId = userId,
                EntryDate = entryDate,
                EntryNote = entryNote,
                EntryCreatedAt = now,
                EntryUpdatedAt = now,
                DayIntervals = ordered.Select(i => new DayInterval
                {
                    IntervalStartHour = i.IntervalStartHour,
                    IntervalEndHour = i.IntervalEndHour,
                    WellbeingLevelId = i.WellbeingLevelId,
                    IntervalNote = i.IntervalNote,
                    IntervalSymptoms = i.Symptoms.Select(s => new IntervalSymptom
                    {
                        SymptomId = s.SymptomId,
                        SeverityLevelId = s.SeverityLevelId,
                        SymptomNote = s.SymptomNote
                    }).ToList()
                }).ToList(),
                Measurements = measurements.Select(m => new Measurement
                {
                    MetricTypeId = m.MetricTypeId,
                    MeasurementValue = m.MeasurementValue,
                    MeasurementHour = m.MeasurementHour,
                    MeasurementNote = m.MeasurementNote
                }).ToList(),
                DayEvents = events.Select(e => new DayEvent
                {
                    EventTypeId = e.EventTypeId,
                    EventHour = e.Hour,
                    EventNote = e.Note
                }).ToList()
            };

            db.DiaryEntries.Add(entry);
            await db.SaveChangesAsync(ct);

            // Build JSON snapshot of patient data for the specified period to share with a doctor
            var periodEntries = await db.DiaryEntries
                .AsNoTracking()
                .Where(d => d.UserId == userId && d.EntryDate >= reportFrom && d.EntryDate <= reportTo)
                .OrderBy(d => d.EntryDate)
                .Select(d => new
                {
                    d.EntryDate,
                    d.EntryNote,
                    IntervalsCount = d.DayIntervals.Count,
                    Symptoms = d.DayIntervals
                        .SelectMany(i => i.IntervalSymptoms)
                        .Select(s => new { s.Symptom.SymptomName, s.SeverityLevel.SeverityName })
                        .ToList(),
                    Measurements = d.Measurements
                        .Select(m => new { m.MetricType.MetricName, m.MeasurementValue, m.MetricType.MetricUnit })
                        .ToList()
                })
                .ToListAsync(ct);

            var snapshotJson = JsonSerializer.Serialize(new
            {
                UserId = userId,
                PeriodFrom = reportFrom,
                PeriodTo = reportTo,
                GeneratedAtUtc = now,
                TotalDaysRecorded = periodEntries.Count,
                Days = periodEntries
            });

            var report = new Report
            {
                UserId = userId,
                ReportPeriodFrom = reportFrom,
                ReportPeriodTo = reportTo,
                ReportGeneratedAt = now,
                ReportContent = snapshotJson
            };

            db.Reports.Add(report);
            await db.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);
            return (entry.DiaryEntryId, report.ReportId);
        }
        catch
        {
            try
            {
                await transaction.RollbackAsync(CancellationToken.None);
            }
            catch
            {
                // Ignore rollback exception if the connection was already lost
            }
            throw;
        }
    }

    // =========================================================================
    // Task 9. At least 6 LINQ queries + SQL extraction via ToQueryString (Task 13)
    // =========================================================================

    // Query 1: Multi-parameter filtering + AsNoTracking (returns DiaryEntry entities)
    public IQueryable<DiaryEntry> BuildFilteredEntriesQuery(
        int? userId,
        DateOnly? fromDate,
        DateOnly? toDate,
        int? symptomId)
    {
        IQueryable<DiaryEntry> query = db.DiaryEntries.AsNoTracking();

        if (userId is not null)
            query = query.Where(d => d.UserId == userId.Value);

        if (fromDate is not null)
            query = query.Where(d => d.EntryDate >= fromDate.Value);

        if (toDate is not null)
            query = query.Where(d => d.EntryDate <= toDate.Value);

        if (symptomId is not null)
            query = query.Where(d => d.DayIntervals.Any(i => i.IntervalSymptoms.Any(s => s.SymptomId == symptomId.Value)));

        return query.OrderByDescending(d => d.EntryDate).ThenBy(d => d.DiaryEntryId);
    }

    // Query 2: Multi-column sorting + DTO projection (Select)
    public IQueryable<SymptomOccurrenceDto> BuildSymptomTimelineProjectionQuery(int userId, byte minSeverityRank)
    {
        return db.IntervalSymptoms
            .AsNoTracking()
            .Where(s => s.DayInterval.DiaryEntry.UserId == userId && s.SeverityLevel.SeverityRank >= minSeverityRank)
            .OrderByDescending(s => s.DayInterval.DiaryEntry.EntryDate)
            .ThenBy(s => s.DayInterval.IntervalStartHour)
            .ThenByDescending(s => s.SeverityLevel.SeverityRank)
            .Select(s => new SymptomOccurrenceDto(
                s.DayInterval.DiaryEntryId,
                s.DayInterval.DiaryEntry.EntryDate,
                s.DayInterval.DiaryEntry.User.UserName,
                s.DayInterval.IntervalStartHour,
                s.DayInterval.IntervalEndHour,
                s.Symptom.SymptomName,
                s.Symptom.SymptomCategory,
                s.SeverityLevel.SeverityName,
                s.SeverityLevel.SeverityRank,
                s.SymptomNote));
    }

    // Query 3: GroupBy with aggregates (Count, Min, Max, Average) for physical metrics
    public IQueryable<MetricStatsDto> BuildMetricStatsGroupByQuery(int? userId)
    {
        var baseQuery = db.Measurements.AsNoTracking();
        if (userId is not null)
            baseQuery = baseQuery.Where(m => m.DiaryEntry.UserId == userId.Value);

        return baseQuery
            .GroupBy(m => new { m.MetricTypeId, m.MetricType.MetricName, m.MetricType.MetricUnit })
            .Where(g => g.Count() >= 1)
            .OrderByDescending(g => g.Count())
            .Select(g => new MetricStatsDto(
                g.Key.MetricTypeId,
                g.Key.MetricName,
                g.Key.MetricUnit,
                g.Count(),
                g.Min(m => m.MeasurementValue),
                g.Max(m => m.MeasurementValue),
                Math.Round(g.Average(m => m.MeasurementValue), 2)));
    }

    // Query 4: Pagination (Skip/Take) + stable sorting + page metadata
    public async Task<PagedResultDto<DiaryEntryDto>> GetPagedDiaryEntriesAsync(
        int? userId,
        int pageNumber,
        int pageSize,
        CancellationToken ct)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        pageNumber = Math.Max(pageNumber, 1);

        IQueryable<DiaryEntry> query = db.DiaryEntries.AsNoTracking();
        if (userId is not null)
            query = query.Where(d => d.UserId == userId.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(d => d.EntryDate)
            .ThenByDescending(d => d.DiaryEntryId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DiaryEntryDto(
                d.DiaryEntryId,
                d.UserId,
                d.User.UserName,
                d.EntryDate,
                d.EntryNote,
                d.EntryCreatedAt,
                d.EntryUpdatedAt))
            .ToListAsync(ct);

        return new PagedResultDto<DiaryEntryDto>(items, totalCount, pageNumber, pageSize);
    }

    // Query 5: Check report freshness (whether any diary entry in the report period was updated after report generation)
    public async Task<IReadOnlyList<(int ReportId, DateOnly PeriodFrom, DateOnly PeriodTo, DateTime GeneratedAt, bool IsOutdated)>>
        GetReportsFreshnessStatusAsync(int userId, CancellationToken ct)
    {
        var raw = await db.Reports
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.ReportGeneratedAt)
            .Select(r => new
            {
                r.ReportId,
                r.ReportPeriodFrom,
                r.ReportPeriodTo,
                r.ReportGeneratedAt,
                IsOutdated = db.DiaryEntries.Any(d =>
                    d.UserId == r.UserId &&
                    d.EntryDate >= r.ReportPeriodFrom &&
                    d.EntryDate <= r.ReportPeriodTo &&
                    d.EntryUpdatedAt > r.ReportGeneratedAt)
            })
            .ToListAsync(ct);

        return raw
            .Select(x => (x.ReportId, x.ReportPeriodFrom, x.ReportPeriodTo, x.ReportGeneratedAt, x.IsOutdated))
            .ToList();
    }
}
