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
public class CartController(MediProDbContext db, IOptions<InventoryOptions> inventory) : ControllerBase
{
    private readonly InventoryOptions _inventory = inventory.Value;
    [HttpGet]
    public async Task<ActionResult<CartDto>> Get(CancellationToken ct)
    {
        var guard = await EnsureStoreApprovedAsync(ct);
        if (guard != null)
            return guard;

        var cart = await GetOrCreateCartTrackedAsync(ct);
        return Ok(MapCart(cart));
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddItem([FromBody] AddCartItemRequest request, CancellationToken ct)
    {
        var guard = await EnsureStoreApprovedAsync(ct);
        if (guard != null)
            return guard;

        var tenantId = User.GetTenantId()!.Value;
        var product = await db.Products.FirstOrDefaultAsync(
            p => p.Id == request.ProductId && p.TenantId == tenantId && p.IsActive,
            ct);
        if (product is null)
            return NotFound(new { message = "Product not found or inactive." });

        if (StockAvailability.IsOutOfStock(product.StockQuantity))
            return BadRequest(new { message = $"{product.SkuCode} is out of stock." });

        var cart = await GetOrCreateCartTrackedAsync(ct);
        var line = cart.Lines.FirstOrDefault(l => l.ProductId == product.Id);
        var existingQty = line?.Quantity ?? 0;
        var newTotal = existingQty + request.Quantity;
        if (_inventory.EnforceStockQuantityCap && product.StockQuantity is int cap && newTotal > cap)
        {
            return BadRequest(new
            {
                message = $"Only {cap} units available for {product.SkuCode}. You already have {existingQty} in your cart.",
            });
        }

        if (line is null)
        {
            line = new CartLine
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                ProductId = product.Id,
                Quantity = request.Quantity,
            };
            db.CartLines.Add(line);
        }
        else
        {
            checked
            {
                line.Quantity += request.Quantity;
            }
        }

        // Update cart timestamp - use direct property update to avoid concurrency issues
        cart.UpdatedAtUtc = DateTime.UtcNow;
        
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Reload and retry if there was a concurrent update
            await db.Entry(cart).ReloadAsync(ct);
            cart.UpdatedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }

        await db.Entry(cart).Collection(c => c.Lines).Query().Include(l => l.Product).LoadAsync(ct);
        return Ok(MapCart(cart));
    }

    [HttpPatch("items/{productId:guid}")]
    public async Task<ActionResult<CartDto>> SetQuantity(Guid productId, [FromBody] SetCartItemQuantityRequest request, CancellationToken ct)
    {
        var guard = await EnsureStoreApprovedAsync(ct);
        if (guard != null)
            return guard;

        var cart = await GetOrCreateCartTrackedAsync(ct);
        var line = cart.Lines.FirstOrDefault(l => l.ProductId == productId);
        if (line is null)
            return NotFound();

        if (request.Quantity <= 0)
            db.CartLines.Remove(line);
        else
        {
            if (StockAvailability.IsOutOfStock(line.Product.StockQuantity))
                return BadRequest(new { message = $"{line.Product.SkuCode} is out of stock." });

            if (_inventory.EnforceStockQuantityCap && line.Product.StockQuantity is int cap && request.Quantity > cap)
            {
                return BadRequest(new
                {
                    message = $"Only {cap} units available for {line.Product.SkuCode}.",
                });
            }

            line.Quantity = request.Quantity;
        }

        cart.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        await db.Entry(cart).Collection(c => c.Lines).Query().Include(l => l.Product).LoadAsync(ct);
        return Ok(MapCart(cart));
    }

    [HttpDelete("items/{productId:guid}")]
    public async Task<ActionResult<CartDto>> RemoveLine(Guid productId, CancellationToken ct)
    {
        var guard = await EnsureStoreApprovedAsync(ct);
        if (guard != null)
            return guard;

        var cart = await GetOrCreateCartTrackedAsync(ct);
        var line = cart.Lines.FirstOrDefault(l => l.ProductId == productId);
        if (line is not null)
        {
            db.CartLines.Remove(line);
            cart.UpdatedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }

        await db.Entry(cart).Collection(c => c.Lines).Query().Include(l => l.Product).LoadAsync(ct);
        return Ok(MapCart(cart));
    }

    [HttpDelete]
    public async Task<ActionResult<CartDto>> Clear(CancellationToken ct)
    {
        var guard = await EnsureStoreApprovedAsync(ct);
        if (guard != null)
            return guard;

        var cart = await GetOrCreateCartTrackedAsync(ct);
        if (cart.Lines.Count > 0)
        {
            db.CartLines.RemoveRange(cart.Lines);
            cart.UpdatedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }

        await db.Entry(cart).Collection(c => c.Lines).Query().Include(l => l.Product).LoadAsync(ct);
        return Ok(MapCart(cart));
    }

    private async Task<ActionResult?> EnsureStoreApprovedAsync(CancellationToken ct)
    {
        if (!User.IsInRole(nameof(UserRole.StoreUser)))
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                detail: "Only approved medical store accounts can use the cart.");
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
                detail: "Store must be approved before ordering.");
        }

        return null;
    }

    private async Task<Cart> GetOrCreateCartTrackedAsync(CancellationToken ct)
    {
        var tenantId = User.GetTenantId()!.Value;
        var userId = User.GetUserId()!.Value;

        var cart = await db.Carts
            .Include(c => c.Lines)
            .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);

        if (cart is not null)
            return cart;

        cart = new Cart
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        db.Carts.Add(cart);
        await db.SaveChangesAsync(ct);

        return await db.Carts
            .Include(c => c.Lines)
            .ThenInclude(l => l.Product)
            .FirstAsync(c => c.Id == cart.Id, ct);
    }

    private static CartDto MapCart(Cart cart)
    {
        decimal subtotal = 0;
        var lines = new List<CartLineDto>();
        foreach (var l in cart.Lines.OrderBy(x => x.Product.Name))
        {
            var price = l.Product.TradePrice;
            var lineTotal = price * l.Quantity;
            subtotal += lineTotal;
            lines.Add(new CartLineDto
            {
                LineId = l.Id,
                ProductId = l.ProductId,
                SkuCode = l.Product.SkuCode,
                Name = l.Product.Name,
                Pack = l.Product.Pack,
                TradePrice = price,
                Quantity = l.Quantity,
                LineTotal = lineTotal,
            });
        }

        return new CartDto
        {
            CartId = cart.Id,
            Lines = lines,
            Subtotal = subtotal,
        };
    }
}
