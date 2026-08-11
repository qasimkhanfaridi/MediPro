using MediPro.Api.Data;
using MediPro.Api.Domain;
using MediPro.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MediPro.Api.Data;

public static class RealisticDataSeeder
{
    public static async Task SeedRealisticDataAsync(
        MediProDbContext db,
        Guid tenantId,
        PasswordHasher<AppUser> passwordHasher,
        string defaultPassword)
    {
        // Check if data already exists
        if (await db.Products.AnyAsync(p => p.TenantId == tenantId && p.SkuCode.StartsWith("MED")))
            return; // Already seeded

        var now = DateTime.UtcNow;
        var random = new Random(12345); // Fixed seed for consistent data

        // ========================================
        // 1. SEED 50 PRODUCTS
        // ========================================
        var products = GetRealisticProducts(tenantId, now, random);
        db.Products.AddRange(products);
        await db.SaveChangesAsync();

        // ========================================
        // 2. SEED 10 PHARMACY STORES
        // ========================================
        var stores = GetRealisticStores(tenantId, now, random);
        db.Stores.AddRange(stores);
        await db.SaveChangesAsync();

        // ========================================
        // 3. SEED STORE USERS (20 sales people)
        // ========================================
        var users = GetRealisticUsers(tenantId, stores, passwordHasher, defaultPassword, now, random);
        db.Users.AddRange(users);
        await db.SaveChangesAsync();

        // ========================================
        // 4. SEED SAMPLE ORDERS
        // ========================================
        var orders = GetSampleOrders(tenantId, stores, products, users, now, random);
        db.Orders.AddRange(orders);
        await db.SaveChangesAsync();
    }

