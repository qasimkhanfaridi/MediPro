using MediPro.Api.Configuration;
using MediPro.Api.Contracts;
using MediPro.Api.Data;
using MediPro.Api.Domain;
using MediPro.Api.Entities;
using MediPro.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MediPro.Api.Controllers;

[ApiController]
[Route("api/admin/stores")]
[Authorize(Roles = nameof(UserRole.DistributorAdmin))]
public class AdminStoresController(
    MediProDbContext db,
    PasswordHasher<AppUser> passwordHasher,
    IHostEnvironment env,
    IOptions<DevSeedOptions> devSeed) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StoreSummaryDto>>> List(CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        if (tenantId is null)
            return Unauthorized();

        var items = await db.Stores.AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .OrderByDescending(s => s.CreatedAtUtc)
            .Select(s => new StoreSummaryDto
            {
                Id = s.Id,
                BusinessName = s.BusinessName,
                City = s.City,
                Area = s.Area,
                Mobile = s.Mobile,
                LicenseNumber = s.LicenseNumber,
                ApprovalStatus = s.ApprovalStatus.ToString(),
                ApprovalNotes = s.ApprovalNotes,
                CreatedAtUtc = s.CreatedAtUtc,
            })
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpPatch("{id:guid}/approval")]
    public async Task<ActionResult<StoreSummaryDto>> SetApproval(Guid id, [FromBody] SetStoreApprovalRequest request, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        if (tenantId is null)
            return Unauthorized();

        if (!Enum.TryParse<StoreApprovalStatus>(request.Status, ignoreCase: true, out var status))
            return BadRequest(new { message = "Invalid status. Use Pending, Approved, Rejected, or Suspended." });

        var store = await db.Stores.FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId, ct);
        if (store is null)
            return NotFound();

        store.ApprovalStatus = status;
        store.ApprovalNotes = request.Notes;
        if (status != StoreApprovalStatus.Pending)
            store.PendingApprovalReminderSentAtUtc = null;
        await db.SaveChangesAsync(ct);

        return Ok(new StoreSummaryDto
        {
            Id = store.Id,
            BusinessName = store.BusinessName,
            City = store.City,
            Area = store.Area,
            Mobile = store.Mobile,
            LicenseNumber = store.LicenseNumber,
            ApprovalStatus = store.ApprovalStatus.ToString(),
            ApprovalNotes = store.ApprovalNotes,
            CreatedAtUtc = store.CreatedAtUtc,
        });
    }

    /// <summary>Insert demo pharmacies (*@demo.medipro.local). Development or DevSeed:AllowDemoCatalogEndpoint.</summary>
    [HttpPost("seed-demo")]
    public async Task<ActionResult<object>> SeedDemoStores(CancellationToken ct)
    {
        var opts = devSeed.Value;
        if (!env.IsDevelopment() && !opts.AllowDemoCatalogEndpoint)
            return NotFound();

        var tenantId = User.GetTenantId();
        if (tenantId is null)
            return Unauthorized();

        var pwd = !string.IsNullOrWhiteSpace(opts.DemoStorePassword)
            ? opts.DemoStorePassword
            : opts.AdminPassword;
        if (string.IsNullOrWhiteSpace(pwd) || pwd.Length < 8)
        {
            return BadRequest(new
            {
                message = "Set DevSeed:DemoStorePassword (or AdminPassword) to at least 8 characters in configuration.",
            });
        }

        var inserted = await DemoStoresSeeder.InsertMissingDemoStoresAsync(
            db,
            tenantId.Value,
            passwordHasher,
            pwd,
            ct);

        return Ok(new
        {
            inserted,
            message = inserted == 0
                ? "All demo store accounts already exist."
                : $"Created {inserted} demo pharmacy account(s). Password is the configured DemoStorePassword (or AdminPassword).",
        });
    }
}
