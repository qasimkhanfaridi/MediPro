using ClosedXML.Excel;
using MediPro.Api.Configuration;
using MediPro.Api.Contracts;
using MediPro.Api.Data;
using MediPro.Api.Domain;
using MediPro.Api.Extensions;
using MediPro.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MediPro.Api.Controllers;

[ApiController]
[Route("api/admin/catalog")]
[Authorize(Roles = nameof(UserRole.DistributorAdmin))]
[RequestSizeLimit(12 * 1024 * 1024)]
public class AdminCatalogController(
    MediProDbContext db,
    ProductExcelImporter importer,
    ProductDtoMapper productMapper,
    IHostEnvironment env,
    IOptions<DevSeedOptions> devSeed) : ControllerBase
{
    [HttpGet("import-template")]
    [AllowAnonymous]
    public IActionResult DownloadImportTemplate()
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Products");
        var headers = new[]
        {
            "SkuCode", "Name", "Pack", "Manufacturer", "SaltComposition", "TradePrice", "Mrp", "StockQuantity",
            "IsActive", "Category", "ImageUrl",
        };
        for (var i = 0; i < headers.Length; i++)
            ws.Cell(1, i + 1).Value = headers[i];

        ws.Cell(2, 1).Value = "SAMPLE-001";
        ws.Cell(2, 2).Value = "Sample tablet";
        ws.Cell(2, 3).Value = "10's";
        ws.Cell(2, 4).Value = "Sample Pharma";
        ws.Cell(2, 5).Value = "Sample salt";
        ws.Cell(2, 6).Value = 100m;
        ws.Cell(2, 7).Value = 120m;
        ws.Cell(2, 8).Value = 50;
        ws.Cell(2, 9).Value = true;
        ws.Cell(2, 10).Value = "Tablet";
        ws.Cell(2, 11).Value = "https://picsum.photos/seed/sample-sku/400/280";

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return File(
            ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "MediPro_Product_Import_Template.xlsx");
    }

    [HttpPost("import")]
    public async Task<ActionResult<ImportProductsResultDto>> ImportProducts(IFormFile file, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        if (tenantId is null)
            return Unauthorized();

        if (file.Length == 0)
            return BadRequest(new { message = "Empty file." });

        var ext = Path.GetExtension(file.FileName);
        if (!string.Equals(ext, ".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Only .xlsx files are supported." });

        await using var stream = file.OpenReadStream();
        var result = await importer.ImportAsync(tenantId.Value, stream, ct);
        return Ok(result);
    }

    /// <summary>Adjust on-hand quantity for a SKU (tenant-scoped). Null stock is treated as zero before applying delta.</summary>
    [HttpPost("stock-adjustment")]
    public async Task<ActionResult<ProductDto>> AdjustStock([FromBody] AdjustStockRequest request, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        if (tenantId is null)
            return Unauthorized();

        var sku = request.SkuCode.Trim();
        if (string.IsNullOrEmpty(sku))
            return BadRequest(new { message = "SkuCode is required." });

        var product = await db.Products.FirstOrDefaultAsync(
            p => p.TenantId == tenantId && p.SkuCode == sku,
            ct);

        if (product is null)
            return NotFound(new { message = $"No product with SKU '{sku}' in your catalogue." });

        var cur = (long)(product.StockQuantity ?? 0);
        var next = cur + request.Delta;
        if (next < 0)
            return BadRequest(new
            {
                message = $"Stock cannot be negative. Current: {cur}, delta: {request.Delta}.",
            });

        if (next > int.MaxValue)
            return BadRequest(new { message = "Resulting quantity is too large." });

        product.StockQuantity = (int)next;
        product.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return Ok(productMapper.Map(product, isAdmin: true));
    }

    /// <summary>Set catalogue availability: in stock (null qty) or out of stock (0).</summary>
    [HttpPost("stock-status")]
    public async Task<ActionResult<ProductDto>> SetStockStatus([FromBody] SetStockStatusRequest request, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        if (tenantId is null)
            return Unauthorized();

        var sku = request.SkuCode.Trim();
        if (string.IsNullOrEmpty(sku))
            return BadRequest(new { message = "SkuCode is required." });

        var product = await db.Products.FirstOrDefaultAsync(
            p => p.TenantId == tenantId && p.SkuCode == sku,
            ct);

        if (product is null)
            return NotFound(new { message = $"No product with SKU '{sku}' in your catalogue." });

        product.StockQuantity = request.InStock ? null : 0;
        product.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return Ok(productMapper.Map(product, isAdmin: true));
    }

    /// <summary>Active SKUs with tracked stock at or below the threshold (default 20).</summary>
    [HttpGet("low-stock")]
    public async Task<ActionResult<LowStockListDto>> LowStock(
        [FromQuery] int threshold = 20,
        [FromQuery] int max = 50,
        CancellationToken ct = default)
    {
        var tenantId = User.GetTenantId();
        if (tenantId is null)
            return Unauthorized();

        threshold = Math.Clamp(threshold, 0, 1_000_000);
        max = Math.Clamp(max, 1, 200);

        var filtered = db.Products.AsNoTracking()
            .Where(p =>
                p.TenantId == tenantId
                && p.IsActive
                && p.StockQuantity != null
                && p.StockQuantity <= threshold);

        var totalMatching = await filtered.CountAsync(ct);
        var items = await filtered
            .OrderBy(p => p.StockQuantity)
            .ThenBy(p => p.Name)
            .Take(max)
            .Select(p => new LowStockItemDto
            {
                SkuCode = p.SkuCode,
                Name = p.Name,
                StockQuantity = p.StockQuantity!.Value,
            })
            .ToListAsync(ct);

        return Ok(new LowStockListDto
        {
            Threshold = threshold,
            TotalMatching = totalMatching,
            Items = items,
        });
    }

    /// <summary>Insert TEST-MED-* demo products (with stock and placeholder images). Development or DevSeed:AllowDemoCatalogEndpoint.</summary>
    [HttpPost("seed-demo-catalog")]
    public async Task<ActionResult<object>> SeedDemoCatalog(CancellationToken ct)
    {
        var opts = devSeed.Value;
        if (!env.IsDevelopment() && !opts.AllowDemoCatalogEndpoint)
            return NotFound();

        var tenantId = User.GetTenantId();
        if (tenantId is null)
            return Unauthorized();

        var inserted = await DemoCatalogSeeder.InsertMissingDemoProductsAsync(db, tenantId.Value, ct);
        return Ok(new { inserted, message = inserted == 0 ? "Catalog already contained all demo SKUs." : $"Inserted {inserted} demo products." });
    }
}
