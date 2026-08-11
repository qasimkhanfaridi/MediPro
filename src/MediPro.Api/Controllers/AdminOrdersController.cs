using MediPro.Api.Configuration;
using MediPro.Api.Contracts;
using MediPro.Api.Data;
using MediPro.Api.Domain;
using MediPro.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MediPro.Api.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = nameof(UserRole.DistributorAdmin))]
public class AdminOrdersController(
    MediProDbContext db,
    IHostEnvironment env,
    IOptions<DevSeedOptions> devSeed) : ControllerBase
{
    /// <summary>Cities and areas for filter dropdowns (from stores + reference list).</summary>
    [HttpGet("location-options")]
    public async Task<ActionResult<OrderLocationOptionsDto>> LocationOptions(CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        if (tenantId is null)
            return Unauthorized();

        var fromDb = await db.Stores.AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .Select(s => new { s.City, s.Area })
            .ToListAsync(ct);

        var areasByCity = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var (city, areas) in DemoStoreLocations.AreasByCity)
        {
            if (!areasByCity.ContainsKey(city))
                areasByCity[city] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var a in areas)
                areasByCity[city].Add(a);
        }

        foreach (var row in fromDb)
        {
            if (string.IsNullOrWhiteSpace(row.City))
                continue;
            if (!areasByCity.TryGetValue(row.City.Trim(), out var set))
            {
                set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                areasByCity[row.City.Trim()] = set;
            }
            if (!string.IsNullOrWhiteSpace(row.Area))
                set.Add(row.Area.Trim());
        }

        var cities = areasByCity.Keys.OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
        var dict = cities.ToDictionary(
            c => c,
            c => (IReadOnlyList<string>)areasByCity[c].OrderBy(a => a, StringComparer.OrdinalIgnoreCase).ToList(),
            StringComparer.OrdinalIgnoreCase);

        return Ok(new OrderLocationOptionsDto
        {
            Cities = cities,
            AreasByCity = dict,
        });
    }

    /// <summary>Backfill store areas and insert demo orders (Development or AllowDemoCatalogEndpoint).</summary>
    [HttpPost("seed-demo")]
    public async Task<ActionResult<object>> SeedDemoOrders(CancellationToken ct)
    {
        var opts = devSeed.Value;
        if (!env.IsDevelopment() && !opts.AllowDemoCatalogEndpoint)
            return NotFound();

        var tenantId = User.GetTenantId();
        if (tenantId is null)
            return Unauthorized();

        var areas = await DemoOrdersSeeder.BackfillStoreAreasAsync(db, tenantId.Value, ct);
        var inserted = await DemoOrdersSeeder.InsertDemoOrdersAsync(db, tenantId.Value, ct);

        return Ok(new
        {
            areasBackfilled = areas,
            inserted,
            message = inserted == 0
                ? "Demo orders already exist or no products/stores available."
                : $"Created {inserted} demo orders across cities/areas.",
        });
    }
}
