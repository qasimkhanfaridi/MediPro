namespace MediPro.Api.Entities;

/// <summary>Buy-X-get-Y-free style offer for a single SKU.</summary>
public class BonusScheme
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public string Title { get; set; } = "";
    /// <summary>Denormalized from product for admin display; not used for matching.</summary>
    public string? Manufacturer { get; set; }
    /// <summary>Required — bonus applies only to this product.</summary>
    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }

    public int BuyQuantity { get; set; }
    public int BonusQuantity { get; set; }
    public string? BannerText { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTime? ValidFromUtc { get; set; }
    public DateTime? ValidToUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