    private static List<Product> GetRealisticProducts(Guid tenantId, DateTime now, Random random)
    {
        var products = new List<Product>();
        var categories = new[] { "Antibiotics", "Painkillers", "Vitamins", "Cardiac", "Diabetic", "Respiratory", "Gastric", "Antiseptic" };
        var manufacturers = new[] 
        { 
            "GSK Pakistan", "Pfizer Pakistan", "Searle Pakistan", "Abbott Laboratories", 
            "Getz Pharma", "Hilton Pharma", "Martin Dow", "PharmEvo", 
            "Novartis Pakistan", "Sanofi Pakistan" 
        };

        // Define realistic medicines
        var medicines = new[]
        {
            // Antibiotics
            new { Name = "Augmentin", Salt = "Amoxicillin + Clavulanic Acid", Pack = "625mg Tab", Category = "Antibiotics", Trade = 245m, Mrp = 280m },
            new { Name = "Azithral", Salt = "Azithromycin", Pack = "500mg Tab", Category = "Antibiotics", Trade = 180m, Mrp = 210m },
            new { Name = "Flagyl", Salt = "Metronidazole", Pack = "400mg Tab", Category = "Antibiotics", Trade = 95m, Mrp = 115m },
            new { Name = "Ciproxin", Salt = "Ciprofloxacin", Pack = "500mg Tab", Category = "Antibiotics", Trade = 165m, Mrp = 195m },
            new { Name = "Klacid", Salt = "Clarithromycin", Pack = "250mg Tab", Category = "Antibiotics", Trade = 320m, Mrp = 365m },
            
            // Painkillers
            new { Name = "Panadol", Salt = "Paracetamol", Pack = "500mg Tab", Category = "Painkillers", Trade = 45m, Mrp = 55m },
            new { Name = "Brufen", Salt = "Ibuprofen", Pack = "400mg Tab", Category = "Painkillers", Trade = 85m, Mrp = 105m },
            new { Name = "Ponstan", Salt = "Mefenamic Acid", Pack = "500mg Cap", Category = "Painkillers", Trade = 125m, Mrp = 150m },
            new { Name = "Disprin", Salt = "Aspirin", Pack = "300mg Tab", Category = "Painkillers", Trade = 38m, Mrp = 48m },
            new { Name = "Synflex", Salt = "Naproxen", Pack = "275mg Tab", Category = "Painkillers", Trade = 95m, Mrp = 118m },
            
            // Vitamins
            new { Name = "Centrum", Salt = "Multivitamins", Pack = "30 Tabs", Category = "Vitamins", Trade = 850m, Mrp = 980m },
            new { Name = "Becosules", Salt = "Vitamin B Complex", Pack = "20 Caps", Category = "Vitamins", Trade = 245m, Mrp = 290m },
            new { Name = "Fefol", Salt = "Iron + Folic Acid", Pack = "30 Caps", Category = "Vitamins", Trade = 165m, Mrp = 195m },
            new { Name = "Ossopan", Salt = "Calcium + Vitamin D", Pack = "30 Tabs", Category = "Vitamins", Trade = 425m, Mrp = 495m },
            new { Name = "Evion", Salt = "Vitamin E", Pack = "400 IU Cap", Category = "Vitamins", Trade = 285m, Mrp = 335m },
            
            // Cardiac
            new { Name = "Concor", Salt = "Bisoprolol", Pack = "5mg Tab", Category = "Cardiac", Trade = 285m, Mrp = 330m },
            new { Name = "Crestor", Salt = "Rosuvastatin", Pack = "10mg Tab", Category = "Cardiac", Trade = 385m, Mrp = 445m },
            new { Name = "Lipitor", Salt = "Atorvastatin", Pack = "20mg Tab", Category = "Cardiac", Trade = 295m, Mrp = 345m },
            new { Name = "Norvasc", Salt = "Amlodipine", Pack = "5mg Tab", Category = "Cardiac", Trade = 195m, Mrp = 235m },
            new { Name = "Lopressor", Salt = "Metoprolol", Pack = "50mg Tab", Category = "Cardiac", Trade = 165m, Mrp = 198m },
            
            // Diabetic
            new { Name = "Glucophage", Salt = "Metformin", Pack = "500mg Tab", Category = "Diabetic", Trade = 125m, Mrp = 155m },
            new { Name = "Diamicron", Salt = "Gliclazide", Pack = "80mg Tab", Category = "Diabetic", Trade = 215m, Mrp = 258m },
            new { Name = "Januvia", Salt = "Sitagliptin", Pack = "100mg Tab", Category = "Diabetic", Trade = 685m, Mrp = 795m },
            new { Name = "Trajenta", Salt = "Linagliptin", Pack = "5mg Tab", Category = "Diabetic", Trade = 595m, Mrp = 695m },
            new { Name = "Amaryl", Salt = "Glimepiride", Pack = "2mg Tab", Category = "Diabetic", Trade = 185m, Mrp = 225m },
            
            // Respiratory
            new { Name = "Ventolin", Salt = "Salbutamol", Pack = "Inhaler", Category = "Respiratory", Trade = 245m, Mrp = 290m },
            new { Name = "Seretide", Salt = "Fluticasone + Salmeterol", Pack = "Inhaler", Category = "Respiratory", Trade = 1250m, Mrp = 1450m },
            new { Name = "Montair", Salt = "Montelukast", Pack = "10mg Tab", Category = "Respiratory", Trade = 285m, Mrp = 335m },
            new { Name = "Bro-Zedex", Salt = "Bromhexine + Terbutaline", Pack = "Syrup", Category = "Respiratory", Trade = 165m, Mrp = 198m },
            new { Name = "Rynathisol", Salt = "Chlorpheniramine", Pack = "4mg Tab", Category = "Respiratory", Trade = 65m, Mrp = 82m },
            
            // Gastric
            new { Name = "Nexium", Salt = "Esomeprazole", Pack = "40mg Tab", Category = "Gastric", Trade = 385m, Mrp = 445m },
            new { Name = "Risek", Salt = "Omeprazole", Pack = "20mg Cap", Category = "Gastric", Trade = 195m, Mrp = 235m },
            new { Name = "Ganaton", Salt = "Itopride", Pack = "50mg Tab", Category = "Gastric", Trade = 265m, Mrp = 315m },
            new { Name = "Motilium", Salt = "Domperidone", Pack = "10mg Tab", Category = "Gastric", Trade = 145m, Mrp = 178m },
            new { Name = "Gelusil", Salt = "Antacid", Pack = "Syrup", Category = "Gastric", Trade = 125m, Mrp = 155m },
            
            // Antiseptic
            new { Name = "Betadine", Salt = "Povidone Iodine", Pack = "Solution 120ml", Category = "Antiseptic", Trade = 285m, Mrp = 335m },
            new { Name = "Dettol", Salt = "Chloroxylenol", Pack = "Liquid 250ml", Category = "Antiseptic", Trade = 395m, Mrp = 465m },
            new { Name = "Savlon", Salt = "Cetrimide + Chlorhexidine", Pack = "Cream 30g", Category = "Antiseptic", Trade = 145m, Mrp = 178m },
            new { Name = "Soframycin", Salt = "Framycetin", Pack = "Cream 15g", Category = "Antiseptic", Trade = 125m, Mrp = 152m },
            new { Name = "Fucidin", Salt = "Fusidic Acid", Pack = "Cream 15g", Category = "Antiseptic", Trade = 365m, Mrp = 425m },
            
            // Additional medicines to reach 50
            new { Name = "Losartan", Salt = "Losartan Potassium", Pack = "50mg Tab", Category = "Cardiac", Trade = 165m, Mrp = 198m },
            new { Name = "Valsartan", Salt = "Valsartan", Pack = "80mg Tab", Category = "Cardiac", Trade = 285m, Mrp = 335m },
            new { Name = "Carvedilol", Salt = "Carvedilol", Pack = "12.5mg Tab", Category = "Cardiac", Trade = 195m, Mrp = 235m },
            new { Name = "Ramipril", Salt = "Ramipril", Pack = "5mg Tab", Category = "Cardiac", Trade = 175m, Mrp = 212m },
            new { Name = "Enalapril", Salt = "Enalapril", Pack = "10mg Tab", Category = "Cardiac", Trade = 145m, Mrp = 178m },
            new { Name = "Clopidogrel", Salt = "Clopidogrel", Pack = "75mg Tab", Category = "Cardiac", Trade = 295m, Mrp = 350m },
            new { Name = "Warfarin", Salt = "Warfarin Sodium", Pack = "5mg Tab", Category = "Cardiac", Trade = 125m, Mrp = 155m },
            new { Name = "Aspirin EC", Salt = "Aspirin Enteric Coated", Pack = "75mg Tab", Category = "Cardiac", Trade = 65m, Mrp = 82m },
            new { Name = "Digoxin", Salt = "Digoxin", Pack = "0.25mg Tab", Category = "Cardiac", Trade = 95m, Mrp = 118m },
            new { Name = "Furosemide", Salt = "Furosemide", Pack = "40mg Tab", Category = "Cardiac", Trade = 45m, Mrp = 58m }
        };

        for (int i = 0; i < medicines.Length; i++)
        {
            var med = medicines[i];
            var manufacturer = manufacturers[random.Next(manufacturers.Length)];
            var stockQty = random.Next(0, 500);
            var isActive = random.Next(100) < 95; // 95% active

            products.Add(new Product
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SkuCode = $"MED{(i + 1):D4}",
                Name = med.Name,
                Pack = med.Pack,
                Manufacturer = manufacturer,
                SaltComposition = med.Salt,
                Category = med.Category,
                TradePrice = med.Trade,
                Mrp = med.Mrp,
                StockQuantity = stockQty,
                IsActive = isActive,
                ImageUrl = $"https://via.placeholder.com/300x300?text={Uri.EscapeDataString(med.Name)}",
                CreatedAtUtc = now.AddDays(-random.Next(1, 180)),
                UpdatedAtUtc = now.AddDays(-random.Next(0, 30))
            });
        }

