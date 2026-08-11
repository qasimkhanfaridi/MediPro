using MediPro.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediPro.Api.Data;

public static class DemoCatalogSeeder
{
    public static async Task<int> InsertMissingDemoProductsAsync(
        MediProDbContext db,
        Guid tenantId,
        CancellationToken ct = default)
    {
        var existing = await db.Products.AsNoTracking()
            .Where(p => p.TenantId == tenantId)
            .Select(p => p.SkuCode)
            .ToListAsync(ct);
        var set = new HashSet<string>(existing, StringComparer.OrdinalIgnoreCase);
        var now = DateTime.UtcNow;
        var toAdd = DemoCatalogSeed.BuildProducts(tenantId, now)
            .Where(p => !set.Contains(p.SkuCode))
            .ToList();
        if (toAdd.Count == 0)
            return 0;

        db.Products.AddRange(toAdd);
        await db.SaveChangesAsync(ct);
        return toAdd.Count;
    }
}
