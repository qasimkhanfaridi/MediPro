using MediPro.Api.Configuration;
using Microsoft.Extensions.Options;

namespace MediPro.Api.Services;

public sealed class PendingStoreReminderBackgroundService(
    IServiceProvider serviceProvider,
    IOptions<NotificationOptions> options,
    ILogger<PendingStoreReminderBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalMinutes = Math.Max(5, options.Value.ReminderCheckIntervalMinutes);
        var delay = TimeSpan.FromMinutes(intervalMinutes);

        // Stagger first run so app startup / migrations finish first.
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            if (options.Value.PendingStoreReminderEnabled)
            {
                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var worker = scope.ServiceProvider.GetRequiredService<PendingStoreReminderWorker>();
                    await worker.ProcessAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "Pending store reminder background run failed.");
                }
            }

            await Task.Delay(delay, stoppingToken);
        }
    }
}
