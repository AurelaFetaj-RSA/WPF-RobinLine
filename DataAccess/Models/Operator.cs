using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Operator
{
    public int OperatorId { get; set; }

    public string? Name { get; set; }

    public string? Role { get; set; }

    public DateTime? HiredDate { get; set; }

    public bool? IsActive { get; set; }

    public int? RoleId { get; set; }

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();

    public virtual ICollection<Production> Productions { get; set; } = new List<Production>();

    public virtual Role? RoleNavigation { get; set; }
}
