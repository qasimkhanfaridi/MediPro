using FluentAssertions;
using MediPro.Api.Data;
using MediPro.Api.Domain;
using MediPro.Api.Entities;
using MediPro.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MediPro.Api.Tests.Services;

public class AdminAlertServiceTests
{
    private MediProDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<MediProDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new MediProDbContext(options);
    }

    [Fact]
    public async Task NotifyNewOrderSubmittedAsync_WithAdminUsers_CreatesNotifications()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var service = AdminAlertServiceTestHelper.Create(context);

        var tenantId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var admin1Id = Guid.NewGuid();
        var admin2Id = Guid.NewGuid();

        context.Users.AddRange(
            new AppUser
            {
                Id = admin1Id,
                TenantId = tenantId,
                Email = "admin1@test.com",
                Role = UserRole.DistributorAdmin,
                PasswordHash = "hash",
                CreatedAtUtc = DateTime.UtcNow
            },
            new AppUser
            {
                Id = admin2Id,
                TenantId = tenantId,
                Email = "admin2@test.com",
                Role = UserRole.DistributorAdmin,
                PasswordHash = "hash",
                CreatedAtUtc = DateTime.UtcNow
            }
        );
        await context.SaveChangesAsync();

        // Act
        await service.NotifyNewOrderSubmittedAsync(
            tenantId,
            orderId,
            "Test Pharmacy",
            1500.50m,
            CancellationToken.None
        );

        // Assert
        var notifications = await context.AdminNotifications.ToListAsync();
        notifications.Should().HaveCount(2);
        
        notifications.Should().AllSatisfy(n =>
        {
            n.TenantId.Should().Be(tenantId);
            n.Type.Should().Be(AdminNotificationType.NewOrderSubmitted);
            n.Title.Should().Be("New order submitted");
            n.Body.Should().Contain("Test Pharmacy");
            n.Body.Should().Contain(orderId.ToString("N"));
            n.Body.Should().MatchRegex(@"1[,\.]?500[,\.]?50"); // Handle different number formats
            n.RelatedOrderId.Should().Be(orderId);
            n.RelatedStoreId.Should().BeNull();
        });

        var recipientIds = notifications.Select(n => n.RecipientUserId).ToList();
        recipientIds.Should().Contain(admin1Id);
        recipientIds.Should().Contain(admin2Id);
    }

    [Fact]
    public async Task NotifyNewOrderSubmittedAsync_NoAdminUsers_DoesNotCreateNotifications()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var service = AdminAlertServiceTestHelper.Create(context);

        var tenantId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        // Add a store user (non-admin)
        context.Users.Add(new AppUser
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = "store@test.com",
            Role = UserRole.StoreUser,
            PasswordHash = "hash",
            CreatedAtUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // Act
        await service.NotifyNewOrderSubmittedAsync(
            tenantId,
            orderId,
            "Test Pharmacy",
            1000m,
            CancellationToken.None
        );

        // Assert
        var notifications = await context.AdminNotifications.ToListAsync();
        notifications.Should().BeEmpty("no admin users exist for the tenant");
    }

    [Fact]
    public async Task NotifyNewStorePendingAsync_WithAdminUsers_CreatesNotifications()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var service = AdminAlertServiceTestHelper.Create(context);

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
            CreatedAtUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // Act
        await service.NotifyNewStorePendingAsync(
            tenantId,
            storeId,
            "New Pharmacy Ltd",
            "LIC-12345",
            CancellationToken.None
        );

        // Assert
        var notifications = await context.AdminNotifications.ToListAsync();
        notifications.Should().HaveCount(1);
        
        var notification = notifications.First();
        notification.TenantId.Should().Be(tenantId);
        notification.RecipientUserId.Should().Be(adminId);
        notification.Type.Should().Be(AdminNotificationType.NewStorePendingApproval);
        notification.Title.Should().Be("New store registration");
        notification.Body.Should().Contain("New Pharmacy Ltd");
        notification.Body.Should().Contain("LIC-12345");
        notification.Body.Should().Contain(storeId.ToString("N"));
        notification.RelatedStoreId.Should().Be(storeId);
        notification.RelatedOrderId.Should().BeNull();
    }

    [Fact]
    public async Task NotifyNewStorePendingAsync_MultipleAdmins_CreatesNotificationForEach()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var service = AdminAlertServiceTestHelper.Create(context);

        var tenantId = Guid.NewGuid();
        var storeId = Guid.NewGuid();

        var adminIds = Enumerable.Range(1, 3).Select(_ => Guid.NewGuid()).ToList();
        
        foreach (var adminId in adminIds)
        {
            context.Users.Add(new AppUser
            {
                Id = adminId,
                TenantId = tenantId,
                Email = $"admin{adminId}@test.com",
                Role = UserRole.DistributorAdmin,
                PasswordHash = "hash",
                CreatedAtUtc = DateTime.UtcNow
            });
        }
        await context.SaveChangesAsync();

        // Act
        await service.NotifyNewStorePendingAsync(
            tenantId,
            storeId,
            "Test Store",
            "DEMO-LIC-0001",
            CancellationToken.None
        );

        // Assert
        var notifications = await context.AdminNotifications.ToListAsync();
        notifications.Should().HaveCount(3);
        
        var recipientIds = notifications.Select(n => n.RecipientUserId).ToList();
        recipientIds.Should().BeEquivalentTo(adminIds);
    }

    [Fact]
    public async Task NotifyNewOrderSubmittedAsync_OnlyNotifiesAdminsFromSameTenant()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var service = AdminAlertServiceTestHelper.Create(context);

        var tenant1Id = Guid.NewGuid();
        var tenant2Id = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        context.Users.AddRange(
            new AppUser
            {
                Id = Guid.NewGuid(),
                TenantId = tenant1Id,
                Email = "admin1@test.com",
                Role = UserRole.DistributorAdmin,
                PasswordHash = "hash",
                CreatedAtUtc = DateTime.UtcNow
            },
            new AppUser
            {
                Id = Guid.NewGuid(),
                TenantId = tenant2Id,
                Email = "admin2@test.com",
                Role = UserRole.DistributorAdmin,
                PasswordHash = "hash",
                CreatedAtUtc = DateTime.UtcNow
            }
        );
        await context.SaveChangesAsync();

        // Act
        await service.NotifyNewOrderSubmittedAsync(
            tenant1Id,
            orderId,
            "Test Store",
            1000m,
            CancellationToken.None
        );

        // Assert
        var notifications = await context.AdminNotifications.ToListAsync();
        notifications.Should().HaveCount(1);
        notifications.First().TenantId.Should().Be(tenant1Id);
    }
}
