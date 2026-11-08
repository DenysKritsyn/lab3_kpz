using System;
using System.Collections.Generic;

namespace lab2_kpz_rev_eng.entities;

public partial class MetricUnit
{
    public int MetricUnitId { get; set; }

    public string UnitName { get; set; } = null!;

    public decimal UnitMinValue { get; set; }

    public decimal UnitMaxValue { get; set; }

    public virtual ICollection<Metric> Metrics { get; set; } = new List<Metric>();
}
