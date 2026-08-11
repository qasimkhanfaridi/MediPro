using MediPro.Api.Configuration;
using Microsoft.Extensions.Options;

namespace MediPro.Api.Services;

/// <summary>
/// Free WhatsApp alerts to a single admin phone via CallMeBot (personal use).
/// Setup: add CallMeBot on WhatsApp, send the activation message, then set phone + api key in config.
/// </summary>
public sealed class CallMeBotWhatsAppSender(
    IOptions<NotificationOptions> options,
    IHttpClientFactory httpClientFactory,
    ILogger<CallMeBotWhatsAppSender> logger) : IWhatsAppSender
{
    private const int MaxMessageLength = 1500;

    public async Task SendAsync(string message, CancellationToken ct = default)
    {
        var opts = options.Value;
        if (!opts.WhatsAppEnabled)
            return;

        if (string.IsNullOrWhiteSpace(opts.CallMeBotPhone) || string.IsNullOrWhiteSpace(opts.CallMeBotApiKey))
        {
            logger.LogWarning("Notifications:WhatsAppEnabled is true but CallMeBot phone or api key is missing.");
            return;
        }

        var text = message.Length <= MaxMessageLength ? message : message[..MaxMessageLength] + "…";
        var phone = opts.CallMeBotPhone.Trim().TrimStart('+');
        var url =
            $"https://api.callmebot.com/whatsapp.php?phone={Uri.EscapeDataString(phone)}&text={Uri.EscapeDataString(text)}&apikey={Uri.EscapeDataString(opts.CallMeBotApiKey.Trim())}";

        try
        {
            var client = httpClientFactory.CreateClient(nameof(CallMeBotWhatsAppSender));
            var response = await client.GetAsync(url, ct);
            if (response.IsSuccessStatusCode)
                logger.LogInformation("Sent WhatsApp alert via CallMeBot.");
            else
                logger.LogWarning("CallMeBot returned HTTP {StatusCode}", (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send WhatsApp alert via CallMeBot.");
        }
    }
}
