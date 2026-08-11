using MediPro.Api.Domain;

namespace MediPro.Api.Entities;

public class Order
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public Guid StoreId { get; set; }
    public Store Store { get; set; } = null!;

    public Guid SubmittedByUserId { get; set; }
    public AppUser SubmittedByUser { get; set; } = null!;

    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "PKR";

    public DateTime SubmittedAtUtc { get; set; }
    public string? Notes { get; set; }
    public string? StatusNotes { get; set; }

    public ICollection<OrderLine> Lines { get; set; } = new List<OrderLine>();
}
