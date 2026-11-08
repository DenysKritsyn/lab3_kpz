using lab2_kpz.Core.AdoNet;
using lab2_kpz.Core.EntityFramework;
using lab2_kpz.Data;
using lab2_kpz.Entities;
using Microsoft.EntityFrameworkCore;
using EfDayEventRepository = lab2_kpz.Core.EntityFramework.Repositories.DayEventRepository;
using EfDayIntervalRepository = lab2_kpz.Core.EntityFramework.Repositories.DayIntervalRepository;
using EfDiaryEntryRepository = lab2_kpz.Core.EntityFramework.Repositories.DiaryEntryRepository;
using AdoDiaryEntryRepository = lab2_kpz.Core.AdoNet.DiaryEntryRepository;

namespace lab2_kpz.Core.DemonstrativeScenarios;

public sealed class ProgramDemonstration(string connectionString)
{
    private readonly string _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

    // Database State
    public async Task ShowDatabaseStateAsync(CancellationToken ct = default)
    {
        Console.WriteLine("========================");
        Console.WriteLine("  CURRENT DATABASE STATE");
        Console.WriteLine("========================");

        await using var context = new SymptomDiaryDbContext();

        Console.WriteLine($"Users             : {await context.Users.CountAsync(ct)}");
        Console.WriteLine($"DiaryEntries      : {await context.DiaryEntries.CountAsync(ct)}");
        Console.WriteLine($"DayIntervals      : {await context.DayIntervals.CountAsync(ct)}");
        Console.WriteLine($"IntervalSymptoms  : {await context.IntervalSymptoms.CountAsync(ct)}");
        Console.WriteLine($"DayEvents         : {await context.DayEvents.CountAsync(ct)}");
        Console.WriteLine($"Measurements      : {await context.Measurements.CountAsync(ct)}");
        Console.WriteLine($"Metrics           : {await context.Metrics.CountAsync(ct)}");
        Console.WriteLine($"Reports           : {await context.Reports.CountAsync(ct)}");
        Console.WriteLine($"Symptoms (Ref)    : {await context.Symptoms.CountAsync(ct)}");
        Console.WriteLine($"ConditionStates   : {await context.ConditionStates.CountAsync(ct)}");
        Console.WriteLine($"SeverityLevels    : {await context.SeverityLevels.CountAsync(ct)}");
        Console.WriteLine($"EventTypes        : {await context.EventTypes.CountAsync(ct)}");
        Console.WriteLine($"MetricUnits       : {await context.MetricUnits.CountAsync(ct)}\n");
    }

    // ADO.NET CRUD for 2 Related Entities (Users & DiaryEntries)
    public async Task RunAdoNetCrudAsync(CancellationToken ct = default)
    {
        Console.WriteLine("=====================================");
        Console.WriteLine(" ADO.NET CRUD (USERS & DIARY ENTRIES)");
        Console.WriteLine("=====================================");

        var userRepository = new UserRepository(_connectionString);
        var diaryRepository = new AdoDiaryEntryRepository(_connectionString);

        var newUser = new User
        {
            UserEmail = $"demo.adonet.{Guid.NewGuid():N}@example.com",
            UserName = "ADO.NET Demo Patient",
            UserBirthDate = new DateOnly(1998, 5, 20),
            UserPhoneNumber = "+380509998877"
        };

        int userId = await userRepository.Create(newUser, ct);
        Console.WriteLine($"[ADO.NET User CREATE] Created UserId = {userId}, Name = '{newUser.UserName}'");

        var readUser = await userRepository.Read(userId, ct);
        Console.WriteLine($"[ADO.NET User READ]   Read Email = '{readUser?.UserEmail}'");

        newUser.UserName = "ADO.NET Demo Patient (Updated)";
        await userRepository.Update(newUser, ct);
        var updatedUser = await userRepository.Read(userId, ct);
        Console.WriteLine($"[ADO.NET User UPDATE] Updated Name = '{updatedUser?.UserName}'");

        var newEntry = new DiaryEntry
        {
            UserId = userId,
            EntryDate = new DateOnly(2026, 11, 10),
            EntryNote = "Initial ADO.NET diary note."
        };

        int entryId = await diaryRepository.Create(newEntry, ct);
        Console.WriteLine($"[ADO.NET DiaryEntry CREATE] Created DiaryEntryId = {entryId} for UserId = {userId}");

        var readEntry = await diaryRepository.Read(entryId, ct);
        Console.WriteLine($"[ADO.NET DiaryEntry READ]   Date = {readEntry?.EntryDate}, Note = '{readEntry?.EntryNote}'");

        newEntry.EntryNote = "Updated ADO.NET diary note.";
        await diaryRepository.Update(newEntry, ct);
        var updatedEntry = await diaryRepository.Read(entryId, ct);
        Console.WriteLine($"[ADO.NET DiaryEntry UPDATE] Updated Note = '{updatedEntry?.EntryNote}'");

        await diaryRepository.Delete(entryId, ct);
        Console.WriteLine($"[ADO.NET DiaryEntry DELETE] Deleted DiaryEntryId = {entryId}");

        await userRepository.Delete(userId, ct);
        Console.WriteLine($"[ADO.NET User DELETE]       Deleted UserId = {userId}\n");
    }

