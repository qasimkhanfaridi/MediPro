using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MediPro.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(sub, out var id) ? id : null;
    }

    public static Guid? GetTenantId(this ClaimsPrincipal user)
    {
        var t = user.FindFirstValue("tenant_id");
        return Guid.TryParse(t, out var id) ? id : null;
    }

    public static Guid? GetStoreId(this ClaimsPrincipal user)
    {
        var s = user.FindFirstValue("store_id");
        return Guid.TryParse(s, out var id) ? id : null;
    }
}
