# Technical Architecture — MediPro

## 1. Goals

1. **Mobile-first web** that performs well on Pakistani mobile networks.
2. **API-first backend** so native apps later **reuse** business logic, auth, and data.
3. **Secure, auditable** B2B ordering suitable for regulated trade context.
4. **Operational simplicity** for small teams—avoid unnecessary microservices early.

---

## 2. Logical architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Clients (phase-dependent)                │
│  Mobile browser (MVP)  │  Desktop browser  │  iOS/Android (later) │
└───────────────┬─────────────────────────────────────────────┘
                │ HTTPS (TLS 1.2+)
┌───────────────▼─────────────────────────────────────────────┐
│                      Application layer                       │
│  • REST (or GraphQL) API                                     │
│  • Authentication / authorization (JWT or session + refresh) │
│  • Business services: catalog, cart, orders, notifications   │
└───────────────┬─────────────────────────────────────────────┘
                │
┌───────────────▼─────────────────────────────────────────────┐
│                     Data layer                               │
│  • Relational DB (PostgreSQL / SQL Server / MySQL — choose)   │
│  • File/object storage for imports & exports (optional)      │
└───────────────┬─────────────────────────────────────────────┘
                │
┌───────────────▼─────────────────────────────────────────────┐
│                  External services                           │
│  SMS / email / push (later) │ Payment provider (later)       │
└─────────────────────────────────────────────────────────────┘
```

---

## 3. Recommended principles

| Principle | Implementation hint |
|-----------|---------------------|
| API-first | No business-critical logic **only** in the SPA; validate on server. |
| Versioned API | `/v1/...` or header versioning for mobile compatibility. |
| Idempotent writes | Order submission uses idempotency key where retries possible. |
| Tenant-ready schema | Even for single distributor MVP, `tenant_id` on core tables if multi-tenant is likely. |
| Audit trail | Orders immutable snapshot; price changes logged. |

---

## 4. Frontend (MVP)

- **Responsive SPA** (React/Vue/Svelte/Next.js—team choice) or **MPA** with HTMX—pick based on skill.
- **Mobile-first CSS** (single breakpoint strategy; avoid desktop-only patterns on critical paths).
- Optional **PWA** manifest in Phase 3 for installability without app store.

---

## 5. Backend

- Monolith or **modular monolith** acceptable for MVP.
- **RBAC** enforced server-side: roles `super_admin`, `distributor_admin`, `store_user` (adjust names to product language).
- **Import pipeline**: async job for large Excel files with progress and error artifact.

---

## 6. Authentication & sessions

- **Recommended:** OAuth2-style **access token (short)** + **refresh token (long, rotatable)** for mobile readiness.
- Password hashing: Argon2id or bcrypt with strong cost parameters.
- Consider **OTP via SMS** for step-up or recovery— Pakistani SMS providers integrate via REST.

---

## 7. Data storage

- **Primary relational DB** for transactional integrity (orders, line items, catalog).
- **Indexes** on search fields used by stores (product name, company, salt) — consider full-text search (PostgreSQL FTS, Elasticsearch later if scale demands).

---

## 8. Notifications

- **MVP:** In-app notification records + optional email.
- **Later:** FCM/APNs for mobile; template messages must be operational, not promotional drug claims.

---

## 9. Security baseline

- HTTPS everywhere; HSTS; secure cookies if cookie-based sessions.
- OWASP ASVS Level 1 minimum; strive for Level 2 on auth and data protection.
- Rate limiting on login and public APIs.
- Input validation on all write endpoints; file upload scanning/size limits for Excel.

---

## 10. DevOps

- **Environments:** development, staging, production.
- **CI:** lint, test, build on each PR; migration checks.
- **Secrets:** environment variables / vault—never commit secrets.
- **Backups:** automated DB backups; quarterly restore drill.

---

## 11. Path to native apps (minimal rework)

1. Stabilize **versioned REST API** consumed by web (same contract apps will use).
2. Add **push registration** endpoints and device tokens table when building mobile.
3. Mobile apps: React Native / Flutter recommended for single codebase iOS+Android; alternatively **Capacitor** wrapping SPA if native UX requirements are modest.

---

## 12. Open technical decisions (ADR recommended)

Record decisions in short ADR files (Architecture Decision Records):

| Topic | Options |
|-------|---------|
| Stack | e.g. Node/Nest, .NET, Laravel, Django—all viable |
| DB | PostgreSQL vs SQL Server (Windows ecosystem fit) |
| Hosting | AWS/Azure/GCP vs local PK cloud—latency & compliance |
| Search | DB FTS vs Meilisearch/Elastic later |

---

## Revision history

| Version | Date | Notes |
|---------|------|-------|
| 1.0 | | Initial |
