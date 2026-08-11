using MediPro.Api.Domain;

namespace MediPro.Api.Entities;

public class AppUser
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    /// <summary>Normalized lowercase email for lookup.</summary>
    public string Email { get; set; } = "";

    public string PasswordHash { get; set; } = "";
    public UserRole Role { get; set; }

    /// <summary>Set when <see cref="Role"/> is <see cref="UserRole.StoreUser"/>.</summary>
    public Guid? StoreId { get; set; }
    public Store? Store { get; set; }

    public Cart? Cart { get; set; }

    public ICollection<AdminNotification> AdminNotifications { get; set; } = new List<AdminNotification>();

    public DateTime CreatedAtUtc { get; set; }
}
