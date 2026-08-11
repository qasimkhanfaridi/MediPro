namespace MediPro.Api.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; }

    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
    public ICollection<Store> Stores { get; set; } = new List<Store>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Cart> Carts { get; set; } = new List<Cart>();
    public ICollection<AdminNotification> AdminNotifications { get; set; } = new List<AdminNotification>();
}
