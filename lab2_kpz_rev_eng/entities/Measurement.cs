using System;
using System.Collections.Generic;

namespace lab2_kpz_rev_eng.entities;

public partial class Measurement
{
    public int MeasurementId { get; set; }

    public int DiaryEntryId { get; set; }

    public TimeOnly MeasuringTime { get; set; }

    public virtual DiaryEntry DiaryEntry { get; set; } = null!;

    public virtual ICollection<Metric> Metrics { get; set; } = new List<Metric>();
}
