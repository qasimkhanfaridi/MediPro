# Client demo, paid rollout & path to internal-grade — MediPro

**Audience:** You (delivery team) and your **client** (distributor / pharmacy chain sponsor).  
**Intent:** Show value early on **low or zero cost**, move to **paid hosting** when they commit, then grow to **internal / enterprise** quality over time.

---

## 1. How to position the project to the client

| Message | What to say (plain language) |
|---------|------------------------------|
| **What it is** | A B2B ordering platform: pharmacies browse your catalogue, cart, submit orders; your team approves stores, manages stock, imports products, sees notifications. |
| **What they see now** | A working **pilot** (web + API), not a finished national product. Good for **Rawalpindi / Islamabad–style** pilot. |
| **What “internal level” means later** | Same business flows, but with company IT standards: proper database, backups, security reviews, integrations (ERP), audit, support SLAs, maybe mobile apps. |
| **Money model** | **Phase 1 demo / pilot** can run on **free or very cheap** hosting so they see it live. **Paid** when they want reliability, custom domain, support, and no “sleeping” servers. |

Avoid promising payment gateway, native apps, or multi-city SaaS until those are in scope and priced.

---

## 2. Three engagement phases (for you and the client)

```text
  DEMO (show)     →     PILOT (try)     →     INTERNAL (run the business on it)
  free / cheap          low paid               proper infra + process
```

### Phase A — Demo / showcase (now)

**Goal:** Client sees real screens, real flows, on a **public URL** (or your laptop + hotspot).

| Deliverable | Detail |
|-------------|--------|
| Live URL | e.g. `https://app.clientname-demo.com` (Cloudflare Pages + free/cheap API) |
| Demo data | Demo catalogue (TEST-MED SKUs), 2–3 demo pharmacies (approved / pending) |
| Walkthrough script | 15–20 min: register store → admin approves → browse → cart → order → admin notification |
| What you do **not** promise | 99.9% uptime, DRAP legal sign-off, payment collection, ERP sync |

**Hosting:** Follow `12-PRODUCTION-DEPLOY-FREE.md` (Fly.io or Oracle + Cloudflare Pages). **Cost to you:** ~$0–5/month.

**Client decision point:** “Do we run a **pilot** with real stores?”

---

### Phase B — Paid pilot (client commits)

**Goal:** 3–15 real pharmacies in one city; distributor ops team uses admin daily.

| Deliverable | Detail |
|-------------|--------|
| Production URL | Custom domain, HTTPS, no cold starts |
| Real catalogue | Excel import of their SKUs (or phased cutover) |
| Real users | Their admin + approved stores only; **no** shared demo passwords |
| Support | You: bugfixes + small changes for **X weeks** (define in SOW) |
| Backups | Daily DB backup; documented restore |

**Hosting (paid but still modest):** ~$10–40/month typical (small VPS or Fly/Railway paid + managed backup). See paid tier in `12-PRODUCTION-DEPLOY-FREE.md` §11.

**Pricing idea (you set with client):**

| Item | Example range (PKR / USD — you decide) |
|------|----------------------------------------|
| Pilot setup (deploy, domain, import, training) | One-time fee |
| Monthly care (hosting + minor fixes) | Monthly retainer |
| Change requests (new features) | Per sprint or hourly |

**Client decision point:** “Do we scale to **internal** rollout?”

---

### Phase C — Internal-grade (future target)

**Goal:** System behaves like **their** internal platform, not a side project.

| Capability | Why it matters |
|------------|----------------|
| **PostgreSQL / SQL Server** | Backups, reporting, no single SQLite file on one VM |
| **CI/CD + staging** | Safe releases; client sees changes before prod |
| **Secrets & RBAC** | IT audit: who can approve stores, export data |
| **Audit log** | Who changed price, stock, order status |
| **Integrations** | ERP, principal price files, optional SMS/email |
| **Monitoring & alerts** | Uptime, error rates, order failures |
| **Mobile apps** (optional) | Phase 4 in milestones |
| **Multi-tenant / multi-branch** | If they grow to groups or SaaS for others |

Maps to **Phase 3–5** in `04-MILESTONES-AND-PHASES.md` and architecture in `05-TECHNICAL-ARCHITECTURE.md`.

**This is a separate commercial phase** — larger budget, longer timeline, often 3–12+ months depending on scope.

