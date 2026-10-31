using System;
using System.Collections.Generic;

namespace Kpz.Lab2.Data.Scaffolded;

public partial class DayEvent
{
    public int DayEventId { get; set; }

    public int DiaryEntryId { get; set; }

    public int EventTypeId { get; set; }

    public byte? EventHour { get; set; }

    public string? EventNote { get; set; }

    public virtual DiaryEntry DiaryEntry { get; set; } = null!;

    public virtual EventType EventType { get; set; } = null!;
}
