using MediPro.Api.Configuration;
using MediPro.Api.Contracts;
using MediPro.Api.Entities;
using Microsoft.Extensions.Options;

namespace MediPro.Api.Services;

public class ProductDtoMapper(IOptions<InventoryOptions> inventory)
{
    private readonly InventoryOptions _inv = inventory.Value;

    public ProductDto Map(Product p, bool isAdmin)
    {
        var dto = new ProductDto
        {
            Id = p.Id,
            SkuCode = p.SkuCode,
            Name = p.Name,
            Pack = p.Pack,
            Manufacturer = p.Manufacturer,
            SaltComposition = p.SaltComposition,
            Category = p.Category,
            TradePrice = p.TradePrice,
            Mrp = p.Mrp,
            IsActive = p.IsActive,
            InStock = StockAvailability.IsInStock(p.StockQuantity),
        };

        if (isAdmin || !_inv.HideStoreStockQuantity)
            dto.StockQuantity = p.StockQuantity;

        return dto;
    }
}
