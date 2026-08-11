using MediPro.Api.Contracts;
using MediPro.Api.Data;
using MediPro.Api.Domain;
using MediPro.Api.Entities;
using MediPro.Api.Extensions;
using MediPro.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediPro.Api.Controllers;

[ApiController]
[Route("api/admin/bonus-schemes")]
[Authorize(Roles = nameof(UserRole.DistributorAdmin))]
public class AdminBonusSchemesController(MediProDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BonusSchemeDto>>> List(CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        if (tenantId is null)
            return Unauthorized();

        var items = await db.BonusSchemes.AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Title)
            .Select(s => new
            {
                Scheme = s,
                ProductName = s.ProductId != null
                    ? db.Products.Where(p => p.Id == s.ProductId).Select(p => p.Name).FirstOrDefault()
                    : null,
            })
            .ToListAsync(ct);

        return Ok(items.Select(x => ToDto(x.Scheme, x.ProductName)).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<BonusSchemeDto>> Create([FromBody] CreateBonusSchemeRequest request, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        if (tenantId is null)
            return Unauthorized();

        var validation = await ValidateRequestAsync(tenantId.Value, request, ct);
        if (validation.Error is not null)
            return validation.Error;

        var scheme = new BonusScheme
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            Title = request.Title.Trim(),
            Manufacturer = validation.Manufacturer,
            ProductId = request.ProductId,
            BuyQuantity = request.BuyQuantity,
            BonusQuantity = request.BonusQuantity,
            BannerText = string.IsNullOrWhiteSpace(request.BannerText) ? null : request.BannerText.Trim(),
            IsActive = request.IsActive,
            SortOrder = request.SortOrder,
            ValidFromUtc = request.ValidFromUtc,
            ValidToUtc = request.ValidToUtc,
            CreatedAtUtc = DateTime.UtcNow,
        };

        db.BonusSchemes.Add(scheme);
        await db.SaveChangesAsync(ct);

        return Ok(ToDto(scheme, validation.ProductName));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BonusSchemeDto>> Update(Guid id, [FromBody] UpdateBonusSchemeRequest request, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        if (tenantId is null)
            return Unauthorized();

        var scheme = await db.BonusSchemes.FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId, ct);
        if (scheme is null)
            return NotFound();

        var validation = await ValidateRequestAsync(tenantId.Value, request, ct);
        if (validation.Error is not null)
            return validation.Error;

        scheme.Title = request.Title.Trim();
        scheme.Manufacturer = validation.Manufacturer;
        scheme.ProductId = request.ProductId;
        scheme.BuyQuantity = request.BuyQuantity;
        scheme.BonusQuantity = request.BonusQuantity;
        scheme.BannerText = string.IsNullOrWhiteSpace(request.BannerText) ? null : request.BannerText.Trim();
        scheme.IsActive = request.IsActive;
        scheme.SortOrder = request.SortOrder;
        scheme.ValidFromUtc = request.ValidFromUtc;
        scheme.ValidToUtc = request.ValidToUtc;

        await db.SaveChangesAsync(ct);
        return Ok(ToDto(scheme, validation.ProductName));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        if (tenantId is null)
            return Unauthorized();

        var scheme = await db.BonusSchemes.FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId, ct);
        if (scheme is null)
            return NotFound();

        db.BonusSchemes.Remove(scheme);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("seed-demo")]
    public async Task<ActionResult<object>> SeedDemo(CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        if (tenantId is null)
            return Unauthorized();

        var inserted = await DemoBonusSchemesSeeder.SeedIfEmptyAsync(db, tenantId.Value, ct);
        return Ok(new
        {
            inserted,
            message = inserted == 0
                ? "Demo bonus schemes already exist or no products available."
                : $"Created {inserted} demo bonus scheme(s) (10+1, 5+1, 20+2 examples).",
        });
    }

    private static BonusSchemeDto ToDto(BonusScheme scheme, string? productName) =>
        new()
        {
            Id = scheme.Id,
            Title = scheme.Title,
            Label = BonusSchemeResolver.FormatLabel(scheme.BuyQuantity, scheme.BonusQuantity),
            Manufacturer = scheme.Manufacturer,
            ProductId = scheme.ProductId,
            ProductName = productName,
            BuyQuantity = scheme.BuyQuantity,
            BonusQuantity = scheme.BonusQuantity,
            BannerText = scheme.BannerText,
            IsActive = scheme.IsActive,
            SortOrder = scheme.SortOrder,
            ValidFromUtc = scheme.ValidFromUtc,
            ValidToUtc = scheme.ValidToUtc,
            CreatedAtUtc = scheme.CreatedAtUtc,
        };

    private async Task<(ActionResult? Error, string? Manufacturer, string? ProductName)> ValidateRequestAsync(
        Guid tenantId,
        CreateBonusSchemeRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return (BadRequest(new { message = "Title is required." }), null, null);

        if (request.BuyQuantity < 1 || request.BonusQuantity < 1)
            return (BadRequest(new { message = "Buy and bonus quantities must be at least 1." }), null, null);

        if (request.ProductId is not Guid productId)
            return (BadRequest(new { message = "Select a product — bonuses apply to one SKU only, not an entire company." }), null, null);

        var product = await db.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId && p.TenantId == tenantId, ct);
        if (product is null)
            return (BadRequest(new { message = "Product not found." }), null, null);

        return (null, product.Manufacturer, product.Name);
    }
}
