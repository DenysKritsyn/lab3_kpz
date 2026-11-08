using lab2_kpz.Data;
using lab2_kpz.Entities;
using Microsoft.EntityFrameworkCore;

namespace lab2_kpz.Core.EntityFramework.Repositories;

public sealed class DayEventRepository(SymptomDiaryDbContext context)
{
    private readonly SymptomDiaryDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<int> Create(DayEvent dayEvent, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dayEvent);

        if (dayEvent.EventEndTime < dayEvent.EventStartTime)
        {
            throw new ArgumentException("EventEndTime cannot be earlier than EventStartTime.", nameof(dayEvent));
        }

        _context.DayEvents.Add(dayEvent);
        await _context.SaveChangesAsync(ct);

        return dayEvent.DayEventId;
    }

    public async Task<DayEvent?> Read(int dayEventId, CancellationToken ct = default)
    {
        return await _context.DayEvents
            .AsNoTracking()
            .Include(e => e.EventType)
            .FirstOrDefaultAsync(e => e.DayEventId == dayEventId, ct);
    }

    public async Task<bool> Update(
        int dayEventId,
        int newEventTypeId,
        TimeOnly newStartTime,
        TimeOnly newEndTime,
        bool isPreciseTime,
        string? newNote,
        CancellationToken ct = default)
    {
        if (newEndTime < newStartTime)
        {
            throw new ArgumentException("End time cannot be earlier than start time.", nameof(newEndTime));
        }

        var dayEvent = await _context.DayEvents
            .FirstOrDefaultAsync(e => e.DayEventId == dayEventId, ct);

        if (dayEvent is null)
        {
            return false;
        }

        dayEvent.EventTypeId = newEventTypeId;
        dayEvent.EventStartTime = newStartTime;
        dayEvent.EventEndTime = newEndTime;
        dayEvent.IsPreciseTime = isPreciseTime;
        dayEvent.EventNote = newNote;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> Delete(int dayEventId, CancellationToken ct = default)
    {
        var dayEvent = await _context.DayEvents
            .FirstOrDefaultAsync(e => e.DayEventId == dayEventId, ct);

        if (dayEvent is null)
        {
            return false;
        }

        _context.DayEvents.Remove(dayEvent);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}

