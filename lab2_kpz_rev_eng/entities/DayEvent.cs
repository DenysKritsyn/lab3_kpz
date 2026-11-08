using System;
using System.Collections.Generic;

namespace lab2_kpz_rev_eng.entities;

public partial class DayEvent
{
    public int DayEventId { get; set; }

    public int DiaryEntryId { get; set; }

    public int EventTypeId { get; set; }

    public TimeOnly EventStartTime { get; set; }

    public TimeOnly EventEndTime { get; set; }

    public bool IsPreciseTime { get; set; }

    public string? EventNote { get; set; }

    public virtual DiaryEntry DiaryEntry { get; set; } = null!;

    public virtual EventType EventType { get; set; } = null!;
}
