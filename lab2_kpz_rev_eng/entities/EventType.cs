using System;
using System.Collections.Generic;

namespace lab2_kpz_rev_eng.entities;

public partial class EventType
{
    public int EventTypeId { get; set; }

    public string EventTypeName { get; set; } = null!;

    public virtual ICollection<DayEvent> DayEvents { get; set; } = new List<DayEvent>();
}
