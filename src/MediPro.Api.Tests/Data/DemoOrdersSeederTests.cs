using FluentAssertions;
using MediPro.Api.Data;
using MediPro.Api.Domain;
using MediPro.Api.Entities;
using MediPro.Api.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MediPro.Api.Tests.Data;

public class DemoOrdersSeederTests
{
    [Fact]
    public async Task BackfillStoreAreasAsync_SetsAreaForDemoStoreUsers()
    {
        await using var db = OrderTestFixtures.CreateInMemoryContext();
        var tenantId = Guid.NewGuid();
        var storeId = Guid.NewGuid();

        db.Tenants.Add(new Tenant
        {
            Id = tenantId,
            Name = "Demo",
            CreatedAtUtc = DateTime.UtcNow,
        });

        db.Stores.Add(new Store
        {
            Id = storeId,
            TenantId = tenantId,
            BusinessName = "Pending area store",
            AddressLine = "Line",
            City = "Rawalpindi",
            Area = "",
            ContactName = "Contact",
            Mobile = "03001234567",
            ApprovalStatus = StoreApprovalStatus.Approved,
            CreatedAtUtc = DateTime.UtcNow,
        });

        db.Users.Add(new AppUser
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = $"store.approved5{DemoStoresSeeder.DemoEmailDomain}",
            PasswordHash = "hash",
            Role = UserRole.StoreUser,
            StoreId = storeId,
            CreatedAtUtc = DateTime.UtcNow,
        });

        await db.SaveChangesAsync();

        var updated = await DemoOrdersSeeder.BackfillStoreAreasAsync(db, tenantId);

        updated.Should().Be(1);
        var store = await db.Stores.FindAsync(storeId);
        store!.Area.Should().Be("Moti Mehel");
        store.City.Should().Be("Rawalpindi");
    }

    [Fact]
    public async Task InsertDemoOrdersAsync_ReturnsZero_WhenDemoOrdersAlreadyExist()
    {
        await using var db = OrderTestFixtures.CreateInMemoryContext();
        var (tenantId, storeId, userId, _) = await OrderTestFixtures.SeedDemoSeedDataAsync(db);

        db.Orders.Add(new Order
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            StoreId = storeId,
            SubmittedByUserId = userId,
            Status = OrderStatus.Submitted,
            TotalAmount = 10m,
            Currency = "PKR",
            SubmittedAtUtc = DateTime.UtcNow,
            Notes = DemoOrdersSeeder.DemoOrderNotesMarker,
        });
        await db.SaveChangesAsync();

        var inserted = await DemoOrdersSeeder.InsertDemoOrdersAsync(db, tenantId);

        inserted.Should().Be(0);
    }

    [Fact]
    public async Task InsertDemoOrdersAsync_CreatesOrdersAcrossApprovedStores()
    {
        await using var db = OrderTestFixtures.CreateInMemoryContext();
        var (tenantId, _, _, _) = await OrderTestFixtures.SeedDemoSeedDataAsync(db);

        var inserted = await DemoOrdersSeeder.InsertDemoOrdersAsync(db, tenantId);

        inserted.Should().BeGreaterThan(0);
        db.Orders.Count(o => o.TenantId == tenantId && o.Notes == DemoOrdersSeeder.DemoOrderNotesMarker)
            .Should().Be(inserted);
    }
}
