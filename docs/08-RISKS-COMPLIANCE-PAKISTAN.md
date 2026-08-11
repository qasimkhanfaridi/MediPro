# Risks, Compliance & Operating Context — Pakistan

## 1. Purpose

Identify **business, legal, operational, and technical risks** for MediPro in **Pakistan**, with mitigations. Not legal advice—engage qualified counsel for binding interpretation.

---

## 2. Regulatory & positioning

| Topic | Risk | Mitigation |
|-------|------|------------|
| DRAP / drug trade | Product appears to facilitate unlicensed trade | Onboard **licensed** retailers only; capture license identifiers per policy; Terms clarify B2B trade between licensed parties. |
| Consumer pharmacy | Misclassified as D2C platform | No patient-facing marketing; no retail checkout for individuals; clear B2B branding. |
| Clinical claims | UI implies therapeutic advice | No “treats X” language; operational labels only; avoid ranking products by implied efficacy. |
| Data protection | Mishandling personal/business data | Privacy notice; data minimization; secure storage; retention policy; access logs for admins. |

---

## 3. Business & operational risks

| Risk | Impact | Mitigation |
|------|--------|------------|
| Catalog vs physical stock mismatch | Trust loss, fulfillment disputes | Clear availability model; frequent updates; “confirm on dispatch” messaging if needed |
| Price disputes | Relationship damage | Immutable order snapshots; audit log on admin price edits |
| Credit exposure | Bad debt if orders imply automatic credit | Explicit credit policy; optional credit limit field in later phase; training |
| Low digital literacy | Low adoption | Urdu support; simple UX; field onboarding support; WhatsApp helpdesk during pilot |
| WhatsApp remains default | Parallel channels fragment truth | Incentivize in-app ordering (speed, accuracy); ops discipline |

---

## 4. Technical & security risks

| Risk | Impact | Mitigation |
|------|--------|------------|
| Account takeover | Fraudulent orders | Strong passwords; rate limits; optional MFA for admins |
| API abuse / scraping | Catalog exfiltration | Auth on all APIs; rate limiting; terms prohibiting misuse |
| Downtime during peak order windows | Revenue & trust loss | Monitoring; backups; incident playbook |
| Excel import errors | Wrong SKUs/prices | Validation report; dry-run mode; role separation for who can import |

---

## 5. Cultural & market context

- **Relationship selling** still matters; digital ordering complements but may not eliminate visits/collections in year one.
- **Mobile data variability** in peri-urban routes affects UX; lightweight pages and clear error states help.
- **Language:** English + Urdu (Roman or script) expectations vary by user—plan content strategy early.

---

## 6. Incident response (starter checklist)

1. Detect (monitoring alerts).
2. Triage (severity, data exposure?).
3. Contain (disable feature, rotate secrets if needed).
4. Communicate (internal + affected stores if data impacted).
5. Post-mortem and preventive actions.

---

## 7. Insurance & contracts

- Consider **cyber** and **E&O** coverage as product scales; **data processing** clauses if outsourcing hosting internationally.

---

## 8. Review cadence

Revisit this document **before pilot**, **before payments go live**, and **before multi-tenant / expansion**.

---

## Revision history

| Version | Date | Notes |
|---------|------|-------|
| 1.0 | | Initial |
