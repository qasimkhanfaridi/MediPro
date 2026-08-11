using System.ComponentModel.DataAnnotations;

namespace MediPro.Api.Contracts;

public sealed class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, MinLength(8)]
    public string Password { get; set; } = "";
}

public sealed class RegisterStoreRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, MinLength(8)]
    public string Password { get; set; } = "";

    [Required, MaxLength(256)]
    public string BusinessName { get; set; } = "";

    [Required, MaxLength(512)]
    public string AddressLine { get; set; } = "";

    [Required, MaxLength(128)]
    public string City { get; set; } = "";

    [MaxLength(128)]
    public string? Area { get; set; }

    [Required, MaxLength(32)]
    public string Mobile { get; set; } = "";

    [Required, MaxLength(256)]
    public string ContactName { get; set; } = "";

    [Required, MaxLength(128)]
    public string LicenseNumber { get; set; } = "";
}

public sealed class AuthResponse
{
    public string AccessToken { get; set; } = "";
    public DateTime ExpiresAtUtc { get; set; }
    public string Role { get; set; } = "";
    public string? StoreApprovalStatus { get; set; }
}

public sealed class StoreSummaryDto
{
    public Guid Id { get; set; }
    public string BusinessName { get; set; } = "";
    public string City { get; set; } = "";
    public string Area { get; set; } = "";
    public string Mobile { get; set; } = "";
    public string? LicenseNumber { get; set; }
    public string ApprovalStatus { get; set; } = "";
    public string? ApprovalNotes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class SetStoreApprovalRequest
{
    [Required]
    public string Status { get; set; } = ""; // Pending, Approved, Rejected, Suspended

    [MaxLength(512)]
    public string? Notes { get; set; }
}

public sealed class ProductDto
{
    public Guid Id { get; set; }
    public string SkuCode { get; set; } = "";
    public string Name { get; set; } = "";
    public string Pack { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    public string SaltComposition { get; set; } = "";
    public string? Category { get; set; }
    public decimal TradePrice { get; set; }
    public decimal? Mrp { get; set; }
    public bool IsActive { get; set; }
    public int? StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    /// <summary>True when product can be ordered (null or positive stock; 0 = out).</summary>
    public bool InStock { get; set; } = true;
    /// <summary>Display label e.g. 10+1 when a bonus scheme applies.</summary>
    public string? BonusLabel { get; set; }
    public string? BonusTitle { get; set; }
    public int? BonusBuyQuantity { get; set; }
    public int? BonusFreeQuantity { get; set; }
    public string? BonusBannerText { get; set; }
}

public sealed class PagedResult<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
}

public sealed class CatalogFilterOptionsDto
{
    public IReadOnlyList<string> Manufacturers { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> Categories { get; set; } = Array.Empty<string>();
}

public sealed class LowStockItemDto
{
    public string SkuCode { get; set; } = "";
    public string Name { get; set; } = "";
    public int StockQuantity { get; set; }
}

public sealed class LowStockListDto
{
    public int Threshold { get; set; }
    public int TotalMatching { get; set; }
    public IReadOnlyList<LowStockItemDto> Items { get; set; } = Array.Empty<LowStockItemDto>();
}

public sealed class CreateProductRequest
{
    [Required, MaxLength(64)]
    public string SkuCode { get; set; } = "";

    [Required, MaxLength(512)]
    public string Name { get; set; } = "";

    [Required, MaxLength(64)]
    public string Pack { get; set; } = "";

    [Required, MaxLength(256)]
    public string Manufacturer { get; set; } = "";

    [Required, MaxLength(512)]
    public string SaltComposition { get; set; } = "";

    [MaxLength(128)]
    public string? Category { get; set; }

    [Range(0, 999999999)]
    public decimal TradePrice { get; set; }

    [Range(0, 999999999)]
    public decimal? Mrp { get; set; }

    public int? StockQuantity { get; set; }

    [MaxLength(1024)]
    public string? ImageUrl { get; set; }
}

public sealed class AdjustStockRequest
{
    [Required, MaxLength(64)]
    public string SkuCode { get; set; } = "";

    /// <summary>Positive to receive stock, negative for corrections (e.g. damage).</summary>
    [Range(-1_000_000, 1_000_000)]
    public int Delta { get; set; }
}

public sealed class SetStockStatusRequest
{
    [Required, MaxLength(64)]
    public string SkuCode { get; set; } = "";

    public bool InStock { get; set; }
}

public sealed class UpdateProductRequest
{
    [MaxLength(512)]
    public string? Name { get; set; }

    [MaxLength(64)]
    public string? Pack { get; set; }

