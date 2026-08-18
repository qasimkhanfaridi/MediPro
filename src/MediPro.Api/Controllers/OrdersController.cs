using MediPro.Api.Contracts;
using MediPro.Api.Configuration;
using MediPro.Api.Data;
using MediPro.Api.Domain;
using MediPro.Api.Entities;
using MediPro.Api.Extensions;
using MediPro.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MediPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController(
    MediProDbContext db,
    IAdminAlertService adminAlerts,
    IOptions<InventoryOptions> inventory) : ControllerBase
{
    private readonly InventoryOptions _inventory = inventory.Value;
    [HttpPost("submit")]
    public async Task<ActionResult<OrderDetailDto>> Submit([FromBody] SubmitOrderRequest? request, CancellationToken ct)
    {
        var guard = await EnsureStoreApprovedAsync(ct);
        if (guard != null)
            return guard;

        var tenantId = User.GetTenantId()!.Value;
        var userId = User.GetUserId()!.Value;
        var storeId = User.GetStoreId()!.Value;

        var cart = await db.Carts
            .Include(c => c.Lines)
            .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);

        if (cart is null || cart.Lines.Count == 0)
            return BadRequest(new { message = "Cart is empty." });

        foreach (var line in cart.Lines)
        {
            if (!line.Product.IsActive)
                return BadRequest(new { message = $"Product {line.Product.SkuCode} is no longer available." });

            if (StockAvailability.IsOutOfStock(line.Product.StockQuantity))
                return BadRequest(new { message = $"{line.Product.SkuCode} is out of stock." });

            if (_inventory.EnforceStockQuantityCap
                && line.Product.StockQuantity is int stock
                && stock < line.Quantity)
            {
                return BadRequest(new
                {
                    message = $"Insufficient stock for {line.Product.SkuCode}. Available: {stock}, requested: {line.Quantity}.",
                });
            }
        }

        var now = DateTime.UtcNow;
        var order = new Order
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            StoreId = storeId,
            SubmittedByUserId = userId,
            Status = OrderStatus.Submitted,
            Currency = "PKR",
            SubmittedAtUtc = now,
            Notes = string.IsNullOrWhiteSpace(request?.Notes) ? null : request!.Notes!.Trim(),
            TotalAmount = 0,
        };

        var schemes = BonusSchemeResolver.FilterActive(
            await db.BonusSchemes.AsNoTracking()
                .Where(s => s.TenantId == tenantId)
                .ToListAsync(ct),
            now);

        decimal total = 0;
        foreach (var line in cart.Lines)
        {
            var unit = line.Product.TradePrice;
            var lineTotal = unit * line.Quantity;
            var scheme = BonusSchemeResolver.ResolveForProduct(schemes, line.Product);
            var bonusQuantity = scheme is null
                ? 0
                : (line.Quantity / scheme.BuyQuantity) * scheme.BonusQuantity;
            total += lineTotal;

            order.Lines.Add(new OrderLine
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = line.ProductId,
                ProductNameSnapshot = line.Product.Name,
                PackSnapshot = line.Product.Pack,
                UnitPriceSnapshot = unit,
                Quantity = line.Quantity,
                BonusLabelSnapshot = scheme is null
                    ? null
                    : BonusSchemeResolver.FormatLabel(scheme.BuyQuantity, scheme.BonusQuantity),
                BonusQuantitySnapshot = bonusQuantity,
                LineTotal = lineTotal,
            });

            if (_inventory.AutoDecrementOnOrder && line.Product.StockQuantity is int sq)
                line.Product.StockQuantity = sq - line.Quantity;
        }

        order.TotalAmount = total;
        db.Orders.Add(order);
        db.CartLines.RemoveRange(cart.Lines);
        cart.UpdatedAtUtc = now;

        await db.SaveChangesAsync(ct);

        var storeName = await db.Stores.AsNoTracking()
            .Where(s => s.Id == storeId)
            .Select(s => s.BusinessName)
            .FirstAsync(ct);

        await adminAlerts.NotifyNewOrderSubmittedAsync(tenantId, order.Id, storeName, order.TotalAmount, ct);

        return Created($"/api/orders/{order.Id}", await MapDetailAsync(order.Id, tenantId, ct));
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderSummaryDto>>> List(
        [FromQuery] Guid? storeId,
        [FromQuery] string? status,
        [FromQuery] string? city,
        [FromQuery] string? area,
        [FromQuery] string? search,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var tenantId = User.GetTenantId();
        if (tenantId is null)
            return Unauthorized();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query =
            from o in db.Orders.AsNoTracking()
            join s in db.Stores.AsNoTracking() on o.StoreId equals s.Id
            where o.TenantId == tenantId
            select new { o, s };

        if (User.IsInRole(nameof(UserRole.StoreUser)))
        {
            var sid = User.GetStoreId();
            if (sid is null)
                return Forbid();
            query = query.Where(x => x.o.StoreId == sid);
        }
        else if (!User.IsInRole(nameof(UserRole.DistributorAdmin)))
        {
            return Forbid();
        }
        else if (storeId is { } filterStore)
        {
            query = query.Where(x => x.o.StoreId == filterStore);
        }

        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<OrderStatus>(status.Trim(), ignoreCase: true, out var st))
        {
            query = query.Where(x => x.o.Status == st);
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            var c = city.Trim();
            query = query.Where(x => x.s.City == c);
        }

        if (!string.IsNullOrWhiteSpace(area))
        {
            var a = area.Trim();
            query = query.Where(x => x.s.Area == a);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.s.BusinessName.ToLower().Contains(term)
                || x.s.ContactName.ToLower().Contains(term)
                || x.s.Mobile.ToLower().Contains(term));
        }

        if (from is { } fromDt)
            query = query.Where(x => x.o.SubmittedAtUtc >= fromDt.Date);

        if (to is { } toDt)
        {
            var end = toDt.Date.AddDays(1).AddTicks(-1);
            query = query.Where(x => x.o.SubmittedAtUtc <= end);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.o.SubmittedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new OrderSummaryDto
            {
                Id = x.o.Id,
                StoreId = x.o.StoreId,
                StoreName = x.s.BusinessName,
                StoreCity = x.s.City,
                StoreArea = x.s.Area,
                StoreMobile = x.s.Mobile,
                Status = x.o.Status.ToString(),
                TotalAmount = x.o.TotalAmount,
                Currency = x.o.Currency,
                SubmittedAtUtc = x.o.SubmittedAtUtc,
            })
            .ToListAsync(ct);

        return Ok(new PagedResult<OrderSummaryDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            Items = items,
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDetailDto>> GetById(Guid id, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        if (tenantId is null)
            return Unauthorized();

        var dto = await MapDetailAsync(id, tenantId.Value, ct);
        if (dto is null)
            return NotFound();

        if (User.IsInRole(nameof(UserRole.StoreUser)))
        {
            var sid = User.GetStoreId();
            if (sid is null || sid != dto.StoreId)
                return Forbid();
        }
        else if (!User.IsInRole(nameof(UserRole.DistributorAdmin)))
        {
            return Forbid();
        }

        return Ok(dto);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = nameof(UserRole.DistributorAdmin))]
    public async Task<ActionResult<OrderDetailDto>> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        if (tenantId is null)
            return Unauthorized();

        if (!Enum.TryParse<OrderStatus>(request.Status, ignoreCase: true, out var next))
            return BadRequest(new { message = "Invalid order status value." });

        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == id && o.TenantId == tenantId, ct);
        if (order is null)
            return NotFound();

        if (!OrderStatusRules.CanTransition(order.Status, next))
        {
            return BadRequest(new
            {
                message = $"Cannot change status from {order.Status} to {next}.",
                allowed = OrderStatusRules.AllowedTargetsSummary(order.Status),
            });
        }

        order.Status = next;
        order.StatusNotes = string.IsNullOrWhiteSpace(request.StatusNotes) ? null : request.StatusNotes.Trim();
        await db.SaveChangesAsync(ct);

        var dto = await MapDetailAsync(order.Id, tenantId.Value, ct);
        return Ok(dto);
    }

    private async Task<ActionResult?> EnsureStoreApprovedAsync(CancellationToken ct)
    {
        if (!User.IsInRole(nameof(UserRole.StoreUser)))
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                detail: "Only approved medical store accounts can submit orders.");
        }

        if (User.GetTenantId() is null || User.GetUserId() is null || User.GetStoreId() is null)
            return Unauthorized();

        var tenantId = User.GetTenantId()!.Value;
        var storeId = User.GetStoreId()!.Value;
        var approved = await db.Stores.AsNoTracking().AnyAsync(
            s => s.Id == storeId && s.TenantId == tenantId && s.ApprovalStatus == StoreApprovalStatus.Approved,
            ct);

        if (!approved)
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                detail: "Store must be approved before submitting orders.");
        }

        return null;
    }

    private async Task<OrderDetailDto?> MapDetailAsync(Guid orderId, Guid tenantId, CancellationToken ct)
    {
        var row = await (
            from o in db.Orders.AsNoTracking()
            join s in db.Stores.AsNoTracking() on o.StoreId equals s.Id
            where o.Id == orderId && o.TenantId == tenantId
            select new
            {
                o,
                s.BusinessName,
                s.AddressLine,
                s.City,
                s.Area,
                s.Mobile,
            }).FirstOrDefaultAsync(ct);

        if (row is null)
            return null;

        var lines = await db.OrderLines.AsNoTracking()
            .Where(l => l.OrderId == orderId)
            .OrderBy(l => l.ProductNameSnapshot)
            .Select(l => new OrderLineDto
            {
                ProductId = l.ProductId,
                ProductNameSnapshot = l.ProductNameSnapshot,
                PackSnapshot = l.PackSnapshot,
                UnitPriceSnapshot = l.UnitPriceSnapshot,
                Quantity = l.Quantity,
                BonusLabelSnapshot = l.BonusLabelSnapshot,
                BonusQuantitySnapshot = l.BonusQuantitySnapshot,
                LineTotal = l.LineTotal,
            })
            .ToListAsync(ct);

        return new OrderDetailDto
        {
            Id = row.o.Id,
            StoreId = row.o.StoreId,
            StoreName = row.BusinessName,
            StoreAddress = row.AddressLine,
            StoreCity = row.City,
            StoreArea = row.Area,
            StoreMobile = row.Mobile,
            Status = row.o.Status.ToString(),
            StatusNotes = row.o.StatusNotes,
            TotalAmount = row.o.TotalAmount,
            Currency = row.o.Currency,
            SubmittedAtUtc = row.o.SubmittedAtUtc,
            Notes = row.o.Notes,
            Lines = lines,
        };
    }
}
