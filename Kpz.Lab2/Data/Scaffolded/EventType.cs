using System;
using System.Collections.Generic;

namespace Kpz.Lab2.Data.Scaffolded;

public partial class EventType
{
    public int EventTypeId { get; set; }

    public string EventName { get; set; } = null!;

    public virtual ICollection<DayEvent> DayEvents { get; set; } = new List<DayEvent>();
}
