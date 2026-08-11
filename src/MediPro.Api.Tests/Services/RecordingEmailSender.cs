using MediPro.Api.Services;

namespace MediPro.Api.Tests.Services;

internal sealed class RecordingEmailSender : IEmailSender
{
    public List<(IReadOnlyList<string> To, string Subject, string Body)> Sent { get; } = [];

    public Task SendAsync(IReadOnlyList<string> toAddresses, string subject, string body, CancellationToken ct = default)
    {
        Sent.Add((toAddresses.ToList(), subject, body));
        return Task.CompletedTask;
    }
}
