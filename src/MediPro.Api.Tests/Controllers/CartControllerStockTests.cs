using FluentAssertions;
using MediPro.Api.Configuration;
using MediPro.Api.Contracts;
using MediPro.Api.Controllers;
using MediPro.Api.Services;
using MediPro.Api.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace MediPro.Api.Tests.Controllers;

public class CartControllerStockTests
{
    [Fact]
    public async Task AddItem_OutOfStock_ReturnsBadRequest()
    {
        await using var db = OrderTestFixtures.CreateInMemoryContext();
        var tenantId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        db.Tenants.Add(new MediPro.Api.Entities.Tenant
        {
            Id = tenantId,
            Name = "T",
            CreatedAtUtc = DateTime.UtcNow,
        });
        db.Stores.Add(new MediPro.Api.Entities.Store
        {
            Id = storeId,
            TenantId = tenantId,
            BusinessName = "S",
            AddressLine = "A",
            City = "C",
            ContactName = "N",
            Mobile = "1",
            ApprovalStatus = MediPro.Api.Domain.StoreApprovalStatus.Approved,
            CreatedAtUtc = DateTime.UtcNow,
        });
        db.Users.Add(new MediPro.Api.Entities.AppUser
        {
            Id = userId,
            TenantId = tenantId,
            Email = "u@test.local",
            PasswordHash = "h",
            Role = MediPro.Api.Domain.UserRole.StoreUser,
            StoreId = storeId,
            CreatedAtUtc = DateTime.UtcNow,
        });
        db.Products.Add(new MediPro.Api.Entities.Product
        {
            Id = productId,
            TenantId = tenantId,
            SkuCode = "OUT-001",
            Name = "Out product",
            Pack = "10",
            Manufacturer = "M",
            SaltComposition = "S",
            TradePrice = 10m,
            IsActive = true,
            StockQuantity = 0,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        var options = Options.Create(new InventoryOptions { EnforceStockQuantityCap = false });
        var controller = new CartController(db, options);
        OrderTestFixtures.AttachUser(
            controller,
            OrderTestFixtures.CreateStoreUserPrincipal(userId, tenantId, storeId));

        var result = await controller.AddItem(
            new AddCartItemRequest { ProductId = productId, Quantity = 1 },
            CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }
}
