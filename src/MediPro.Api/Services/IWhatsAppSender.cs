namespace MediPro.Api.Services;

public interface IWhatsAppSender
{
    Task SendAsync(string message, CancellationToken ct = default);
}
