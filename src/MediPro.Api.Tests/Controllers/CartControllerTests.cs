using System.Security.Claims;
using FluentAssertions;
using MediPro.Api.Contracts;
using MediPro.Api.Controllers;
using MediPro.Api.Data;
using MediPro.Api.Domain;
using MediPro.Api.Entities;
using MediPro.Api.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MediPro.Api.Tests.Controllers;

public class CartControllerTests
{
    private static CartController CreateCartController(MediProDbContext db) =>
        new(db, TestInventoryOptions.Default);

    private MediProDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<MediProDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new MediProDbContext(options);
    }

    private ClaimsPrincipal CreateStoreUserPrincipal(Guid userId, Guid tenantId, Guid storeId)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("tenant_id", tenantId.ToString()),
            new Claim("store_id", storeId.ToString()),
            new Claim(ClaimTypes.Role, "StoreUser")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    private async Task<(Guid tenantId, Guid storeId, Guid userId, Guid productId)> SeedTestDataAsync(MediProDbContext db)
    {
        var tenantId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        db.Stores.Add(new Store
        {
            Id = storeId,
            TenantId = tenantId,
            BusinessName = "Test Store",
            AddressLine = "Test Address",
            City = "Test City",
            ContactName = "Test Contact",
            Mobile = "1234567890",
            ApprovalStatus = StoreApprovalStatus.Approved,
            CreatedAtUtc = DateTime.UtcNow
        });

        db.Users.Add(new AppUser
        {
            Id = userId,
            TenantId = tenantId,
            Email = "test@test.com",
            PasswordHash = "hash",
            Role = UserRole.StoreUser,
            StoreId = storeId,
            CreatedAtUtc = DateTime.UtcNow
        });

        db.Products.Add(new Product
        {
            Id = productId,
            TenantId = tenantId,
            SkuCode = "TEST001",
            Name = "Test Product",
            Pack = "10 Tabs",
            Manufacturer = "Test Pharma",
            SaltComposition = "Test Salt",
            TradePrice = 100m,
            Mrp = 120m,
            IsActive = true,
            StockQuantity = 50,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        return (tenantId, storeId, userId, productId);
    }

    [Fact]
    public async Task Get_NoExistingCart_CreatesEmptyCart()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var (tenantId, storeId, userId, _) = await SeedTestDataAsync(db);
        
        var controller = CreateCartController(db);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = CreateStoreUserPrincipal(userId, tenantId, storeId)
            }
        };

        // Act
        var result = await controller.Get(CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var cart = okResult.Value.Should().BeOfType<CartDto>().Subject;
        cart.Lines.Should().BeEmpty();
        cart.Subtotal.Should().Be(0);
    }

    [Fact]
    public async Task AddItem_NewProduct_AddsToCart()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var (tenantId, storeId, userId, productId) = await SeedTestDataAsync(db);
        
        var controller = CreateCartController(db);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = CreateStoreUserPrincipal(userId, tenantId, storeId)
            }
        };

        var request = new AddCartItemRequest
        {
            ProductId = productId,
            Quantity = 5
        };

        // Act
        var result = await controller.AddItem(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var cart = okResult.Value.Should().BeOfType<CartDto>().Subject;
        cart.Lines.Should().HaveCount(1);
        cart.Lines[0].ProductId.Should().Be(productId);
        cart.Lines[0].Quantity.Should().Be(5);
        cart.Lines[0].TradePrice.Should().Be(100m);
        cart.Lines[0].LineTotal.Should().Be(500m);
        cart.Subtotal.Should().Be(500m);
    }

    [Fact]
    public async Task AddItem_ExistingProduct_IncreasesQuantity()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var (tenantId, storeId, userId, productId) = await SeedTestDataAsync(db);
        
        var controller = CreateCartController(db);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = CreateStoreUserPrincipal(userId, tenantId, storeId)
            }
        };

        // Add first time
        await controller.AddItem(new AddCartItemRequest { ProductId = productId, Quantity = 3 }, CancellationToken.None);

        // Act - Add again
        var result = await controller.AddItem(new AddCartItemRequest { ProductId = productId, Quantity = 2 }, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var cart = okResult.Value.Should().BeOfType<CartDto>().Subject;
        cart.Lines.Should().HaveCount(1);
        cart.Lines[0].Quantity.Should().Be(5);
        cart.Lines[0].LineTotal.Should().Be(500m);
    }

    [Fact]
    public async Task AddItem_ExceedsStock_ReturnsBadRequest()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var (tenantId, storeId, userId, productId) = await SeedTestDataAsync(db);
        
        var controller = CreateCartController(db);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = CreateStoreUserPrincipal(userId, tenantId, storeId)
            }
        };

        var request = new AddCartItemRequest
        {
            ProductId = productId,
            Quantity = 51 // Stock is 50
        };

        // Act
        var result = await controller.AddItem(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task AddItem_InactiveProduct_ReturnsNotFound()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var (tenantId, storeId, userId, productId) = await SeedTestDataAsync(db);
        
        // Make product inactive
        var product = await db.Products.FindAsync(productId);
        product!.IsActive = false;
        await db.SaveChangesAsync();
        
        var controller = CreateCartController(db);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = CreateStoreUserPrincipal(userId, tenantId, storeId)
            }
        };

        var request = new AddCartItemRequest
        {
            ProductId = productId,
            Quantity = 1
        };

        // Act
        var result = await controller.AddItem(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task SetQuantity_ExistingItem_UpdatesQuantity()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var (tenantId, storeId, userId, productId) = await SeedTestDataAsync(db);
        
        var controller = CreateCartController(db);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = CreateStoreUserPrincipal(userId, tenantId, storeId)
            }
        };

        // Add item first
        await controller.AddItem(new AddCartItemRequest { ProductId = productId, Quantity = 5 }, CancellationToken.None);

        // Act - Update quantity
        var result = await controller.SetQuantity(productId, new SetCartItemQuantityRequest { Quantity = 10 }, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var cart = okResult.Value.Should().BeOfType<CartDto>().Subject;
        cart.Lines[0].Quantity.Should().Be(10);
        cart.Lines[0].LineTotal.Should().Be(1000m);
    }

    [Fact]
    public async Task SetQuantity_ZeroQuantity_RemovesItem()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var (tenantId, storeId, userId, productId) = await SeedTestDataAsync(db);
        
        var controller = CreateCartController(db);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = CreateStoreUserPrincipal(userId, tenantId, storeId)
            }
        };

        // Add item first
        await controller.AddItem(new AddCartItemRequest { ProductId = productId, Quantity = 5 }, CancellationToken.None);

        // Act - Set to zero
        var result = await controller.SetQuantity(productId, new SetCartItemQuantityRequest { Quantity = 0 }, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var cart = okResult.Value.Should().BeOfType<CartDto>().Subject;
        cart.Lines.Should().BeEmpty();
        cart.Subtotal.Should().Be(0);
    }

    [Fact]
    public async Task RemoveLine_ExistingItem_RemovesFromCart()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var (tenantId, storeId, userId, productId) = await SeedTestDataAsync(db);
        
        var controller = CreateCartController(db);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = CreateStoreUserPrincipal(userId, tenantId, storeId)
            }
        };

        // Add item first
        await controller.AddItem(new AddCartItemRequest { ProductId = productId, Quantity = 5 }, CancellationToken.None);

        // Act
        var result = await controller.RemoveLine(productId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var cart = okResult.Value.Should().BeOfType<CartDto>().Subject;
        cart.Lines.Should().BeEmpty();
    }

    [Fact]
    public async Task Clear_WithItems_RemovesAllItems()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var (tenantId, storeId, userId, productId) = await SeedTestDataAsync(db);
        
        // Add second product
        var productId2 = Guid.NewGuid();
        db.Products.Add(new Product
        {
            Id = productId2,
            TenantId = tenantId,
            SkuCode = "TEST002",
            Name = "Test Product 2",
            Pack = "20 Tabs",
            Manufacturer = "Test Pharma",
            SaltComposition = "Test Salt 2",
            TradePrice = 200m,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        
        var controller = CreateCartController(db);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = CreateStoreUserPrincipal(userId, tenantId, storeId)
            }
        };

        // Add multiple items
        await controller.AddItem(new AddCartItemRequest { ProductId = productId, Quantity = 5 }, CancellationToken.None);
        await controller.AddItem(new AddCartItemRequest { ProductId = productId2, Quantity = 3 }, CancellationToken.None);

        // Act
        var result = await controller.Clear(CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var cart = okResult.Value.Should().BeOfType<CartDto>().Subject;
        cart.Lines.Should().BeEmpty();
        cart.Subtotal.Should().Be(0);
    }

    [Fact]
    public async Task AddItem_UnapprovedStore_ReturnsForbidden()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var tenantId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        // Create unapproved store
        db.Stores.Add(new Store
        {
            Id = storeId,
            TenantId = tenantId,
            BusinessName = "Test Store",
            AddressLine = "Test Address",
            City = "Test City",
            ContactName = "Test Contact",
            Mobile = "1234567890",
            ApprovalStatus = StoreApprovalStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        });

        db.Users.Add(new AppUser
        {
            Id = userId,
            TenantId = tenantId,
            Email = "test@test.com",
            PasswordHash = "hash",
            Role = UserRole.StoreUser,
            StoreId = storeId,
            CreatedAtUtc = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        
        var controller = CreateCartController(db);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = CreateStoreUserPrincipal(userId, tenantId, storeId)
            }
        };

        var request = new AddCartItemRequest
        {
            ProductId = productId,
            Quantity = 1
        };

        // Act
        var result = await controller.AddItem(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task AddItem_MultipleProducts_CalculatesCorrectSubtotal()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var (tenantId, storeId, userId, productId) = await SeedTestDataAsync(db);
        
        // Add second product
        var productId2 = Guid.NewGuid();
        db.Products.Add(new Product
        {
            Id = productId2,
            TenantId = tenantId,
            SkuCode = "TEST002",
            Name = "Test Product 2",
            Pack = "20 Tabs",
            Manufacturer = "Test Pharma",
            SaltComposition = "Test Salt 2",
            TradePrice = 250m,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        
        var controller = CreateCartController(db);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = CreateStoreUserPrincipal(userId, tenantId, storeId)
            }
        };

        // Act - Add multiple products
        await controller.AddItem(new AddCartItemRequest { ProductId = productId, Quantity = 4 }, CancellationToken.None);
        var result = await controller.AddItem(new AddCartItemRequest { ProductId = productId2, Quantity = 2 }, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var cart = okResult.Value.Should().BeOfType<CartDto>().Subject;
        cart.Lines.Should().HaveCount(2);
        cart.Subtotal.Should().Be(900m); // (4 * 100) + (2 * 250) = 400 + 500 = 900
    }
}
