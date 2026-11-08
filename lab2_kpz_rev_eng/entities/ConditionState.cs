using System;
using System.Collections.Generic;

namespace lab2_kpz_rev_eng.entities;

public partial class ConditionState
{
    public int ConditionStateId { get; set; }

    public string ConditionStateName { get; set; } = null!;

    public virtual ICollection<DayInterval> DayIntervals { get; set; } = new List<DayInterval>();
}
