# Mobile strategy — MediPro (no dedicated mobile developer)

**Audience:** Delivery lead, client sponsor, future mobile hire.  
**Status:** Planning only — no mobile app in build scope yet.  
**Related:** `04-MILESTONES-AND-PHASES.md` (Phase 4), `05-TECHNICAL-ARCHITECTURE.md`, UAT `https://medipro-uat.fly.dev`

---

## 1. Current position

| Fact | Implication |
|------|-------------|
| MediPro is **API-first** (ASP.NET Core REST + JWT) | A future app reuses the same `/api/*` endpoints as the web SPA |
| Web app is **mobile-friendly** (React / Vite) | Pharmacies can already order from a phone browser |
| **No mobile developer** on the team now | Do **not** start Flutter/React Native/native until you hire or contract one |
| Client may ask “do you have an app?” | Answer: **web works on phones today**; store apps are Phase 4 when staffed |

**Recommendation for this stage:** Document the options, keep shipping the web pilot, and only wrap or rebuild when there is budget + a mobile-capable person.

---

## 2. What Cordova is (yes — we know it)

**Apache Cordova** (formerly PhoneGap) takes a **web app** (HTML/CSS/JavaScript) and packages it inside a native **WebView** shell so you can publish to the **Google Play Store** / **Apple App Store**.

```text
Your web UI (React/Vite)  →  Cordova/Capacitor shell  →  .apk / .aab / .ipa
         │
         └── still calls the same MediPro API over HTTPS
```

| Pros | Cons |
|------|------|
| Reuses existing MediPro.Web — no rewrite | App store listing still needs icons, privacy policy, signing keys |
| One web codebase for browser + “app” | Performance and feel are “website in a box,” not true native |
| Smaller learning curve for **web** developers | Cordova plugins can be outdated; tooling is older |
| Can show client an icon on the phone home screen | iOS builds need a Mac + Apple Developer account ($99/yr) |

### Cordova vs Capacitor (important)

| | Cordova | Capacitor (Ionic) |
|--|---------|-------------------|
| Idea | Same: wrap web in WebView | Same idea, **modern** successor |
| Maintenance | Mature but quieter | Active; preferred for new projects |
| Fits MediPro React/Vite | Possible | **Better fit** if you wrap the SPA |
| Plugins | cordova-* | Capacitor plugins |

**If the goal is “put our existing web in the Play Store without a mobile specialist,” prefer Capacitor over classic Cordova** — same concept, better DX. Mention Cordova to clients as the *category* (“hybrid / WebView app”); implement with Capacitor when the time comes.

---

## 3. Options ranked for *this* stage

| Option | Effort without mobile hire | Client perception | When to choose |
|--------|----------------------------|-------------------|----------------|
| **A. Mobile web only** (current) | None | “Works on phone in Chrome/Safari” | **Default now** |
| **B. PWA** (Add to Home Screen) | Low (web skills) | Icon on phone, no store | Nice interim if client wants “app-like” |
| **C. Capacitor / Cordova wrap** | Medium (web + store accounts) | Real Play Store listing | When client insists on store icon and you have ~1–2 weeks + Android focus |
| **D. Flutter / React Native** | High | Best UX long-term | After hiring mobile (or agency) — Phase 4 |
| **E. Native Kotlin/Swift** | Highest | Best platform fit | Usually overkill for B2B ordering |

**Do not start D or E without a mobile developer.**  
**Do not promise Cordova/Capacitor delivery dates** until store accounts and a maintainer are clear.

---

## 4. What already exists for “mobile API”

Nothing separate is required. Pharmacy app (any stack) would use:

| Flow | Endpoints |
|------|-----------|
| Auth | `POST /api/auth/login`, `POST /api/auth/register-store` |
| Catalogue | `GET /api/products`, `GET /api/products/{id}`, `GET /api/products/filters` |
| Offers | `GET /api/bonus-schemes` |
| Cart | `GET/POST/PATCH/DELETE /api/cart...` |
| Orders | `POST /api/orders/submit`, `GET /api/orders`, `GET /api/orders/{id}` |

Admin (stores, Excel, stock, bonus CRUD) can stay **web-only** for a long time.

**Later API work (when a real app ships):** refresh tokens, optional `/api/v1` versioning, push device registration — see Phase 4 `M4.1`.

---

## 5. Suggested client messaging

> “MediPro is built API-first so a mobile app can reuse the same backend. Today pharmacies use the **mobile web** (works on Android/iPhone browsers). A Play Store / App Store app is a **later phase** — either wrapping this web app (hybrid / Cordova-style) or a dedicated app once we have mobile capacity. We are not blocked on ordering; we are sequencing store apps after the pilot is stable.”

Avoid promising:

- Native apps in the current pilot SOW  
- Offline-first catalogue without a scoped project  
- iOS + Android same week without Mac + Apple account  

---

## 6. If Cordova/Capacitor is chosen later — checklist

Only when client commits and someone owns store releases:

1. Confirm **Android-first** (majority of PK pharmacy devices).  
2. Google Play Developer account (~ one-time fee).  
3. Wrap `MediPro.Web` production build with **Capacitor** (or Cordova if mandated).  
4. Point app config at production API URL (not localhost).  
5. Privacy policy URL + screenshots for store listing.  
6. Test: login, catalogue, cart, order submit on mid-range Android.  
7. Plan updates: every web release may need a new store build if you ship a frozen bundle (or use a remote URL strategy — document trade-offs with counsel/security).

**Remote URL vs bundled assets:** loading the live website inside the WebView reduces store update frequency but some stores discourage “thin” wrappers; decide with client and store policies.

---

## 7. Decision log (fill when ready)

| Date | Decision | Owner |
|------|----------|--------|
| | Stay on mobile web for pilot | |
| | PWA “Add to Home Screen” yes/no | |
| | Hybrid (Capacitor/Cordova) after pilot yes/no | |
| | Hire Flutter/RN vs agency vs wrap-only | |

---

## 8. Bottom line

| Question | Answer |
|----------|--------|
| Do we need a mobile developer *now*? | **No** — keep pilot on web |
| Is Cordova a real option? | **Yes** — hybrid wrap of the web app; prefer **Capacitor** as the modern approach |
| Where do we start? | This document + keep API stable; no new mobile codebase until staffed or hybrid is explicitly funded |
| First technical step when ready | Capacitor Android wrap **or** hire for Flutter/RN — not both at once |

---

*Created for planning while MediPro has no dedicated mobile developer. Revisit before Phase 4 (mobile apps & payments).*
