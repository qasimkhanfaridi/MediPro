using MediPro.Api.Services;

namespace MediPro.Api.Tests.Services;

internal sealed class RecordingWhatsAppSender : IWhatsAppSender
{
    public List<string> Sent { get; } = [];

    public Task SendAsync(string message, CancellationToken ct = default)
    {
        Sent.Add(message);
        return Task.CompletedTask;
    }
}
