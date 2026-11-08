using lab2_kpz.Core.EntityFramework;
using lab2_kpz.Data;
using lab2_kpz.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace lab2_kpz.Core.DemonstrativeScenarios;

public sealed class LinqLoggingScenario
{
    public async Task RunSqlAnalysisScenarioAsync(CancellationToken ct = default)
    {
        Console.WriteLine("---------------------------------");
        Console.WriteLine("  SCENARIO: LINQ TO SQL ANALYSIS ");
        Console.WriteLine("---------------------------------\n");

        await using var context = new SymptomDiaryDbContext();
        var linqQueries = new LinqQueries(context);

        // ------------------------------------------------------------------
        // QUERY 1: GroupBy with Aggregates (GetSymptomStatistics)
        // ------------------------------------------------------------------
        IQueryable<SymptomStatisticsDto> symptomStatsQuery = linqQueries.GetSymptomStatistics();

        Console.WriteLine("--- [QUERY 1] GROUP BY WITH AGGREGATES (SYMPTOM STATISTICS) ---");
        Console.WriteLine("Generated SQL:");
        Console.WriteLine(symptomStatsQuery.ToQueryString());
        // Console.WriteLine("\nORM SQL Explanation:");
        // Console.WriteLine("Tables     : IntervalSymptoms AS i, Symptoms AS s");
        // Console.WriteLine("JOIN       : INNER JOIN Symptoms AS s ON i.SymptomId = s.SymptomId");
        // Console.WriteLine("GROUP BY   : i.SymptomId, s.SymptomName");
        // Console.WriteLine("Projection : SymptomId, SymptomName, COUNT(*), ROUND(AVG(CAST(SeverityLevelId AS float)), 2), MIN, MAX");
        // Console.WriteLine("ORDER BY   : COUNT(*) DESC, s.SymptomName ASC\n");

        var statsResult = await symptomStatsQuery.ToListAsync(ct);
        Console.WriteLine($"Returned {statsResult.Count} grouped symptom records.\n");

        // ------------------------------------------------------------------
        // QUERY 2: Pagination composed with Sorting (GetPagedDiaryEntries)
        // ------------------------------------------------------------------
        int pageNumber = 2;
        int pageSize = 3;

        IQueryable<DiaryEntry> pagedQuery = linqQueries
            .GetSortedDiaryEntries()
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        Console.WriteLine("--- [QUERY 2] PAGINATION COMPOSED WITH SORTING (SKIP + TAKE) ---");
        Console.WriteLine("Generated SQL:");
        Console.WriteLine(pagedQuery.ToQueryString());
        // Console.WriteLine("\nORM SQL Explanation:");
        // Console.WriteLine("  * Tables     : DiaryEntries AS d");
        // Console.WriteLine("  * Projection : Explicit columns of DiaryEntries (DiaryEntryId, UserId, EntryDate, EntryNote, etc.)");
        // Console.WriteLine("  * ORDER BY   : d.EntryDate DESC, d.DiaryEntryId ASC");
        // Console.WriteLine("  * Pagination : OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY (executed natively on SQL Server)\n");

        var pagedResult = await pagedQuery.ToListAsync(ct);
        Console.WriteLine($"Returned {pagedResult.Count} items for page {pageNumber}.\n");

        // ------------------------------------------------------------------
        // QUERY 3: Multi-Parameter Filtering + DTO Projection with JOIN (via EF Core Logging)
        // ------------------------------------------------------------------
        int eventTypeId = 1;
        var fromTime = new TimeOnly(7, 0);
        var toTime = new TimeOnly(22, 0);

        var loggingOptions = new DbContextOptionsBuilder<SymptomDiaryDbContext>()
            .LogTo(
                Console.WriteLine,
                [DbLoggerCategory.Database.Command.Name],
                LogLevel.Information)
            .EnableSensitiveDataLogging()
            .Options;

        await using var loggingContext = new SymptomDiaryDbContext(loggingOptions);

        Console.WriteLine("--- [QUERY 3] FILTERING + JOIN + DTO PROJECTION (EF CORE LOGGING) ---");
        // Console.WriteLine("\nORM SQL Explanation:");
        // Console.WriteLine("  * Tables     : DayEvents AS d, DiaryEntries AS d0, EventTypes AS e");
        // Console.WriteLine("  * JOIN       : INNER JOIN DiaryEntries AS d0 ON d.DiaryEntryId = d0.DiaryEntryId");
        // Console.WriteLine("                 INNER JOIN EventTypes AS e ON d.EventTypeId = e.EventTypeId");
        // Console.WriteLine("  * WHERE      : d.EventTypeId = @__eventTypeId_0 AND d.EventStartTime >= @__fromTime_1 AND d.EventEndTime <= @__toTime_2");
        // Console.WriteLine("  * ORDER BY   : d.EventStartTime ASC");
        // Console.WriteLine("  * Projection : Only 6 requested DTO columns instead of SELECT *\n");

        var filteredEvents = await loggingContext.DayEvents
            .AsNoTracking()
            .Where(e => e.EventTypeId == eventTypeId
                     && e.EventStartTime >= fromTime
                     && e.EventEndTime <= toTime)
            .OrderBy(e => e.EventStartTime)
            .Select(e => new
            {
                e.DayEventId,
                e.DiaryEntry.EntryDate,
                e.EventType.EventTypeName,
                e.EventStartTime,
                e.EventEndTime,
                e.EventNote
            })
            .ToListAsync(ct);

        Console.WriteLine($"\nReturned {filteredEvents.Count} matching events.\n");
    }
}