    // ADO.NET Complex SQL Queries (JOIN 4 tables + GROUP BY with aggregates)
    public async Task RunComplexSqlQueriesAsync(CancellationToken ct = default)
    {
        Console.WriteLine("=============================");
        Console.WriteLine("  ADO.NET COMPLEX SQL QUERIES");
        Console.WriteLine("=============================");

        var complexRepo = new ComplexRequestsRepository(_connectionString);

        Console.WriteLine("--- Query A: Measurement Log Report (4-Table JOIN with WHERE & ORDER BY) ---");
        var logs = await complexRepo.GetMeasurementLogReport(
            userId: 1,
            fromDate: new DateOnly(2026, 11, 1),
            toDate: new DateOnly(2026, 11, 10),
            ct: ct);

        foreach (var log in logs)
        {
            Console.WriteLine($"{log.EntryDate} {log.MeasuringTime} | {log.MetricName}: {log.MetricValue} {log.UnitName}");
        }

        Console.WriteLine("\n--- Query B: Aggregated Health Metrics (GROUP BY + COUNT, AVG, MIN, MAX) ---");
        var summaries = await complexRepo.GetMetricSummaryReport(userId: 1, ct: ct);

        foreach (var s in summaries)
        {
            Console.WriteLine($"{s.MetricName} ({s.UnitName}) | Count: {s.TotalMeasurements}, Avg: {s.AverageValue}, Min: {s.MinValue}, Max: {s.MaxValue}");
        }
        Console.WriteLine();
    }

    // ADO.NET Transaction (Commit & Rollback)
    public async Task RunAdoNetTransactionScenarioAsync(CancellationToken ct = default)
    {
        Console.WriteLine("=========================================");
        Console.WriteLine("  ADO.NET TRANSACTION (COMMIT & ROLLBACK)");
        Console.WriteLine("=========================================");

        var transactionScenario = new TransactionScenario(_connectionString);
        await transactionScenario.RunCommitScenarioAsync(diaryEntryId: 1, ct: ct);
        await transactionScenario.RunRollbackScenarioAsync(diaryEntryId: 1, ct: ct);
    }

