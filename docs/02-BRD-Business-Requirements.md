# Business Requirements Document (BRD) — MediPro

## 1. Document control

| Field | Value |
|-------|--------|
| Product | MediPro |
| Version | 1.0 (draft) |
| Primary market | Pakistan |
| Initial geography | Rawalpindi & Islamabad |

---

## 2. Executive summary

MediPro is a **B2B digital ordering and distributor operations platform** for pharmaceutical distribution. It addresses rising field-force and fuel costs by shifting **routine order capture** to a **mobile-first web** experience while giving distributor staff **administration, catalog control, inventory signals, and reporting** in one system. The solution is positioned as **trade software** between licensed distributor and licensed retailers—not a consumer pharmacy or medical advice service.

---

## 3. Business goals

| ID | Goal | Priority |
|----|------|----------|
| BG1 | Lower operational cost of order collection for high-frequency, repeat purchases | High |
| BG2 | Improve accuracy of ordered SKU, pack size, and pricing vs manual channels | High |
| BG3 | Increase visibility of demand and stock risk (near expiry, low stock) for operations | High |
| BG4 | Enable phased rollout and measurable adoption before national or multi-tenant expansion | Medium |
| BG5 | Preserve option to productize for additional distributors (multi-tenant) without redesigning core flows | Medium |

---

## 4. Stakeholder needs summary

### 4.1 Distributor (business / operations)

- Single roster of **affiliated medical stores** with status and contact data.
- **Orders dashboard**: new, in progress, fulfilled; exceptions visible.
- **Catalog management** including **bulk import** (Excel/CSV) for large SKU sets.
- **Stock / inventory awareness**: at minimum alerts and quantity signals aligned with operational practice (see open questions).
- **Notifications/reminders** for operational events (new order, low stock, pending approvals).
- **Reporting/summary**: orders by period, store, product line; export where required.

### 4.2 Medical store (retail pharmacy — B2B buyer)

- **Mobile-friendly** browsing and ordering without requiring desktop software.
- **Trustworthy catalog**: product identity, pack, company, price tiers as applicable, search by name / company / salt.
- **Simple cart and submit**; visibility of **order status** and **history**.
- **Account**: signup/profile; subject to **admin approval** if required by policy.

### 4.3 Field / relationship staff (optional phase)

- Reduced need for order-writing visits; potential role for **route support**, **new SKU education**, **collections**—out of scope for BRD detail unless productized as roles later.

---

## 5. Business rules (high level)

| BR-ID | Rule |
|-------|------|
| BR-01 | Only **verified** B2B accounts may place orders; verification criteria defined by distributor policy. |
| BR-02 | Prices, schemes, and credit limits are **authoritative from distributor configuration**; stores see what policy allows. |
| BR-03 | Order submission creates an **auditable record** (who, when, lines, totals). |
| BR-04 | Product identity must distinguish **SKU/pack** to prevent wrong fulfillment. |
| BR-05 | Platform does not provide **diagnosis, prescribing, or therapeutic recommendations**. |

---

## 6. Functional business requirements

### 6.1 Identity, access, and onboarding

| Req ID | Requirement |
|--------|-------------|
| FR-B-01 | Support **role-based access** (minimum: distributor admin, medical store user). |
| FR-B-02 | Medical stores can **register** and complete **profile** (business name, address, license fields as policy requires, mobile, contact person). |
| FR-B-03 | Distributor can **approve, suspend, or deactivate** store accounts. |
| FR-B-04 | Authentication must be fit for **business use** (secure passwords, session handling; MFA phased per security review). |

### 6.2 Catalog & search

| Req ID | Requirement |
|--------|-------------|
| FR-B-05 | Maintain **product master** with fields needed for trade: name, SKU/code, pack, company/manufacturer, salt/composition where applicable, category, list/trade price tiers as applicable. |
| FR-B-06 | **Search and filters**: by product name, company, salt/composition; extensible to barcode/SKU in later phase. |
| FR-B-07 | **Bulk catalog maintenance** via **Excel/CSV import** with validation summary (success/errors). |

### 6.3 Ordering

| Req ID | Requirement |
|--------|-------------|
| FR-B-08 | Store users build a **cart** with line quantities respecting **minimum order rules** if configured. |
| FR-B-09 | **Submit order** generates status workflow (e.g., pending → confirmed → picking → dispatched → delivered/cancelled—exact states in PRD). |
| FR-B-10 | Store users view **order history** and **current status**. |

### 6.4 Distributor operations

| Req ID | Requirement |
|--------|-------------|
| FR-B-11 | Admin views **orders summary** and can act per operational procedure (accept/reject/hold with reason). |
| FR-B-12 | **Notifications** inside app and/or channels agreed (SMS/email/push later) for critical events. |
| FR-B-13 | **Inventory/stock**: minimum viable definition agreed—either integrate feed from existing stock system or manual/update fields with **low-stock / expiry** alerts per PRD. |

### 6.5 Payments & settlement

| Req ID | Requirement |
|--------|-------------|
| FR-B-14 | Support **order totals and invoice alignment** for traditional settlement (bank transfer, cash on delivery, credit terms). |
| FR-B-15 | **Online payment gateway** as a phased capability once order flow is stable (see milestones). |

---

## 7. Non-functional business expectations

| Category | Expectation |
|----------|-------------|
| Usability | Primary journeys completable on **smartphone browsers** without horizontal scrolling clutter. |
| Availability | Business-hours reliability targets set with hosting SLA; communicate maintenance windows. |
| Performance | Acceptable search and cart response on typical Pakistani mobile networks (quantify in NFR). |
| Audit | Orders and catalog price changes traceable for disputes and reconciliation. |
| Security | Protect business and personal data; least-privilege access; secure transport (HTTPS). |

---

## 8. Regulatory & positioning (Pakistan)

- MediPro must operate as **B2B trade facilitation** between licensed entities; **DRAP** and provincial regulations apply to **medicine trade**, not to software alone—but **marketing, data, and claims** must avoid implying clinical services.
- **Terms of use**, **privacy notice**, and **KYC fields** for stores should be reviewed with qualified local counsel before public pilot.

*(Detail in `08-RISKS-COMPLIANCE-PAKISTAN.md`.)*

---

## 9. Out of scope (business)

- Direct-to-patient sales or prescription handling.
- Automated therapeutic substitution or “suggested alternatives” unless legally cleared as a separate initiative.
- Full ERP replacement on day one unless explicitly funded.

---

## 10. Dependencies & integrations (business)

| Dependency | Impact |
|------------|--------|
| Existing ERP/accounting | May define stock figures and invoicing; integration optional in phases. |
| SMS gateway | OTP or alerts if adopted. |
| Payment provider | When online payments go live. |

---

## 11. Open questions (resolve before build lock)

1. **Stock source of truth**: ERP integration vs manual updates vs periodic import?
2. **Pricing model**: fixed list, store-specific contracts, schemes/discounts complexity in MVP?
3. **Credit**: hard limits in system vs offline-only discipline?
4. **Pilot cohort size** and **training model** for first 4–8 weeks?
5. **Multi-tenant**: single distributor only for v1 schema vs tenant-ready from day one?

---

## 12. Traceability

Business requirements map to **PRD** (`03-PRD-Product-Requirements.md`) and **milestones** (`04-MILESTONES-AND-PHASES.md`). Changes to BRD trigger PRD and milestone review.

---

## 13. Approvals

| Role | Name | Date |
|------|------|------|
| Business sponsor | | |
| Product owner | | |
| Compliance / legal (advisory) | | |
