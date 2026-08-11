# Production deployment plan (free tier) — MediPro

**Goal:** Put the current MVP online at **$0/month** for a small pilot (few pharmacies, low traffic).  
**Stack today:** ASP.NET Core 9 API + SQLite + React (Vite) SPA.  
**Project root:** `B:\MediPro`

This is a **plan**, not an automated pipeline. Pick **one** option below and follow its checklist.

---

## 1. What “free” really means

| Expectation | Reality on free hosting |
|-------------|-------------------------|
| Always fast | Free tiers often **sleep** after idle (Render, Railway hobby) or have **CPU/RAM caps**. |
| Unlimited users | Fine for **pilot** (tens of stores), not national scale. |
| SQLite on server | Works if the DB file lives on a **persistent disk** (volume), not ephemeral container storage. |
| HTTPS | Usually **included** (platform or Cloudflare). |
| Email/SMS | **Not included** — use separate free tiers later (e.g. Resend, Twilio trial) or skip for pilot. |

**Pilot recommendation:** **Oracle Cloud Always Free VM** (always on, persistent disk) **or** **Fly.io** (simple Docker, small persistent volume).  
**Fastest to try:** **Render (API) + Cloudflare Pages (web)** — accept cold starts on the API.

---

## 2. Pre-deploy checklist (all options)

Do these **before** any public URL:

| # | Task | Why |
|---|------|-----|
| 1 | Turn off dev seed in production | `DevSeed:Enabled: false`, empty `AdminPassword`, `SeedDemoCatalog` / `SeedDemoStores` false |
| 2 | Set strong `Jwt:SigningKey` | Env var / secret manager — **never** commit; 32+ random characters |
| 3 | Create production admin manually | One distributor admin via DB script or one-time secure bootstrap — not `ChangeMe!12345` |
| 4 | Set `Cors:AllowedOrigins` | Only your real SPA URL(s), e.g. `https://app.yourdomain.com` |
| 5 | Set `ASPNETCORE_ENVIRONMENT=Production` | Disables dev-only demo endpoints unless you explicitly enable them |
| 6 | Backup `MediPro.db` | Daily copy off-server (cron + object storage or manual for pilot) |
| 7 | Custom domain (optional) | Cloudflare free DNS + SSL in front of app |
| 8 | Smoke test | Login admin → approve store → catalogue → cart → submit order |

**Do not** rely on SQLite for multi-region HA; for pilot in one city it is acceptable.

---

## 3. Recommended architecture (free)

```text
                    ┌─────────────────────┐
   Users (mobile)   │  Cloudflare (free)   │  DNS + HTTPS + optional CDN
        │           └──────────┬──────────┘
        ▼                      │
   ┌────────────┐      ┌───────▼────────┐
   │  React SPA │      │  ASP.NET API    │
   │  (static)  │─────►│  + SQLite file  │
   └────────────┘      │  on persistent  │
        HTTPS           │  volume / disk  │
                        └─────────────────┘
```

**Two URLs (simplest operationally):**

- `https://app.<yourdomain>` → static SPA (Cloudflare Pages)
- `https://api.<yourdomain>` → API (Fly.io / Render / VM)

Set in SPA build: `VITE_API_ORIGIN=https://api.<yourdomain>` (already supported in `MediPro.Web` via `apiUrl` / `apiFetch`).

**One URL (fewer moving parts):** host SPA `dist` inside API `wwwroot` and serve from Kestrel (requires a small code change — see §7).

---

## 4. Option A — Oracle Cloud Always Free (best “always on” $0)

