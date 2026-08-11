using FluentAssertions;
using MediPro.Api.Contracts;
using MediPro.Api.Entities;
using MediPro.Api.Services;
using Xunit;

namespace MediPro.Api.Tests.Services;

public class BonusSchemeResolverTests
{
    [Fact]
    public void FormatLabel_ReturnsBuyPlusBonus()
    {
        BonusSchemeResolver.FormatLabel(10, 1).Should().Be("10+1");
        BonusSchemeResolver.FormatLabel(20, 2).Should().Be("20+2");
    }

    [Fact]
    public void ResolveForProduct_MatchesProductSchemeOnly()
    {
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Manufacturer = "GSK Pakistan" };
        var schemes = new[]
        {
            new BonusScheme { Manufacturer = "GSK Pakistan", BuyQuantity = 5, BonusQuantity = 1, IsActive = true },
            new BonusScheme { ProductId = productId, BuyQuantity = 10, BonusQuantity = 1, IsActive = true },
        };

        var resolved = BonusSchemeResolver.ResolveForProduct(schemes, product);
        resolved!.BuyQuantity.Should().Be(10);
    }

    [Fact]
    public void ResolveForProduct_IgnoresManufacturerWideScheme()
    {
        var product = new Product { Id = Guid.NewGuid(), Manufacturer = "Getz Pharma" };
        var schemes = new[]
        {
            new BonusScheme { Manufacturer = "Getz Pharma", BuyQuantity = 5, BonusQuantity = 1, IsActive = true },
        };

        BonusSchemeResolver.ResolveForProduct(schemes, product).Should().BeNull();
    }

    [Fact]
    public void ApplyToProduct_SetsBonusFieldsOnDto()
    {
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Manufacturer = "Abbott" };
        var dto = new ProductDto { Id = productId, Manufacturer = "Abbott" };
        var schemes = new[]
        {
            new BonusScheme
            {
                Title = "Abbott promo",
                ProductId = productId,
                Manufacturer = "Abbott",
                BuyQuantity = 20,
                BonusQuantity = 2,
                IsActive = true,
            },
        };

        BonusSchemeResolver.ApplyToProduct(dto, product, schemes);

        dto.BonusLabel.Should().Be("20+2");
        dto.BonusTitle.Should().Be("Abbott promo");
        dto.BonusBuyQuantity.Should().Be(20);
        dto.BonusFreeQuantity.Should().Be(2);
    }

    [Fact]
    public void FilterActive_ExcludesSchemesWithoutProductId()
    {
        var schemes = new[]
        {
            new BonusScheme { ProductId = Guid.NewGuid(), IsActive = true },
            new BonusScheme { Manufacturer = "Legacy", IsActive = true },
        };

        BonusSchemeResolver.FilterActive(schemes, DateTime.UtcNow).Should().HaveCount(1);
    }
}
