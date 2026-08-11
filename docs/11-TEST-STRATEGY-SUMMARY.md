# Test Strategy Summary — MediPro

## 1. Objectives

- Prevent **wrong catalog or pricing** reaching stores.
- Ensure **order integrity** (immutable snapshots, valid state transitions).
- Validate **mobile-first** UX on representative devices/browsers.

---

## 2. Test levels

| Level | Scope | Owner |
|-------|--------|--------|
| Unit | Services, pricing snapshot, validation rules | Engineering |
| Integration | API + DB; import pipeline | Engineering |
| E2E | Critical paths: login → search → cart → submit → admin confirm | QA / automation |
| UAT | Business acceptance against PRD | Product + Ops |
| Security | Auth, RBAC, OWASP spot checks | Tech lead / external |

---

## 3. Critical paths (must pass each release)

1. Store user: login → browse/search → add to cart → submit order → see confirmation + history.
2. Admin: login → approve store (if pending) → view new order → advance status → store sees update.
3. Admin: Excel import **valid** file → products visible; **invalid** file → errors downloadable.
4. Unauthorized role cannot access admin APIs (automated negative tests).

---

## 4. Device & browser matrix (indicative)

- Chrome Android (recent), Safari iOS (recent), plus one mid-tier Android device for pilot realism.
- Viewports: 360×640, 390×844, plus desktop admin.

---

## 5. Non-functional checks

- Load: smoke test on search/cart under nominal concurrency before pilot expand.
- Backup: restore drill monthly post go-live.

---

## Revision history

| Version | Date | Notes |
|---------|------|-------|
| 1.0 | | Initial |
