using System;
using System.Collections.Generic;

namespace lab2_kpz_rev_eng.entities;

public partial class Symptom
{
    public int SymptomId { get; set; }

    public string SymptomName { get; set; } = null!;

    public virtual ICollection<IntervalSymptom> IntervalSymptoms { get; set; } = new List<IntervalSymptom>();
}
