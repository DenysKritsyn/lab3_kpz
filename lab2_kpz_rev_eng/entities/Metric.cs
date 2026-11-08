using System;
using System.Collections.Generic;

namespace lab2_kpz_rev_eng.entities;

public partial class Metric
{
    public int MetricId { get; set; }

    public int MeasurementId { get; set; }

    public int MetricUnitId { get; set; }

    public string MetricName { get; set; } = null!;

    public decimal MetricValue { get; set; }

    public virtual Measurement Measurement { get; set; } = null!;

    public virtual MetricUnit MetricUnit { get; set; } = null!;
}
