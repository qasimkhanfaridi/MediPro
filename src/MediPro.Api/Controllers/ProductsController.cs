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
[Route("api/[controller]")]
[Authorize]
public class ProductsController(MediProDbContext db, ProductDtoMapper productMapper) : ControllerBase
{
    private static string? NormalizeCatalogFilter(string? value, int maxLen)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        var t = value.Trim();
        return t.Length > maxLen ? t[..maxLen] : t;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> List(
        [FromQuery] string? search,
        [FromQuery] string? manufacturer,
        [FromQuery] string? category,
        [FromQuery] string? salt,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var tenantId = User.GetTenantId();
        if (tenantId is null)
            return Unauthorized();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var isAdmin = User.IsInRole(nameof(UserRole.DistributorAdmin));
        if (!isAdmin)
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
                return Problem(statusCode: StatusCodes.Status403Forbidden, detail: "Store must be approved to browse the catalog.");
        }

        var query = db.Products.AsNoTracking().Where(p => p.TenantId == tenantId);
        if (!isAdmin)
            query = query.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            // Lower both sides: SQLite translates string.Contains to instr(), which is case-sensitive.
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term)
                || p.Manufacturer.ToLower().Contains(term)
                || p.SaltComposition.ToLower().Contains(term)
                || p.SkuCode.ToLower().Contains(term));
        }

        var m = NormalizeCatalogFilter(manufacturer, 256);
        if (m is not null)
            query = query.Where(p => p.Manufacturer == m);

        var cat = NormalizeCatalogFilter(category, 128);
        if (cat is not null)
            query = query.Where(p => p.Category != null && p.Category == cat);

        var saltTerm = NormalizeCatalogFilter(salt, 512)?.ToLowerInvariant();
        if (saltTerm is not null)
            query = query.Where(p => p.SaltComposition.ToLower().Contains(saltTerm));

        var total = await query.CountAsync(ct);
        var products = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var schemes = await LoadActiveSchemesAsync(tenantId.Value, ct);
        var items = products.Select(p =>
        {
            var dto = productMapper.Map(p, isAdmin);
            EnrichWithBonus(dto, p, schemes);
            return dto;
        }).ToList();

        return Ok(new PagedResult<ProductDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            Items = items,
        });
    }

    /// <summary>
    /// Distinct manufacturers and categories for filter dropdowns. Absolute template so routing
    /// cannot merge this with <c>{id:guid}</c>.
    /// </summary>
    [HttpGet("~/api/products/filters")]
    public async Task<ActionResult<CatalogFilterOptionsDto>> ListCatalogFilters(CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        if (tenantId is null)
            return Unauthorized();

        var isAdmin = User.IsInRole(nameof(UserRole.DistributorAdmin));
        if (!isAdmin)
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
                    detail: "Store must be approved to browse the catalog.");
            }
        }

        var baseQuery = db.Products.AsNoTracking().Where(p => p.TenantId == tenantId);
        if (!isAdmin)
            baseQuery = baseQuery.Where(p => p.IsActive);

        var manufacturers = await baseQuery
            .Select(p => p.Manufacturer)
            .Distinct()
            .OrderBy(x => x)
            .Take(500)
            .ToListAsync(ct);

        var categories = await baseQuery
            .Where(p => p.Category != null && p.Category != "")
            .Select(p => p.Category!)
            .Distinct()
            .OrderBy(x => x)
            .Take(200)
            .ToListAsync(ct);

        return Ok(new CatalogFilterOptionsDto
        {
            Manufacturers = manufacturers,
            Categories = categories,
        });
    }

    [HttpGet("{id:guid}", Order = 1)]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        if (tenantId is null)
            return Unauthorized();

        var isAdmin = User.IsInRole(nameof(UserRole.DistributorAdmin));
        if (!isAdmin)
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
                    detail: "Store must be approved to view product details.");
            }
        }

        var query = db.Products.AsNoTracking()
            .Where(p => p.Id == id && p.TenantId == tenantId);
        if (!isAdmin)
            query = query.Where(p => p.IsActive);

        var product = await query.FirstOrDefaultAsync(ct);

        if (product is null)
            return NotFound(new { message = "Product not found." });

        var schemes = await LoadActiveSchemesAsync(tenantId.Value, ct);
        var dto = productMapper.Map(product, isAdmin);
        EnrichWithBonus(dto, product, schemes);

        return Ok(dto);
    }

    private static void EnrichWithBonus(ProductDto dto, Product product, IReadOnlyList<BonusScheme> schemes) =>
        BonusSchemeResolver.ApplyToProduct(dto, product, schemes);

    private async Task<List<BonusScheme>> LoadActiveSchemesAsync(Guid tenantId, CancellationToken ct)
    {
        var schemes = await db.BonusSchemes.AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .ToListAsync(ct);
        return BonusSchemeResolver.FilterActive(schemes, DateTime.UtcNow).ToList();
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.DistributorAdmin))]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        if (tenantId is null)
            return Unauthorized();

        var sku = request.SkuCode.Trim();
        if (await db.Products.AnyAsync(p => p.TenantId == tenantId && p.SkuCode == sku, ct))
            return Conflict(new { message = "SKU already exists for this tenant." });

        var now = DateTime.UtcNow;
        var product = new Product
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            SkuCode = sku,
            Name = request.Name.Trim(),
            Pack = request.Pack.Trim(),
            Manufacturer = request.Manufacturer.Trim(),
            SaltComposition = request.SaltComposition.Trim(),
            Category = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim(),
            TradePrice = request.TradePrice,
            Mrp = request.Mrp,
            IsActive = true,
            StockQuantity = request.StockQuantity ?? null,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        db.Products.Add(product);
        await db.SaveChangesAsync(ct);

        return Created($"/api/products/{product.Id}", productMapper.Map(product, isAdmin: true));
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.DistributorAdmin))]
    public async Task<ActionResult<ProductDto>> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        if (tenantId is null)
            return Unauthorized();

        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, ct);
        if (product is null)
            return NotFound();

        if (!string.IsNullOrWhiteSpace(request.Name))
            product.Name = request.Name.Trim();
        if (!string.IsNullOrWhiteSpace(request.Pack))
            product.Pack = request.Pack.Trim();
        if (!string.IsNullOrWhiteSpace(request.Manufacturer))
            product.Manufacturer = request.Manufacturer.Trim();
        if (!string.IsNullOrWhiteSpace(request.SaltComposition))
            product.SaltComposition = request.SaltComposition.Trim();
        if (request.Category is not null)
            product.Category = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim();
        if (request.TradePrice is { } tp)
            product.TradePrice = tp;
        if (request.Mrp is not null)
            product.Mrp = request.Mrp;
        if (request.IsActive is { } active)
            product.IsActive = active;
        if (request.StockQuantity is not null)
            product.StockQuantity = request.StockQuantity;

        product.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return Ok(productMapper.Map(product, isAdmin: true));
    }

    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Roles = nameof(UserRole.DistributorAdmin))]
    public async Task<ActionResult<ProductDto>> Deactivate(Guid id, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        if (tenantId is null)
            return Unauthorized();

        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, ct);
        if (product is null)
            return NotFound();

        product.IsActive = false;
        product.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return Ok(productMapper.Map(product, isAdmin: true));
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize(Roles = nameof(UserRole.DistributorAdmin))]
    public async Task<ActionResult<ProductDto>> Activate(Guid id, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        if (tenantId is null)
            return Unauthorized();

        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, ct);
        if (product is null)
            return NotFound();

        product.IsActive = true;
        product.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return Ok(productMapper.Map(product, isAdmin: true));
    }
}
