# Data Model Sketch — MediPro (Conceptual)

## 1. Purpose

High-level entity relationships for engineering design—not a final DDL. Adjust types, naming, and normalization during implementation.

---

## 2. Core entities

### Tenant (optional but recommended for future SaaS)

| Field | Notes |
|-------|--------|
| id | PK |
| name | Distributor legal/trade name |
| settings | JSON: currency, tax flags, order workflow enum |

---

### User

| Field | Notes |
|-------|--------|
| id | PK |
| tenant_id | FK → Tenant |
| email / mobile | Unique per tenant per identifier policy |
| password_hash | |
| role | enum: super_admin, distributor_admin, store_user |
| status | active, suspended |
| store_id | Nullable; FK when role is store_user |

---

### Store (medical store / pharmacy account)

| Field | Notes |
|-------|--------|
| id | PK |
| tenant_id | FK |
| business_name | |
| address_line1/2 | |
| city | Index for RWP/ISB filters |
| province | |
| license_number | As per policy |
| ntn | Optional |
| contact_name | |
| mobile | |
| approval_status | pending, approved, rejected, suspended |
| created_at / updated_at | |

---

### Product

| Field | Notes |
|-------|--------|
| id | PK |
| tenant_id | FK |
| sku_code | Unique per tenant |
| name | |
| pack | e.g. "10's", "120 ml" |
| form | tablet, syrup, etc. |
| manufacturer | company |
| salt_composition | text for search |
| category_id | FK optional |
| mrp / trade_price | Decimal; snapshot rules on order |
| is_active | |
| stock_quantity | If model A |
| availability_band | If model B (enum) |
| created_at / updated_at | |

---

### Category (optional)

| Field | Notes |
|-------|--------|
| id | PK |
| tenant_id | FK |
| name | |

---

### Cart / CartLine

**Approach:** Server-side cart keyed by `user_id`.

| CartLine | Notes |
|----------|--------|
| id | PK |
| user_id | FK |
| product_id | FK |
| quantity | |
| unit_price_snapshot | Optional pre-submit |

---

### Order

| Field | Notes |
|-------|--------|
| id | PK |
| tenant_id | FK |
| store_id | FK |
| user_id | Who submitted |
| status | enum per PRD |
| total_amount | |
| currency | PKR default |
| submitted_at | |
| notes | Optional |

---

### OrderLine

| Field | Notes |
|-------|--------|
| id | PK |
| order_id | FK |
| product_id | FK (historical reference even if product changes) |
| product_name_snapshot | |
| pack_snapshot | |
| unit_price_snapshot | |
| quantity | |
| line_total | |

---

### Notification

| Field | Notes |
|-------|--------|
| id | PK |
| user_id | Recipient |
| type | new_order, order_status, low_stock, etc. |
| payload | JSON |
| read_at | Nullable |
| created_at | |

---

### AuditLog (minimum)

| Field | Notes |
|-------|--------|
| id | PK |
| actor_user_id | |
| entity_type | product, order, store |
| entity_id | |
| action | price_change, status_change |
| old_value / new_value | JSON |

---

### ImportJob (Excel bulk)

| Field | Notes |
|-------|--------|
| id | PK |
| tenant_id | |
| created_by | user id |
| status | queued, processing, completed, failed |
| error_file_url | Optional |

---

## 3. Relationship diagram (text)

```
Tenant 1──* User
Tenant 1──* Store
Tenant 1──* Product
Store 1──* Order
Order 1──* OrderLine ──* Product (reference + snapshots)
User 1──* CartLine ──* Product
User 1──* Notification
```

---

## 4. Indexing hints

- Product: tenant_id + lower(name), tenant_id + manufacturer, full-text on name+salt if supported.
- Order: tenant_id + submitted_at DESC, store_id + submitted_at DESC.

---

## Revision history

| Version | Date | Notes |
|---------|------|-------|
| 1.0 | | Initial conceptual model |
