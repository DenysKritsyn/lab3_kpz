using System;
using System.Collections.Generic;

namespace lab2_kpz_rev_eng.entities;

public partial class SeverityLevel
{
    public int SeverityLevelId { get; set; }

    public string SeverityName { get; set; } = null!;

    public virtual ICollection<IntervalSymptom> IntervalSymptoms { get; set; } = new List<IntervalSymptom>();
}
