using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Failure
{
    public int FailureId { get; set; }

    public int? ProductionId { get; set; }

    public int? ResultId { get; set; }

    public string? Description { get; set; }

    public string? Severity { get; set; }

    public DateTime? Timestamp { get; set; }

    public virtual Production? Production { get; set; }

    public virtual Result? Result { get; set; }
}
