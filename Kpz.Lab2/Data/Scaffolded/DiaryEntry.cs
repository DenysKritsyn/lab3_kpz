using System;
using System.Collections.Generic;

namespace Kpz.Lab2.Data.Scaffolded;

public partial class DiaryEntry
{
    public int DiaryEntryId { get; set; }

    public int UserId { get; set; }

    public DateOnly EntryDate { get; set; }

    public string? EntryNote { get; set; }

    public DateTime EntryCreatedAt { get; set; }

    public DateTime EntryUpdatedAt { get; set; }

    public byte[] EntryRowVersion { get; set; } = null!;

    public virtual ICollection<DayEvent> DayEvents { get; set; } = new List<DayEvent>();

    public virtual ICollection<DayInterval> DayIntervals { get; set; } = new List<DayInterval>();

    public virtual ICollection<Measurement> Measurements { get; set; } = new List<Measurement>();

    public virtual User User { get; set; } = null!;
}
