# Client demo feedback — backlog (May 2026)

Feedback from the client UAT demo. Items are grouped by theme with **priority**, **scope**, and **status**.

---

## Priority legend

| P | Meaning | Target |
|---|---------|--------|
| **P0** | Demo blocker / daily ops pain | Next sprint |
| **P1** | Important for pilot | Phase 2 pilot |
| **P2** | Internal-grade / nice-to-have | Phase 3+ |

---

## 1. Admin order line details (P0)

**Feedback:** On admin orders, no product list — which SKUs, how many units.

**Current state:** API already returns `GET /api/orders/{id}` with `lines[]` (name, pack, qty, price). UI only showed summary (store, total, status).

**Done / in progress:**

- [x] Admin **Orders console** (`/admin/orders`) with expandable **line items** (product, pack, qty, price)
- [x] Admin filters: **city**, **store search**, **status**, **date range** (API + UI)
- [ ] Optional: add **SKU snapshot** on order lines at submit time (for historical accuracy if product renamed)

---

## 2. Admin order filters (P0)

**Feedback:** Hundreds of orders per day — filter by **area/city**, date range, status, etc.

**Planned filters:**

| Filter | API param | Notes |
|--------|-----------|--------|
| City | `city` | Matches store city (contains) — **shipped** |
| Store name search | `search` | Business name / mobile — **shipped** |
| Status | `status` | Submitted, Confirmed, … — **shipped** |
| Date from / to | `from`, `to` | Submitted date — **shipped** |
| Store id | `storeId` | Exact store |
| Page size | `pageSize` | Up to 100 for admin |

**Later:** route/area field on store (if city is not enough), export CSV.

---

## 3. Admin add / remove product one-by-one (P1)

**Feedback:** Need to add or remove a product without only Excel.

**Current state:** Admin UI at `/admin/products`; deactivate/activate endpoints; stock in/out via `POST /api/admin/catalog/stock-status`.

**Plan:**

- [x] Admin form: SKU, name, pack, manufacturer, prices, category
- [x] **Deactivate** product (`IsActive = false`) — “remove from catalogue” without deleting history
- [x] `PATCH /api/products/{id}` for edits (API; full edit UI optional later)

---

## 4. Stock model — hide quantity, manual in/out (P0 business, P1 build)

**Feedback:** Billing + manual orders in another system → web stock counts **mismatch**. Stores should **not see quantities**; only **in stock / out of stock**; out-of-stock products **greyed out**, not orderable.

**Plan:**

| Change | Detail | Status |
|--------|--------|--------|
| Store catalogue | Hide numeric `stockQuantity`; show **Available** / **Out of stock** only | ✅ |
| Admin | Toggle **In stock** / **Stocked out** (manual), not tied to auto-decrement | ✅ |
| Order submit | **Stop auto-decrement** of `StockQuantity` (config flag `Inventory:AutoDecrementOnOrder`) | ✅ |
| Cart / PDP | Block add when out of stock; grey card styling | ✅ |

**Note:** Admin may keep internal notes or separate ERP; MediPro becomes **order capture**, not system of record for qty.

---

## 5. Notifications — WhatsApp / email / SMS (P1)

**Feedback:** Notify on new orders / registrations via WhatsApp, email, or mobile.

**Free / low-cost options (Pakistan-friendly):**

| Channel | Free tier | Use case |
|---------|-----------|----------|
| **Email** | [Resend](https://resend.com) 100/day, [Brevo](https://www.brevo.com) 300/day | Admin inbox on new order |
| **WhatsApp** | [CallMeBot](https://www.callmebot.com/blog/free-api-whatsapp-messages/) (free, one admin phone) | Pilot alerts to ops mobile |
| **SMS** | Usually paid (Jazz, Twilio); trials only | OTP / urgent alerts |

**Plan:**

- [x] `NotificationOptions` in config (email on/off, admin addresses, SMTP)
- [x] Send email on `NotifyNewOrderSubmittedAsync` + new store pending (when `EmailEnabled`)
- [x] WhatsApp via CallMeBot when `WhatsAppEnabled` (free pilot — one phone)
- [ ] Phase 2: Meta WhatsApp Business API (paid at scale)

---

## 6. Pending store reminder after 24 hours (P1)

**Feedback:** If registration not approved/rejected, **remind admin after 24h**.

**Plan:**

- [x] Background job: `PendingStoreReminderBackgroundService` scans every 15 min (configurable)
- [x] `Store.PendingApprovalReminderSentAtUtc` — one reminder per pending store
- [x] In-app notification (`PendingStoreApprovalReminder`) + optional email
- [x] Do not spam: one reminder until approval status changes (flag cleared when not Pending)

---

## Suggested delivery order

```text
Sprint 1 (now)     → Order details UI + admin filters (#1, #2)
Sprint 2           → Stock visibility + manual in/out (#4)
Sprint 3           → Admin product add/deactivate (#3)
Sprint 4           → Email notifications + 24h pending reminder (#5, #6)
Pilot go-live      → M2.1 with paid host + real catalogue import
```

---

## Client messaging (what to say)

> “We’ve logged your feedback. Order line detail and admin filters are first. Stock will move to a simple in-stock/out-of-stock model so it doesn’t fight your billing system. WhatsApp/email alerts and 24-hour pending reminders are planned for the next phase; email can be enabled on free tiers for the pilot.”

---

*Update status checkboxes as items ship.*
