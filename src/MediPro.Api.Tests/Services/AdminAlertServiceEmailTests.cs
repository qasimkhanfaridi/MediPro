using FluentAssertions;
using MediPro.Api.Configuration;
using MediPro.Api.Data;
using MediPro.Api.Domain;
using MediPro.Api.Entities;
using MediPro.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MediPro.Api.Tests.Services;

public class AdminAlertServiceEmailTests
{
    private static MediProDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MediProDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new MediProDbContext(options);
    }

    [Fact]
    public async Task NotifyNewOrderSubmittedAsync_SendsEmailToAdminAddresses()
    {
        await using var context = CreateContext();
        var email = new RecordingEmailSender();
        var tenantId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        context.Users.Add(new AppUser
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = "admin@pharmacy.test",
            Role = UserRole.DistributorAdmin,
            PasswordHash = "hash",
            CreatedAtUtc = DateTime.UtcNow,
        });
        await context.SaveChangesAsync();

        var service = AdminAlertServiceTestHelper.Create(
            context,
            email,
            options: new NotificationOptions { AppBaseUrl = "https://uat.example" });

        await service.NotifyNewOrderSubmittedAsync(tenantId, orderId, "City Med", 999m, CancellationToken.None);

        email.Sent.Should().HaveCount(1);
        email.Sent[0].To.Should().ContainSingle("admin@pharmacy.test");
        email.Sent[0].Subject.Should().Contain("New order");
        email.Sent[0].Body.Should().Contain("City Med");
        email.Sent[0].Body.Should().Contain("https://uat.example/admin/orders");
    }

    [Fact]
    public async Task NotifyNewOrderSubmittedAsync_SendsWhatsAppMessage()
    {
        await using var context = CreateContext();
        var whatsApp = new RecordingWhatsAppSender();
        var tenantId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        context.Users.Add(new AppUser
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = "admin@test.com",
            Role = UserRole.DistributorAdmin,
            PasswordHash = "hash",
            CreatedAtUtc = DateTime.UtcNow,
        });
        await context.SaveChangesAsync();

        var service = AdminAlertServiceTestHelper.Create(
            context,
            whatsAppSender: whatsApp,
            options: new NotificationOptions { AppBaseUrl = "https://uat.example" });

        await service.NotifyNewOrderSubmittedAsync(tenantId, orderId, "City Med", 500m, CancellationToken.None);

        whatsApp.Sent.Should().HaveCount(1);
        whatsApp.Sent[0].Should().Contain("City Med");
        whatsApp.Sent[0].Should().Contain("500");
    }

    [Fact]
    public async Task NotifyPendingStoreReminderAsync_CreatesReminderNotificationAndMarksStore()
    {
        await using var context = CreateContext();
        var tenantId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        context.Users.Add(new AppUser
        {
            Id = adminId,
            TenantId = tenantId,
            Email = "admin@test.com",
            Role = UserRole.DistributorAdmin,
            PasswordHash = "hash",
            CreatedAtUtc = DateTime.UtcNow,
        });
        context.Stores.Add(new Store
        {
            Id = storeId,
            TenantId = tenantId,
            BusinessName = "Slow Pharmacy",
            ApprovalStatus = StoreApprovalStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow.AddHours(-30),
        });
        await context.SaveChangesAsync();

        var service = AdminAlertServiceTestHelper.Create(context);
        await service.NotifyPendingStoreReminderAsync(tenantId, storeId, "Slow Pharmacy", CancellationToken.None);

        var notifications = await context.AdminNotifications.ToListAsync();
        notifications.Should().HaveCount(1);
        notifications[0].Type.Should().Be(AdminNotificationType.PendingStoreApprovalReminder);
        notifications[0].Title.Should().Contain("Reminder");

        var store = await context.Stores.FirstAsync(s => s.Id == storeId);
        store.PendingApprovalReminderSentAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task NotifyPendingStoreReminderAsync_SecondCall_IsIdempotent()
    {
        await using var context = CreateContext();
        var tenantId = Guid.NewGuid();
        var storeId = Guid.NewGuid();

        context.Users.Add(new AppUser
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = "admin@test.com",
            Role = UserRole.DistributorAdmin,
            PasswordHash = "hash",
            CreatedAtUtc = DateTime.UtcNow,
        });
        context.Stores.Add(new Store
        {
            Id = storeId,
            TenantId = tenantId,
            BusinessName = "Once Only",
            ApprovalStatus = StoreApprovalStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-2),
        });
        await context.SaveChangesAsync();

        var service = AdminAlertServiceTestHelper.Create(context);
        await service.NotifyPendingStoreReminderAsync(tenantId, storeId, "Once Only", CancellationToken.None);
        await service.NotifyPendingStoreReminderAsync(tenantId, storeId, "Once Only", CancellationToken.None);

        (await context.AdminNotifications.CountAsync()).Should().Be(1);
    }
}
