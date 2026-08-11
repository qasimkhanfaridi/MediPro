using MediPro.Api.Domain;
using MediPro.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MediPro.Api.Data;

/// <summary>Demo pharmacies under the primary tenant (dev / QA logins).</summary>
public static class DemoStoresSeeder
{
    public const string DemoEmailDomain = "@demo.medipro.local";

    private sealed record DemoStoreRow(
        string EmailLocalPart,
        string BusinessName,
        string AddressLine,
        string City,
        string Area,
        string Mobile,
        string ContactName,
        StoreApprovalStatus Status,
        string? ApprovalNotes);

    private static readonly DemoStoreRow[] Rows =
    [
        new("store.approved1", "Capital Cure Pharmacy", "Plot 14, Jinnah Avenue", "Islamabad", "F-7 Markaz", "03017654001", "Dr. Ayesha Khan", StoreApprovalStatus.Approved, null),
        new("store.approved2", "Crescent Care Pharmacy", "Bank Road", "Rawalpindi", "Saddar", "03017654002", "Mr. Bilal Ahmed", StoreApprovalStatus.Approved, null),
        new("store.approved3", "Lawn Medical Store", "Main Boulevard", "Lahore", "Gulberg III", "03017654003", "Ms. Sara Malik", StoreApprovalStatus.Approved, null),
        new("store.approved4", "Saddar Discount Chemist", "Preedy Street", "Karachi", "Saddar", "03017654004", "Mr. Omar Farooq", StoreApprovalStatus.Approved, null),
        new("store.approved5", "Moti Mehel Family Pharmacy", "Moti Mehel Road", "Rawalpindi", "Moti Mehel", "03017654008", "Mr. Hamza Raza", StoreApprovalStatus.Approved, null),
        new("store.approved6", "Raja Bazaar Medical Store", "Raja Bazaar", "Rawalpindi", "Raja Bazaar", "03017654009", "Mr. Usman Ali", StoreApprovalStatus.Approved, null),
        new("store.approved7", "Bahria Town Chemist", "Phase 7 Commercial", "Rawalpindi", "Bahria Town Phase 7", "03017654010", "Ms. Nadia Shah", StoreApprovalStatus.Approved, null),
        new("store.approved8", "Blue Area Pharma", "Jinnah Avenue", "Islamabad", "Blue Area", "03017654011", "Mr. Faisal Khan", StoreApprovalStatus.Approved, null),
        new("store.pending1", "Newtown Medical Store", "Sector F-7 Markaz", "Islamabad", "F-7 Markaz", "03017654005", "Mr. Pending One", StoreApprovalStatus.Pending, null),
        new("store.pending2", "Canal Road Chemists", "Susan Road", "Faisalabad", "Susan Road", "03017654006", "Ms. Pending Two", StoreApprovalStatus.Pending, null),
        new("store.rejected1", "Quick Rx (Demo Rejected)", "Unknown Plaza", "Lahore", "Model Town", "03017654007", "Rejected Demo", StoreApprovalStatus.Rejected, "Demo seed: incomplete license (sample only)."),
    ];

    /// <returns>Number of new store accounts created.</returns>
    public static async Task<int> InsertMissingDemoStoresAsync(
        MediProDbContext db,
        Guid tenantId,
        PasswordHasher<AppUser> passwordHasher,
        string plainPassword,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(plainPassword) || plainPassword.Length < 8)
            return 0;

        var created = 0;
        var now = DateTime.UtcNow;

        foreach (var row in Rows)
        {
            var email = $"{row.EmailLocalPart}{DemoEmailDomain}".ToLowerInvariant();
            var exists = await db.Users.AnyAsync(
                u => u.TenantId == tenantId && u.Email == email,
                ct);
            if (exists)
                continue;

            var store = new Store
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                BusinessName = row.BusinessName,
                AddressLine = row.AddressLine,
                City = row.City,
                Area = row.Area,
                Mobile = row.Mobile,
                ContactName = row.ContactName,
                LicenseNumber = $"DEMO-LIC-{row.Mobile[^4..]}",
                ApprovalStatus = row.Status,
                ApprovalNotes = row.ApprovalNotes,
                CreatedAtUtc = now,
            };

            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Email = email,
                PasswordHash = passwordHasher.HashPassword(new AppUser(), plainPassword),
                Role = UserRole.StoreUser,
                StoreId = store.Id,
                CreatedAtUtc = now,
            };

            db.Stores.Add(store);
            db.Users.Add(user);
            created++;
        }

        if (created > 0)
            await db.SaveChangesAsync(ct);

        return created;
    }
}
