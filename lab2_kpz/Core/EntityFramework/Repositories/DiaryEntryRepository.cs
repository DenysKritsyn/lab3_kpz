using lab2_kpz.Data;
using lab2_kpz.Entities;
using Microsoft.EntityFrameworkCore;

namespace lab2_kpz.Core.EntityFramework.Repositories;

public sealed class DiaryEntryRepository(SymptomDiaryDbContext context)
{
    private readonly SymptomDiaryDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<int> Create(DiaryEntry entry, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var now = DateTime.UtcNow;
        entry.EntryCreatedAt = now;
        entry.EntryUpdatedAt = now;

        _context.DiaryEntries.Add(entry);
        await _context.SaveChangesAsync(ct);

        return entry.DiaryEntryId;
    }

    public async Task<DiaryEntry?> Read(int diaryEntryId, CancellationToken ct = default)
    {
        return await _context.DiaryEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DiaryEntryId == diaryEntryId, ct);
    }

    public async Task<bool> Update(int diaryEntryId, string? newNote, CancellationToken ct = default)
    {
        var entry = await _context.DiaryEntries
            .FirstOrDefaultAsync(d => d.DiaryEntryId == diaryEntryId, ct);

        if (entry is null)
        {
            return false;
        }

        entry.EntryNote = newNote;
        entry.EntryUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> Delete(int diaryEntryId, CancellationToken ct = default)
    {
        var entry = await _context.DiaryEntries
            .FirstOrDefaultAsync(d => d.DiaryEntryId == diaryEntryId, ct);

        if (entry is null)
        {
            return false;
        }

        // DayIntervals, DayEvents and Measurements are removed by DeleteBehavior.Cascade
        _context.DiaryEntries.Remove(entry);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}

