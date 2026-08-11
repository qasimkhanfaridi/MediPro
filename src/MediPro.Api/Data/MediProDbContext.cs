using MediPro.Api.Domain;
using MediPro.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediPro.Api.Data;

public class MediProDbContext(DbContextOptions<MediProDbContext> options) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartLine> CartLines => Set<CartLine>();
    public DbSet<AdminNotification> AdminNotifications => Set<AdminNotification>();
    public DbSet<BonusScheme> BonusSchemes => Set<BonusScheme>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(256).IsRequired();
        });

        modelBuilder.Entity<AppUser>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Email).HasMaxLength(320).IsRequired();
            e.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
            e.HasOne(x => x.Tenant).WithMany(x => x.Users).HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Store).WithMany(x => x.Users).HasForeignKey(x => x.StoreId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Store>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.BusinessName).HasMaxLength(256).IsRequired();
            e.Property(x => x.AddressLine).HasMaxLength(512).IsRequired();
            e.Property(x => x.City).HasMaxLength(128).IsRequired();
            e.Property(x => x.Area).HasMaxLength(128).IsRequired();
            e.Property(x => x.ContactName).HasMaxLength(256).IsRequired();
            e.Property(x => x.Mobile).HasMaxLength(32).IsRequired();
            e.HasOne(x => x.Tenant).WithMany(x => x.Stores).HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.SkuCode).HasMaxLength(64).IsRequired();
            e.Property(x => x.Name).HasMaxLength(512).IsRequired();
            e.Property(x => x.Pack).HasMaxLength(64).IsRequired();
            e.Property(x => x.Manufacturer).HasMaxLength(256).IsRequired();
            e.Property(x => x.SaltComposition).HasMaxLength(512).IsRequired();
            e.Property(x => x.Category).HasMaxLength(128);
            e.Property(x => x.ImageUrl).HasMaxLength(1024);
            e.Property(x => x.TradePrice).HasPrecision(18, 4);
            e.Property(x => x.Mrp).HasPrecision(18, 4);
            e.HasIndex(x => new { x.TenantId, x.SkuCode }).IsUnique();
            e.HasOne(x => x.Tenant).WithMany(x => x.Products).HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.TotalAmount).HasPrecision(18, 4);
            e.Property(x => x.Currency).HasMaxLength(8).IsRequired();
            e.HasOne(x => x.Tenant).WithMany(x => x.Orders).HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Store).WithMany(x => x.Orders).HasForeignKey(x => x.StoreId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.SubmittedByUser).WithMany().HasForeignKey(x => x.SubmittedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderLine>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ProductNameSnapshot).HasMaxLength(512).IsRequired();
            e.Property(x => x.PackSnapshot).HasMaxLength(64).IsRequired();
            e.Property(x => x.UnitPriceSnapshot).HasPrecision(18, 4);
            e.Property(x => x.LineTotal).HasPrecision(18, 4);
            e.HasOne(x => x.Order).WithMany(x => x.Lines).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Product).WithMany(x => x.OrderLines).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cart>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.UserId).IsUnique();
            e.HasOne(x => x.Tenant).WithMany(x => x.Carts).HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.User).WithOne(x => x.Cart).HasForeignKey<Cart>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CartLine>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.CartId, x.ProductId }).IsUnique();
            e.HasOne(x => x.Cart).WithMany(x => x.Lines).HasForeignKey(x => x.CartId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Product).WithMany(x => x.CartLines).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AdminNotification>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(256).IsRequired();
            e.Property(x => x.Body).HasMaxLength(2048).IsRequired();
            e.HasIndex(x => new { x.RecipientUserId, x.CreatedAtUtc });
            e.HasOne(x => x.Tenant).WithMany(x => x.AdminNotifications).HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.RecipientUser).WithMany(x => x.AdminNotifications).HasForeignKey(x => x.RecipientUserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BonusScheme>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(256).IsRequired();
            e.Property(x => x.Manufacturer).HasMaxLength(256);
            e.Property(x => x.BannerText).HasMaxLength(512);
            e.HasIndex(x => new { x.TenantId, x.IsActive });
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.SetNull);
        });
    }
}