---

## 3. What to show the client in the demo (checklist)

Run through in this order:

1. **Home / sign in** — distributor admin (`admin@…` only on demo; explain production gets their own).
2. **Admin → Registered stores** — list loads; **Approve** a pending pharmacy.
3. **Admin → Load demo products** (or show imported Excel) — catalogue populated.
4. **Store login** — approved pharmacy; header shows **Pharmacy · Approved**.
5. **Catalogue** — search, company/category filters, product detail, stock badge.
6. **Cart → submit order**.
7. **Admin → Notifications** — new order alert.
8. **Admin → Orders** — status update (if shown in UI).
9. **Optional:** low stock list, stock adjustment, Excel import template.

**Prepare for questions:**

| Question | Short answer |
|----------|--------------|
| Is data secure? | HTTPS, passwords hashed, JWT; production uses strong secrets and private DB. |
| Works on phone? | Yes — mobile-first web; native app later if needed. |
| Our Excel catalogue? | Yes — import template + upload on admin. |
| Payments? | Orders only for now; payment gateway is a later phase. |
| If internet is slow? | Lightweight pages; pilot in one city first. |

---

## 4. Suggested commercial story (one slide)

```text
Today     →  Live demo + optional free/cheap pilot URL
Next      →  Paid pilot (real stores, support, backups) — low monthly cost
Future    →  Internal platform (DB, integrations, audit, scale) — scoped project
```

You are **not** selling “free forever.” You are selling **proof now**, **affordable pilot**, **investment for internal-grade later**.

---

## 5. Technical path aligned with client journey

| Client phase | Technical focus | Milestone refs |
|--------------|-----------------|----------------|
| Demo | Free deploy, demo seed, fix UX bugs | Pre–M1.6 |
| Paid pilot | Persistent DB, domain, backups, turn off DevSeed | M2.1–M2.4 |
| Internal | SQL Server/Postgres, CI/CD, audit, integrations | M3.x–M5.x |

**Current build status (approx.):** Phase 1 MVP **~80%** — enough to demo; not yet internal-grade.

---

## 6. Risks to manage with the client (honesty builds trust)

| Risk | Mitigation in demo/pilot |
|------|---------------------------|
| Free host sleeps / slow | Use Fly.io or Oracle for demo; move to paid before real pilot |
| SQLite limits | Fine for pilot; plan Postgres/SQL Server before “internal” |
| Catalogue empty | Seed demo or import before every client meeting |
| Regulatory / DRAP | Position as **ordering workflow tool**; legal review is client’s counsel |
| Scope creep | Written SOW: what’s in pilot vs Phase C |

---

## 7. Simple SOW outline (copy for proposals)

**In scope — Demo / pilot**

- Hosted web app + API
- Store registration & approval
- Catalogue browse, cart, order submit
- Admin: stores, import, stock adjust, notifications
- Training session (1–2 hours)
- Bugfix window: ___ weeks

**Out of scope (unless priced)**

- Payment gateway
- Native iOS/Android
- ERP integration
- Multi-distributor SaaS
- DRAP / legal certification
- 24/7 SLA

**Optional add-ons**

- Custom domain & branding
- SMS on new order
- Reporting / exports
- Dedicated staging environment

---

## 8. Your next actions (practical)

| # | Action |
|---|--------|
| 1 | Deploy **demo URL** (see `12-PRODUCTION-DEPLOY-FREE.md`, Option B or C) |
| 2 | Record a **2-minute screen video** of the walkthrough (backup if live demo fails) |
| 3 | Prepare **one-page PDF** from Executive Overview + this doc §3 checklist |
| 4 | Agree with client: **pilot store count**, **duration**, **paid pilot fee** |
| 5 | After “yes” — turn off demo seed, real admin, import their catalogue, paid host |

---

## 9. Related documents

| Doc | Use |
|-----|-----|
| `00-EXECUTIVE-OVERVIEW.md` | Sponsor one-pager |
| `04-MILESTONES-AND-PHASES.md` | Formal phase language |
| `12-PRODUCTION-DEPLOY-FREE.md` | Demo hosting steps |
| `06-USER-STORIES-AND-ACCEPTANCE.md` | Acceptance for pilot sign-off |

---

*Use this doc in client meetings; adjust pricing and timelines to your contract.*
