using System;
using System.Collections.Generic;

namespace lab2_kpz_rev_eng.entities;

public partial class IntervalSymptom
{
    public int DayIntervalId { get; set; }

    public int SymptomId { get; set; }

    public int SeverityLevelId { get; set; }

    public string? SymptomNote { get; set; }

    public virtual DayInterval DayInterval { get; set; } = null!;

    public virtual SeverityLevel SeverityLevel { get; set; } = null!;

    public virtual Symptom Symptom { get; set; } = null!;
}
