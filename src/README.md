# MediPro — implementation (`src`)

Stack: **ASP.NET Core 9 Web API** (`MediPro.Api`) + **React + TypeScript** (`MediPro.Web`, Vite 6).

Product specs: **`B:\MediPro\docs\`** (BRD, PRD, milestones).

## What is implemented now

| Area | Details |
|------|---------|
| **Database** | **EF Core 9 + SQLite** (`MediPro.db`). Migrations under `MediPro.Api/Data/Migrations` (includes **Cart** / **CartLine**). |
| **Bootstrap** | Migrations on start, default **tenant**, optional **DevSeed** admin (`appsettings.Development.json`). |
| **Auth** | JWT — `POST /api/auth/login`, `POST /api/auth/register-store` (store **Pending**). |
| **Roles** | `DistributorAdmin`, `StoreUser`. |
| **Admin stores** | `GET /api/admin/stores`, `PATCH /api/admin/stores/{id}/approval`. |
| **Catalog** | `GET /api/products` (paged, `search`); `POST /api/products` (admin). Store catalog requires **Approved** store. |
| **Excel import** | `GET /api/admin/catalog/import-template` (anonymous — column layout). `POST /api/admin/catalog/import` multipart `file` (**.xlsx**, admin JWT). Row-level errors; valid rows inserted. |
| **Cart** | `GET /api/cart`, `POST /api/cart/items`, `PATCH /api/cart/items/{productId}`, `DELETE /api/cart/items/{productId}`, `DELETE /api/cart` — **approved StoreUser** only. |
| **Orders** | `POST /api/orders/submit` (from cart, price snapshots, optional **stock** decrement), `GET /api/orders`, `GET /api/orders/{id}`, `PATCH /api/orders/{id}/status` (admin; **allowed status transitions**). |
| **Admin alerts** | `GET /api/admin/notifications`, `PATCH /api/admin/notifications/{id}/read`. Notifications for admins on **new order** and **new store registration**. |
| **Health** | `GET /api/health`. |
| **Web dev shell** | React: login, products, cart/order (store), **Excel import + notifications** (admin), orders list; `localStorage` auth summary. |

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) 9.x  
- [Node.js](https://nodejs.org/) 20.19+ or 22.x (recommended for tooling)  
- Optional: `dotnet tool update -g dotnet-ef` (match 9.x for migrations)

## Configuration

- **Connection string:** `ConnectionStrings:DefaultConnection` in `MediPro.Api/appsettings.json` (SQLite file path).  
- **JWT:** `Jwt` section — set **`SigningKey`** to a long random string (use [user-secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) or environment variables for real deployments).  
- **Dev admin:** `MediPro.Api/appsettings.Development.json` → `DevSeed` creates `admin@medipro.local` / `ChangeMe!12345` on **first database creation only**. Change the password locally.

## Deploy for client testing (UAT)

See **`B:\MediPro\docs\DEPLOY-CLIENT-TESTING.md`** — one URL on Fly.io (Docker image includes API + web).

```powershell
cd B:\MediPro\src
fly auth login
# Edit fly.toml app name, then:
fly apps create your-app-name
fly volumes create medipro_data --app your-app-name --region sin --size 1
.\deploy-client-test.ps1 -AppName your-app-name -SetSecrets
```

---

## Run locally (two terminals)

**Terminal 1 — API** (`http://localhost:5020` from launch profile):

```powershell
cd B:\MediPro\src
dotnet run --project MediPro.Api
```

If **`dotnet run` exits immediately** with a **CLR / shadow-stack** error on Windows 10, use the workaround script (runs `dotnet exec` from the API folder):

```powershell
cd B:\MediPro\src
.\run-api.ps1
```

**Terminal 2 — web** (`http://localhost:5173`):

```powershell
cd B:\MediPro\src\MediPro.Web
npm install
npm run dev
```

The SPA proxies `/api` to the API. Use **`MediPro.Api/MediPro.Api.http`** in VS / REST Client to exercise auth and admin APIs with a bearer token.

## New EF migration (after model changes)

```powershell
cd B:\MediPro\src
dotnet ef migrations add YourMigrationName --project MediPro.Api --startup-project MediPro.Api --output-dir Data/Migrations
dotnet ef database update --project MediPro.Api --startup-project MediPro.Api
```

## Build note (Windows)

If **`dotnet build` fails with CS2012** (DLL in use), stop any running `dotnet run` / IIS Express using the API, or build **`Release`**:  
`dotnet build MediPro.sln -c Release`

## Next implementation steps (PRD / milestones)

1. **Outbound alerts** — email/SMS/push when orders or registrations arrive.  
2. **Import v2** — CSV, upsert by SKU, dry-run preview.  
3. **SQL Server / PostgreSQL** for production.  
4. **Refresh tokens**, audit, hardened deployment.  
5. **React** — proper routing, store/admin layouts, production build hosting.

## Git

```powershell
cd B:\MediPro\src
git init
```

`.gitignore` ignores `*.db` and build artifacts.
