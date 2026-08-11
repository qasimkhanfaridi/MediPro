# ?? MediPro - Realistic Test Data Seeder

## ? Implementation Complete!

A comprehensive realistic data seeder has been created to populate your MediPro database with test data for development and testing.

---

## ?? What Gets Seeded

### **50 Realistic Products**
- Real Pakistani pharmaceutical products
- Authentic medicine names and salt compositions
- 8 categories: Antibiotics, Painkillers, Vitamins, Cardiac, Diabetic, Respiratory, Gastric, Antiseptic
- Realistic trade prices and MRP in PKR
- Stock quantities (0-500 units)
- Product images (placeholders)
- Real manufacturers (GSK, Pfizer, Abbott, Getz Pharma, etc.)

**Examples:**
- Augmentin 625mg (Amoxicillin + Clavulanic Acid)
- Panadol 500mg (Paracetamol)
- Glucophage 500mg (Metformin)
- Concor 5mg (Bisoprolol)
- Nexium 40mg (Esomeprazole)

### **10 Pharmacy Stores**
- Realistic store names and locations
- Cities: Rawalpindi, Islamabad, Lahore, Karachi, Peshawar, Multan, Faisalabad
- Valid license numbers
- Contact information
- Approval statuses:
  - 8 Approved stores
  - 1 Pending approval
  - 1 Suspended

**Store Examples:**
- City Medical Store, Saddar Rawalpindi
- Royal Pharmacy, Blue Area Islamabad
- Medicare Pharmacy, Mall Road Lahore
- Health Plus Pharmacy, DHA Karachi

### **20 Store Users (Sales People)**
- 2 users per approved store
- Realistic Pakistani names
- Email addresses based on store names
- Login enabled with default password
- Profile images (placeholders ready)

**User Examples:**
- ahmed.khan@citymedical.com
- bilal.hassan@royalpharmacy.com
- usman.ali@medicare.com

### **40-80 Sample Orders**
- 5-10 orders per approved store
- Various order statuses (Submitted, Confirmed, Processing, Dispatched, Delivered)
- 2-8 products per order
- Realistic quantities and amounts
- Historical order dates (last 90 days)

---

## ?? How to Enable

### **Option 1: Already Enabled (Default)**

The seeder is already enabled in `appsettings.Development.json`:

```json
"DevSeed": {
  "Enabled": true,
  "AdminEmail": "admin@medipro.local",
  "AdminPassword": "ChangeMe!12345",
  "SeedDemoCatalog": true,
  "SeedDemoStores": true,
  "DemoStorePassword": "ChangeMe!12345",
  "AllowDemoCatalogEndpoint": true,
  "SeedRealisticData": true  ? NEW!
}
```

### **Option 2: Manual Configuration**

If you want to control it separately, edit `appsettings.Development.json`:

```json
"DevSeed": {
  "SeedRealisticData": true   // Enable realistic data
}
```

---

## ?? Running the Seeder

### **Automatic (On First Run)**

The seeder runs automatically when you start the API for the first time with an empty database:

```powershell
cd B:\MediPro\src\MediPro.Api
.\start-api.ps1
```

**Expected console output:**
```
warn: MediPro.Api.Data.DbInitializer[0]
      Dev seed: created distributor admin admin@medipro.local
warn: MediPro.Api.Data.DbInitializer[0]
      Dev seed: inserted realistic test data (50 products, 10 stores, 20 users)
```

### **Re-seeding (If Needed)**

If you need to reset and re-seed the database:

```powershell
# Delete the database
Remove-Item B:\MediPro\src\MediPro.Api\medipro.db -Force

# Restart the API
cd B:\MediPro\src\MediPro.Api
.\start-api.ps1
```

The seeder will run again and create fresh data.

---

## ?? Test Credentials

### **Admin Account**
```
Email: admin@medipro.local
Password: ChangeMe!12345
Role: Distributor Admin
```

### **Store Users (20 total)**
```
Password: ChangeMe!12345 (for all)

Example logins:
- ahmed.khan0@citymedicalstore.com
- ali.hassan1@citymedicalstore.com
- hassan.malik2@royalpharmacy.com
- usman.shah3@royalpharmacy.com
... (and 16 more)
```

---

## ?? Seeded Data Structure

### Products (50)
```
MED0001 - Augmentin 625mg Tab
MED0002 - Azithral 500mg Tab
MED0003 - Flagyl 400mg Tab
...
MED0050 - Furosemide 40mg Tab
```

**Fields:**
- SKU Code (MED0001-MED0050)
- Product Name
- Salt Composition
- Pack Size
- Manufacturer
- Category
- Trade Price (PKR)
- MRP (PKR)
- Stock Quantity (0-500)
- Image URL (placeholder)
- Active Status

### Stores (10)
```
1. City Medical Store - Rawalpindi ? Approved
2. Royal Pharmacy - Islamabad ? Approved
3. Medicare Pharmacy - Lahore ? Approved
4. Health Plus Pharmacy - Karachi ? Approved
5. Life Care Medical - Peshawar ? Approved
6. Cure Well Pharmacy - Multan ? Approved
7. Green Cross Pharmacy - Faisalabad ? Approved
8. MedEx Pharmacy - Rawalpindi ? Approved
9. Wellness Pharmacy - Islamabad ? Pending
10. Prime Medical Store - Lahore ? Suspended
```

