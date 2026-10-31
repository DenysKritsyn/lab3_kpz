using System;
using System.Collections.Generic;

namespace Kpz.Lab2.Data.Scaffolded;

public partial class DayInterval
{
    public int DayIntervalId { get; set; }

    public int DiaryEntryId { get; set; }

    public byte IntervalStartHour { get; set; }

    public byte IntervalEndHour { get; set; }

    public int WellbeingLevelId { get; set; }

    public string? IntervalNote { get; set; }

    public virtual DiaryEntry DiaryEntry { get; set; } = null!;

    public virtual ICollection<IntervalSymptom> IntervalSymptoms { get; set; } = new List<IntervalSymptom>();

    public virtual WellbeingLevel WellbeingLevel { get; set; } = null!;
}
