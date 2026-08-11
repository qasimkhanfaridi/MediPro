using MediPro.Api.Configuration;
using Microsoft.Extensions.Options;

namespace MediPro.Api.Tests.Helpers;

public static class TestInventoryOptions
{
    public static IOptions<InventoryOptions> Default { get; } = Options.Create(new InventoryOptions
    {
        HideStoreStockQuantity = true,
        AutoDecrementOnOrder = false,
        EnforceStockQuantityCap = true,
    });
}
