using FluentAssertions;
using MediPro.Api.Contracts;
using MediPro.Api.Controllers;
using MediPro.Api.Services;
using MediPro.Api.Tests.Helpers;
using MediPro.Api.Tests.Helpers;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace MediPro.Api.Tests.Controllers;

public class OrdersControllerListTests
{
    private static OrdersController CreateController(MediPro.Api.Data.MediProDbContext db) =>
        new(db, Mock.Of<IAdminAlertService>(), TestInventoryOptions.Default);

    private static CartController CreateCartController(MediPro.Api.Data.MediProDbContext db) =>
        new(db, TestInventoryOptions.Default);

    private static Task<ActionResult<PagedResult<OrderSummaryDto>>> ListAsync(
        OrdersController controller,
        string? city = null,
        string? area = null) =>
        controller.List(
            storeId: null,
            status: null,
            city: city,
            area: area,
            search: null,
            from: null,
            to: null,
            page: 1,
            pageSize: 50,
            ct: CancellationToken.None);

    [Fact]
    public async Task List_AsAdmin_FiltersByCity_ReturnsMatchingOrdersOnly()
    {
        await using var db = OrderTestFixtures.CreateInMemoryContext();
        var seed = await OrderTestFixtures.SeedOrdersAcrossCitiesAsync(db);
        var controller = CreateController(db);
        OrderTestFixtures.AttachUser(controller, OrderTestFixtures.CreateAdminPrincipal(seed.AdminUserId, seed.TenantId));

        var result = await ListAsync(controller, city: "Rawalpindi");

        var page = result.Result.Should().BeOfType<OkObjectResult>().Subject.Value
            .Should().BeOfType<PagedResult<OrderSummaryDto>>().Subject;
        page.Items.Should().HaveCount(2);
        page.Items.Should().OnlyContain(o => o.StoreCity == "Rawalpindi");
    }

    [Fact]
    public async Task List_AsAdmin_FiltersByCityAndArea_ReturnsSingleMatchingOrder()
    {
        await using var db = OrderTestFixtures.CreateInMemoryContext();
        var seed = await OrderTestFixtures.SeedOrdersAcrossCitiesAsync(db);
        var controller = CreateController(db);
        OrderTestFixtures.AttachUser(controller, OrderTestFixtures.CreateAdminPrincipal(seed.AdminUserId, seed.TenantId));

        var result = await ListAsync(controller, city: "Rawalpindi", area: "Moti Mehel");

        var page = result.Result.Should().BeOfType<OkObjectResult>().Subject.Value
            .Should().BeOfType<PagedResult<OrderSummaryDto>>().Subject;
        page.Items.Should().ContainSingle();
        page.Items[0].Id.Should().Be(seed.OrderMotiMehelId);
        page.Items[0].StoreCity.Should().Be("Rawalpindi");
        page.Items[0].StoreArea.Should().Be("Moti Mehel");
    }

    [Fact]
    public async Task List_AsAdmin_CityFilter_UsesExactMatch_NotPartial()
    {
        await using var db = OrderTestFixtures.CreateInMemoryContext();
        var seed = await OrderTestFixtures.SeedOrdersAcrossCitiesAsync(db);
        var controller = CreateController(db);
        OrderTestFixtures.AttachUser(controller, OrderTestFixtures.CreateAdminPrincipal(seed.AdminUserId, seed.TenantId));

        var result = await ListAsync(controller, city: "Rawal");

        var page = result.Result.Should().BeOfType<OkObjectResult>().Subject.Value
            .Should().BeOfType<PagedResult<OrderSummaryDto>>().Subject;
        page.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task List_AsAdmin_FiltersByAreaOnly_ReturnsMatchingOrders()
    {
        await using var db = OrderTestFixtures.CreateInMemoryContext();
        var seed = await OrderTestFixtures.SeedOrdersAcrossCitiesAsync(db);
        var controller = CreateController(db);
        OrderTestFixtures.AttachUser(controller, OrderTestFixtures.CreateAdminPrincipal(seed.AdminUserId, seed.TenantId));

        var result = await ListAsync(controller, area: "Blue Area");

        var page = result.Result.Should().BeOfType<OkObjectResult>().Subject.Value
            .Should().BeOfType<PagedResult<OrderSummaryDto>>().Subject;
        page.Items.Should().ContainSingle();
        page.Items[0].Id.Should().Be(seed.OrderBlueAreaId);
        page.Items[0].StoreArea.Should().Be("Blue Area");
    }

    [Fact]
    public async Task List_AsAdmin_IncludesStoreAreaInResponse()
    {
        await using var db = OrderTestFixtures.CreateInMemoryContext();
        var seed = await OrderTestFixtures.SeedOrdersAcrossCitiesAsync(db);
        var controller = CreateController(db);
        OrderTestFixtures.AttachUser(controller, OrderTestFixtures.CreateAdminPrincipal(seed.AdminUserId, seed.TenantId));

        var result = await ListAsync(controller);

        var page = result.Result.Should().BeOfType<OkObjectResult>().Subject.Value
            .Should().BeOfType<PagedResult<OrderSummaryDto>>().Subject;
        page.Items.Should().HaveCount(3);
        page.Items.Should().Contain(o => o.StoreArea == "Moti Mehel");
        page.Items.Should().Contain(o => o.StoreArea == "Raja Bazaar");
        page.Items.Should().Contain(o => o.StoreArea == "Blue Area");
    }

    [Fact]
    public async Task List_AsStoreUser_OnlyReturnsOwnOrders()
    {
        await using var db = OrderTestFixtures.CreateInMemoryContext();
        var seed = await OrderTestFixtures.SeedOrdersAcrossCitiesAsync(db);
        var controller = CreateController(db);
        OrderTestFixtures.AttachUser(
            controller,
            OrderTestFixtures.CreateStoreUserPrincipal(seed.UserMotiMehelId, seed.TenantId, seed.StoreMotiMehelId));

        var result = await ListAsync(controller);

        var page = result.Result.Should().BeOfType<OkObjectResult>().Subject.Value
            .Should().BeOfType<PagedResult<OrderSummaryDto>>().Subject;
        page.Items.Should().ContainSingle();
        page.Items[0].Id.Should().Be(seed.OrderMotiMehelId);
        page.Items[0].StoreArea.Should().Be("Moti Mehel");
    }

    [Fact]
    public async Task List_AsStoreUser_CityFilterStillScopedToOwnStore()
    {
        await using var db = OrderTestFixtures.CreateInMemoryContext();
        var seed = await OrderTestFixtures.SeedOrdersAcrossCitiesAsync(db);
        var controller = CreateController(db);
        OrderTestFixtures.AttachUser(
            controller,
            OrderTestFixtures.CreateStoreUserPrincipal(seed.UserMotiMehelId, seed.TenantId, seed.StoreMotiMehelId));

        var matching = await ListAsync(controller, city: "Rawalpindi", area: "Moti Mehel");
        var otherArea = await ListAsync(controller, city: "Rawalpindi", area: "Raja Bazaar");

        matching.Result.Should().BeOfType<OkObjectResult>().Subject.Value
            .Should().BeOfType<PagedResult<OrderSummaryDto>>().Subject.Items.Should().ContainSingle();
        otherArea.Result.Should().BeOfType<OkObjectResult>().Subject.Value
            .Should().BeOfType<PagedResult<OrderSummaryDto>>().Subject.Items.Should().BeEmpty();
    }
}
