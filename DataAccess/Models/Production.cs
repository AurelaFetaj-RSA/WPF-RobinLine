using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Production
{
    public int ProdId { get; set; }

    public int? ShiftId { get; set; }

    public int? ItemId { get; set; }

    public int? ResultId { get; set; }

    public DateTime? Timestamp { get; set; }

    public int? OperatorId { get; set; }

    public virtual ICollection<Failure> Failures { get; set; } = new List<Failure>();

    public virtual Item? Item { get; set; }

    public virtual Operator? Operator { get; set; }

    public virtual Result? Result { get; set; }

    public virtual Shift? Shift { get; set; }
}
