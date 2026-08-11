using MediPro.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediPro.Api.Data;

public static class DemoBonusSchemesSeeder
{
  public static async Task<int> SeedIfEmptyAsync(MediProDbContext db, Guid tenantId, CancellationToken ct = default)
  {
    if (await db.BonusSchemes.AnyAsync(s => s.TenantId == tenantId, ct))
      return 0;

    var products = await db.Products.AsNoTracking()
      .Where(p => p.TenantId == tenantId && p.IsActive)
      .OrderBy(p => p.SkuCode)
      .Take(3)
      .ToListAsync(ct);

    if (products.Count == 0)
      return 0;

    var now = DateTime.UtcNow;
    var created = 0;
    var offers = new (int buy, int bonus)[]
    {
      (10, 1),
      (5, 1),
      (20, 2),
    };

    for (var i = 0; i < products.Count && i < offers.Length; i++)
    {
      var product = products[i];
      var (buy, bonus) = offers[i];
      var label = $"{buy}+{bonus}";
      db.BonusSchemes.Add(new BonusScheme
      {
        Id = Guid.NewGuid(),
        TenantId = tenantId,
        Title = $"{product.Name} {label} promo",
        ProductId = product.Id,
        Manufacturer = product.Manufacturer,
        BuyQuantity = buy,
        BonusQuantity = bonus,
        BannerText = $"{product.Name}: {label} — get {bonus} free on this product",
        IsActive = true,
        SortOrder = i + 1,
        CreatedAtUtc = now,
      });
      created++;
    }

    if (created > 0)
      await db.SaveChangesAsync(ct);

    return created;
  }
}
