using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Shift
{
    public int ShiftId { get; set; }

    public string? Description { get; set; }

    public TimeSpan? StartTime { get; set; }

    public TimeSpan? EndTime { get; set; }

    public int? TargetProd { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Production> Productions { get; set; } = new List<Production>();
}
