using MediPro.Api.Contracts;

using MediPro.Api.Entities;



namespace MediPro.Api.Services;



public static class BonusSchemeResolver

{

    public static string FormatLabel(int buyQuantity, int bonusQuantity) =>

        $"{buyQuantity}+{bonusQuantity}";



    public static IReadOnlyList<BonusScheme> FilterActive(

        IEnumerable<BonusScheme> schemes,

        DateTime utcNow)

    {

        return schemes

            .Where(s => s.IsActive)

            .Where(s => s.ProductId is not null)

            .Where(s => s.ValidFromUtc is null || s.ValidFromUtc <= utcNow)

            .Where(s => s.ValidToUtc is null || s.ValidToUtc >= utcNow)

            .OrderBy(s => s.SortOrder)

            .ThenBy(s => s.Title, StringComparer.OrdinalIgnoreCase)

            .ToList();

    }



    public static BonusScheme? ResolveForProduct(IEnumerable<BonusScheme> activeSchemes, Product product) =>

        activeSchemes.FirstOrDefault(s => s.ProductId == product.Id);



    public static void ApplyToProduct(ProductDto dto, Product product, IEnumerable<BonusScheme> activeSchemes)

    {

        var scheme = ResolveForProduct(activeSchemes, product);

        if (scheme is null)

            return;



        dto.BonusLabel = FormatLabel(scheme.BuyQuantity, scheme.BonusQuantity);

        dto.BonusTitle = scheme.Title;

        dto.BonusBuyQuantity = scheme.BuyQuantity;

        dto.BonusFreeQuantity = scheme.BonusQuantity;

        dto.BonusBannerText = scheme.BannerText;

    }



    public static BonusSchemeSummaryDto ToSummary(BonusScheme scheme, Product? product = null) =>

        new()

        {

            Id = scheme.Id,

            Title = scheme.Title,

            Label = FormatLabel(scheme.BuyQuantity, scheme.BonusQuantity),

            BuyQuantity = scheme.BuyQuantity,

            BonusQuantity = scheme.BonusQuantity,

            Manufacturer = product?.Manufacturer ?? scheme.Manufacturer,

            ProductId = scheme.ProductId,

            ProductName = product?.Name,

            BannerText = scheme.BannerText ?? BuildDefaultBanner(scheme, product),

            SortOrder = scheme.SortOrder,

        };



    public static string BuildDefaultBanner(BonusScheme scheme, Product? product = null)

    {

        var label = FormatLabel(scheme.BuyQuantity, scheme.BonusQuantity);

        if (product is not null)

            return $"{product.Name}: order {label} — get {scheme.BonusQuantity} free";

        return $"{scheme.Title}: {label} bonus";

    }

}


