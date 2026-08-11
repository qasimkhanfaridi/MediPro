using FluentAssertions;
using MediPro.Api.Configuration;
using MediPro.Api.Contracts;
using MediPro.Api.Controllers;
using MediPro.Api.Data;
using MediPro.Api.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace MediPro.Api.Tests.Controllers;

public class AdminOrdersControllerTests
{
    private static AdminOrdersController CreateController(
        MediProDbContext db,
        bool isDevelopment = true,
        bool allowDemoEndpoint = true)
    {
        var env = new Mock<IHostEnvironment>();
        env.Setup(e => e.EnvironmentName).Returns(isDevelopment ? Environments.Development : Environments.Production);
        var options = Options.Create(new DevSeedOptions { AllowDemoCatalogEndpoint = allowDemoEndpoint });
        return new AdminOrdersController(db, env.Object, options);
    }

    [Fact]
    public async Task LocationOptions_ReturnsReferenceCitiesIncludingRawalpindiAndMotiMehel()
    {
        await using var db = OrderTestFixtures.CreateInMemoryContext();
        var seed = await OrderTestFixtures.SeedOrdersAcrossCitiesAsync(db);
        var controller = CreateController(db);
        OrderTestFixtures.AttachUser(controller, OrderTestFixtures.CreateAdminPrincipal(seed.AdminUserId, seed.TenantId));

        var result = await controller.LocationOptions(CancellationToken.None);

        var dto = result.Result.Should().BeOfType<OkObjectResult>().Subject.Value
            .Should().BeOfType<OrderLocationOptionsDto>().Subject;
        dto.Cities.Should().Contain("Rawalpindi");
        dto.Cities.Should().Contain("Islamabad");
        dto.AreasByCity["Rawalpindi"].Should().Contain("Moti Mehel");
        dto.AreasByCity["Rawalpindi"].Should().Contain("Raja Bazaar");
    }

    [Fact]
    public async Task LocationOptions_MergesAreasFromDatabaseStores()
    {
        await using var db = OrderTestFixtures.CreateInMemoryContext();
        var seed = await OrderTestFixtures.SeedOrdersAcrossCitiesAsync(db);
        var controller = CreateController(db);
        OrderTestFixtures.AttachUser(controller, OrderTestFixtures.CreateAdminPrincipal(seed.AdminUserId, seed.TenantId));

        var result = await controller.LocationOptions(CancellationToken.None);

        var dto = result.Result.Should().BeOfType<OkObjectResult>().Subject.Value
            .Should().BeOfType<OrderLocationOptionsDto>().Subject;
        dto.AreasByCity["Rawalpindi"].Should().Contain("Moti Mehel");
        dto.AreasByCity["Islamabad"].Should().Contain("Blue Area");
    }

    [Fact]
    public async Task SeedDemoOrders_InsertsOrdersWhenNoneExist()
    {
        await using var db = OrderTestFixtures.CreateInMemoryContext();
        var (tenantId, _, _, _) = await OrderTestFixtures.SeedDemoSeedDataAsync(db);
        var adminId = Guid.NewGuid();
        db.Users.Add(new MediPro.Api.Entities.AppUser
        {
            Id = adminId,
            TenantId = tenantId,
            Email = "admin-seed@test.local",
            PasswordHash = "hash",
            Role = MediPro.Api.Domain.UserRole.DistributorAdmin,
            CreatedAtUtc = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        var controller = CreateController(db);
        OrderTestFixtures.AttachUser(controller, OrderTestFixtures.CreateAdminPrincipal(adminId, tenantId));

        var result = await controller.SeedDemoOrders(CancellationToken.None);

        var payload = result.Result.Should().BeOfType<OkObjectResult>().Subject.Value;
        payload.Should().NotBeNull();
        var inserted = (int)payload!.GetType().GetProperty("inserted")!.GetValue(payload)!;
        inserted.Should().BeGreaterThan(0);
        db.Orders.Count(o => o.TenantId == tenantId && o.Notes == DemoOrdersSeeder.DemoOrderNotesMarker)
            .Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SeedDemoOrders_IsIdempotent_DoesNotDuplicateOrders()
    {
        await using var db = OrderTestFixtures.CreateInMemoryContext();
        var (tenantId, _, _, _) = await OrderTestFixtures.SeedDemoSeedDataAsync(db);
        var adminId = Guid.NewGuid();
        db.Users.Add(new MediPro.Api.Entities.AppUser
        {
            Id = adminId,
            TenantId = tenantId,
            Email = "admin-seed2@test.local",
            PasswordHash = "hash",
            Role = MediPro.Api.Domain.UserRole.DistributorAdmin,
            CreatedAtUtc = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        var controller = CreateController(db);
        OrderTestFixtures.AttachUser(controller, OrderTestFixtures.CreateAdminPrincipal(adminId, tenantId));

        await controller.SeedDemoOrders(CancellationToken.None);
        var countAfterFirst = db.Orders.Count(o =>
            o.TenantId == tenantId && o.Notes == DemoOrdersSeeder.DemoOrderNotesMarker);

        var second = await controller.SeedDemoOrders(CancellationToken.None);
        var countAfterSecond = db.Orders.Count(o =>
            o.TenantId == tenantId && o.Notes == DemoOrdersSeeder.DemoOrderNotesMarker);

        countAfterSecond.Should().Be(countAfterFirst);
        var payload = second.Result.Should().BeOfType<OkObjectResult>().Subject.Value;
        var inserted = (int)payload!.GetType().GetProperty("inserted")!.GetValue(payload)!;
        inserted.Should().Be(0);
    }

    [Fact]
    public async Task SeedDemoOrders_WhenNotAllowed_ReturnsNotFound()
    {
        await using var db = OrderTestFixtures.CreateInMemoryContext();
        var seed = await OrderTestFixtures.SeedOrdersAcrossCitiesAsync(db);
        var controller = CreateController(db, isDevelopment: false, allowDemoEndpoint: false);
        OrderTestFixtures.AttachUser(controller, OrderTestFixtures.CreateAdminPrincipal(seed.AdminUserId, seed.TenantId));

        var result = await controller.SeedDemoOrders(CancellationToken.None);

        result.Result.Should().BeOfType<NotFoundResult>();
    }
}
