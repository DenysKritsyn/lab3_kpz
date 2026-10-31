using Kpz.Lab2.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kpz.Lab2.Services;

public sealed class TaskManager(IServiceProvider serviceProvider, string connectionString)
{
    private readonly AdoNetDiaryService _adoService = new(connectionString);

    public async Task RunMigrationAndVerifySeedDataAsync(CancellationToken ct)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SymptomDiaryDbContext>();

        await db.Database.MigrateAsync(ct);

        int wellbeingCount = await db.WellbeingLevels.CountAsync(ct);
        int severityCount = await db.SeverityLevels.CountAsync(ct);
        int symptomsCount = await db.Symptoms.CountAsync(ct);
        int eventTypesCount = await db.EventTypes.CountAsync(ct);
        int metricTypesCount = await db.MetricTypes.CountAsync(ct);
        int usersCount = await db.Users.CountAsync(ct);

        int entriesCount = await db.DiaryEntries.CountAsync(ct);
        int intervalsCount = await db.DayIntervals.CountAsync(ct);
        int intervalSymptomsCount = await db.IntervalSymptoms.CountAsync(ct);
        int dayEventsCount = await db.DayEvents.CountAsync(ct);
        int measurementsCount = await db.Measurements.CountAsync(ct);
        int reportsCount = await db.Reports.CountAsync(ct);

        int refAndMainTotal = wellbeingCount + severityCount + symptomsCount + eventTypesCount + metricTypesCount + usersCount;
        int transactionalTotal = entriesCount + intervalsCount + intervalSymptomsCount + dayEventsCount + measurementsCount + reportsCount;

        Console.WriteLine("  Reference tables (HasData):");
        Console.WriteLine($"    - WellbeingLevels : {wellbeingCount}");
        Console.WriteLine($"    - SeverityLevels  : {severityCount}");
        Console.WriteLine($"    - Symptoms        : {symptomsCount}");
        Console.WriteLine($"    - EventTypes      : {eventTypesCount}");
        Console.WriteLine($"    - MetricTypes     : {metricTypesCount}");

