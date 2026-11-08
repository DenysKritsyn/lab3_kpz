using System;
using System.Collections.Generic;

namespace lab2_kpz_rev_eng.entities;

public partial class Report
{
    public int ReportId { get; set; }

    public int UserId { get; set; }

    public DateOnly ReportPeriodFrom { get; set; }

    public DateOnly ReportPeriodTo { get; set; }

    public DateTime ReportGeneratedAt { get; set; }

    public string ReportSummary { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
