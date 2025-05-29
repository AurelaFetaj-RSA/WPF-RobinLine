using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Result
{
    public int ResultId { get; set; }

    public int? CycleTime { get; set; }

    public bool? IsDefective { get; set; }

    public virtual ICollection<Failure> Failures { get; set; } = new List<Failure>();

    public virtual ICollection<Production> Productions { get; set; } = new List<Production>();
}