        Console.WriteLine("  Main & Transactional tables (UseAsyncSeeding):");
        Console.WriteLine($"    - Users           : {usersCount}");
        Console.WriteLine($"    - DiaryEntries    : {entriesCount}");
        Console.WriteLine($"    - DayIntervals    : {intervalsCount}");
        Console.WriteLine($"    - IntervalSymptoms: {intervalSymptomsCount}");
        Console.WriteLine($"    - DayEvents       : {dayEventsCount}");
        Console.WriteLine($"    - Measurements    : {measurementsCount}");
        Console.WriteLine($"    - Reports         : {reportsCount}");
        Console.WriteLine($"  TOTAL: Reference/Main = {refAndMainTotal} (required >= 10), Transactional = {transactionalTotal} (required >= 30).\n");
    }

    public async Task RunAdoNetCrudDemoAsync(CancellationToken ct)
    {
        int demoUserId = await GetDefaultUserIdAsync(ct);
        var adoDate = new DateOnly(2026, 10, 28);

        await CleanupEntryByDateAsync(demoUserId, adoDate, ct);

        int newEntryId = await _adoService.CreateDiaryEntryAsync(
            demoUserId,
            adoDate,
            "ADO.NET: Initial day entry note",
            ct);
        Console.WriteLine($"  [CREATE DiaryEntry] Created DiaryEntryId = {newEntryId}");

        int newMeasurementId = await _adoService.CreateMeasurementAsync(
            newEntryId,
            metricTypeId: 1,
            measurementValue: 37.65m,
            measurementHour: 5,
            measurementNote: "ADO.NET: Temperature check at 11:00",
            ct);
        Console.WriteLine($"  [CREATE Measurement] Created MeasurementId = {newMeasurementId}");

        var readEntry = await _adoService.GetDiaryEntryByIdAsync(newEntryId, ct);
        Console.WriteLine($"  [READ DiaryEntry] User: {readEntry?.UserName} | Date: {readEntry?.EntryDate} | Note: {readEntry?.EntryNote}");

        await _adoService.UpdateDiaryEntryNoteAsync(newEntryId, "ADO.NET: Updated note after follow-up check", ct);
        await _adoService.UpdateMeasurementAsync(newMeasurementId, 37.20m, 6, "ADO.NET: Fever decreased", ct);

        var measurementsList = await _adoService.GetMeasurementsByEntryAsync(newEntryId, ct);
        foreach (var m in measurementsList)
        {
            Console.WriteLine($"  [READ Measurement after UPDATE] ID={m.MeasurementId}: {m.MetricName} = {m.MeasurementValue} {m.MetricUnit} (hour +{m.MeasurementHour} from 06:00) — {m.MeasurementNote}");
        }

        await _adoService.DeleteMeasurementAsync(newMeasurementId, ct);
        await _adoService.DeleteDiaryEntryAsync(newEntryId, ct);
        Console.WriteLine($"  [DELETE] Temporary records MeasurementId={newMeasurementId} and DiaryEntryId={newEntryId} deleted successfully.\n");
    }

    public async Task RunAdoNetComplexQueriesDemoAsync(CancellationToken ct)
    {
        var severeSymptoms = await _adoService.SearchSymptomOccurrencesAsync(
            userId: null,
            minSeverityRank: 2,
            fromDate: new DateOnly(2026, 10, 20),
            ct);

        Console.WriteLine($"  1) Multi-table JOIN (6 tables, symptom occurrences with SeverityRank >= 2): found {severeSymptoms.Count} rows");
        foreach (var item in severeSymptoms)
        {
            Console.WriteLine($"     - {item.EntryDate} [{item.IntervalStartHour:D2}..{item.IntervalEndHour:D2}h from 06:00] {item.UserName}: {item.SymptomName} ({item.SeverityName}, rank {item.SeverityRank}) — {item.SymptomNote}");
        }

        var symptomStats = await _adoService.GetSymptomAggregatedStatsAsync(minOccurrences: 1, ct);
        Console.WriteLine("\n  2) GROUP BY + 4 aggregates (COUNT occurrences, SUM duration hours, AVG & MAX severity rank):");
        foreach (var st in symptomStats)
        {
            Console.WriteLine($"     - Symptom '{st.SymptomName}' ({st.SymptomCategory}): count = {st.OccurrencesCount}, totalHours = {st.TotalHoursDuration}, avgRank = {st.AverageSeverityRank:F2}, maxRank = {st.MaxSeverityRank}");
        }
        Console.WriteLine();
    }

    public async Task RunAdoNetTransactionsDemoAsync(CancellationToken ct)
    {
        int demoUserId = await GetDefaultUserIdAsync(ct);
        var txCommitDate = new DateOnly(2026, 10, 24);
        var txRollbackDate = new DateOnly(2026, 10, 25);

        await CleanupEntryByDateAsync(demoUserId, txCommitDate, ct);
        await CleanupEntryByDateAsync(demoUserId, txRollbackDate, ct);

        int committedEntryId = await _adoService.RecordFullDayEntryInTransactionAsync(
            demoUserId,
            txCommitDate,
            "ADO.NET Transaction (Commit): Full day record",
            intervals:
            [
                new IntervalInputDto(0, 8, WellbeingLevelId: 2, "Morning interval",
                [
                    new IntervalSymptomInputDto(SymptomId: 1, SeverityLevelId: 1, "Mild morning headache")
                ]),
                new IntervalInputDto(8, 16, WellbeingLevelId: 1, "Afternoon interval without pain", [])
            ],
            measurements:
            [
                new MeasurementInputDto(MetricTypeId: 1, MeasurementValue: 36.70m, MeasurementHour: 2, "Morning temperature"),
                new MeasurementInputDto(MetricTypeId: 4, MeasurementValue: 74.00m, MeasurementHour: 2, "Morning heart rate")
            ],
            ct);

        Console.WriteLine($"  1) [COMMIT SUCCESS] Created DiaryEntryId={committedEntryId} across 4 tables (DiaryEntries, DayIntervals, IntervalSymptoms, Measurements).");

        try
        {
            await _adoService.RecordFullDayEntryInTransactionAsync(
                demoUserId,
                txRollbackDate,
                "ADO.NET Transaction (Rollback): Must be rolled back",
                intervals:
                [
                    new IntervalInputDto(2, 10, WellbeingLevelId: 3, "First interval",
                    [
                        new IntervalSymptomInputDto(SymptomId: 3, SeverityLevelId: 2, "Cough")
                    ]),
                    new IntervalInputDto(10, 18, WellbeingLevelId: 2, "Second interval", [])
                ],
                measurements:
                [
                    // Invalid temperature 49.50 °C exceeds [34.00; 43.00] bounds -> triggers Rollback
                    new MeasurementInputDto(MetricTypeId: 1, MeasurementValue: 49.50m, MeasurementHour: 5, "Invalid temperature value")
                ],
                ct);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"  2) [ROLLBACK TRIGGERED] Caught business rule violation: {ex.Message}");
        }

        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SymptomDiaryDbContext>();
        bool existsAfterRollback = await db.DiaryEntries.AnyAsync(d => d.UserId == demoUserId && d.EntryDate == txRollbackDate, ct);
        Console.WriteLine($"     Database check after Rollback: entry for {txRollbackDate} exists = {existsAfterRollback} (expected False).\n");
    }

    public async Task RunEfCoreCrudAndGraphTransactionDemoAsync(CancellationToken ct)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SymptomDiaryDbContext>();
        var efService = new EfCoreDiaryService(db);

        await db.Users.Where(u => u.UserEmail == "temp.patient@example.com").ExecuteDeleteAsync(ct);

        var createdUser = await efService.CreateUserAsync(
            "google-oauth2|999999",
            "temp.patient@example.com",
            "Taras Bondar",
            new DateOnly(2001, 8, 12),
            "User",
            ct);
        Console.WriteLine($"  [EF CRUD User] Created UserId={createdUser.UserId} ({createdUser.UserName})");

        await efService.UpdateUserNameAsync(createdUser.UserId, "Taras Bondar (Updated)", ct);
        var fetchedUser = await efService.GetUserByIdAsNoTrackingAsync(createdUser.UserId, ct);
        Console.WriteLine($"  [EF CRUD User AsNoTracking] After update: {fetchedUser?.UserName}");

        var tempEntry = await efService.CreateDiaryEntryAsync(
            createdUser.UserId,
            new DateOnly(2026, 10, 26),
            "EF Core: Temporary day entry",
            ct);

        var tempEvent = await efService.CreateDayEventAsync(
            tempEntry.DiaryEntryId,
            eventTypeId: 1,
            eventHour: 6,
            eventNote: "Work stress",
            ct);

        await efService.UpdateDayEventNoteAsync(tempEvent.DayEventId, 7, "Severe meeting stress (updated)", ct);

        var eventsForEntry = await efService.GetDayEventsByEntryAsNoTrackingAsync(tempEntry.DiaryEntryId, ct);
        Console.WriteLine($"  [EF CRUD DayEvent] Events in entry: {eventsForEntry.Count}, note: '{eventsForEntry[0].EventNote}'");

        // Negative scenario: verify DeleteBehavior.Restrict on reference table Symptoms
        try
        {
            var symptomInUse = await db.Symptoms.FirstAsync(s => s.SymptomId == 1, ct);
            db.Symptoms.Remove(symptomInUse);
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            db.ChangeTracker.Clear();
            Console.WriteLine($"  [DeleteBehavior.Restrict Check] Deleting referenced SymptomId=1 blocked with: {ex.GetType().Name}");
        }

        await efService.DeleteDayEventAsync(tempEvent.DayEventId, ct);
        await efService.DeleteDiaryEntryAsync(tempEntry.DiaryEntryId, ct);
        await efService.DeleteUserAsync(createdUser.UserId, ct);
        Console.WriteLine("  [EF CRUD Delete] Temporary entities DayEvent, DiaryEntry, and User deleted.");

        int demoUserId = await GetDefaultUserIdAsync(ct);
        var graphDate = new DateOnly(2026, 10, 27);
        await CleanupEntryByDateAsync(demoUserId, graphDate, ct);

        var (graphEntryId, generatedReportId) = await efService.CreateFullEntryGraphAndGenerateReportAsync(
            demoUserId,
            graphDate,
            "EF Graph Transaction: Full day entry + doctor report generation",
            intervals:
            [
                new IntervalInputDto(1, 9, WellbeingLevelId: 2, "Morning tiredness",
                [
                    new IntervalSymptomInputDto(SymptomId: 7, SeverityLevelId: 1, "Post-workout fatigue")
                ])
            ],
            measurements:
            [
                new MeasurementInputDto(MetricTypeId: 2, MeasurementValue: 124.00m, MeasurementHour: 3, "Normal systolic BP"),
                new MeasurementInputDto(MetricTypeId: 3, MeasurementValue: 80.00m, MeasurementHour: 3, "Normal diastolic BP")
            ],
            events: [(EventTypeId: 4, Hour: (byte?)2, Note: "Morning 5km run")],
            reportFrom: new DateOnly(2026, 10, 20),
            reportTo: graphDate,
            ct);

        Console.WriteLine($"  [EF Complex Graph Transaction] Saved entity graph DiaryEntryId={graphEntryId} and generated snapshot ReportId={generatedReportId}.\n");
    }

    public async Task RunLinqQueriesAndSqlDiagnosticsDemoAsync(CancellationToken ct)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SymptomDiaryDbContext>();
        var efService = new EfCoreDiaryService(db);

        int demoUserId = await GetDefaultUserIdAsync(ct);

        // 1. Multi-parameter filter + AsNoTracking
        var q1 = efService.BuildFilteredEntriesQuery(
            demoUserId,
            new DateOnly(2026, 10, 20),
            new DateOnly(2026, 10, 27),
            symptomId: 1);
        var filteredEntries = await q1.ToListAsync(ct);
        Console.WriteLine($"  1) Multi-parameter filter (UserId={demoUserId}, SymptomId=1 'Headache'): found {filteredEntries.Count} entries");

        // 2. Sorting + DTO projection
        var q2 = efService.BuildSymptomTimelineProjectionQuery(demoUserId, minSeverityRank: 1);
        var timeline = await q2.ToListAsync(ct);
        Console.WriteLine($"  2) DTO Projection & multi-column sorting: retrieved {timeline.Count} symptom timeline rows");

        // 3. Eager loading with Include / ThenInclude + AsNoTracking
        var firstEntryId = filteredEntries.First().DiaryEntryId;
        var fullEntryGraph = await efService.GetDiaryEntryWithGraphAsync(firstEntryId, ct);
        Console.WriteLine($"  3) Include/ThenInclude graph for DiaryEntryId={firstEntryId} ({fullEntryGraph?.EntryDate}): intervals={fullEntryGraph?.DayIntervals.Count}, events={fullEntryGraph?.DayEvents.Count}, measurements={fullEntryGraph?.Measurements.Count}");

        // 4. GroupBy with aggregates
        var q3 = efService.BuildMetricStatsGroupByQuery(demoUserId);
        var metricStats = await q3.ToListAsync(ct);
        Console.WriteLine("  4) LINQ GroupBy on physical metrics:");
        foreach (var ms in metricStats)
        {
            Console.WriteLine($"     - {ms.MetricName}: count={ms.MeasurementsCount}, min={ms.MinMeasuredValue}, max={ms.MaxMeasuredValue}, avg={ms.AverageMeasuredValue} {ms.MetricUnit}");
        }

        // 5. Pagination
        var page1 = await efService.GetPagedDiaryEntriesAsync(demoUserId, pageNumber: 1, pageSize: 2, ct);
        var page2 = await efService.GetPagedDiaryEntriesAsync(demoUserId, pageNumber: 2, pageSize: 2, ct);
        Console.WriteLine($"  5) Pagination (PageSize=2, TotalCount={page1.TotalCount}):");
        Console.WriteLine($"     - Page 1 dates: {string.Join(", ", page1.Items.Select(x => x.EntryDate.ToString()))}");
        Console.WriteLine($"     - Page 2 dates: {string.Join(", ", page2.Items.Select(x => x.EntryDate.ToString()))}");

        // 6. Report freshness check
        var reportsStatus = await efService.GetReportsFreshnessStatusAsync(demoUserId, ct);
        Console.WriteLine("  6) Report freshness status (checks if any entry in period was modified after report generation):");
        foreach (var r in reportsStatus)
        {
            Console.WriteLine($"     - ReportId={r.ReportId} ({r.PeriodFrom}..{r.PeriodTo}): IsOutdated = {r.IsOutdated}");
        }

        // Task 13: Extract generated SQL via ToQueryString()
        Console.WriteLine("\n  [Task 13] Extracted SQL via ToQueryString():");
        Console.WriteLine("  --- Query 1 SQL (Multi-parameter filtering with EXISTS) ---");
        Console.WriteLine(q1.ToQueryString());
        Console.WriteLine("  --- Query 2 SQL (DTO Projection with INNER JOINs and ORDER BY) ---");
        Console.WriteLine(q2.ToQueryString());
        Console.WriteLine("  --- Query 3 SQL (GroupBy with COUNT, MIN, MAX, AVG) ---");
        Console.WriteLine(q3.ToQueryString());
        Console.WriteLine();
    }

    public async Task RunOptimisticConcurrencyDemoAsync(CancellationToken ct)
    {
        using var scope1 = serviceProvider.CreateScope();
        using var scope2 = serviceProvider.CreateScope();

        var db1 = scope1.ServiceProvider.GetRequiredService<SymptomDiaryDbContext>();
        var db2 = scope2.ServiceProvider.GetRequiredService<SymptomDiaryDbContext>();

        int targetId = await db1.DiaryEntries
            .OrderBy(d => d.DiaryEntryId)
            .Select(d => d.DiaryEntryId)
            .FirstAsync(ct);

        // Both isolated DbContext instances load the same DiaryEntry concurrently
        var entrySessionA = await db1.DiaryEntries.SingleAsync(d => d.DiaryEntryId == targetId, ct);
        var entrySessionB = await db2.DiaryEntries.SingleAsync(d => d.DiaryEntryId == targetId, ct);

        Console.WriteLine($"  Session A and Session B loaded DiaryEntryId={targetId} with RowVersion={Convert.ToHexString(entrySessionA.EntryRowVersion)}");

        // Session A updates and saves first
        entrySessionA.EntryNote = "Session A (Mobile): Added evening symptom note.";
        entrySessionA.EntryUpdatedAt = DateTime.UtcNow;
        await db1.SaveChangesAsync(ct);
        Console.WriteLine($"  Session A saved changes successfully. New RowVersion={Convert.ToHexString(entrySessionA.EntryRowVersion)}");

        // Session B tries to save using its stale EntryRowVersion token
        entrySessionB.EntryNote = "Session B (Desktop): Attempting to overwrite with stale version.";
        entrySessionB.EntryUpdatedAt = DateTime.UtcNow;

        try
        {
            await db2.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Console.WriteLine($"  [CONCURRENCY CONFLICT CAUGHT] {ex.GetType().Name}!");
            var conflictedEntry = ex.Entries.Single();
            var databaseValues = await conflictedEntry.GetDatabaseValuesAsync(ct);

            if (databaseValues is null)
            {
                Console.WriteLine("  The record was deleted by another user.");
            }
            else
            {
                var dbNote = databaseValues.GetValue<string?>(nameof(entrySessionB.EntryNote));
                Console.WriteLine($"  Current value in DB (from Session A): '{dbNote}'");
                Console.WriteLine("  Resolving conflict: refreshing OriginalValues with current DB rowversion and merging notes.");

                entrySessionB.EntryNote = $"{dbNote} | Merged with Session B (Desktop).";
                conflictedEntry.OriginalValues.SetValues(databaseValues);
                await db2.SaveChangesAsync(ct);
                Console.WriteLine($"  Re-save succeeded! Updated RowVersion={Convert.ToHexString(entrySessionB.EntryRowVersion)}\n");
            }
        }
    }

    public async Task RunSecondMigrationAndScaffoldVerificationDemoAsync(CancellationToken ct)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SymptomDiaryDbContext>();

        var appliedMigrations = (await db.Database.GetAppliedMigrationsAsync(ct)).ToList();
        Console.WriteLine($"  Applied migrations in DB ({appliedMigrations.Count}):");
        foreach (var m in appliedMigrations)
        {
            Console.WriteLine($"    - {m}");
        }

        // Verify the newly added UserPhoneNumber property from Migration 2
        int demoUserId = await GetDefaultUserIdAsync(ct);
        await db.Users
            .Where(u => u.UserId == demoUserId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.UserPhoneNumber, "+380671112233"), ct);

        var userWithPhone = await db.Users
            .AsNoTracking()
            .FirstAsync(u => u.UserId == demoUserId, ct);

        Console.WriteLine($"  Verified new column UserPhoneNumber on '{userWithPhone.UserName}': {userWithPhone.UserPhoneNumber}");

        // Verify Reverse Engineering (Scaffolded DbContext) against the live database
        var scaffoldOptions = new DbContextOptionsBuilder<Kpz.Lab2.Data.Scaffolded.ScaffoldedSymptomDiaryDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var scaffoldDb = new Kpz.Lab2.Data.Scaffolded.ScaffoldedSymptomDiaryDbContext(scaffoldOptions);
        int scaffoldedUsersCount = await scaffoldDb.Users.CountAsync(ct);
        int scaffoldedEntriesCount = await scaffoldDb.DiaryEntries.CountAsync(ct);
        Console.WriteLine($"  Verified ScaffoldedSymptomDiaryDbContext (Reverse Engineering): Users={scaffoldedUsersCount}, DiaryEntries={scaffoldedEntriesCount}\n");
    }

    private async Task<int> GetDefaultUserIdAsync(CancellationToken ct)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SymptomDiaryDbContext>();
        return await db.Users.OrderBy(u => u.UserId).Select(u => u.UserId).FirstAsync(ct);
    }

    private async Task CleanupEntryByDateAsync(int userId, DateOnly date, CancellationToken ct)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SymptomDiaryDbContext>();
        await db.DiaryEntries
            .Where(d => d.UserId == userId && d.EntryDate == date)
            .ExecuteDeleteAsync(ct);
    }
}

