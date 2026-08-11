using MediPro.Api.Configuration;
using MediPro.Api.Domain;
using MediPro.Api.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MediPro.Api.Data;

public static class DbInitializer
{
    private const string DefaultTenantName = "Primary distributor";

    public static async Task InitializeAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MediProDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DbInitializer));
        var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        var seedOpts = scope.ServiceProvider.GetService<IOptions<DevSeedOptions>>()?.Value;
        var passwordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasher<AppUser>>();

        await db.Database.MigrateAsync();

        if (!await db.Tenants.AnyAsync())
        {
            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = DefaultTenantName,
                CreatedAtUtc = DateTime.UtcNow,
            };
            db.Tenants.Add(tenant);

            if (seedOpts is { Enabled: true } && !string.IsNullOrWhiteSpace(seedOpts.AdminPassword))
            {
                var email = seedOpts.AdminEmail.Trim().ToLowerInvariant();
                var admin = new AppUser
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenant.Id,
                    Email = email,
                    PasswordHash = passwordHasher.HashPassword(new AppUser(), seedOpts.AdminPassword),
                    Role = UserRole.DistributorAdmin,
                    StoreId = null,
                    CreatedAtUtc = DateTime.UtcNow,
                };
                db.Users.Add(admin);
                logger.LogWarning("Dev seed: created distributor admin {Email}", email);
            }
            else
            {
                logger.LogInformation(
                    "Empty database: no DevSeed admin created. Set DevSeed:Enabled and DevSeed:AdminPassword in appsettings.Development.json.");
            }

            await db.SaveChangesAsync();

            if (seedOpts is { SeedDemoCatalog: true })
            {
                var inserted = await DemoCatalogSeeder.InsertMissingDemoProductsAsync(db, tenant.Id);
                if (inserted > 0)
                {
                    logger.LogWarning(
                        "Dev seed: inserted {Count} demo catalogue products for tenant {TenantId}",
                        inserted,
                        tenant.Id);
                }

                var bonusInserted = await DemoBonusSchemesSeeder.SeedIfEmptyAsync(db, tenant.Id);
                if (bonusInserted > 0)
                    logger.LogWarning("Dev seed: inserted {Count} demo bonus schemes", bonusInserted);
            }

            await SeedDemoStoresIfEnabledAsync(db, tenant.Id, seedOpts, passwordHasher, logger);
            await SeedDemoOrdersIfEnabledAsync(db, tenant.Id, seedOpts, logger);

            // Seed realistic test data
            if (seedOpts is { SeedRealisticData: true })
            {
                await RealisticDataSeeder.SeedRealisticDataAsync(db, tenant.Id, passwordHasher, seedOpts.AdminPassword);
                logger.LogWarning("Dev seed: inserted realistic test data (50 products, 10 stores, 20 users)");
            }

            return;
        }

        // Database already had tenants (e.g. created before demo seed existed): in Development,
        // fill completely empty catalogues so local UI is not stuck at 0 products.
        if (env.IsDevelopment() && seedOpts is { SeedDemoCatalog: true })
        {
            var tenantIds = await db.Tenants.AsNoTracking().Select(t => t.Id).ToListAsync();
            foreach (var tid in tenantIds)
            {
                var hasAny = await db.Products.AnyAsync(p => p.TenantId == tid);
                if (hasAny)
                    continue;

                var inserted = await DemoCatalogSeeder.InsertMissingDemoProductsAsync(db, tid);
                if (inserted > 0)
                {
                    logger.LogWarning(
                        "Dev seed: inserted {Count} demo products for existing empty tenant {TenantId}",
                        inserted,
                        tid);
                }
            }
        }

        if (seedOpts is { SeedDemoStores: true })
        {
            var tenantIds = await db.Tenants.AsNoTracking().Select(t => t.Id).ToListAsync();
            foreach (var tid in tenantIds)
            {
                await SeedDemoStoresIfEnabledAsync(db, tid, seedOpts, passwordHasher, logger);
            }
        }

        if (seedOpts is { SeedDemoOrders: true })
        {
            var tenantIds = await db.Tenants.AsNoTracking().Select(t => t.Id).ToListAsync();
            foreach (var tid in tenantIds)
            {
                await SeedDemoOrdersIfEnabledAsync(db, tid, seedOpts, logger);
            }
        }

        if (seedOpts is { SeedDemoCatalog: true })
        {
            var tenantIds = await db.Tenants.AsNoTracking().Select(t => t.Id).ToListAsync();
            foreach (var tid in tenantIds)
            {
                var bonusInserted = await DemoBonusSchemesSeeder.SeedIfEmptyAsync(db, tid);
                if (bonusInserted > 0)
                    logger.LogWarning("Dev seed: inserted {Count} demo bonus schemes for tenant {TenantId}", bonusInserted, tid);
            }
        }

        // Seed realistic data for existing tenants
        if (env.IsDevelopment() && seedOpts is { SeedRealisticData: true })
        {
            var tenantIds = await db.Tenants.AsNoTracking().Select(t => t.Id).ToListAsync();
            foreach (var tid in tenantIds)
            {
                await RealisticDataSeeder.SeedRealisticDataAsync(db, tid, passwordHasher, seedOpts.AdminPassword);
            }
        }
    }

    private static async Task SeedDemoStoresIfEnabledAsync(
        MediProDbContext db,
        Guid tenantId,
        DevSeedOptions? seedOpts,
        PasswordHasher<AppUser> passwordHasher,
        ILogger logger)
    {
        if (seedOpts is not { SeedDemoStores: true })
            return;

        var pwd = !string.IsNullOrWhiteSpace(seedOpts.DemoStorePassword)
            ? seedOpts.DemoStorePassword
            : seedOpts.AdminPassword;
        if (string.IsNullOrWhiteSpace(pwd) || pwd.Length < 8)
        {
            logger.LogInformation("Dev seed: Skipping demo stores — set DevSeed:DemoStorePassword (or AdminPassword) to at least 8 characters.");
            return;
        }

        var n = await DemoStoresSeeder.InsertMissingDemoStoresAsync(db, tenantId, passwordHasher, pwd);
        if (n > 0)
            logger.LogWarning("Dev seed: created {Count} demo pharmacy accounts for tenant {TenantId}", n, tenantId);
    }

    private static async Task SeedDemoOrdersIfEnabledAsync(
        MediProDbContext db,
        Guid tenantId,
        DevSeedOptions? seedOpts,
        ILogger logger)
    {
        if (seedOpts is not { SeedDemoOrders: true })
            return;

        var areas = await DemoOrdersSeeder.BackfillStoreAreasAsync(db, tenantId);
        if (areas > 0)
            logger.LogWarning("Dev seed: backfilled Area on {Count} demo stores for tenant {TenantId}", areas, tenantId);

        var n = await DemoOrdersSeeder.InsertDemoOrdersAsync(db, tenantId);
        if (n > 0)
            logger.LogWarning("Dev seed: created {Count} demo orders for tenant {TenantId}", n, tenantId);
    }
}