    [MaxLength(256)]
    public string? Manufacturer { get; set; }

    [MaxLength(512)]
    public string? SaltComposition { get; set; }

    [MaxLength(128)]
    public string? Category { get; set; }

    [Range(0, 999999999)]
    public decimal? TradePrice { get; set; }

    [Range(0, 999999999)]
    public decimal? Mrp { get; set; }

    public bool? IsActive { get; set; }

    public int? StockQuantity { get; set; }

    [MaxLength(1024)]
    public string? ImageUrl { get; set; }
}

public sealed class AddCartItemRequest
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(1, 99999)]
    public int Quantity { get; set; } = 1;
}

public sealed class SetCartItemQuantityRequest
{
    [Range(0, 99999)]
    public int Quantity { get; set; }
}

public sealed class CartLineDto
{
    public Guid LineId { get; set; }
    public Guid ProductId { get; set; }
    public string SkuCode { get; set; } = "";
    public string Name { get; set; } = "";
    public string Pack { get; set; } = "";
    public decimal TradePrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}

public sealed class CartDto
{
    public Guid CartId { get; set; }
    public IReadOnlyList<CartLineDto> Lines { get; set; } = Array.Empty<CartLineDto>();
    public decimal Subtotal { get; set; }
}

public sealed class SubmitOrderRequest
{
    [MaxLength(1024)]
    public string? Notes { get; set; }
}

public sealed class OrderLineDto
{
    public Guid ProductId { get; set; }
    public string ProductNameSnapshot { get; set; } = "";
    public string PackSnapshot { get; set; } = "";
    public decimal UnitPriceSnapshot { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}

public sealed class OrderSummaryDto
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = "";
    public string StoreCity { get; set; } = "";
    public string StoreArea { get; set; } = "";
    public string StoreMobile { get; set; } = "";
    public string Status { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "";
    public DateTime SubmittedAtUtc { get; set; }
}

public sealed class OrderDetailDto
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = "";
    public string Status { get; set; } = "";
    public string? StatusNotes { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "";
    public DateTime SubmittedAtUtc { get; set; }
    public string? Notes { get; set; }
    public IReadOnlyList<OrderLineDto> Lines { get; set; } = Array.Empty<OrderLineDto>();
}

public sealed class UpdateOrderStatusRequest
{
    [Required]
    public string Status { get; set; } = "";

    [MaxLength(512)]
    public string? StatusNotes { get; set; }
}

public sealed class OrderLocationOptionsDto
{
    public IReadOnlyList<string> Cities { get; set; } = Array.Empty<string>();
    public IReadOnlyDictionary<string, IReadOnlyList<string>> AreasByCity { get; set; }
        = new Dictionary<string, IReadOnlyList<string>>();
}

public sealed class BonusSchemeDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string Label { get; set; } = "";
    public string? Manufacturer { get; set; }
    public Guid? ProductId { get; set; }
    public string? ProductName { get; set; }
    public int BuyQuantity { get; set; }
    public int BonusQuantity { get; set; }
    public string? BannerText { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime? ValidFromUtc { get; set; }
    public DateTime? ValidToUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class BonusSchemeSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string Label { get; set; } = "";
    public int BuyQuantity { get; set; }
    public int BonusQuantity { get; set; }
    public string? Manufacturer { get; set; }
    public Guid? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string BannerText { get; set; } = "";
    public int SortOrder { get; set; }
}

public class CreateBonusSchemeRequest
{
    [Required, MaxLength(256)]
    public string Title { get; set; } = "";

    [MaxLength(256)]
    public string? Manufacturer { get; set; }

    public Guid? ProductId { get; set; }

    [Range(1, 9999)]
    public int BuyQuantity { get; set; }

    [Range(1, 9999)]
    public int BonusQuantity { get; set; }

    [MaxLength(512)]
    public string? BannerText { get; set; }

    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTime? ValidFromUtc { get; set; }
    public DateTime? ValidToUtc { get; set; }
}

public sealed class UpdateBonusSchemeRequest : CreateBonusSchemeRequest;

public sealed class ImportRowErrorDto
{
    public int RowNumber { get; set; }
    public string Message { get; set; } = "";
}

public sealed class ImportProductsResultDto
{
    public int TotalRowsAttempted { get; set; }
    public int InsertedCount { get; set; }
    public int SkippedOrFailedCount { get; set; }
    public List<ImportRowErrorDto> Errors { get; set; } = new();
}

public sealed class AdminNotificationDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = "";
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public Guid? RelatedOrderId { get; set; }
    public Guid? RelatedStoreId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public bool IsRead { get; set; }
}
