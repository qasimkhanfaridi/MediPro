namespace MediPro.Api.Entities;

public class Product
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public string SkuCode { get; set; } = "";
    public string Name { get; set; } = "";
    public string Pack { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    public string SaltComposition { get; set; } = "";
    public string? Category { get; set; }

    public decimal TradePrice { get; set; }
    public decimal? Mrp { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Optional quantity-on-hand for MVP stock signals.</summary>
    public int? StockQuantity { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
    public ICollection<CartLine> CartLines { get; set; } = new List<CartLine>();
}
