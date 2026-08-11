using MediPro.Api.Domain;
using MediPro.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediPro.Api.Data;

public static class DemoOrdersSeeder
{
    public const string DemoOrderNotesMarker = "__DEMO_ORDER_SEED__";

    /// <summary>Backfill Area on demo stores created before Area column existed.</summary>
    public static async Task<int> BackfillStoreAreasAsync(MediProDbContext db, Guid tenantId, CancellationToken ct = default)
    {
        var map = new Dictionary<string, (string City, string Area)>(StringComparer.OrdinalIgnoreCase)
        {
            ["store.approved1"] = ("Islamabad", "F-7 Markaz"),
            ["store.approved2"] = ("Rawalpindi", "Saddar"),
            ["store.approved3"] = ("Lahore", "Gulberg III"),
            ["store.approved4"] = ("Karachi", "Saddar"),
            ["store.approved5"] = ("Rawalpindi", "Moti Mehel"),
            ["store.approved6"] = ("Rawalpindi", "Raja Bazaar"),
            ["store.approved7"] = ("Rawalpindi", "Bahria Town Phase 7"),
            ["store.approved8"] = ("Islamabad", "Blue Area"),
            ["store.pending1"] = ("Islamabad", "G-9"),
            ["store.pending2"] = ("Faisalabad", "Susan Road"),
            ["store.rejected1"] = ("Lahore", "Model Town"),
        };

        var users = await db.Users.AsNoTracking()
            .Where(u => u.TenantId == tenantId && u.StoreId != null && u.Email.EndsWith(DemoStoresSeeder.DemoEmailDomain))
            .Select(u => new { u.Email, u.StoreId })
            .ToListAsync(ct);

        var updated = 0;
        foreach (var u in users)
        {
            var local = u.Email[..u.Email.IndexOf('@')];
            if (!map.TryGetValue(local, out var loc))
                continue;

            var store = await db.Stores.FirstOrDefaultAsync(s => s.Id == u.StoreId && s.TenantId == tenantId, ct);
            if (store is null)
                continue;

            var changed = false;
            if (string.IsNullOrWhiteSpace(store.Area))
            {
                store.Area = loc.Area;
                changed = true;
            }
            if (string.Equals(store.City, loc.City, StringComparison.OrdinalIgnoreCase) == false && store.City.Length < 3)
            {
                store.City = loc.City;
                changed = true;
            }

            if (changed)
                updated++;
        }

        if (updated > 0)
            await db.SaveChangesAsync(ct);

        return updated;
    }

    /// <returns>Number of demo orders inserted.</returns>
    public static async Task<int> InsertDemoOrdersAsync(MediProDbContext db, Guid tenantId, CancellationToken ct = default)
    {
        await BackfillStoreAreasAsync(db, tenantId, ct);

        if (await db.Orders.AnyAsync(o => o.TenantId == tenantId && o.Notes == DemoOrderNotesMarker, ct))
            return 0;

        var stores = await db.Stores.AsNoTracking()
            .Where(s => s.TenantId == tenantId && s.ApprovalStatus == StoreApprovalStatus.Approved)
            .OrderBy(s => s.City)
            .ThenBy(s => s.Area)
            .ToListAsync(ct);

        if (stores.Count == 0)
            return 0;

        var storeUsers = await db.Users.AsNoTracking()
            .Where(u => u.TenantId == tenantId && u.StoreId != null && u.Role == UserRole.StoreUser)
            .ToDictionaryAsync(u => u.StoreId!.Value, u => u.Id, ct);

        var products = await db.Products.AsNoTracking()
            .Where(p => p.TenantId == tenantId && p.IsActive)
            .OrderBy(p => p.SkuCode)
            .Take(40)
            .ToListAsync(ct);

        if (products.Count == 0)
            return 0;

        var rng = new Random(4242);
        var statuses = new[]
        {
            OrderStatus.Submitted, OrderStatus.Confirmed, OrderStatus.Processing,
            OrderStatus.Dispatched, OrderStatus.Delivered, OrderStatus.OnHold,
        };

        var created = 0;
        var now = DateTime.UtcNow;

        // Spread ~24 orders across cities/areas and last 14 days
        for (var i = 0; i < 24; i++)
        {
            var store = stores[i % stores.Count];
            if (!storeUsers.TryGetValue(store.Id, out var userId))
                continue;

            var lineCount = rng.Next(2, 6);
            var picked = products.OrderBy(_ => rng.Next()).Take(lineCount).ToList();
            var orderId = Guid.NewGuid();
            decimal total = 0;
            var lines = new List<OrderLine>();

            foreach (var p in picked)
            {
                var qty = rng.Next(1, 12);
                var unit = p.TradePrice;
                var lineTotal = unit * qty;
                total += lineTotal;
                lines.Add(new OrderLine
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = p.Id,
                    ProductNameSnapshot = p.Name,
                    PackSnapshot = p.Pack,
                    UnitPriceSnapshot = unit,
                    Quantity = qty,
                    LineTotal = lineTotal,
                });
            }

            var daysAgo = rng.Next(0, 14);
            var submitted = now.AddDays(-daysAgo).AddHours(-rng.Next(0, 20));

            db.Orders.Add(new Order
            {
                Id = orderId,
                TenantId = tenantId,
                StoreId = store.Id,
                SubmittedByUserId = userId,
                Status = statuses[rng.Next(statuses.Length)],
                Currency = "PKR",
                SubmittedAtUtc = submitted,
                Notes = DemoOrderNotesMarker,
                TotalAmount = total,
                Lines = lines,
            });
            created++;
        }

        if (created > 0)
            await db.SaveChangesAsync(ct);

        return created;
    }
}
