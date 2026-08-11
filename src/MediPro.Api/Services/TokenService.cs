using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MediPro.Api.Configuration;
using MediPro.Api.Domain;
using MediPro.Api.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MediPro.Api.Services;

public interface ITokenService
{
    string CreateAccessToken(AppUser user);
}

public sealed class TokenService(IOptions<JwtOptions> jwtOptions) : ITokenService
{
    private readonly JwtOptions _opts = jwtOptions.Value;

    public string CreateAccessToken(AppUser user)
    {
        if (string.IsNullOrWhiteSpace(_opts.SigningKey))
            throw new InvalidOperationException("Jwt:SigningKey is not configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("tenant_id", user.TenantId.ToString()),
        };

        if (user.StoreId is { } sid)
            claims.Add(new Claim("store_id", sid.ToString()));

        var token = new JwtSecurityToken(
            issuer: _opts.Issuer,
            audience: _opts.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_opts.ExpiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
