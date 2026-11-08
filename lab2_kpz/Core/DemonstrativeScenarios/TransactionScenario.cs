using lab2_kpz.Core.AdoNet;
using lab2_kpz.Data;
using Microsoft.EntityFrameworkCore;

namespace lab2_kpz.Core.DemonstrativeScenarios;

public sealed class TransactionScenario(string connectionString)
{
    private readonly Transactions _transactions = new(connectionString);

    public async Task RunCommitScenarioAsync(int diaryEntryId = 1, CancellationToken ct = default)
    {
        Console.WriteLine("--------------------------------");
        Console.WriteLine("  SCENARIO 1: TRANSACTION COMMIT");
        Console.WriteLine("--------------------------------");

        DiaryStateSnapshot before = await GetStateSnapshotAsync(diaryEntryId, ct);
        PrintSnapshot("STATE BEFORE COMMIT", before);

        int newMeasurementId = await _transactions.AddMeasurementWithAuditAsync(
            diaryEntryId: diaryEntryId,
            measuringTime: new TimeOnly(12, 30),
            metricUnitId: 1, // Degrees Celsius (34.00 .. 43.00)
            metricName: "Body Temperature",
            metricValue: 36.70m,
            simulateMissingReport: false,
            ct: ct);

        Console.WriteLine($"\n[COMMIT SUCCESS] Created MeasurementId = {newMeasurementId}\n");

        DiaryStateSnapshot after = await GetStateSnapshotAsync(diaryEntryId, ct);
        PrintSnapshot("STATE AFTER COMMIT", after);
        Console.WriteLine();
    }

    public async Task RunRollbackScenarioAsync(int diaryEntryId = 1, CancellationToken ct = default)
    {
        Console.WriteLine("----------------------------------");
        Console.WriteLine("  SCENARIO 2: TRANSACTION ROLLBACK");
        Console.WriteLine("----------------------------------");

        DiaryStateSnapshot before = await GetStateSnapshotAsync(diaryEntryId, ct);
        PrintSnapshot("STATE BEFORE ROLLBACK", before);

        try
        {

            await _transactions.AddMeasurementWithAuditAsync(
                diaryEntryId: diaryEntryId,
                measuringTime: new TimeOnly(15, 45),
                metricUnitId: 1,
                metricName: "Body Temperature",
                metricValue: 38.60m,
                simulateMissingReport: true, // Intentional error. First 2 of transaction stages will be completed, but it won't be able to find a report
                ct: ct);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"\n[ROLLBACK EXECUTED] Caught expected error:\n    \"{ex.Message}\"\n");
        }

        DiaryStateSnapshot after = await GetStateSnapshotAsync(diaryEntryId, ct);
        PrintSnapshot("STATE AFTER ROLLBACK", after);

        Console.WriteLine("\n[VERIFICATION RESULT] Even though DiaryEntry, Measurement and Metric were touched in the DB,");
        Console.WriteLine("the failure to find an active Report at the final step rolled back all 3 tables completely.\n");
    }

    private static async Task<DiaryStateSnapshot> GetStateSnapshotAsync(int diaryEntryId, CancellationToken ct)
    {
        await using var context = new SymptomDiaryDbContext();

        var diary = await context.DiaryEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DiaryEntryId == diaryEntryId, ct)
            ?? throw new InvalidOperationException($"DiaryEntry with ID {diaryEntryId} was not found.");

        int measurementsCount = await context.Measurements
            .AsNoTracking()
            .CountAsync(m => m.DiaryEntryId == diaryEntryId, ct);

        int metricsCount = await context.Metrics
            .AsNoTracking()
            .CountAsync(m => m.Measurement.DiaryEntryId == diaryEntryId, ct);

        var report = await context.Reports
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.UserId == diary.UserId
                                   && r.ReportPeriodFrom <= diary.EntryDate
                                   && r.ReportPeriodTo >= diary.EntryDate, ct);

        return new DiaryStateSnapshot
        {
            MeasurementsCount = measurementsCount,
            MetricsCount = metricsCount,
            DiaryEntryUpdatedAt = diary.EntryUpdatedAt,
            ReportUpdatedAt = report?.ReportGeneratedAt,
            ReportSummary = report?.ReportSummary
        };
    }

    private static void PrintSnapshot(string header, DiaryStateSnapshot snapshot)
    {
        Console.WriteLine($"--- {header} ---");
        Console.WriteLine($"Measurements Quantity : {snapshot.MeasurementsCount}");
        Console.WriteLine($"Metrics Quantity      : {snapshot.MetricsCount}");
        Console.WriteLine($"Diary Entry LastUpdate: {snapshot.DiaryEntryUpdatedAt:yyyy-MM-dd HH:mm:ss.fff}");
        Console.WriteLine($"Report Last Update    : {(snapshot.ReportUpdatedAt.HasValue ? snapshot.ReportUpdatedAt.Value.ToString("yyyy-MM-dd HH:mm:ss.fff") : "N/A")}");
        Console.WriteLine($"Report Summary        : {snapshot.ReportSummary ?? "N/A"}");
    }
}

public sealed class DiaryStateSnapshot
{
    public int MeasurementsCount { get; init; }
    public int MetricsCount { get; init; }
    public DateTime DiaryEntryUpdatedAt { get; init; }
    public DateTime? ReportUpdatedAt { get; init; }
    public string? ReportSummary { get; init; }
}
