using System.Security.Claims;
using MediPro.Api.Data;
using MediPro.Api.Domain;
using MediPro.Api.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediPro.Api.Tests.Helpers;

public sealed record OrderLocationSeed(
    Guid TenantId,
    Guid AdminUserId,
    Guid StoreMotiMehelId,
    Guid StoreRajaBazaarId,
    Guid StoreBlueAreaId,
    Guid UserMotiMehelId,
    Guid UserRajaBazaarId,
    Guid UserBlueAreaId,
    Guid OrderMotiMehelId,
    Guid OrderRajaBazaarId,
    Guid OrderBlueAreaId);

public static class OrderTestFixtures
{
    public static MediProDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<MediProDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MediProDbContext(options);
    }

    public static ClaimsPrincipal CreateAdminPrincipal(Guid userId, Guid tenantId)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("tenant_id", tenantId.ToString()),
            new Claim(ClaimTypes.Role, nameof(UserRole.DistributorAdmin)),
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    public static ClaimsPrincipal CreateStoreUserPrincipal(Guid userId, Guid tenantId, Guid storeId)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("tenant_id", tenantId.ToString()),
            new Claim("store_id", storeId.ToString()),
            new Claim(ClaimTypes.Role, nameof(UserRole.StoreUser)),
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    public static void AttachUser(ControllerBase controller, ClaimsPrincipal user)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user },
        };
    }

    public static async Task<OrderLocationSeed> SeedOrdersAcrossCitiesAsync(MediProDbContext db)
    {
        var tenantId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid();

        db.Tenants.Add(new Tenant
        {
            Id = tenantId,
            Name = "Test Distributor",
            CreatedAtUtc = DateTime.UtcNow,
        });

        db.Users.Add(new AppUser
        {
            Id = adminUserId,
            TenantId = tenantId,
            Email = "admin@test.local",
            PasswordHash = "hash",
            Role = UserRole.DistributorAdmin,
            CreatedAtUtc = DateTime.UtcNow,
        });

        var storeMotiId = Guid.NewGuid();
        var storeRajaId = Guid.NewGuid();
        var storeBlueId = Guid.NewGuid();
        var userMotiId = Guid.NewGuid();
        var userRajaId = Guid.NewGuid();
        var userBlueId = Guid.NewGuid();

        db.Stores.AddRange(
            CreateStore(storeMotiId, tenantId, "Moti Mehel Pharmacy", "Rawalpindi", "Moti Mehel"),
            CreateStore(storeRajaId, tenantId, "Raja Bazaar Pharmacy", "Rawalpindi", "Raja Bazaar"),
            CreateStore(storeBlueId, tenantId, "Blue Area Pharmacy", "Islamabad", "Blue Area"));

        db.Users.AddRange(
            CreateStoreUser(userMotiId, tenantId, storeMotiId, "moti@test.local"),
            CreateStoreUser(userRajaId, tenantId, storeRajaId, "raja@test.local"),
            CreateStoreUser(userBlueId, tenantId, storeBlueId, "blue@test.local"));

        var orderMotiId = Guid.NewGuid();
        var orderRajaId = Guid.NewGuid();
        var orderBlueId = Guid.NewGuid();

        db.Orders.AddRange(
            CreateOrder(orderMotiId, tenantId, storeMotiId, userMotiId),
            CreateOrder(orderRajaId, tenantId, storeRajaId, userRajaId),
            CreateOrder(orderBlueId, tenantId, storeBlueId, userBlueId));

        await db.SaveChangesAsync();

        return new OrderLocationSeed(
            tenantId,
            adminUserId,
            storeMotiId,
            storeRajaId,
            storeBlueId,
            userMotiId,
            userRajaId,
            userBlueId,
            orderMotiId,
            orderRajaId,
            orderBlueId);
    }

    public static async Task<(Guid TenantId, Guid StoreId, Guid UserId, Guid ProductId)> SeedDemoSeedDataAsync(
        MediProDbContext db)
    {
        var tenantId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        db.Tenants.Add(new Tenant
        {
            Id = tenantId,
            Name = "Demo Tenant",
            CreatedAtUtc = DateTime.UtcNow,
        });

        db.Stores.Add(CreateStore(storeId, tenantId, "Demo Moti Store", "Rawalpindi", "Moti Mehel"));

        db.Users.Add(new AppUser
        {
            Id = userId,
            TenantId = tenantId,
            Email = $"store.approved5{DemoStoresSeeder.DemoEmailDomain}",
            PasswordHash = "hash",
            Role = UserRole.StoreUser,
            StoreId = storeId,
            CreatedAtUtc = DateTime.UtcNow,
        });

        db.Products.Add(new Product
        {
            Id = productId,
            TenantId = tenantId,
            SkuCode = "TEST-MED-001",
            Name = "Demo Product",
            Pack = "10 Tabs",
            Manufacturer = "Demo Pharma",
            SaltComposition = "Demo Salt",
            TradePrice = 50m,
            Mrp = 60m,
            IsActive = true,
            StockQuantity = 100,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        });

        await db.SaveChangesAsync();
        return (tenantId, storeId, userId, productId);
    }

    private static Store CreateStore(Guid id, Guid tenantId, string name, string city, string area) =>
        new()
        {
            Id = id,
            TenantId = tenantId,
            BusinessName = name,
            AddressLine = "Test address",
            City = city,
            Area = area,
            ContactName = "Contact",
            Mobile = "03001234567",
            ApprovalStatus = StoreApprovalStatus.Approved,
            CreatedAtUtc = DateTime.UtcNow,
        };

    private static AppUser CreateStoreUser(Guid id, Guid tenantId, Guid storeId, string email) =>
        new()
        {
            Id = id,
            TenantId = tenantId,
            Email = email,
            PasswordHash = "hash",
            Role = UserRole.StoreUser,
            StoreId = storeId,
            CreatedAtUtc = DateTime.UtcNow,
        };

    private static Order CreateOrder(Guid id, Guid tenantId, Guid storeId, Guid userId) =>
        new()
        {
            Id = id,
            TenantId = tenantId,
            StoreId = storeId,
            SubmittedByUserId = userId,
            Status = OrderStatus.Submitted,
            TotalAmount = 250m,
            Currency = "PKR",
            SubmittedAtUtc = DateTime.UtcNow.AddDays(-1),
        };
}
