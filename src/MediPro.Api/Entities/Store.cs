using MediPro.Api.Domain;

namespace MediPro.Api.Entities;

public class Store
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public string BusinessName { get; set; } = "";
    public string AddressLine { get; set; } = "";
    public string City { get; set; } = "";
    /// <summary>Neighbourhood / sector within city (e.g. Moti Mehel, Saddar).</summary>
    public string Area { get; set; } = "";
    public string? LicenseNumber { get; set; }
    public string ContactName { get; set; } = "";
    public string Mobile { get; set; } = "";

    public StoreApprovalStatus ApprovalStatus { get; set; }
    public string? ApprovalNotes { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    /// <summary>When set, a one-time 24h pending-approval reminder was sent to admins.</summary>
    public DateTime? PendingApprovalReminderSentAtUtc { get; set; }

    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