**Cost:** $0 if you stay within [Always Free limits](https://www.oracle.com/cloud/free/) (e.g. Ampere VM or small AMD VM).  
**Pros:** VM does not sleep; full control; SQLite on block volume.  
**Cons:** Sign-up and console learning curve; you manage OS updates.

### Steps (summary)

1. Create OCI account → create **Always Free** compute instance (Ubuntu 22.04).
2. Open firewall: **80/443** (and **5020** only if testing without reverse proxy).
3. Install **Docker** (or .NET 9 runtime directly).
4. Build & publish API on the VM (or pull image from GHCR).
5. Mount volume for `/data/MediPro.db` → connection string `Data Source=/data/MediPro.db`.
6. Install **Caddy** or **nginx** as reverse proxy with automatic HTTPS (Let’s Encrypt).
7. Build SPA with `VITE_API_ORIGIN=https://api.yourdomain` → deploy `dist` to **Cloudflare Pages** (still free) or serve from nginx on same VM.

### Production env vars (API)

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:8080
ConnectionStrings__DefaultConnection=Data Source=/data/MediPro.db
Jwt__SigningKey=<long-random-secret>
Cors__AllowedOrigins__0=https://app.yourdomain.com
DevSeed__Enabled=false
```

### Backup

```bash
# cron daily: copy DB to home or rclone to free tier storage
cp /data/MediPro.db /backup/MediPro-$(date +%F).db
```

---

## 5. Option B — Fly.io (good balance, Docker)

**Cost:** Free allowance with card on file; small apps often stay within free credits.  
**Pros:** Docker deploy, **persistent volume** for SQLite, HTTPS included.  
**Cons:** Must package app; monitor usage.

### Steps (summary)

1. Install [flyctl](https://fly.io/docs/hands-on/install-flyctl/).
2. Add `Dockerfile` in `MediPro.Api` (multi-stage: `dotnet publish` → runtime image).
3. `fly volumes create medipro_data --size 1` (region near users, e.g. `sin` if available).
4. Mount volume at `/data`, connection string `Data Source=/data/MediPro.db`.
5. Set secrets: `Jwt__SigningKey`, `Cors__AllowedOrigins__0`, disable DevSeed.
6. Deploy API: `fly deploy`.
7. Deploy SPA to **Cloudflare Pages**; set `VITE_API_ORIGIN=https://<app>.fly.dev`.

### Fly notes

- Run migrations on startup (already in `DbInitializer`) — first boot creates schema.
- Scale to **0** is optional; for pilot keep **1** machine so stores do not hit cold start.

---

## 6. Option C — Render + Cloudflare Pages (easiest setup)

**Cost:** $0 on Render free web service + Cloudflare Pages.  
**Pros:** Git-connected deploy, minimal ops.  
**Cons:** API **spins down** after ~15 min idle → first request slow (bad for demo unless you ping it).

### API (Render)

1. New **Web Service** → connect GitHub repo `MediPro.Api`.
2. Runtime: **Docker** or **.NET**.
3. Add **disk** (if available on plan) for SQLite path; if no disk, SQLite resets on redeploy — **use Render only with persistent disk** or switch to Option A/B.
4. Environment variables as in §4.
5. Health check path: `/api/health`.

### Web (Cloudflare Pages)

1. Build command: `cd MediPro.Web && npm ci && npm run build`
2. Output directory: `MediPro.Web/dist`
3. Environment variable: `VITE_API_ORIGIN=https://<your-render-service>.onrender.com`
4. Custom domain: `app.yourdomain.com`

### Keep-alive (optional, hacky)

Free cron (e.g. cron-job.org) hits `/api/health` every 10 minutes — reduces sleep; not guaranteed forever.

---

## 7. Optional: single host (API serves SPA)

To use **one URL** and avoid CORS split:

1. `dotnet publish` API.
2. Copy `MediPro.Web/dist/*` → `MediPro.Api/wwwroot/`.
3. In `Program.cs`: `app.UseDefaultFiles(); app.UseStaticFiles(); app.MapFallbackToFile("index.html");` (after `MapControllers`, before `Run`).
4. Deploy only the API container/VM; set `VITE_API_ORIGIN` empty in build so SPA uses same origin `/api`.

Good for Fly.io or Oracle with one domain.

---

## 8. Build commands (reference)

From `B:\MediPro\src`:

```powershell
# API
dotnet publish MediPro.Api\MediPro.Api.csproj -c Release -o .\publish\api

# Web (point at production API)
cd MediPro.Web
$env:VITE_API_ORIGIN="https://api.yourdomain.com"
npm ci
npm run build
# dist/ → Cloudflare Pages or wwwroot/
```

---

## 9. Security minimum for public pilot

| Item | Action |
|------|--------|
| TLS | Force HTTPS at proxy (Caddy/nginx/Cloudflare) |
| Secrets | Env vars only; rotate JWT key if leaked |
| Admin password | Strong, unique; not demo password |
| Rate limit | Add later (middleware) on `/api/auth/login` |
| File upload | Excel import already size-limited in code — keep admin-only |
| Logs | Do not log passwords or JWTs |

---

## 10. Suggested timeline (1–2 weeks)

| Week | Task |
|------|------|
| 1 | Pick Option A or B; Dockerfile + production `appsettings` overrides; deploy staging URL |
| 1 | Cloudflare Pages + `VITE_API_ORIGIN`; CORS + JWT on API |
| 1 | Manual UAT on production URL (admin + approved store) |
| 2 | Backup cron; custom domain; pilot store onboarding (3–5 pharmacies) |
| 2 | Document support channel + rollback (restore DB file) |

---

## 11. When to pay (later)

Move off “totally free” when you need:

- **SQL Server / PostgreSQL** (managed DB, no SQLite file ops)
- **No cold starts** and SLA
- **SMS/email** at volume
- **Multiple regions** or high availability

Typical next step: **~$5–25/month** VPS (Hetzner, DigitalOcean) or managed DB + small App Service — still cheap, not free.

---

## 12. Quick decision guide

| If you want… | Choose |
|--------------|--------|
| Always on, full control, $0 long term | **Option A — Oracle Always Free VM** |
| Docker, simple deploy, persistent SQLite | **Option B — Fly.io** |
| Fastest first deploy, okay with sleep | **Option C — Render + Cloudflare Pages** |
| One domain, fewer CORS issues | **§7 single host** on A or B |

---

## 13. Related docs

- Milestones: `04-MILESTONES-AND-PHASES.md` (Phase 2 pilot = production go-live **M2.1**)
- Architecture: `05-TECHNICAL-ARCHITECTURE.md`
- Run locally: `src/README.md`

---

*Revision: initial free production deployment plan for current SQLite MVP.*
