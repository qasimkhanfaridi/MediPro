# Deploy MediPro for client testing (UAT)

**One public URL** — API + web app in a single Docker image (Fly.io free/cheap tier).  
**Project:** `B:\MediPro\src`

---

## What the client gets

| URL | Example |
|-----|---------|
| App (sign in, catalogue, admin) | `https://medipro-client-test.fly.dev` |
| Health check | `https://…/api/health` |

**First-time login (after you set secrets below):**

| Role | Email | Password |
|------|-------|----------|
| Distributor admin | `admin@medipro.local` | value you set in `DevSeed__AdminPassword` |
| Demo pharmacy (approved) | `store.approved1@demo.medipro.local` | `DevSeed__DemoStorePassword` |

Demo catalogue (~87 SKUs) and demo stores are created on **first** database init when seed flags are on.

---

## Prerequisites

1. [Fly.io account](https://fly.io/app/sign-up) (card may be required; small UAT often stays in free allowance).
2. [flyctl](https://fly.io/docs/hands-on/install-flyctl/) installed.
3. [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for local build test, optional).
4. PowerShell on Windows.

---

## Step 1 — Login and create app

```powershell
cd B:\MediPro\src
fly auth login
```

Pick a **unique app name** and edit `fly.toml` → `app = 'your-unique-name'`.

```powershell
fly apps create your-unique-name
fly volumes create medipro_data --app your-unique-name --region sin --size 1
```

`sin` (Singapore) is a reasonable default near Pakistan. List regions: `fly platform regions`.

---

## Step 2 — Set secrets (required before first deploy)

Generate a JWT key (32+ characters), e.g. in PowerShell:

```powershell
$jwt = -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 48 | ForEach-Object {[char]$_})
$jwt
```

Set secrets (replace passwords with strong values you share securely with the client):

```powershell
fly secrets set `
  Jwt__SigningKey="PASTE_YOUR_48_CHAR_RANDOM_KEY_HERE" `
  DevSeed__AdminPassword="ClientTestAdmin!2026" `
  DevSeed__DemoStorePassword="ClientTestStore!2026" `
  --app your-unique-name
```

**Do not commit passwords to git.**

### Optional — admin email alerts (AM6)

In-app notifications work without SMTP. To also email distributor admins on **new orders**, **new pending stores**, and **24h pending reminders**, set secrets (example [Resend](https://resend.com) SMTP):

```powershell
fly secrets set `
  Notifications__EmailEnabled=true `
  Notifications__SmtpHost=smtp.resend.com `
  Notifications__SmtpPort=587 `
  Notifications__UseStartTls=true `
  Notifications__SmtpUsername=resend `
  Notifications__SmtpPassword="YOUR_RESEND_API_KEY" `
  Notifications__FromEmail=alerts@your-verified-domain.com `
  Notifications__AdditionalAdminEmails="ops@client.com" `
  --app your-unique-name
```

`Notifications__AppBaseUrl` is already `https://medipro-uat.fly.dev` in `fly.toml`. Reminder job runs in-process every 15 minutes (configurable).

**Do not commit SMTP passwords to git.**

### Optional — free WhatsApp alerts (CallMeBot)

For a **free** pilot alert to one admin mobile (new order, pending store, 24h reminder):

1. On WhatsApp, message **CallMeBot** (+34 684 72 39 62) with: `I allow callmebot to send me messages`
2. CallMeBot replies with your **apikey**
3. Set Fly secrets (phone = country code + number, no `+`):

```powershell
fly secrets set `
  Notifications__WhatsAppEnabled=true `
  Notifications__CallMeBotPhone=923001234567 `
  Notifications__CallMeBotApiKey="YOUR_CALLMEBOT_KEY" `
  --app your-unique-name
```

Free for personal use; one recipient phone. For multi-admin or high volume, use Meta WhatsApp Business API (paid).

**Do not commit CallMeBot keys to git.**

---

## Step 3 — Deploy

```powershell
cd B:\MediPro\src
fly deploy --app your-unique-name
```

First deploy may take several minutes (Docker build on Fly).

Open:

```powershell
fly open --app your-unique-name
```

---

## Step 4 — Verify before client demo

| Check | Expected |
|-------|----------|
| `/api/health` | JSON `status` ok |
| Sign in as admin | Works |
| Admin → stores list | Demo stores visible |
| Catalogue | Products listed (not 0) |
| Store login `store.approved1@demo.medipro.local` | Approved, can browse |
| Submit order | Admin notification |

If catalogue is empty, sign in as admin → **Load demo products** (allowed when `AllowDemoCatalogEndpoint` is true).

---

## Step 5 — Share with client

Send:

1. **URL:** `https://your-unique-name.fly.dev`
2. **Admin** email/password (secure channel).
3. **Sample store** email/password for pharmacy flow.
4. **15-minute demo script** — see `13-CLIENT-DEMO-AND-ROADMAP.md` §3.

---

## Optional — deploy script

```powershell
cd B:\MediPro\src
.\deploy-client-test.ps1 -AppName your-unique-name
```

---

## Local Docker test (before Fly)

```powershell
cd B:\MediPro\src
docker build -t medipro:uat .
docker run --rm -p 8080:8080 `
  -e ASPNETCORE_ENVIRONMENT=Production `
  -e ConnectionStrings__DefaultConnection="Data Source=/tmp/MediPro.db" `
  -e Jwt__SigningKey="LOCAL_TEST_KEY_32_CHARS_MINIMUM____" `
  -e DevSeed__Enabled=true `
  -e DevSeed__AdminPassword="ChangeMe!12345" `
  -e DevSeed__SeedDemoCatalog=true `
  -e DevSeed__SeedDemoStores=true `
  -e DevSeed__DemoStorePassword="ChangeMe!12345" `
  -e DevSeed__AllowDemoCatalogEndpoint=true `
  medipro:uat
```

Open `http://localhost:8080`

---

## After client approves paid pilot

1. Set `DevSeed__Enabled=false` and remove demo passwords from secrets.
2. Change admin email/password via new admin user or DB migration script.
3. Import client’s real Excel catalogue.
4. Turn off `AllowDemoCatalogEndpoint` when demo buttons are no longer needed.
5. Custom domain: `fly certs add app.clientdomain.com`

---

## Troubleshooting

| Problem | Fix |
|---------|-----|
| 502 / app not listening | Check `fly logs`; wait for migrations on first boot |
| Empty catalogue | Admin → Load demo products; or redeploy with `SeedDemoCatalog` on empty DB only |
| 404 on `/api/products/filters` | Redeploy latest image (`fly deploy`) |
| DB lost after redeploy | Ensure volume mount `medipro_data` exists and `ConnectionStrings` points to `/data/MediPro.db` |
| CLR crash on Windows dev | Use Docker or `run-api.ps1` for local API |

---

## Files added for deployment

| File | Purpose |
|------|---------|
| `src/Dockerfile` | Build API + SPA |
| `src/fly.toml` | Fly app config |
| `src/.dockerignore` | Smaller image |
| `src/deploy-client-test.ps1` | Helper script |
| `MediPro.Api/appsettings.Production.json` | Production defaults |
| `Program.cs` | SPA from `wwwroot`, Fly HTTPS headers |

---

*Client UAT deploy guide — pair with `12-PRODUCTION-DEPLOY-FREE.md` and `13-CLIENT-DEMO-AND-ROADMAP.md`.*
