using lab2_kpz.Data;
using lab2_kpz.Entities;
using Microsoft.EntityFrameworkCore;

namespace lab2_kpz.Core.DemonstrativeScenarios;

public sealed class ConcurrencyScenario
{
    public async Task RunDatabaseWinsScenarioAsync(int diaryEntryId = 1, CancellationToken ct = default)
    {
        Console.WriteLine("--------------------------------------------------");
        Console.WriteLine("  SCENARIO: OPTIMISTIC CONCURRENCY (DATABASE WINS)");
        Console.WriteLine("--------------------------------------------------");

        await using var context1 = new SymptomDiaryDbContext();
        await using var context2 = new SymptomDiaryDbContext();

        // Both independent DbContext instances load the same DiaryEntry concurrently
        var entryUser1 = await context1.DiaryEntries.FirstOrDefaultAsync(d => d.DiaryEntryId == diaryEntryId, ct);
        var entryUser2 = await context2.DiaryEntries.FirstOrDefaultAsync(d => d.DiaryEntryId == diaryEntryId, ct);

        Console.WriteLine("--- INITIAL STATE ---");
        Console.WriteLine($"DiaryEntryId      : {entryUser1.DiaryEntryId}");
        Console.WriteLine($"Initial Note      : {entryUser1.EntryNote}");
        Console.WriteLine($"Initial UpdatedAt : {entryUser1.EntryUpdatedAt:yyyy-MM-dd HH:mm:ss.fff}");
        Console.WriteLine($"Initial Version   : 0x{Convert.ToHexString(entryUser1.EntryRowVersion)}");

        // First context modifies and saves the entity first
        entryUser1.EntryNote = "Patient reported mild headache relief.";
        entryUser1.EntryUpdatedAt = DateTime.UtcNow;
        await context1.SaveChangesAsync(ct);

        Console.WriteLine("\n[CONTEXT 1 SAVED SUCCESSFULLY]");
        Console.WriteLine($"Client1 Note       : {entryUser1.EntryNote}");
        Console.WriteLine($"Client1 UpdatedAt  : {entryUser1.EntryUpdatedAt:yyyy-MM-dd HH:mm:ss.fff}");
        Console.WriteLine($"Client1 Version    : 0x{Convert.ToHexString(entryUser1.EntryRowVersion)}");

        // Second context tries to save its own changes with stale EntryRowVersion
        entryUser2.EntryNote = "Conflicting note from second session.";
        entryUser2.EntryUpdatedAt = DateTime.UtcNow;

        try
        {
            Console.WriteLine("\n[CONTEXT 2 SAVING FAILED]");
            Console.WriteLine($"Client2 Note       : {entryUser2.EntryNote}");
            Console.WriteLine($"Client2 UpdatedAt  : {entryUser2.EntryUpdatedAt:yyyy-MM-dd HH:mm:ss.fff}");
            Console.WriteLine($"Client2 Version    : 0x{Convert.ToHexString(entryUser2.EntryRowVersion)}");
            await context2.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Console.WriteLine("\n[CONCURRENCY CONFLICT DETECTED] Caught DbUpdateConcurrencyException");

            var conflictingEntry = ex.Entries.Single();
            var databaseValues = await conflictingEntry.GetDatabaseValuesAsync(ct);

            if (databaseValues is null)
            {
                Console.WriteLine("Resolution: The record was deleted by another user.");
                return;
            }

            // Resolution strategy: Database Wins - reload entity state from the database
            await conflictingEntry.ReloadAsync(ct);

            await using var context3 = new SymptomDiaryDbContext();
            var entryUser3 = await context3.DiaryEntries
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DiaryEntryId == diaryEntryId, ct);

            Console.WriteLine("\n[CONTEXT 3 ACTUAL DB STATE]");
            Console.WriteLine($"Result Note       : {entryUser3?.EntryNote}");
            Console.WriteLine($"Result UpdatedAt  : {entryUser3?.EntryUpdatedAt:yyyy-MM-dd HH:mm:ss.fff}");
            Console.WriteLine($"Result Version    : 0x{(entryUser3 is not null ? Convert.ToHexString(entryUser3.EntryRowVersion) : "N/A")}\n");

            Console.WriteLine("--- CONFLICT RESULT ---");
            Console.WriteLine($"Client 1 note stays      ");
            Console.WriteLine($"Client 2 note is rejected");
        }
    }
}

