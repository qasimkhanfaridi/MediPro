using FluentAssertions;
using MediPro.Api.Extensions;
using System.Security.Claims;
using Xunit;

namespace MediPro.Api.Tests.Extensions;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetUserId_ValidSubClaim_ReturnsGuid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim("sub", userId.ToString())
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetUserId();

        // Assert
        result.Should().Be(userId);
    }

    [Fact]
    public void GetUserId_ValidNameIdentifierClaim_ReturnsGuid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetUserId();

        // Assert
        result.Should().Be(userId);
    }

    [Fact]
    public void GetUserId_MissingSubClaim_ReturnsNull()
    {
        // Arrange
        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetUserId();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetUserId_InvalidGuid_ReturnsNull()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("sub", "not-a-guid")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetUserId();

        // Assert
        result.Should().BeNull("invalid GUID format should return null");
    }

    [Fact]
    public void GetTenantId_ValidTenantIdClaim_ReturnsGuid()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim("tenant_id", tenantId.ToString())
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetTenantId();

        // Assert
        result.Should().Be(tenantId);
    }

    [Fact]
    public void GetTenantId_MissingClaim_ReturnsNull()
    {
        // Arrange
        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetTenantId();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetTenantId_InvalidGuid_ReturnsNull()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("tenant_id", "not-a-guid")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetTenantId();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetStoreId_ValidStoreIdClaim_ReturnsGuid()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim("store_id", storeId.ToString())
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetStoreId();

        // Assert
        result.Should().Be(storeId);
    }

    [Fact]
    public void GetStoreId_MissingClaim_ReturnsNull()
    {
        // Arrange
        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetStoreId();

        // Assert
        result.Should().BeNull("store_id is optional for admin users");
    }

    [Fact]
    public void GetStoreId_InvalidGuid_ReturnsNull()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("store_id", "invalid-guid")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetStoreId();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void MultipleClaims_AllExtensionsWork()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        
        var claims = new[]
        {
            new Claim("sub", userId.ToString()),
            new Claim("tenant_id", tenantId.ToString()),
            new Claim("store_id", storeId.ToString())
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        // Act & Assert
        principal.GetUserId().Should().Be(userId);
        principal.GetTenantId().Should().Be(tenantId);
        principal.GetStoreId().Should().Be(storeId);
    }
}
