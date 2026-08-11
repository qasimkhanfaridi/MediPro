using MediPro.Api.Domain;

namespace MediPro.Api.Entities;

public class AdminNotification
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public Guid RecipientUserId { get; set; }
    public AppUser RecipientUser { get; set; } = null!;

    public AdminNotificationType Type { get; set; }
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";

    public Guid? RelatedOrderId { get; set; }
    public Guid? RelatedStoreId { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ReadAtUtc { get; set; }
}
