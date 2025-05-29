namespace DataAccess.Models;

public partial class Item
{
    public int ItemId { get; set; }

    public string? ModelName { get; set; }

    public int? PartType { get; set; }

    public int? Type { get; set; }

    public DateTime? Timestamp { get; set; }

    public int? OperatorId { get; set; }

    public int? SizeId { get; set; }

    public virtual Operator? Operator { get; set; }

    public virtual Size? Size { get; set; }

    public virtual ICollection<Production> Productions { get; set; } = new List<Production>();

    public virtual ICollection<Target> Targets { get; set; } = new List<Target>();
}
