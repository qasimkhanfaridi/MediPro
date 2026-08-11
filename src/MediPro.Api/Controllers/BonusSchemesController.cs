using MediPro.Api.Contracts;
using MediPro.Api.Data;
using MediPro.Api.Domain;
using MediPro.Api.Extensions;
using MediPro.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediPro.Api.Controllers;

[ApiController]
[Route("api/bonus-schemes")]
[Authorize]
public class BonusSchemesController(MediProDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BonusSchemeSummaryDto>>> ListActive(CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        if (tenantId is null)
            return Unauthorized();

        if (!User.IsInRole(nameof(UserRole.DistributorAdmin)))
        {
            if (!User.IsInRole(nameof(UserRole.StoreUser)))
                return Forbid();

            var storeId = User.GetStoreId();
            if (storeId is null)
                return Forbid();

            var approval = await db.Stores.AsNoTracking()
                .Where(s => s.Id == storeId && s.TenantId == tenantId)
                .Select(s => s.ApprovalStatus)
                .FirstOrDefaultAsync(ct);

            if (approval != StoreApprovalStatus.Approved)
            {
                return Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    detail: "Store must be approved to view bonus offers.");
            }
        }

        var now = DateTime.UtcNow;
        var schemes = await db.BonusSchemes.AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Title)
            .ToListAsync(ct);

        var active = BonusSchemeResolver.FilterActive(schemes, now);
        if (active.Count == 0)
            return Ok(Array.Empty<BonusSchemeSummaryDto>());

        var productIds = active.Where(s => s.ProductId != null).Select(s => s.ProductId!.Value).Distinct().ToList();
        var products = await db.Products.AsNoTracking()
            .Where(p => p.TenantId == tenantId && productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        var summaries = active
            .Select(s =>
            {
                products.TryGetValue(s.ProductId ?? Guid.Empty, out var product);
                var resolvedProduct = s.ProductId is not null ? product : null;
                return BonusSchemeResolver.ToSummary(s, resolvedProduct);
            })
            .ToList();

        return Ok(summaries);
    }
}
