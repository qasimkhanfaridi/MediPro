using MediPro.Api.Contracts;
using MediPro.Api.Data;
using MediPro.Api.Domain;
using MediPro.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediPro.Api.Controllers;

[ApiController]
[Route("api/admin/notifications")]
[Authorize(Roles = nameof(UserRole.DistributorAdmin))]
public class AdminNotificationsController(MediProDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminNotificationDto>>> List(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var tenantId = User.GetTenantId();
        var userId = User.GetUserId();
        if (tenantId is null || userId is null)
            return Unauthorized();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.AdminNotifications.AsNoTracking()
            .Where(n => n.TenantId == tenantId && n.RecipientUserId == userId);

        if (unreadOnly)
            query = query.Where(n => n.ReadAtUtc == null);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(n => n.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new AdminNotificationDto
            {
                Id = n.Id,
                Type = n.Type.ToString(),
                Title = n.Title,
                Body = n.Body,
                RelatedOrderId = n.RelatedOrderId,
                RelatedStoreId = n.RelatedStoreId,
                CreatedAtUtc = n.CreatedAtUtc,
                IsRead = n.ReadAtUtc != null,
            })
            .ToListAsync(ct);

        return Ok(new PagedResult<AdminNotificationDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            Items = items,
        });
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<ActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var userId = User.GetUserId();
        if (tenantId is null || userId is null)
            return Unauthorized();

        var n = await db.AdminNotifications.FirstOrDefaultAsync(
            x => x.Id == id && x.TenantId == tenantId && x.RecipientUserId == userId,
            ct);

        if (n is null)
            return NotFound();

        if (n.ReadAtUtc is null)
        {
            n.ReadAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }

        return NoContent();
    }
}
