namespace MediPro.Api.Entities;

public class Cart
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public DateTime UpdatedAtUtc { get; set; }

    public ICollection<CartLine> Lines { get; set; } = new List<CartLine>();
}
