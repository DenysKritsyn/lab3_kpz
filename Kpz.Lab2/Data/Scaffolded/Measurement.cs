using System;
using System.Collections.Generic;

namespace Kpz.Lab2.Data.Scaffolded;

public partial class Measurement
{
    public int MeasurementId { get; set; }

    public int DiaryEntryId { get; set; }

    public int MetricTypeId { get; set; }

    public decimal MeasurementValue { get; set; }

    public byte? MeasurementHour { get; set; }

    public string? MeasurementNote { get; set; }

    public virtual DiaryEntry DiaryEntry { get; set; } = null!;

    public virtual MetricType MetricType { get; set; } = null!;
}
