# ?? MediPro - Running Instructions

## ? Backend Started Successfully!

I can see from the logs that:
- ? Database initialized
- ? Realistic data seeded (50 products, 10 stores, 20 users, orders)
- ? API listening on **http://localhost:5020**

---

## ?? To Run Both Projects:

### **Method 1: Two Separate PowerShell Windows (Recommended)**

**Window 1 - Backend API:**
```powershell
cd B:\MediPro\src\MediPro.Api
.\start-api.ps1
```

**Window 2 - Frontend Web:**
```powershell
cd B:\MediPro\src\MediPro.Web
.\start-web.ps1
```

---

### **Method 2: Using Batch File**

Double-click this file in Windows Explorer:
```
B:\MediPro\start-medipro.bat
```

This will open two windows automatically.

---

### **Method 3: Manual Commands**

**Terminal 1:**
```powershell
cd B:\MediPro\src\MediPro.Api
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:DOTNET_EnableWriteXorExecute="0"
dotnet bin\Debug\net9.0\MediPro.Api.dll --urls "http://localhost:5020"
```

**Terminal 2:**
```powershell
cd B:\MediPro\src\MediPro.Web
npm run dev
```

---

## ?? Access URLs

Once both are running:

| Service | URL |
|---------|-----|
| **Frontend** | http://localhost:5173 |
| **Backend API** | http://localhost:5020 |
| **API Health** | http://localhost:5020/api/health |

---

## ?? Login Credentials

### **Admin Account:**
```
Email: admin@medipro.local
Password: ChangeMe!12345
```

### **Store Users (20 available):**
```
Password: ChangeMe!12345 (same for all)

Examples:
- ahmed.khan0@citymedicalstore.com
- bilal.hassan2@royalpharmacy.com
- usman.ali4@medicarepharmacy.com
```

---

## ?? What's Available in Database

After the seeding I saw in the logs:
- ? **50 Products** across 8 categories
- ? **10 Pharmacy Stores** (8 approved, 1 pending, 1 suspended)
- ? **20 Store Users** (2 per approved store)
- ? **40-80 Sample Orders** with order lines

---

## ? Next Steps

1. **Open Terminal 1** and run:
   ```powershell
   cd B:\MediPro\src\MediPro.Api
   .\start-api.ps1
   ```

2. **Open Terminal 2** and run:
   ```powershell
   cd B:\MediPro\src\MediPro.Web
   .\start-web.ps1
   ```

3. **Open Browser** to http://localhost:5173

4. **Login** with admin credentials and explore!

---

## ?? Your Database is Ready!

The realistic test data has been seeded successfully:
- Real Pakistani medicines (Augmentin, Panadol, Glucophage, etc.)
- Actual pharmacy stores across Pakistan
- 20 test user accounts
- Sample orders with history

**Everything is ready for testing!** ??
