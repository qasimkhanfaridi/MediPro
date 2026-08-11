namespace MediPro.Api.Services;

public static class StockAvailability
{
    /// <summary>null or &gt; 0 = available; 0 = out of stock.</summary>
    public static bool IsInStock(int? stockQuantity) =>
        stockQuantity is null or > 0;

    public static bool IsOutOfStock(int? stockQuantity) =>
        stockQuantity == 0;
}
