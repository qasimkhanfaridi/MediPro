using MediPro.Api.Configuration;
using MediPro.Api.Data;
using MediPro.Api.Domain;
using MediPro.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MediPro.Api.Services;

public interface IAdminAlertService
{
    Task NotifyNewOrderSubmittedAsync(Guid tenantId, Guid orderId, string storeName, decimal totalAmount, CancellationToken ct);
    Task NotifyNewStorePendingAsync(Guid tenantId, Guid storeId, string businessName, string? licenseNumber, CancellationToken ct);
    Task NotifyPendingStoreReminderAsync(Guid tenantId, Guid storeId, string businessName, CancellationToken ct);
}

public sealed class AdminAlertService(
    MediProDbContext db,
    IEmailSender emailSender,
    IWhatsAppSender whatsAppSender,
    IOptions<NotificationOptions> notificationOptions) : IAdminAlertService
{
    public async Task NotifyNewOrderSubmittedAsync(Guid tenantId, Guid orderId, string storeName, decimal totalAmount, CancellationToken ct)
    {
        var adminIds = await GetAdminUserIdsAsync(tenantId, ct);
        if (adminIds.Count == 0)
            return;

        var now = DateTime.UtcNow;
        foreach (var id in adminIds)
        {
            db.AdminNotifications.Add(new AdminNotification
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RecipientUserId = id,
                Type = AdminNotificationType.NewOrderSubmitted,
                Title = "New order submitted",
                Body = $"{storeName} submitted order {orderId:N} for {totalAmount:N2} PKR.",
                RelatedOrderId = orderId,
                RelatedStoreId = null,
                CreatedAtUtc = now,
            });
        }

        await db.SaveChangesAsync(ct);

        var opts = notificationOptions.Value;
        var adminLink = $"{opts.AppBaseUrl.TrimEnd('/')}/admin/orders";
        await SendAdminEmailAsync(
            tenantId,
            "[MediPro] New order submitted",
            $"""
            {storeName} submitted a new order.

            Order id: {orderId:N}
            Total: {totalAmount:N2} PKR

            Review orders: {adminLink}
            """,
            ct);
        await whatsAppSender.SendAsync(
            $"MediPro: {storeName} submitted order {orderId:N} for {totalAmount:N2} PKR. {adminLink}",
            ct);
    }

    public async Task NotifyNewStorePendingAsync(Guid tenantId, Guid storeId, string businessName, string? licenseNumber, CancellationToken ct)
    {
        var adminIds = await GetAdminUserIdsAsync(tenantId, ct);
        if (adminIds.Count == 0)
            return;

        var licensePart = string.IsNullOrWhiteSpace(licenseNumber)
            ? "licence not provided"
            : $"licence {licenseNumber.Trim()}";
        var now = DateTime.UtcNow;
        foreach (var id in adminIds)
        {
            db.AdminNotifications.Add(new AdminNotification
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RecipientUserId = id,
                Type = AdminNotificationType.NewStorePendingApproval,
                Title = "New store registration",
                Body = $"{businessName} is pending approval ({licensePart}; store id {storeId:N}).",
                RelatedOrderId = null,
                RelatedStoreId = storeId,
                CreatedAtUtc = now,
            });
        }

        await db.SaveChangesAsync(ct);

        var opts = notificationOptions.Value;
        var adminLink = $"{opts.AppBaseUrl.TrimEnd('/')}/admin";
        await SendAdminEmailAsync(
            tenantId,
            "[MediPro] New store pending approval",
            $"""
            {businessName} registered and is waiting for approval.

            Licence: {(string.IsNullOrWhiteSpace(licenseNumber) ? "(not provided)" : licenseNumber.Trim())}
            Store id: {storeId:N}

            Review stores: {adminLink}
            """,
            ct);
        await whatsAppSender.SendAsync(
            $"MediPro: {businessName} pending approval ({licensePart}). {adminLink}",
            ct);
    }

    public async Task NotifyPendingStoreReminderAsync(Guid tenantId, Guid storeId, string businessName, CancellationToken ct)
    {
        var store = await db.Stores.FirstOrDefaultAsync(
            s => s.Id == storeId && s.TenantId == tenantId, ct);

        if (store is null
            || store.ApprovalStatus != StoreApprovalStatus.Pending
            || store.PendingApprovalReminderSentAtUtc is not null)
            return;

        var adminIds = await GetAdminUserIdsAsync(tenantId, ct);
        if (adminIds.Count == 0)
            return;

        var now = DateTime.UtcNow;
        var hours = Math.Max(1, notificationOptions.Value.PendingStoreReminderHours);

        foreach (var id in adminIds)
        {
            db.AdminNotifications.Add(new AdminNotification
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RecipientUserId = id,
                Type = AdminNotificationType.PendingStoreApprovalReminder,
                Title = "Reminder: store still pending",
                Body = $"{businessName} has been pending approval for over {hours} hours (store id {storeId:N}).",
                RelatedOrderId = null,
                RelatedStoreId = storeId,
                CreatedAtUtc = now,
            });
        }

        store.PendingApprovalReminderSentAtUtc = now;
        await db.SaveChangesAsync(ct);

        var opts = notificationOptions.Value;
        var adminLink = $"{opts.AppBaseUrl.TrimEnd('/')}/admin";
        await SendAdminEmailAsync(
            tenantId,
            "[MediPro] Reminder: store still pending approval",
            $"""
            {businessName} has been pending approval for more than {hours} hours.

            Store id: {storeId:N}
            Registered: {store.CreatedAtUtc:u}

            Review stores: {adminLink}
            """,
            ct);
        await whatsAppSender.SendAsync(
            $"MediPro reminder: {businessName} still pending approval ({hours}h+). {adminLink}",
            ct);
    }

    private async Task<List<Guid>> GetAdminUserIdsAsync(Guid tenantId, CancellationToken ct) =>
        await db.Users.AsNoTracking()
            .Where(u => u.TenantId == tenantId && u.Role == UserRole.DistributorAdmin)
            .Select(u => u.Id)
            .ToListAsync(ct);

    private async Task SendAdminEmailAsync(Guid tenantId, string subject, string body, CancellationToken ct)
    {
        var addresses = await ResolveAdminEmailAddressesAsync(tenantId, ct);
        await emailSender.SendAsync(addresses, subject, body, ct);
    }

    private async Task<List<string>> ResolveAdminEmailAddressesAsync(Guid tenantId, CancellationToken ct)
    {
        var fromUsers = await db.Users.AsNoTracking()
            .Where(u => u.TenantId == tenantId && u.Role == UserRole.DistributorAdmin)
            .Select(u => u.Email)
            .ToListAsync(ct);

        var extra = notificationOptions.Value.AdditionalAdminEmails ?? [];

        return fromUsers
            .Concat(extra)
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