**Fields:**
- Business Name
- Full Address
- City
- License Number
- Contact Name
- Mobile Number
- Approval Status
- Approval Notes

### Users (20)
```
Store 1: ahmed.khan0@..., ali.hassan1@...
Store 2: hassan.malik2@..., usman.shah3@...
Store 3: bilal.ahmed4@..., zain.raza5@...
...
Store 8: shahid.siddiqui14@..., rizwan.iqbal15@...
```

**Fields:**
- Email (store-based)
- Password (same for all)
- Role (StoreUser)
- Store ID (linked)
- Created date

### Orders (40-80)
Each approved store has 5-10 sample orders with:
- Random products (2-8 per order)
- Various statuses
- Realistic quantities
- Total amounts calculated
- Historical dates (last 90 days)

---

## ?? Product Images & Profile Pictures

### Product Images
Currently using placeholder URLs:
```
https://via.placeholder.com/300x300?text=ProductName
```

### Profile Pictures (Ready to Add)
The seeder is prepared to support profile pictures. To add:

1. Place profile images in `wwwroot/images/profiles/`
2. Update user records with image URLs
3. Images can be added via admin panel or API

**Suggested structure:**
```
wwwroot/
  images/
    profiles/
      user-{guid}.jpg
    products/
      product-{sku}.jpg
```

---

## ?? Testing Scenarios

### **Scenario 1: Admin Testing**
1. Login as admin@medipro.local
2. View all 50 products in catalog
3. See 10 stores (8 approved, 1 pending, 1 suspended)
4. View orders from all stores
5. Approve/reject pending stores

### **Scenario 2: Store User Testing**
1. Login as any store user
2. Browse product catalog (50 items)
3. Add products to cart
4. Submit order
5. View order history (5-10 existing orders)

### **Scenario 3: Multi-Store Testing**
1. Test with different stores
2. Verify data isolation
3. Check order history per store
4. Test approval workflows

### **Scenario 4: Product Management**
1. Search through 50 products
2. Filter by category (8 categories)
3. Sort by price/name
4. Check stock levels
5. View product images

---

## ?? Customization

### Adding More Products

Edit `MediPro.Api/Data/RealisticDataSeeder.cs`:

```csharp
var medicines = new[]
{
    new { Name = "Your Product", Salt = "Active Ingredient", 
          Pack = "Pack Size", Category = "Category", 
          Trade = 100m, Mrp = 120m },
    // Add more...
};
```

### Adding More Stores

Edit the `GetRealisticStores` method:

```csharp
var storeNames = new[]
{
    "Your Pharmacy Name",
    // Add more...
};
```

### Changing Default Password

Edit `appsettings.Development.json`:

```json
"DevSeed": {
  "AdminPassword": "YourNewPassword123!",
  "DemoStorePassword": "YourNewPassword123!"
}
```

---

## ?? Database Statistics (After Seeding)

```
? 1 Tenant
? 1 Admin User
? 50 Products (across 8 categories)
? 10 Pharmacy Stores
? 20 Store Users (Sales People)
? 40-80 Sample Orders
? 160-640 Order Lines
? Various statuses and dates
```

**Total Records: ~300-800** depending on random order generation

---

## ?? Benefits

### ? **Realistic Data**
- Real medicine names and compositions
- Authentic Pakistani pharmaceutical context
- Realistic prices in PKR
- Proper categorization

### ? **Complete Test Coverage**
- All user roles represented
- Various order statuses
- Multiple stores and locations
- Historical data for reporting

### ? **Development Ready**
- Immediate testing capability
- No manual data entry needed
- Consistent across environments
- Easy to reset and refresh

### ? **Demo Ready**
- Impressive for presentations
- Real-world scenario simulation
- Professional appearance
- Client demo capability

---

## ?? Files Created

| File | Purpose |
|------|---------|
| `MediPro.Api/Data/RealisticDataSeeder.cs` | Main seeder implementation |
| `MediPro.Api/Configuration/DevSeedOptions.cs` | Updated with new option |
| `MediPro.Api/Data/DbInitializer.cs` | Updated to call seeder |
| `appsettings.Development.json` | Enabled realistic data seeding |
| `REALISTIC-DATA-SEEDER-GUIDE.md` | This documentation |

---

## ?? Next Steps

### 1. **Start the API**
```powershell
cd B:\MediPro\src\MediPro.Api
.\start-api.ps1
```

### 2. **Verify Data**
- Check console logs for "inserted realistic test data"
- Login as admin
- Browse products, stores, users
- Check sample orders

### 3. **Start Testing**
- Login with different user accounts
- Test all features with realistic data
- Create new orders
- Test search and filtering

### 4. **Add Real Images (Optional)**
- Replace placeholder URLs
- Add product photos
- Add user profile pictures
- Update image paths in database

---

## ?? Ready to Test!

Your MediPro application now has:
- ? 50 realistic pharmaceutical products
- ? 10 pharmacy stores across Pakistan
- ? 20 sales people/store users
- ? 40-80 sample orders with history
- ? Realistic data structure
- ? Ready for immediate testing
- ? Demo-ready presentation data

**Start the API and explore your fully populated database!** ??

---

**Note:** All seeded data is for development/testing only. In production, disable `SeedRealisticData` and use real data entry workflows.
