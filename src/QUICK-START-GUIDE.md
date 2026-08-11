# MediPro - Quick Start Guide

## ?? Running the Application

### The CLR Shadow Stack Issue
You encountered a .NET 9 runtime issue with shadow stacks. I've applied fixes to work around this.

### ? Solution: Use the PowerShell Scripts

I've created helper scripts for you:

---

## ?? Step-by-Step Instructions

### ?? EASIEST METHOD: Double-Click to Start Both

Simply **double-click** this file in File Explorer:
```
B:\MediPro\start-medipro.bat
```

This will open two PowerShell windows:
- One for the Backend API (port 5020)
- One for the Frontend Web (port 5173)

---

### Method 2: Manual - Two Separate Terminals

#### Step 1: Start the Backend API

Open a **PowerShell** terminal and run:

```powershell
cd B:\MediPro\src\MediPro.Api
.\start-api.ps1
```

OR run this command directly:

```powershell
cd B:\MediPro\src\MediPro.Api
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:DOTNET_EnableWriteXorExecute="0"
dotnet bin\Debug\net9.0\MediPro.Api.dll --urls "http://localhost:5020"
```

**Expected Output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5020
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

#### Step 2: Start the Frontend Web App

Open a **SECOND PowerShell** terminal and run:

```powershell
cd B:\MediPro\src\MediPro.Web
.\start-web.ps1
```

OR run this command directly:

```powershell
cd B:\MediPro\src\MediPro.Web
npm run dev
```

**Expected Output:**
```
VITE v5.x.x  ready in xxx ms

?  Local:   http://localhost:5173/
?  Network: use --host to expose
```

---

## ?? Default Test Credentials

### Admin Account
- **Email:** `admin@medipro.local`
- **Password:** `ChangeMe!12345`
- **Role:** Distributor Admin (full access)

### Demo Store Accounts
The system creates demo pharmacy/store accounts automatically.
- **Password:** `ChangeMe!12345` (same for all demo stores)
- Check the database or logs for store usernames/emails

---

## ?? Application URLs

| Component | URL | Description |
|-----------|-----|-------------|
| Frontend Web | http://localhost:5173 | React + TypeScript UI |
| Backend API | http://localhost:5020 | .NET 9 REST API |
| API Docs | http://localhost:5020/openapi/v1.json | OpenAPI specification |
| Health Check | http://localhost:5020/api/health | API health status |

---

## ?? Testing the Application

### 1. Login as Admin
- Navigate to http://localhost:5173
- Use admin credentials above
- Access admin features (catalog management, store approvals, orders)

### 2. Test Store User Flow
- Login with a demo store account
- Browse catalog
- Add products to cart
- Submit order
- View order history

### 3. API Testing
Use tools like Postman or curl to test API endpoints:

```bash
# Health check
curl http://localhost:5020/api/health

# Login
curl -X POST http://localhost:5020/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@medipro.local","password":"ChangeMe!12345"}'
```

---

## ?? Database Location

The SQLite database is created at:
```
B:\MediPro\src\MediPro.Api\medipro.db
```

You can view/edit it with tools like:
- DB Browser for SQLite
- Azure Data Studio with SQLite extension
- Visual Studio SQL Server Object Explorer

---

## ?? Troubleshooting

### If the API still crashes with CLR assertion:

**Option 1:** Use the DLL directly (recommended)
```powershell
cd B:\MediPro\src\MediPro.Api
dotnet bin\Debug\net9.0\MediPro.Api.dll --urls "http://localhost:5020"
```

**Option 2:** Update .NET 9 to the latest patch version
```powershell
# Check current version
dotnet --version

# Update to latest .NET 9
# Download from: https://dotnet.microsoft.com/download/dotnet/9.0
```

**Option 3:** Temporarily downgrade to .NET 8
Edit `MediPro.Api.csproj` and change:
```xml
<TargetFramework>net8.0</TargetFramework>
```

### If Frontend doesn't connect to API:

1. Verify backend is running on port 5020
2. Check CORS configuration in `Program.cs`
3. Check browser console for errors

### If Login Fails:

1. Verify database was created and seeded
2. Check API logs for errors
3. Verify credentials are correct

---

## ?? What's Implemented (MVP Status: ~85%)

### ? Backend Features
- Authentication & Authorization (JWT)
- Multi-tenant architecture
- Product catalog CRUD
- Order management with status workflow
- Shopping cart
- Admin notifications
- Store approval system
- Excel import for products

### ? Frontend Features
- Login/Authentication
- Product catalog browsing
- Product detail pages
- Shopping cart
- Order submission & history
- Admin console

### ? Pending for Production
- Complete QA testing
- Security audit
- API documentation
- User guides
- Monitoring/logging setup

---

## ?? Need Help?

If you encounter issues:
1. Check terminal output for error messages
2. Verify both API and Web are running
3. Check database was created successfully
4. Verify firewall isn't blocking localhost:5020 or localhost:5173

---

**Happy Testing! ??**