        return products;
    }

    private static List<Store> GetRealisticStores(Guid tenantId, DateTime now, Random random)
    {
        var stores = new List<Store>();
        var cities = new[] { "Rawalpindi", "Islamabad", "Lahore", "Karachi", "Peshawar", "Multan", "Faisalabad" };
        var storeNames = new[]
        {
            "City Medical Store", "Royal Pharmacy", "Medicare Pharmacy", "Health Plus Pharmacy",
            "Life Care Medical", "Cure Well Pharmacy", "Green Cross Pharmacy", "MedEx Pharmacy",
            "Wellness Pharmacy", "Prime Medical Store"
        };

        var areas = new[]
        {
            "Saddar", "Commercial Area", "Main Boulevard", "Mall Road", "Blue Area", 
            "Bahria Town", "DHA", "Gulberg", "Model Town", "Satellite Town"
        };

        var contactNames = new[]
        {
            "Ahmed Khan", "Bilal Hassan", "Usman Ali", "Zain Malik", "Fahad Raza",
            "Hassan Shah", "Imran Siddiqui", "Junaid Iqbal", "Kamran Ahmed", "Naveed Hussain"
        };

        for (int i = 0; i < 10; i++)
        {
            var city = cities[i % cities.Length];
            var area = areas[random.Next(areas.Length)];
            var status = i < 8 ? StoreApprovalStatus.Approved : (i == 8 ? StoreApprovalStatus.Pending : StoreApprovalStatus.Suspended);

            stores.Add(new Store
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                BusinessName = storeNames[i],
                AddressLine = $"{random.Next(1, 99)} {area}, {city}",
                City = city,
                LicenseNumber = $"PH-{city.Substring(0, 3).ToUpper()}-{2020 + i}-{random.Next(1000, 9999)}",
                ContactName = contactNames[i],
                Mobile = $"03{random.Next(10, 99)}-{random.Next(1000000, 9999999)}",
                ApprovalStatus = status,
                ApprovalNotes = status == StoreApprovalStatus.Approved ? "Verified and approved" : 
                               status == StoreApprovalStatus.Pending ? "Pending verification" : "Under review",
                CreatedAtUtc = now.AddDays(-random.Next(30, 365))
            });
        }

        return stores;
    }

    private static List<AppUser> GetRealisticUsers(
        Guid tenantId,
        List<Store> stores,
        PasswordHasher<AppUser> passwordHasher,
        string defaultPassword,
        DateTime now,
        Random random)
    {
        var users = new List<AppUser>();
        var firstNames = new[] { "Ahmed", "Ali", "Hassan", "Usman", "Bilal", "Zain", "Fahad", "Imran", "Kamran", "Naveed", "Junaid", "Tariq", "Rashid", "Salman", "Waqas", "Asim", "Faisal", "Shahid", "Rizwan", "Adnan" };
        var lastNames = new[] { "Khan", "Ali", "Hassan", "Shah", "Malik", "Ahmed", "Raza", "Hussain", "Iqbal", "Siddiqui" };

        int userIndex = 0;
        // Create 2 users per approved store (20 total)
        foreach (var store in stores.Where(s => s.ApprovalStatus == StoreApprovalStatus.Approved))
        {
            for (int i = 0; i < 2; i++)
            {
                var firstName = firstNames[userIndex % firstNames.Length];
                var lastName = lastNames[random.Next(lastNames.Length)];
                var email = $"{firstName.ToLower()}.{lastName.ToLower()}{userIndex}@{store.BusinessName.Replace(" ", "").ToLower()}.com";

                users.Add(new AppUser
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    Email = email,
                    PasswordHash = passwordHasher.HashPassword(new AppUser(), defaultPassword),
                    Role = UserRole.StoreUser,
                    StoreId = store.Id,
                    CreatedAtUtc = now.AddDays(-random.Next(10, 180))
                });

                userIndex++;
            }
        }

        return users;
    }

    private static List<Order> GetSampleOrders(
        Guid tenantId,
        List<Store> stores,
        List<Product> products,
        List<AppUser> users,
        DateTime now,
        Random random)
    {
        var orders = new List<Order>();
        var statuses = new[] 
        { 
            OrderStatus.Submitted, OrderStatus.Confirmed, OrderStatus.Processing, 
            OrderStatus.Dispatched, OrderStatus.Delivered 
        };

        // Create 5-10 orders for each approved store
        foreach (var store in stores.Where(s => s.ApprovalStatus == StoreApprovalStatus.Approved))
        {
            var storeUsers = users.Where(u => u.StoreId == store.Id).ToList();
            if (!storeUsers.Any()) continue;

            var orderCount = random.Next(5, 11);
            for (int i = 0; i < orderCount; i++)
            {
                var orderedBy = storeUsers[random.Next(storeUsers.Count)];
                var orderDate = now.AddDays(-random.Next(1, 90));
                var status = statuses[random.Next(statuses.Length)];

                // Select 2-8 random products for the order
                var orderProducts = products.OrderBy(_ => random.Next()).Take(random.Next(2, 9)).ToList();
                
                var orderLines = orderProducts.Select(p => new OrderLine
                {
                    Id = Guid.NewGuid(),
                    ProductId = p.Id,
                    ProductNameSnapshot = p.Name,
                    PackSnapshot = p.Pack,
                    UnitPriceSnapshot = p.TradePrice,
                    Quantity = random.Next(1, 20),
                    LineTotal = 0 // Will be calculated
                }).ToList();

                // Calculate line totals
                foreach (var line in orderLines)
                {
                    line.LineTotal = line.Quantity * line.UnitPriceSnapshot;
                }

                var totalAmount = orderLines.Sum(ol => ol.LineTotal);

                orders.Add(new Order
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    StoreId = store.Id,
                    SubmittedByUserId = orderedBy.Id,
                    Status = status,
                    TotalAmount = totalAmount,
                    Currency = "PKR",
                    Lines = orderLines,
                    SubmittedAtUtc = orderDate,
                    Notes = $"Test order #{orders.Count + 1}"
                });
            }
        }

        return orders;
    }
}
