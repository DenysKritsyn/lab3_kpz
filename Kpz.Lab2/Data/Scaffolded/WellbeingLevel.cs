using System;
using System.Collections.Generic;

namespace Kpz.Lab2.Data.Scaffolded;

public partial class WellbeingLevel
{
    public int WellbeingLevelId { get; set; }

    public string WellbeingName { get; set; } = null!;

    public byte WellbeingRank { get; set; }

    public virtual ICollection<DayInterval> DayIntervals { get; set; } = new List<DayInterval>();
}
