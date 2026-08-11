# Milestones & Phases — MediPro

## 1. Purpose

This document defines **delivery phases**, **milestones**, **deliverables**, and **exit criteria** so engineering, operations, and sponsors share one timeline model. Dates are placeholders—fill during project planning.

---

## 2. Phase overview

| Phase | Name | Goal |
|-------|------|------|
| 0 | Discovery & foundation | Lock scope, legal framing, pilot cohort, architecture choices |
| 1 | MVP build | Mobile-first web + API + admin + catalog + orders |
| 2 | Pilot (RWP/ISB) | Live usage with limited stores; iterate on ops feedback |
| 3 | Hardening & scale | Performance, monitoring, broader rollout, optional PWA |
| 4 | Mobile apps & payments | Native clients on shared API; payments when justified |
| 5 | Expansion | Multi-city / multi-tenant / integrations per business case |

---

## Phase 0 — Discovery & foundation

**Duration (indicative):** 2–4 weeks  

### Milestones

| ID | Milestone | Deliverables |
|----|-----------|--------------|
| M0.1 | Scope baseline | Signed-off MVP from PRD; resolved open questions (stock model, tenant model, pricing complexity) |
| M0.2 | Legal & compliance pack | Draft Terms, Privacy, B2B positioning review with counsel (advisory) |
| M0.3 | Pilot plan | List of pilot stores/areas, training approach, success metrics |
| M0.4 | Architecture decision record | Stack choice, hosting region, API style, auth approach (`05-TECHNICAL-ARCHITECTURE.md`) |

### Exit criteria

- Product owner accepts MVP backlog for Phase 1.
- Risk register initialized (`08-RISKS-COMPLIANCE-PAKISTAN.md`).
- No unresolved **blocker** on stock source-of-truth approach.

---

## Phase 1 — MVP build

**Duration (indicative):** 8–12 weeks (depends on team size)

### Milestones

| ID | Milestone | Deliverables |
|----|-----------|--------------|
| M1.1 | Foundation | Repo, CI/CD, environments (dev/stage/prod), authentication, roles skeleton |
| M1.2 | Catalog core | Product model, admin CRUD, Excel import with validation report |
| M1.3 | Store experience | Mobile-first browse/search/filter, PDP, server-side cart |
| M1.4 | Orders | Submit pipeline, status workflow, store order history, admin order console |
| M1.5 | Ops extras | Dashboard summaries (minimal), in-app notifications, stock model A or B |
| M1.6 | MVP release candidate | QA sign-off checklist, security pass, UAT with internal users |

### Exit criteria

- All **Must** items in PRD MVP are implemented and UAT-passed.
- Documentation: API overview, admin user guide (short), store user guide (short).
- Rollback plan for first deployment exists.

---

## Phase 2 — Pilot (Rawalpindi / Islamabad)

**Duration (indicative):** 4–8 weeks active pilot

### Milestones

| ID | Milestone | Deliverables |
|----|-----------|--------------|
| M2.1 | Pilot go-live | Production cutover for pilot cohort; support channel live |
| M2.2 | Weekly ops reviews | Feedback log; prioritized bugfix/hardening backlog |
| M2.3 | Metrics checkpoint | Report vs pilot KPIs (adoption, accuracy, time-to-confirm) |
| M2.4 | Pilot retrospective | Go/no-go for broader rollout; update PRD/milestones |

### Exit criteria

- KPIs meet **minimum agreed floor** OR documented reasons and remediation plan.
- P1 defects resolved within SLA; known issues listed for Phase 3.

---

## Phase 3 — Hardening & scale-up

**Duration (indicative):** ongoing in parallel with growth

### Milestones

| ID | Milestone | Deliverables |
|----|-----------|--------------|
| M3.1 | Performance & reliability | Load targets met; backups tested; disaster recovery basics |
| M3.2 | PWA (optional) | Installability, icons, basic offline-friendly shell if approved |
| M3.3 | Expanded rollout | Wave-based onboarding by area/route; training materials updated |
| M3.4 | Integrations (optional) | CSV export schedules; ERP file import if batch sync chosen |

### Exit criteria

- Monitoring/alerting covers critical paths.
- Support playbooks updated for higher volume.

---

## Phase 4 — Mobile applications & payments

**Duration (indicative):** 8–16+ weeks (mobile); payments may be shorter if scoped alone

### Milestones

| ID | Milestone | Deliverables |
|----|-----------|--------------|
| M4.1 | API hardening for mobile | Token lifecycle, device/session policy, push notification hooks |
| M4.2 | Mobile client v1 | iOS and/or Android using shared API (React Native / Flutter / native—per ADR) |
| M4.3 | Push notifications | Order events via FCM/APNs as applicable |
| M4.4 | Payments phase 1 | Manual reconciliation enhancements OR gateway for agreed method(s) |

### Exit criteria

- Parity with web for **core order flows** OR documented deltas accepted by business.
- PCI/security approach documented if card data touches platform (prefer redirect/hosted where possible).

---

## Phase 5 — Expansion (national / multi-tenant / enterprise)

### Milestones (examples)

| ID | Milestone | Deliverables |
|----|-----------|--------------|
| M5.1 | Multi-tenant model | Isolation, billing per tenant if SaaS, admin super-role |
| M5.2 | Enterprise integrations | ERP connectors, principal-specific reporting |
| M5.3 | Geographic expansion | Replication playbook, latency/legal review |

---

## 3. Cross-cutting milestones (all phases)

| Activity | Frequency |
|----------|-----------|
| Security review | Before pilot, before payments, after major changes |
| Backup restore test | Monthly post go-live |
| Dependency updates | Monthly patch cadence |

---

## 4. RAID log (template)

Maintain **Risks, Assumptions, Issues, Dependencies** in a living document or tool.

| Type | Item | Owner | Status |
|------|------|-------|--------|
| Risk | Catalog drift vs physical stock | Ops | Open |
| Assumption | Pilot stores have smartphones + data | Sponsor | Open |

---

## 5. Approval & planning

**Next step:** Assign owners and dates to M0–M1 milestones in your project management tool; link each milestone to epics/user stories in `06-USER-STORIES-AND-ACCEPTANCE.md`.

---

## Revision history

| Version | Date | Notes |
|---------|------|-------|
| 1.0 | | Initial |
