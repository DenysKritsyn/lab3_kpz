using lab2_kpz.Data;
using lab2_kpz.Entities;
using Microsoft.EntityFrameworkCore;

namespace lab2_kpz.Core.EntityFramework;

public sealed class CreateReport(SymptomDiaryDbContext context)
{
    // ConditionState scale from seed data: 1 = Critical, 2 = Bad, 3 = Fair, 4 = Good, 5 = Excellent
    private const double PoorConditionThreshold = 2.5;
    private const double StableConditionThreshold = 3.5;

    private readonly SymptomDiaryDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<Report> ExecuteAsync(int userId, DateOnly periodFrom, DateOnly periodTo, CancellationToken ct = default)
    {
        if (periodFrom > periodTo)
        {
            throw new ArgumentException("periodFrom cannot be later than periodTo.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        try
        {
            // Load User with already existing Reports
            var user = await _context.Users
                .Include(u => u.Reports)
                .FirstOrDefaultAsync(u => u.UserId == userId, ct)
                ?? throw new InvalidOperationException($"User with ID {userId} was not found.");

            // Report for exactly the same period must not be duplicated
            if (user.Reports.Any(r => r.ReportPeriodFrom == periodFrom && r.ReportPeriodTo == periodTo))
            {
                throw new InvalidOperationException(
                    $"Report for user {userId} for period {periodFrom} - {periodTo} already exists.");
            }

            // Load the entity graph: DiaryEntries -> DayIntervals -> IntervalSymptoms -> Symptom
            var entries = await _context.DiaryEntries
                .Include(d => d.DayIntervals)
                    .ThenInclude(i => i.IntervalSymptoms)
                        .ThenInclude(s => s.Symptom)
                .Where(d => d.UserId == userId
                         && d.EntryDate >= periodFrom
                         && d.EntryDate <= periodTo)
                .AsSplitQuery()
                .ToListAsync(ct);

            var intervals = entries.SelectMany(d => d.DayIntervals).ToList();

            if (intervals.Count == 0)
            {
                throw new InvalidOperationException(
                    $"No diary intervals found for user {userId} in period {periodFrom} - {periodTo}. Report cannot be created.");
            }

            // Analyse the graph in memory
            double averageCondition = intervals.Average(i => i.ConditionStateId);

            var mostFrequentSymptom = intervals
                .SelectMany(i => i.IntervalSymptoms)
                .GroupBy(s => s.Symptom.SymptomName)
                .Select(g => new { SymptomName = g.Key, Occurrences = g.Count() })
                .OrderByDescending(x => x.Occurrences)
                .ThenBy(x => x.SymptomName)
                .FirstOrDefault();

            string summary = BuildSummary(
                averageCondition,
                mostFrequentSymptom?.SymptomName,
                mostFrequentSymptom?.Occurrences ?? 0,
                entries.Count,
                intervals.Count);

            var now = DateTime.UtcNow;

            // Add new Report to the graph via navigation collection of the User
            var report = new Report
            {
                ReportPeriodFrom = periodFrom,
                ReportPeriodTo = periodTo,
                ReportGeneratedAt = now,
                ReportSummary = summary
            };
            user.Reports.Add(report);

            // Mark analysed diary entries in the graph as touched by report generation
            foreach (var entry in entries)
            {
                entry.EntryUpdatedAt = now;
            }

            // Persist the whole graph and commit atomically
            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return report;
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            _context.ChangeTracker.Clear();
            throw;
        }
    }

    private static string BuildSummary(
        double averageCondition,
        string? mostFrequentSymptom,
        int symptomOccurrences,
        int entriesCount,
        int intervalsCount)
    {
        string symptomPart = mostFrequentSymptom is null
            ? "No symptoms were recorded during the period."
            : $"Most frequent symptom: '{mostFrequentSymptom}' ({symptomOccurrences} occurrences).";

        string statsPart = $"Analysed {entriesCount} diary entries and {intervalsCount} intervals. " +
                           $"Average condition score: {averageCondition:F2} / 5.";

        string verdict = averageCondition switch
        {
            < PoorConditionThreshold =>
                "POOR CONDITION: The patient's wellbeing was predominantly bad or critical. " +
                "An urgent consultation with the attending physician is recommended.",
            < StableConditionThreshold =>
                "STABLE CONDITION: The patient's wellbeing was fair with periodic deterioration. " +
                "Continue current treatment and keep monitoring the symptoms.",
            _ =>
                "GOOD CONDITION: The patient's wellbeing was predominantly good or excellent. " +
                "No additional medical intervention is required."
        };

        return $"{verdict} {symptomPart} {statsPart}";
    }
}

