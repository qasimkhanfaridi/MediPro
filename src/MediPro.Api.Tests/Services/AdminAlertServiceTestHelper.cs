using MediPro.Api.Configuration;
using MediPro.Api.Data;
using MediPro.Api.Services;
using Microsoft.Extensions.Options;

namespace MediPro.Api.Tests.Services;

internal static class AdminAlertServiceTestHelper
{
    public static AdminAlertService Create(
        MediProDbContext db,
        IEmailSender? emailSender = null,
        IWhatsAppSender? whatsAppSender = null,
        NotificationOptions? options = null) =>
        new(
            db,
            emailSender ?? new RecordingEmailSender(),
            whatsAppSender ?? new RecordingWhatsAppSender(),
            Options.Create(options ?? new NotificationOptions()));
}