    // EF Core CRUD for 3 Entities (DiaryEntries, DayIntervals, DayEvents)
    public async Task RunEfCoreCrudAsync(CancellationToken ct = default)
    {
        Console.WriteLine("=========================================================");
        Console.WriteLine("  EF CORE CRUD (DIARY ENTRIES, DAY INTERVALS, DAY EVENTS)");
        Console.WriteLine("=========================================================");

        await using var context = new SymptomDiaryDbContext();
        var diaryRepo = new EfDiaryEntryRepository(context);
        var intervalRepo = new EfDayIntervalRepository(context);
        var eventRepo = new EfDayEventRepository(context);

        // Clean up any leftover entry for this test date if present
        var testDate = new DateOnly(2026, 11, 25);
        var existing = await context.DiaryEntries.FirstOrDefaultAsync(d => d.UserId == 1 && d.EntryDate == testDate, ct);
        if (existing is not null)
        {
            await diaryRepo.Delete(existing.DiaryEntryId, ct);
        }

        // 1) DiaryEntry CRUD
        var entry = new DiaryEntry
        {
            UserId = 1,
            EntryDate = testDate,
            EntryNote = "EF Core CRUD test entry"
        };
        int entryId = await diaryRepo.Create(entry, ct);
        Console.WriteLine($"[EF DiaryEntry CREATE] Created DiaryEntryId = {entryId}");

        await diaryRepo.Update(entryId, "EF Core CRUD updated entry note", ct);
        var readEntry = await diaryRepo.Read(entryId, ct);
        Console.WriteLine($"[EF DiaryEntry READ/UPDATE] Note = '{readEntry?.EntryNote}'");

        // 2) DayInterval CRUD
        var interval = new DayInterval
        {
            DiaryEntryId = entryId,
            IntervalStartTime = new TimeOnly(9, 0),
            IntervalEndTime = new TimeOnly(13, 0),
            ConditionStateId = 4,
            IntervalNote = "Morning interval"
        };
        int intervalId = await intervalRepo.Create(interval, ct);
        Console.WriteLine($"[EF DayInterval CREATE] Created DayIntervalId = {intervalId}");

        await intervalRepo.Update(intervalId, new TimeOnly(9, 30), new TimeOnly(14, 0), 5, "Updated morning interval", ct);
        var readInterval = await intervalRepo.Read(intervalId, ct);
        Console.WriteLine($"[EF DayInterval READ/UPDATE] {readInterval?.IntervalStartTime}-{readInterval?.IntervalEndTime}, State = {readInterval?.ConditionState.ConditionStateName}");

        // 3) DayEvent CRUD
        var dayEvent = new DayEvent
        {
            DiaryEntryId = entryId,
            EventTypeId = 2,
            EventStartTime = new TimeOnly(10, 0),
            EventEndTime = new TimeOnly(10, 45),
            IsPreciseTime = true,
            EventNote = "Light morning stretching"
        };
        int eventId = await eventRepo.Create(dayEvent, ct);
        Console.WriteLine($"[EF DayEvent CREATE] Created DayEventId = {eventId}");

        await eventRepo.Update(eventId, 2, new TimeOnly(10, 15), new TimeOnly(11, 0), true, "Updated workout session", ct);
        var readEvent = await eventRepo.Read(eventId, ct);
        Console.WriteLine($"[EF DayEvent READ/UPDATE] Type = {readEvent?.EventType.EventTypeName}, Note = '{readEvent?.EventNote}'");

        // Delete entities
        await eventRepo.Delete(eventId, ct);
        await intervalRepo.Delete(intervalId, ct);
        await diaryRepo.Delete(entryId, ct);
        Console.WriteLine("[EF DELETE] Cleaned up DayEvent, DayInterval, and DiaryEntry.\n");
    }

