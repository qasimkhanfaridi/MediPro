using FluentAssertions;
using MediPro.Api.Services;
using Xunit;

namespace MediPro.Api.Tests.Services;

public class StockAvailabilityTests
{
    [Theory]
    [InlineData(null, true)]
    [InlineData(1, true)]
    [InlineData(50, true)]
    [InlineData(0, false)]
    public void IsInStock_Works(int? qty, bool expected) =>
        StockAvailability.IsInStock(qty).Should().Be(expected);

    [Fact]
    public void IsOutOfStock_OnlyWhenZero() =>
        StockAvailability.IsOutOfStock(0).Should().BeTrue();
}
