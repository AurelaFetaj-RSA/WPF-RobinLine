using DataAccess.Models;

namespace DataAccess.Models;
public partial class Size
{
    public int SizeId { get; set; }

    public string? SizeValue { get; set; }

    public string? SizeType { get; set; }

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
