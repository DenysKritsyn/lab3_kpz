using System;
using System.Collections.Generic;

namespace lab2_kpz_rev_eng.entities;

public partial class DayInterval
{
    public int DayIntervalId { get; set; }

    public int DiaryEntryId { get; set; }

    public TimeOnly IntervalStartTime { get; set; }

    public TimeOnly IntervalEndTime { get; set; }

    public int ConditionStateId { get; set; }

    public string? IntervalNote { get; set; }

    public virtual ConditionState ConditionState { get; set; } = null!;

    public virtual DiaryEntry DiaryEntry { get; set; } = null!;

    public virtual ICollection<IntervalSymptom> IntervalSymptoms { get; set; } = new List<IntervalSymptom>();
}
