# Admin functionality — milestones

**Audience:** Delivery team, client sponsor, ops.  
**UAT:** https://medipro-uat.fly.dev  
**Related:** `04-MILESTONES-AND-PHASES.md`, `14-CLIENT-DEMO-FEEDBACK-BACKLOG.md`

---

## Status legend

| Symbol | Meaning |
|--------|---------|
| ✅ | Shipped on UAT |
| 🟡 | Partial |
| 🔲 | Planned / not started |
| ❌ | Out of scope for current phase |

---

## Overview

```text
AM1 Stores          ✅
AM2 Catalogue       ✅ (add/deactivate UI; edit via API PATCH)
AM3 Orders          ✅
AM4 Bonus schemes   ✅ (display; cart math later)
AM5 Stock model     ✅
AM6 Alerts          ✅ (email via SMTP config; WhatsApp later)
AM7 Pilot pack      🟡
AM8 Internal-grade  🔲 Phase C
```

---

## AM1 — Distributor console foundation ✅

| # | Deliverable | UI / API | Status |
|---|-------------|----------|--------|
| 1.1 | Admin JWT login | `POST /api/auth/login` | ✅ |
| 1.2 | Console hub | `/admin` | ✅ |
| 1.3 | List stores | `GET /api/admin/stores` | ✅ |
| 1.4 | Approve / reject store | `POST /api/admin/stores/{id}/approval` | ✅ |
| 1.5 | Demo stores seed | `POST /api/admin/stores/seed-demo` | ✅ |

**Exit:** Only approved pharmacies can order.

---

## AM2 — Catalogue operations ✅

| # | Deliverable | UI / API | Status |
|---|-------------|----------|--------|
| 2.1 | Excel import + template | `/admin`, `POST /api/admin/catalog/import` | ✅ |
| 2.2 | Low-stock report | `GET /api/admin/catalog/low-stock` | ✅ |
| 2.3 | Stock adjust by SKU (legacy qty) | `POST /api/admin/catalog/stock-adjustment` | ✅ |
| 2.4 | Demo catalogue seed | `POST /api/admin/catalog/seed-demo-catalog` | ✅ |
| 2.5 | Add product (form) | `/admin/products`, `POST /api/products` | ✅ |
| 2.6 | Edit product | `PATCH /api/products/{id}` | ✅ |
| 2.7 | Deactivate product | `POST /api/products/{id}/deactivate` | ✅ |

**Exit:** Day-to-day SKU changes without Excel.

---

## AM3 — Orders operations ✅

| # | Deliverable | UI / API | Status |
|---|-------------|----------|--------|
| 3.1 | Orders console | `/admin/orders` | ✅ |
| 3.2 | Line items (expand) | `GET /api/orders/{id}` | ✅ |
| 3.3 | Filters: city, area, store, status, dates | `GET /api/orders` | ✅ |
| 3.4 | Update status | `POST /api/orders/{id}/status` | ✅ |
| 3.5 | Demo orders seed | `POST /api/admin/orders/seed-demo` | ✅ |
| 3.6 | Export CSV | — | 🔲 |
| 3.7 | SKU snapshot on order lines | — | 🔲 |

**Exit:** Ops can process hundreds of orders per day.

---

## AM4 — Bonus / offers ✅

| # | Deliverable | UI / API | Status |
|---|-------------|----------|--------|
| 4.1 | CRUD bonus schemes | `/admin/bonus-schemes` | ✅ |
| 4.2 | Product-specific (e.g. 10+1 on one SKU) | `BonusScheme.ProductId` required | ✅ |
| 4.3 | ~~Company-wide~~ | Removed — bonus is per product only | ❌ |
| 4.4 | Store banners + product badges | `/catalog`, `GET /api/bonus-schemes` | ✅ |
| 4.5 | Free qty on checkout | — | 🔲 |

**Exit:** Offers visible on store portal; optional auto-free qty later.

---

## AM5 — Stock model (simplified) ✅

| # | Deliverable | Status |
|---|-------------|--------|
| 5.1 | Store: Available / Out of stock only (hide qty) | ✅ |
| 5.2 | Admin: Set in stock / out of stock | ✅ `POST /api/admin/catalog/stock-status` |
| 5.3 | Grey out + block add-to-cart when out | ✅ |
| 5.4 | Config: `Inventory:AutoDecrementOnOrder` (default off) | ✅ |

**Exit:** Web stock does not conflict with external billing.

---

## AM6 — Alerts & reminders ✅

| # | Deliverable | Status |
|---|-------------|--------|
| 6.1 | In-app notifications | ✅ |
| 6.2 | Email on new order | ✅ (`Notifications:EmailEnabled` + SMTP) |
| 6.3 | Email on pending store | ✅ |
| 6.4 | 24h pending-store reminder job | ✅ hosted service + `PendingApprovalReminderSentAtUtc` |
| 6.5 | WhatsApp (CallMeBot, free pilot) | ✅ optional config |

**Config:** `Notifications` section in `appsettings.json` / Fly `[env]`. Email: set `EmailEnabled=true` and SMTP. WhatsApp (free): [CallMeBot](https://www.callmebot.com/blog/free-api-whatsapp-messages/) — activate on your phone, then set `WhatsAppEnabled=true`, `CallMeBotPhone`, `CallMeBotApiKey` via Fly secrets. One admin phone only; Meta Business API is paid for production scale.

---

## AM7 — Pilot go-live pack 🟡

| # | Deliverable | Status |
|---|-------------|--------|
| 7.1 | Fly UAT deploy | ✅ |
| 7.2 | Client catalogue import | 🔲 |
| 7.3 | Production passwords (no demo) | 🔲 |
| 7.4 | Admin quick-start guide | 🔲 |

---

## AM8 — Internal-grade admin 🔲

Audit log, multi-admin RBAC, PostgreSQL reporting, ERP export, staging CI/CD — see `13-CLIENT-DEMO-AND-ROADMAP.md` Phase C.

---

## Delivery order (sprints)

| Sprint | Milestones | Focus |
|--------|------------|--------|
| Done | AM1, AM3, AM4 | Orders, stores, offers |
| **Current** | **AM6** | Email + 24h reminder |
| Done | AM5, AM2 | Stock in/out + product UI |
| Pilot | AM7 | Real data + training |
| Later | AM8 | Enterprise |

---

## Revision history

| Version | Date | Notes |
|---------|------|-------|
| 1.0 | May 2026 | Initial admin milestone map |
| 1.1 | May 2026 | AM5/AM2 marked in progress |
