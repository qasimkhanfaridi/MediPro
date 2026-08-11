# User Stories & Acceptance Criteria — MediPro

## 1. Conventions

- **Format:** As a \<role\>, I want \<capability\>, so that \<benefit\>.
- **AC:** Given / When / Then style bullet points.
- **IDs:** US-XXX for traceability to backlog tools.

**Roles:** `Store User`, `Distributor Admin`, `System` (background jobs).

---

## 2. Epic: Authentication & store onboarding

### US-001 — Store registration

**As a** medical store representative, **I want** to register my business on MediPro, **so that** I can be verified and place B2B orders.

**Acceptance criteria**

- AC1: Registration collects required fields per PRD (business name, address, city, mobile, contact name, license/NTN fields as configured).
- AC2: Duplicate mobile/email is rejected with a clear message.
- AC3: New accounts enter **pending** state until admin approval (if enabled).
- AC4: User receives confirmation of submission (screen + optional SMS/email if configured).

---

### US-002 — Admin approves store

**As a** distributor admin, **I want** to approve or reject pending store registrations, **so that** only legitimate B2B buyers order.

**Acceptance criteria**

- AC1: Admin sees list of pending stores with key profile fields.
- AC2: Approve transitions account to **active**; reject records reason and notifies store user if notification channel exists.
- AC3: Suspended stores cannot place new orders (existing orders remain visible read-only per policy).

---

### US-003 — Login & logout

**As a** user, **I want** to log in securely and log out, **so that** my account stays protected on shared devices.

**Acceptance criteria**

- AC1: Login requires identifier + password; invalid credentials show generic error (no user enumeration detail).
- AC2: Logout invalidates session/tokens per auth design.
- AC3: Multiple failed attempts trigger temporary lockout or CAPTCHA (policy-driven).

---

## 3. Epic: Catalog & search

### US-010 — Browse catalog (mobile)

**As a** store user, **I want** to browse and filter products on my phone, **so that** I can find items quickly without desktop software.

**Acceptance criteria**

- AC1: Catalog displays on viewport widths ≥320px without broken layouts.
- AC2: Filters by company, salt/composition, category work in combination with search text.
- AC3: Inactive products do not appear in browse/search for new selection.

---

### US-011 — Product detail

**As a** store user, **I want** to see full trade details for a SKU, **so that** I order the correct pack and price.

**Acceptance criteria**

- AC1: Shows name, code, pack, company, salt, price fields per PRD.
- AC2: Add-to-cart respects availability rules (cannot exceed limits).

---

### US-012 — Bulk import products

**As a** distributor admin, **I want** to import many products from Excel, **so that** onboarding the catalog is fast and less error-prone.

**Acceptance criteria**

- AC1: Template documented and downloadable from admin UI.
- AC2: Import produces summary: rows succeeded, failed, with downloadable error detail for failed rows.
- AC3: Large imports run without blocking browser (async job acceptable).

---

## 4. Epic: Cart & ordering

### US-020 — Build cart

**As a** store user, **I want** to add lines and quantities to a cart, **so that** I can submit one consolidated order.

**Acceptance criteria**

- AC1: Cart persists across sessions per defined policy (server-side cart).
- AC2: Removing lines and updating quantities updates totals immediately.
- AC3: Deactivated or unavailable SKUs are flagged before submit.

---

### US-021 — Submit order

**As a** store user, **I want** to submit my cart as an order, **so that** the distributor can fulfill it.

**Acceptance criteria**

- AC1: Submit captures price snapshot and prevents silent price change post-submit.
- AC2: Order receives unique reference ID visible to store user.
- AC3: Empty cart cannot be submitted.
- AC4: Confirmation screen shows order ID and next steps.

---

### US-022 — Track orders

**As a** store user, **I want** to see status and history of my orders, **so that** I can follow fulfillment.

**Acceptance criteria**

- AC1: List supports pagination or infinite scroll.
- AC2: Detail shows lines, quantities, prices at order time, current status, timestamps.

---

### US-023 — Process orders (admin)

**As a** distributor admin, **I want** to see new orders and change their status, **so that** operations stay coordinated.

**Acceptance criteria**

- AC1: Admin queue shows new orders first; filters by date, store, status.
- AC2: Status transitions follow allowed workflow; illegal transitions blocked with message.
- AC3: Reject/hold requires comment visible to store user (if productized).

---

## 5. Epic: Inventory & alerts

### US-030 — Low-stock visibility

**As a** distributor admin, **I want** low-stock signals per MVP model, **so that** I can replenish before stockouts.

**Acceptance criteria**

- AC1: Matches chosen MVP model (quantity threshold **or** enumerated band).
- AC2: Admin dashboard or list highlights SKUs breaching threshold.

---

## 6. Epic: Notifications & reporting

### US-040 — Operational notifications

**As a** distributor admin, **I want** to be notified when a new order arrives, **so that** I respond quickly.

**Acceptance criteria**

- AC1: In-app notification list records new order events for admin users.
- AC2: Store users receive in-app (and optional email) when order state changes as configured.

---

### US-041 — Summary reporting

**As a** distributor admin, **I want** a summary of orders over time, **so that** I can review volume and exceptions.

**Acceptance criteria**

- AC1: Dashboard shows counts for configurable periods (e.g., today, 7d, 30d).
- AC2: Optional CSV export for orders in date range (Should priority).

---

## 7. Non-functional stories (samples)

### US-NFR-001 — HTTPS only

**As** security policy, **the system must** serve all traffic over HTTPS with valid TLS certificates.

**AC:** HTTP requests redirect to HTTPS; HSTS enabled on production.

---

## 8. Traceability

Map stories to **PRD** requirement IDs in Jira/Azure DevOps/GitHub Issues. Example: US-021 ↔ F-032, F-040.

---

## Revision history

| Version | Date | Notes |
|---------|------|-------|
| 1.0 | | Initial backlog seed |
