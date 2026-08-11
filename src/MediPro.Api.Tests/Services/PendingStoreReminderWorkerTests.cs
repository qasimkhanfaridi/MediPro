using FluentAssertions;
using MediPro.Api.Configuration;
using MediPro.Api.Data;
using MediPro.Api.Domain;
using MediPro.Api.Entities;
using MediPro.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace MediPro.Api.Tests.Services;

public class PendingStoreReminderWorkerTests
{
    [Fact]
    public async Task ProcessAsync_OverduePendingStore_TriggersReminder()
    {
        var options = new DbContextOptionsBuilder<MediProDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new MediProDbContext(options);

        var tenantId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        db.Users.Add(new AppUser
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = "admin@test.com",
            Role = UserRole.DistributorAdmin,
            PasswordHash = "hash",
            CreatedAtUtc = DateTime.UtcNow,
        });
        db.Stores.Add(new Store
        {
            Id = storeId,
            TenantId = tenantId,
            BusinessName = "Late Reg",
            ApprovalStatus = StoreApprovalStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow.AddHours(-48),
        });
        await db.SaveChangesAsync();

        var alerts = AdminAlertServiceTestHelper.Create(db);
        var worker = new PendingStoreReminderWorker(
            db,
            alerts,
            Options.Create(new NotificationOptions { PendingStoreReminderHours = 24 }),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<PendingStoreReminderWorker>.Instance);

        await worker.ProcessAsync(CancellationToken.None);

        var store = await db.Stores.FirstAsync(s => s.Id == storeId);
        store.PendingApprovalReminderSentAtUtc.Should().NotBeNull();
        (await db.AdminNotifications.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task ProcessAsync_RecentPendingStore_DoesNotRemind()
    {
        var options = new DbContextOptionsBuilder<MediProDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new MediProDbContext(options);

        var tenantId = Guid.NewGuid();
        db.Users.Add(new AppUser
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = "admin@test.com",
            Role = UserRole.DistributorAdmin,
            PasswordHash = "hash",
            CreatedAtUtc = DateTime.UtcNow,
        });
        db.Stores.Add(new Store
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BusinessName = "Fresh Reg",
            ApprovalStatus = StoreApprovalStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow.AddHours(-2),
        });
        await db.SaveChangesAsync();

        var worker = new PendingStoreReminderWorker(
            db,
            AdminAlertServiceTestHelper.Create(db),
            Options.Create(new NotificationOptions { PendingStoreReminderHours = 24 }),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<PendingStoreReminderWorker>.Instance);

        await worker.ProcessAsync(CancellationToken.None);

        (await db.AdminNotifications.CountAsync()).Should().Be(0);
    }
}
