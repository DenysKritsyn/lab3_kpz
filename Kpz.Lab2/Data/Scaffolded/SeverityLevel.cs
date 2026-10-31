using System;
using System.Collections.Generic;

namespace Kpz.Lab2.Data.Scaffolded;

public partial class SeverityLevel
{
    public int SeverityLevelId { get; set; }

    public string SeverityName { get; set; } = null!;

    public byte SeverityRank { get; set; }

    public virtual ICollection<IntervalSymptom> IntervalSymptoms { get; set; } = new List<IntervalSymptom>();
}
