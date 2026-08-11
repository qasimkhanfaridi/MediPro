namespace MediPro.Api.Services;

public interface IEmailSender
{
    Task SendAsync(IReadOnlyList<string> toAddresses, string subject, string body, CancellationToken ct = default);
}
