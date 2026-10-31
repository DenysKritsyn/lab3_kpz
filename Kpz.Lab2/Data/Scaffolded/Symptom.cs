using System;
using System.Collections.Generic;

namespace Kpz.Lab2.Data.Scaffolded;

public partial class Symptom
{
    public int SymptomId { get; set; }

    public string SymptomName { get; set; } = null!;

    public string? SymptomCategory { get; set; }

    public virtual ICollection<IntervalSymptom> IntervalSymptoms { get; set; } = new List<IntervalSymptom>();
}
