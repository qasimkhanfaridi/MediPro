using MediPro.Api.Configuration;
using MediPro.Api.Contracts;
using MediPro.Api.Data;
using MediPro.Api.Domain;
using MediPro.Api.Entities;
using MediPro.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MediPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    MediProDbContext db,
    ITokenService tokens,
    PasswordHasher<AppUser> passwordHasher,
    IOptions<JwtOptions> jwtOptions,
    IAdminAlertService adminAlerts) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct);
        if (user is null)
            return Unauthorized();

        var verify = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verify is PasswordVerificationResult.Failed)
            return Unauthorized();

        if (user.Role == UserRole.StoreUser && user.StoreId is { } storeId)
        {
            var status = await db.Stores.AsNoTracking()
                .Where(s => s.Id == storeId)
                .Select(s => s.ApprovalStatus)
                .FirstAsync(ct);
            if (status is StoreApprovalStatus.Rejected or StoreApprovalStatus.Suspended)
                return Problem(statusCode: StatusCodes.Status403Forbidden, detail: "Store account is not permitted to sign in.");
        }

        return Ok(await BuildAuthResponseAsync(user, null, ct));
    }

    [HttpPost("register-store")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> RegisterStore([FromBody] RegisterStoreRequest request, CancellationToken ct)
    {
        var tenant = await db.Tenants.OrderBy(t => t.CreatedAtUtc).FirstOrDefaultAsync(ct);
        if (tenant is null)
            return Problem(statusCode: StatusCodes.Status503ServiceUnavailable, detail: "System is not provisioned yet.");

        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.TenantId == tenant.Id && u.Email == email, ct))
            return Conflict(new { message = "Email is already registered." });

        if (string.IsNullOrWhiteSpace(request.LicenseNumber))
            return BadRequest(new { message = "Drug sale licence number is required." });

        var store = new Store
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            BusinessName = request.BusinessName.Trim(),
            AddressLine = request.AddressLine.Trim(),
            City = request.City.Trim(),
            Area = string.IsNullOrWhiteSpace(request.Area) ? "" : request.Area.Trim(),
            Mobile = request.Mobile.Trim(),
            ContactName = request.ContactName.Trim(),
            LicenseNumber = request.LicenseNumber.Trim(),
            ApprovalStatus = StoreApprovalStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow,
        };

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Email = email,
            PasswordHash = passwordHasher.HashPassword(new AppUser(), request.Password),
            Role = UserRole.StoreUser,
            StoreId = store.Id,
            CreatedAtUtc = DateTime.UtcNow,
        };

        db.Stores.Add(store);
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        await adminAlerts.NotifyNewStorePendingAsync(
            tenant.Id, store.Id, store.BusinessName, store.LicenseNumber, ct);

        return Ok(await BuildAuthResponseAsync(user, StoreApprovalStatus.Pending, ct));
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(AppUser user, StoreApprovalStatus? storeStatusOverride, CancellationToken ct)
    {
        var expires = DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpiryMinutes);
        StoreApprovalStatus? storeStatus = storeStatusOverride;
        if (user.Role == UserRole.StoreUser && user.StoreId is { } sid && storeStatus is null)
        {
            storeStatus = await db.Stores.AsNoTracking()
                .Where(s => s.Id == sid)
                .Select(s => s.ApprovalStatus)
                .FirstOrDefaultAsync(ct);
        }

        return new AuthResponse
        {
            AccessToken = tokens.CreateAccessToken(user),
            ExpiresAtUtc = expires,
            Role = user.Role.ToString(),
            StoreApprovalStatus = storeStatus?.ToString(),
        };
    }
}
