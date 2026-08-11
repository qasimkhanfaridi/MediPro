# Project Charter — MediPro

## 1. Document control

| Field | Value |
|-------|--------|
| Product working name | MediPro |
| Region (initial) | Pakistan — Rawalpindi & Islamabad |
| Document type | Project Charter |
| Status | Draft for stakeholder review |

---

## 2. Background & problem statement

Licensed pharmaceutical distributors traditionally rely on **field bookers** (vehicles, fuel, salaries) to visit medical stores, collect orders, and maintain relationships. Rising operational costs (especially fuel) pressure margins while **order capture remains manual** and **inventory visibility** across stores is uneven.

MediPro aims to **digitize B2B ordering** between the distributor and affiliated retail pharmacies/medical stores, reduce redundant travel for routine order-taking, and improve **catalog consistency**, **order accuracy**, and **inventory visibility**—starting within an existing high-trust distribution network.

---

## 3. Vision

Enable **fast, accurate, auditable B2B medicine orders** from verified medical stores through a **mobile-first web application**, backed by distributor-side **admin tools** for catalog, stores, orders, stock signals, and bulk data operations—with **minimal rework** when native mobile apps are introduced via a shared backend API.

---

## 4. Objectives (measurable intent)

| ID | Objective |
|----|-----------|
| O1 | Reduce cost and delay of routine order capture vs phone/paper loops for onboarded stores. |
| O2 | Provide distributor operators a single place for orders, store roster, catalog maintenance, and bulk imports. |
| O3 | Ship **mobile-first** UX so primary usage on phones does not require a desktop. |
| O4 | Architect **API-first** services so iOS/Android clients can be added later without duplicating business logic. |
| O5 | Phase rollout (pilot → expansion) with clear metrics rather than a single “big bang” to all stores. |

---

## 5. In scope (initial product intent)

- Web application optimized for **mobile browsers** (responsive / progressive enhancement).
- **Role-based access**: distributor admin/operations vs medical store users (additional internal roles as needed).
- **Product catalog** with search/filter (product, company, salt/composition, categories as applicable).
- **Orders**: cart, submit, status visibility, history.
- **Admin**: connected stores, summary dashboards, notifications/reminders (see PRD for detail), **inventory/stock signals**, **bulk product import** (e.g. Excel).
- **Foundation for payments**: structured order totals and reconciliation hooks; full payment gateway scope phased per PRD.

---

## 6. Out of scope (initial charter — confirm in BRD/PRD)

- Consumer-facing pharmacy or direct patient sales.
- Clinical decision support, prescribing, or substitution recommendations.
- Cold-chain logistics execution (unless later phase explicitly adds integrations).
- Guaranteed integration with every external ERP before pilot success.

---

## 7. Stakeholders (template — fill names)

| Role | Responsibility |
|------|------------------|
| Sponsor / business owner | Funding, priority, retail network alignment |
| Distributor operations | Catalog accuracy, pricing, credit policy, fulfillment |
| Store liaison / pilot lead | Onboarding, training, feedback loops |
| Product owner | Backlog, acceptance, milestone scope |
| Tech lead / vendor | Architecture, security, delivery |
| Compliance advisor | Trade licensing positioning, terms of use |

---

## 8. Success criteria (pilot — illustrative targets)

Define numeric targets before pilot kickoff. Examples:

- **Adoption:** X% of pilot stores placing ≥1 order/week within N weeks.
- **Quality:** Order line accuracy vs fulfillment under Y% discrepancy rate.
- **Operations:** Measurable reduction in phone reorder loops or booker trips for routine SKUs (baseline vs post).

---

## 9. Constraints & assumptions

- Users are **licensed B2B parties** in Pakistan; onboarding must support **verification** per business policy.
- **Credit-based trading** is common; payment integration may follow stable order flow.
- Peak usage on **mobile networks** implies performance and offline-tolerant UX patterns where feasible.

---

## 10. Approval

| Name | Role | Signature / Date |
|------|------|------------------|
| | | |
| | | |

---

*Charter aligns with BRD and milestone phases; revise together when scope or region changes.*
