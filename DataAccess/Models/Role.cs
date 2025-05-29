using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public virtual ICollection<Operator> Operators { get; set; } = new List<Operator>();
}
