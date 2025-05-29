using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Target
{
    public int TargetId { get; set; }

    public int? ItemId { get; set; }

    public int? HourlyTarget { get; set; }

    public int? ShiftTarget { get; set; }

    public string? EfficencyGoal { get; set; }

    public virtual Item? Item { get; set; }
}