    // EF Core Complex Transactional Operation with Entity Graph (CreateReport)
    public async Task RunEfCoreComplexBusinessOperationAsync(CancellationToken ct = default)
    {
        Console.WriteLine("==================================================");
        Console.WriteLine("  EF CORE ENTITY GRAPH TRANSACTION (CREATE REPORT)");
        Console.WriteLine("==================================================");

        await using var context = new SymptomDiaryDbContext();
        var periodFrom = new DateOnly(2026, 11, 1);
        var periodTo = new DateOnly(2026, 11, 15);

        // Remove previously generated demo report for this exact period
        var oldReport = await context.Reports
            .FirstOrDefaultAsync(r => r.UserId == 1 && r.ReportPeriodFrom == periodFrom && r.ReportPeriodTo == periodTo, ct);
        if (oldReport is not null)
        {
            context.Reports.Remove(oldReport);
            await context.SaveChangesAsync(ct);
        }

        var reportCreator = new CreateReport(context);
        var report = await reportCreator.ExecuteAsync(
            userId: 1,
            periodFrom: periodFrom,
            periodTo: periodTo,
            ct: ct);

        Console.WriteLine($"[REPORT GENERATED] ReportId = {report.ReportId}");
        Console.WriteLine($"Period      : {report.ReportPeriodFrom} .. {report.ReportPeriodTo}");
        Console.WriteLine($"GeneratedAt : {report.ReportGeneratedAt:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"Summary     : {report.ReportSummary}\n");
    }

    // Pagination (Skip + Take composed with Sorting)
    public async Task RunPaginationAsync(CancellationToken ct = default)
    {
        Console.WriteLine("===========================================");
        Console.WriteLine("  PAGINATION OF DIARY ENTRIES (SKIP + TAKE)");
        Console.WriteLine("===========================================");

        await using var context = new SymptomDiaryDbContext();
        var linqQueries = new LinqQueries(context);

        int pageSize = 2;
        var page1 = await linqQueries.GetPagedDiaryEntriesAsync(pageNumber: 1, pageSize: pageSize, ct: ct);

        Console.WriteLine($"Total Records: {page1.TotalCount} | Page Size: {page1.PageSize} | Total Pages: {page1.TotalPages}");
        Console.WriteLine($"--- Page {page1.PageNumber} of {page1.TotalPages} ---");
        foreach (var item in page1.Items)
        {
            Console.WriteLine($"DiaryEntryId = {item.DiaryEntryId} | Date = {item.EntryDate} | UserId = {item.UserId} | Note = {item.EntryNote}");
        }

        if (page1.TotalPages >= 2)
        {
            var page2 = await linqQueries.GetPagedDiaryEntriesAsync(pageNumber: 2, pageSize: pageSize, ct: ct);
            Console.WriteLine($"--- Page {page2.PageNumber} of {page2.TotalPages} ---");
            foreach (var item in page2.Items)
            {
                Console.WriteLine($"DiaryEntryId = {item.DiaryEntryId} | Date = {item.EntryDate} | UserId = {item.UserId} | Note = {item.EntryNote}");
            }
        }
        Console.WriteLine();
    }

    // Optimistic Concurrency Conflict (Database Wins)
    public async Task RunConcurrencyConflictScenarioAsync(CancellationToken ct = default)
    {
        Console.WriteLine("==============================================");
        Console.WriteLine("  OPTIMISTIC CONCURRENCY CONFLICT (ROWVERSION)");
        Console.WriteLine("==============================================");

        var concurrencyScenario = new ConcurrencyScenario();
        await concurrencyScenario.RunDatabaseWinsScenarioAsync(diaryEntryId: 1, ct: ct);
        Console.WriteLine();
    }

    // SQL Logging & ToQueryString Analysis
    public async Task RunSqlLoggingScenarioAsync(CancellationToken ct = default)
    {
        Console.WriteLine("=====================================");
        Console.WriteLine(" SQL LOGGING & TOQUERYSTRING ANALYSIS");
        Console.WriteLine("=====================================");

        var loggingScenario = new LinqLoggingScenario();
        await loggingScenario.RunSqlAnalysisScenarioAsync(ct);
    }

    // Run full demonstration suite sequentially
    public async Task RunAllAsync(CancellationToken ct = default)
    {
        await ShowDatabaseStateAsync(ct);
        await RunAdoNetCrudAsync(ct);
        await RunComplexSqlQueriesAsync(ct);
        await RunAdoNetTransactionScenarioAsync(ct);
        await RunEfCoreCrudAsync(ct);
        await RunEfCoreComplexBusinessOperationAsync(ct);
        await RunPaginationAsync(ct);
        await RunConcurrencyConflictScenarioAsync(ct);
        await RunSqlLoggingScenarioAsync(ct);
    }
}

