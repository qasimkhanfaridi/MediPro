namespace MediPro.Api.Configuration;

public sealed class InventoryOptions
{
    public const string SectionName = "Inventory";

    /// <summary>When true, store catalogue shows In stock / Out of stock only (no numeric qty).</summary>
    public bool HideStoreStockQuantity { get; set; } = true;

    /// <summary>When false, order submit does not reduce StockQuantity (recommended for external billing).</summary>
    public bool AutoDecrementOnOrder { get; set; }

    /// <summary>When false with simplified view, cart does not cap quantity by on-hand stock.</summary>
    public bool EnforceStockQuantityCap { get; set; } = true;
}
