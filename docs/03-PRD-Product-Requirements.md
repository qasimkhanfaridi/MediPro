# Product Requirements Document (PRD) — MediPro

## 1. Document control

| Field | Value |
|-------|--------|
| Product | MediPro |
| Version | 1.0 (draft) |
| Depends on | `02-BRD-Business-Requirements.md` |

---

## 2. Product overview

MediPro delivers **two primary experiences** on a **mobile-first web** client:

1. **Medical store**: discover catalog, search/filter, cart, submit orders, track status/history, manage profile.
2. **Distributor admin**: manage stores, catalog (including bulk import), orders pipeline, alerts/notifications, inventory signals, summaries/reports.

Future **native apps** consume the **same backend APIs** with no duplicated business rules on clients.

---

## 3. Personas (product-level)

| Persona | Goals | Pain points |
|---------|--------|-------------|
| Store owner / buyer | Fast reorder, correct SKU/pack, clear price | Slow callbacks, wrong items on phone orders |
| Distributor dispatcher | Clear queue of orders, exceptions | Fragmented WhatsApp/paper |
| Catalog manager | Bulk updates, fewer errors | Spreadsheet chaos |
| System admin (internal) | User lifecycle, audit | — |

---

## 4. MVP scope (release 1.0 — proposal)

**In MVP**

- Responsive web app (mobile-first); HTTPS; Urdu/English UI **as decided** (i18n-ready structure recommended).
- Auth: signup/login for stores; admin-created or invited admin users; password reset flow.
- Store approval workflow (pending → active/suspended).
- Product CRUD + **Excel/CSV import** with row-level validation report.
- Catalog browse + **search**: name, company, salt/composition; pagination.
- Cart + order submit + order list/detail + status states **defined below**.
- Admin: store list, order board/list with filters, export CSV optional.
- **In-app notifications** list for key events (minimum); email/SMS optional stretch.
- **Stock**: MVP options—**(A)** manual quantity per SKU updated by admin, **(B)** “in stock / low / out” flags only—pick one before development.

**Explicitly post-MVP (examples)**

- Native iOS/Android apps (same API).
- Payment gateway integration.
- Deep ERP/bi-directional sync.
- Advanced schemes, batch-level ordering, barcode scanning.
- Offline-first PWA sync (can be phased).

---

## 5. Functional requirements (detailed)

### 5.1 Authentication & profiles

| ID | Requirement | Priority |
|----|-------------|----------|
| F-001 | Email or mobile-based login (policy: choose primary identifier for PK market). | Must |
| F-002 | Secure password storage; rate limiting on auth endpoints. | Must |
| F-003 | Store profile: legal/business name, address, city (RWP/ISB filter), NTN/license fields as required, mobile, contact name. | Must |
| F-004 | Admin can list/filter stores by status, city, name. | Must |
| F-005 | Admin can approve/reject/suspend store with optional reason. | Must |

### 5.2 Catalog

| ID | Requirement | Priority |
|----|-------------|----------|
| F-010 | Product fields: internal code, name, pack size, form factor, manufacturer/company, salt/composition, category, MRP or trade price per policy, active flag, optional image URL. | Must |
| F-011 | Import template documented; partial success handling with downloadable error file. | Must |
| F-012 | Deactivate product hides from new carts but preserves history visibility. | Must |
| F-013 | Admin audit log for price changes (who/when/old/new) — minimum viable log table. | Should |

### 5.3 Store catalog experience

| ID | Requirement | Priority |
|----|-------------|----------|
| F-020 | Product listing with filters (company, salt, category). | Must |
| F-021 | Full-text or indexed search on name + company + salt. | Must |
| F-022 | Product detail shows all trade-critical fields for ordering. | Must |
| F-023 | Add to cart with quantity; enforce positive integers; max qty caps if configured. | Must |

### 5.4 Cart & checkout

| ID | Requirement | Priority |
|----|-------------|----------|
| F-030 | Cart persisted per user/session policy (server-side cart preferred). | Must |
| F-031 | Show line totals and order total; taxes/fees per policy (placeholder fields OK if rates TBD). | Must |
| F-032 | Submit order creates immutable snapshot of prices/quantities at submission time. | Must |
| F-033 | Handle concurrent edits: if product deactivated mid-cart, warn before submit. | Should |

### 5.5 Order lifecycle

**Suggested states (configurable in admin):**

`draft` (optional) → `submitted` → `confirmed` | `on_hold` | `rejected` → `processing` → `dispatched` → `delivered` | `cancelled`

| ID | Requirement | Priority |
|----|-------------|----------|
| F-040 | Store sees orders with current state and timestamps. | Must |
| F-041 | Admin can transition states with comment on rejection/hold. | Must |
| F-042 | Notifications on: new order (admin), order accepted/rejected (store), optional dispatch. | Should |

### 5.6 Inventory (MVP — choose one model)

| ID | Requirement | Priority |
|----|-------------|----------|
| F-050-A | **Quantities**: admin maintains qty; low-stock threshold alert. | Must (if model A) |
| F-050-B | **Enumerated availability**: in stock / low / out only. | Must (if model B) |

Near-expiry alerts require batch data—**post-MVP** unless batch fields imported.

### 5.7 Reporting

| ID | Requirement | Priority |
|----|-------------|----------|
| F-060 | Admin dashboard: orders today/7d/30d; count by status; top SKUs optional. | Should |
| F-061 | Export orders CSV for date range. | Should |

### 5.8 Payments (post-MVP unless promoted)

| ID | Requirement | Priority |
|----|-------------|----------|
| F-070 | Record payment status manually against order (paid/unpaid/partial) if credit workflow needs visibility. | Could |
| F-071 | Payment gateway—see milestones Phase 2+. | Later |

---

## 6. Non-functional requirements (NFR)

| ID | Category | Requirement |
|----|----------|----------------|
| NFR-01 | Security | OWASP ASVS aligned practices; parameterized queries; HTTPS only; secrets in vault/env. |
| NFR-02 | Privacy | Privacy policy; data minimization; retention policy documented. |
| NFR-03 | Performance | P95 search < 2s on MVP dataset under nominal load (tune targets). |
| NFR-04 | Accessibility | WCAG 2.1 Level A target for core flows where feasible. |
| NFR-05 | Observability | Structured logs, error tracking, basic uptime monitoring. |
| NFR-06 | API | Versioned REST (or GraphQL) with JWT/OAuth2-style tokens for future mobile clients. |

---

## 7. Assumptions

- Pilot stores receive **training** and **support channel** (WhatsApp/helpdesk) during rollout.
- **Single distributor tenant** for MVP unless BRD mandates multi-tenant from day one.

---

## 8. Dependencies

- Hosting (cloud PK-friendly region if latency-sensitive).
- SMS provider if OTP used.
- Legal review of terms before pilot with external stores beyond trusted circle.

---

## 9. Success metrics (product)

| Metric | Example target |
|--------|----------------|
| Weekly active ordering stores (pilot cohort) | Set per pilot plan |
| Order error rate vs fulfillment | Track discrepancies |
| Time from submit to confirm | Median hours |
| Catalog import success rate | % rows valid first pass |

---

## 10. Out of scope (PRD — recap)

Consumer sales, clinical features, full ERP, native apps in MVP.

---

## 11. Revision history

| Version | Date | Author | Notes |
|---------|------|--------|-------|
| 1.0 | | | Initial draft |
