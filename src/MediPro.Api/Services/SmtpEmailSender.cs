using System.Net;
using System.Net.Mail;
using MediPro.Api.Configuration;
using Microsoft.Extensions.Options;

namespace MediPro.Api.Services;

public sealed class SmtpEmailSender(
    IOptions<NotificationOptions> options,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(IReadOnlyList<string> toAddresses, string subject, string body, CancellationToken ct = default)
    {
        var opts = options.Value;
        if (!opts.EmailEnabled || toAddresses.Count == 0)
            return;

        if (string.IsNullOrWhiteSpace(opts.SmtpHost))
        {
            logger.LogWarning("Notifications:EmailEnabled is true but SmtpHost is empty; skipping email.");
            return;
        }

        if (string.IsNullOrWhiteSpace(opts.FromEmail))
        {
            logger.LogWarning("Notifications:FromEmail is empty; skipping email.");
            return;
        }

        var recipients = toAddresses
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (recipients.Count == 0)
            return;

        using var message = new MailMessage
        {
            From = new MailAddress(opts.FromEmail, opts.FromDisplayName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false,
        };

        foreach (var to in recipients)
            message.To.Add(to);

        using var client = new SmtpClient(opts.SmtpHost, opts.SmtpPort)
        {
            EnableSsl = opts.UseStartTls,
            DeliveryMethod = SmtpDeliveryMethod.Network,
        };

        if (!string.IsNullOrWhiteSpace(opts.SmtpUsername))
            client.Credentials = new NetworkCredential(opts.SmtpUsername, opts.SmtpPassword ?? "");

        try
        {
            await client.SendMailAsync(message, ct);
            logger.LogInformation("Sent notification email to {Count} recipient(s): {Subject}", recipients.Count, subject);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send notification email: {Subject}", subject);
        }
    }
}
