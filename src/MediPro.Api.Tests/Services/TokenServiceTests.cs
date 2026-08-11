using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using MediPro.Api.Configuration;
using MediPro.Api.Domain;
using MediPro.Api.Entities;
using MediPro.Api.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace MediPro.Api.Tests.Services;

public class TokenServiceTests
{
    private readonly JwtOptions _jwtOptions;
    private readonly ITokenService _tokenService;

    public TokenServiceTests()
    {
        _jwtOptions = new JwtOptions
        {
            SigningKey = "test-signing-key-minimum-32-characters-required-for-HS256!!",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpiryMinutes = 60
        };

        var options = Options.Create(_jwtOptions);
        _tokenService = new TokenService(options);
    }

    [Fact]
    public void CreateAccessToken_ValidAdminUser_ReturnsValidToken()
    {
        // Arrange
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Email = "admin@test.com",
            Role = UserRole.DistributorAdmin,
            StoreId = null
        };

        // Act
        var token = _tokenService.CreateAccessToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        jwtToken.Issuer.Should().Be(_jwtOptions.Issuer);
        jwtToken.Audiences.Should().Contain(_jwtOptions.Audience);
        
        var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        subClaim.Should().NotBeNull();
        subClaim!.Value.Should().Be(user.Id.ToString());
        
        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email);
        emailClaim.Should().NotBeNull();
        emailClaim!.Value.Should().Be(user.Email);
        
        var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
        roleClaim.Should().NotBeNull();
        roleClaim!.Value.Should().Be(UserRole.DistributorAdmin.ToString());
        
        var tenantClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "tenant_id");
        tenantClaim.Should().NotBeNull();
        tenantClaim!.Value.Should().Be(user.TenantId.ToString());
    }

    [Fact]
    public void CreateAccessToken_StoreUser_IncludesStoreId()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Email = "store@test.com",
            Role = UserRole.StoreUser,
            StoreId = storeId
        };

        // Act
        var token = _tokenService.CreateAccessToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        var storeClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "store_id");
        storeClaim.Should().NotBeNull();
        storeClaim!.Value.Should().Be(storeId.ToString());
    }

    [Fact]
    public void CreateAccessToken_AdminUser_DoesNotIncludeStoreId()
    {
        // Arrange
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Email = "admin@test.com",
            Role = UserRole.DistributorAdmin,
            StoreId = null
        };

        // Act
        var token = _tokenService.CreateAccessToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        var storeClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "store_id");
        storeClaim.Should().BeNull("admin users should not have a store_id claim");
    }

    [Fact]
    public void CreateAccessToken_ValidUser_TokenExpiresInConfiguredTime()
    {
        // Arrange
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Email = "test@test.com",
            Role = UserRole.StoreUser,
            StoreId = Guid.NewGuid()
        };

        var beforeCreation = DateTime.UtcNow;

        // Act
        var token = _tokenService.CreateAccessToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        var expectedExpiry = beforeCreation.AddMinutes(_jwtOptions.ExpiryMinutes);
        
        jwtToken.ValidTo.Should().BeCloseTo(expectedExpiry, TimeSpan.FromMinutes(1));
        jwtToken.ValidFrom.Should().BeCloseTo(beforeCreation, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void CreateAccessToken_MissingSigningKey_ThrowsException()
    {
        // Arrange
        var badOptions = new JwtOptions
        {
            SigningKey = "",
            Issuer = "test",
            Audience = "test",
            ExpiryMinutes = 60
        };
        var options = Options.Create(badOptions);
        var service = new TokenService(options);

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Email = "test@test.com",
            Role = UserRole.StoreUser
        };

        // Act
        Action act = () => service.CreateAccessToken(user);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*SigningKey*");
    }
}
