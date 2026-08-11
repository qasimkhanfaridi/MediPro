# ?? MediPro - Test Status Report

## ? Current Status: NOT RUNNING

Both services are currently **stopped**.

---

## ?? How to Start the Projects

### ? FASTEST: Double-Click to Start

Navigate to `B:\MediPro` in File Explorer and **double-click**:
```
start-medipro.bat
```

This will open two PowerShell windows and start both services automatically.

---

### ?? Manual Start (Two Terminals)

#### Terminal 1 - Backend API:
```powershell
cd B:\MediPro\src\MediPro.Api
.\start-api.ps1
```

**Wait for this message:**
```
Now listening on: http://localhost:5020
Application started.
```

#### Terminal 2 - Frontend Web:
```powershell
cd B:\MediPro\src\MediPro.Web
.\start-web.ps1
```

**Wait for this message:**
```
?  Local:   http://localhost:5173/
```

---

## ? How to Test if Projects are Running

### Quick Test Commands:

Run these in PowerShell to check status:

```powershell
# Check Backend API
Invoke-WebRequest -Uri "http://localhost:5020/api/health" -UseBasicParsing

# Check Frontend Web
Invoke-WebRequest -Uri "http://localhost:5173" -UseBasicParsing
```

**Expected output if running:**
- Backend: `StatusCode: 200` 
- Frontend: `StatusCode: 200`

---

### Or use the status checker script:

```powershell
cd B:\MediPro
powershell -ExecutionPolicy Bypass -File .\test-status.ps1
```

---

## ?? Access URLs (Once Running)

| Service | URL | Purpose |
|---------|-----|---------|
| **Frontend Web** | http://localhost:5173 | Main application UI |
| **Backend API** | http://localhost:5020 | REST API |
| **API Health** | http://localhost:5020/api/health | Health check endpoint |
| **OpenAPI Spec** | http://localhost:5020/openapi/v1.json | API documentation |

---

## ?? Test Login Credentials

Once both services are running, open http://localhost:5173 and login:

**Admin Account:**
- Email: `admin@medipro.local`
- Password: `ChangeMe!12345`

**Demo Store Accounts:**
- Password: `ChangeMe!12345` (for all demo stores)
- Check logs or database for store emails

---

## ?? Testing Checklist

### Step 1: Start Services ?
- [ ] Backend API started on port 5020
- [ ] Frontend Web started on port 5173

### Step 2: Verify Services ?
```powershell
# Run this to verify both are running:
cd B:\MediPro
powershell -ExecutionPolicy Bypass -File .\test-status.ps1
```

### Step 3: Test Frontend ?
- [ ] Open http://localhost:5173
- [ ] Login page loads
- [ ] Login with admin credentials
- [ ] Dashboard/catalog page loads

### Step 4: Test Admin Features ?
- [ ] View product catalog
- [ ] Search/filter products
- [ ] Add/edit products (if admin)
- [ ] View orders
- [ ] Check notifications

### Step 5: Test Store Features ?
- [ ] Browse catalog
- [ ] Add items to cart
- [ ] View cart
- [ ] Submit order
- [ ] View order history

### Step 6: Test API Directly ?
```powershell
# Health check
Invoke-RestMethod -Uri "http://localhost:5020/api/health"

# Login
$body = @{
    email = "admin@medipro.local"
    password = "ChangeMe!12345"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5020/api/auth/login" -Method Post -Body $body -ContentType "application/json"
```

---

## ?? What to Look For

### Backend API Console:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5020
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
warn: MediPro.Api.Data.DbInitializer[0]
      Dev seed: created distributor admin admin@medipro.local
```

### Frontend Web Console:
```
VITE v5.x.x  ready in xxx ms

?  Local:   http://localhost:5173/
?  Network: use --host to expose
?  press h + enter to show help
```

---

## ?? Troubleshooting

### If Backend Doesn't Start:
1. Check if port 5020 is already in use:
   ```powershell
   Get-NetTCPConnection -LocalPort 5020
   ```
2. Check for CLR errors (already fixed in start scripts)
3. Verify .NET 9 is installed: `dotnet --version`

### If Frontend Doesn't Start:
1. Check if port 5173 is already in use
2. Verify node_modules installed: `Test-Path "B:\MediPro\src\MediPro.Web\node_modules"`
3. Run `npm install` if needed

### If Login Fails:
1. Verify backend is running (check http://localhost:5020/api/health)
2. Check browser console for CORS errors
3. Verify database was created: `Test-Path "B:\MediPro\src\MediPro.Api\medipro.db"`

---

## ?? Helpful Files

| File | Purpose |
|------|---------|
| `start-medipro.bat` | Double-click to start both services |
| `test-status.ps1` | Check if services are running |
| `START-HERE.md` | Quick start guide |
| `QUICK-START-GUIDE.md` | Detailed documentation |

---

## ?? Next Steps

1. **Start the services** using `start-medipro.bat`
2. **Verify they're running** using `test-status.ps1`
3. **Open browser** to http://localhost:5173
4. **Login** with admin credentials
5. **Start testing** the features!

---

**Need help? Check the logs in the PowerShell windows where the services are running.** ??
