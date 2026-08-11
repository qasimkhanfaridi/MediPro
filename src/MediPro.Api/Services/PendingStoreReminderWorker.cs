using MediPro.Api.Configuration;
using MediPro.Api.Data;
using MediPro.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MediPro.Api.Services;

public sealed class PendingStoreReminderWorker(
    MediProDbContext db,
    IAdminAlertService adminAlerts,
    IOptions<NotificationOptions> options,
    ILogger<PendingStoreReminderWorker> logger)
{
    public async Task ProcessAsync(CancellationToken ct)
    {
        if (!options.Value.PendingStoreReminderEnabled)
            return;

        var hours = Math.Max(1, options.Value.PendingStoreReminderHours);
        var cutoff = DateTime.UtcNow.AddHours(-hours);

        var overdue = await db.Stores.AsNoTracking()
            .Where(s => s.ApprovalStatus == StoreApprovalStatus.Pending
                && s.CreatedAtUtc <= cutoff
                && s.PendingApprovalReminderSentAtUtc == null)
            .Select(s => new { s.TenantId, s.Id, s.BusinessName })
            .ToListAsync(ct);

        foreach (var store in overdue)
        {
            try
            {
                await adminAlerts.NotifyPendingStoreReminderAsync(
                    store.TenantId, store.Id, store.BusinessName, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Pending store reminder failed for store {StoreId}", store.Id);
            }
        }
    }
}
