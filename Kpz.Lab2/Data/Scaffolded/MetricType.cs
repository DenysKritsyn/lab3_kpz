using System;
using System.Collections.Generic;

namespace Kpz.Lab2.Data.Scaffolded;

public partial class MetricType
{
    public int MetricTypeId { get; set; }

    public string MetricName { get; set; } = null!;

    public string MetricUnit { get; set; } = null!;

    public decimal MetricMinValue { get; set; }

    public decimal MetricMaxValue { get; set; }

    public virtual ICollection<Measurement> Measurements { get; set; } = new List<Measurement>();
}
