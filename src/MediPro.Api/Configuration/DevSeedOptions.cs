namespace MediPro.Api.Configuration;

/// <summary>Optional development-only bootstrap account.</summary>
public sealed class DevSeedOptions
{
    public const string SectionName = "DevSeed";

    public bool Enabled { get; set; }
    public string AdminEmail { get; set; } = "admin@medipro.local";
    public string AdminPassword { get; set; } = "";

    /// <summary>On first database init, insert TEST-MED-* demo products with stock and image URLs.</summary>
    public bool SeedDemoCatalog { get; set; }

    /// <summary>Allow POST /api/admin/catalog/seed-demo-catalog (also allowed when host environment is Development).</summary>
    public bool AllowDemoCatalogEndpoint { get; set; }

    /// <summary>Create demo pharmacy accounts (see DemoStoresSeeder) when DevSeed runs.</summary>
    public bool SeedDemoStores { get; set; }

    /// <summary>Password for all *@demo.medipro.local store users (min 8 characters). Falls back to AdminPassword if blank.</summary>
    public string DemoStorePassword { get; set; } = "";

    /// <summary>Seed realistic test data: 50 products, 10 stores, 20 users with sample orders.</summary>
    public bool SeedRealisticData { get; set; }

    /// <summary>Insert demo orders across cities/areas (see DemoOrdersSeeder).</summary>
    public bool SeedDemoOrders { get; set; }
}
