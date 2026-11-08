using lab3_kpz.Data;
using lab3_kpz.Entities;
using Microsoft.EntityFrameworkCore;

namespace lab3_kpz.Core.EntityFramework.Repositories;

public sealed class DayIntervalRepository(SymptomDiaryDbContext context)
{
    private readonly SymptomDiaryDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<int> Create(DayInterval interval, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(interval);

        if (interval.IntervalEndTime <= interval.IntervalStartTime)
        {
            throw new ArgumentException("IntervalEndTime must be later than IntervalStartTime.", nameof(interval));
        }

        _context.DayIntervals.Add(interval);
        await _context.SaveChangesAsync(ct);

        return interval.DayIntervalId;
    }

    public async Task<DayInterval?> Read(int dayIntervalId, CancellationToken ct = default)
    {
        return await _context.DayIntervals
            .AsNoTracking()
            .Include(i => i.ConditionState)
            .FirstOrDefaultAsync(i => i.DayIntervalId == dayIntervalId, ct);
    }

    public async Task<bool> Update(
        int dayIntervalId,
        TimeOnly newStartTime,
        TimeOnly newEndTime,
        int newConditionStateId,
        string? newNote,
        CancellationToken ct = default)
    {
        if (newEndTime <= newStartTime)
        {
            throw new ArgumentException("End time must be later than start time.", nameof(newEndTime));
        }

        var interval = await _context.DayIntervals
            .FirstOrDefaultAsync(i => i.DayIntervalId == dayIntervalId, ct);

        if (interval is null)
        {
            return false;
        }

        interval.IntervalStartTime = newStartTime;
        interval.IntervalEndTime = newEndTime;
        interval.ConditionStateId = newConditionStateId;
        interval.IntervalNote = newNote;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> Delete(int dayIntervalId, CancellationToken ct = default)
    {
        var interval = await _context.DayIntervals
            .FirstOrDefaultAsync(i => i.DayIntervalId == dayIntervalId, ct);

        if (interval is null)
        {
            return false;
        }

        // IntervalSymptoms are removed by DeleteBehavior.Cascade
        _context.DayIntervals.Remove(interval);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}

