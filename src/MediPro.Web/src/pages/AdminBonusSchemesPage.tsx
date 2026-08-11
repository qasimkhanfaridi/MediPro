import { type FormEvent, useCallback, useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { apiFetch, parseErrorDetail } from '../api/client'
import type { BonusSchemeDto, ProductDto } from '../api/types'
import { useMediPro } from '../medipro/MediProProvider'

const emptyForm = {
  title: '',
  productSearch: '',
  productId: '',
  buyQuantity: '10',
  bonusQuantity: '1',
  bannerText: '',
  isActive: true,
  sortOrder: '0',
}

export function AdminBonusSchemesPage() {
  const mp = useMediPro()
  const [schemes, setSchemes] = useState<BonusSchemeDto[]>([])
  const [error, setError] = useState<string | null>(null)
  const [msg, setMsg] = useState<string | null>(null)
  const [busy, setBusy] = useState<string | null>(null)
  const [form, setForm] = useState(emptyForm)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [productHits, setProductHits] = useState<ProductDto[]>([])

  const previewLabel = useMemo(() => {
    const buy = Number.parseInt(form.buyQuantity, 10)
    const bonus = Number.parseInt(form.bonusQuantity, 10)
    if (Number.isNaN(buy) || Number.isNaN(bonus) || buy < 1 || bonus < 1) return '—'
    return `${buy}+${bonus}`
  }, [form.buyQuantity, form.bonusQuantity])

  const loadSchemes = useCallback(async () => {
    if (!mp.token) return
    setError(null)
    setBusy('load')
    try {
      const res = await apiFetch('/api/admin/bonus-schemes', { accessToken: mp.token })
      const text = await res.text()
      if (!res.ok) {
        setError(parseErrorDetail(text) || `HTTP ${res.status}`)
        setSchemes([])
        return
      }
      setSchemes(JSON.parse(text) as BonusSchemeDto[])
    } finally {
      setBusy(null)
    }
  }, [mp.token])

  useEffect(() => {
    if (mp.token && mp.isAdmin) void loadSchemes()
  }, [mp.token, mp.isAdmin, loadSchemes])

  useEffect(() => {
    if (!mp.token || form.productSearch.trim().length < 2) {
      setProductHits([])
      return
    }
    const q = form.productSearch.trim()
    let cancelled = false
    void (async () => {
      const res = await apiFetch(
        `/api/products?search=${encodeURIComponent(q)}&pageSize=8`,
        { accessToken: mp.token },
      )
      if (!res.ok || cancelled) return
      const data = (await res.json()) as { items: ProductDto[] }
      if (!cancelled) setProductHits(data.items)
    })()
    return () => {
      cancelled = true
    }
  }, [form.productSearch, mp.token])

  function resetForm() {
    setForm(emptyForm)
    setEditingId(null)
    setProductHits([])
  }

  function startEdit(scheme: BonusSchemeDto) {
    setEditingId(scheme.id)
    setForm({
      title: scheme.title,
      productSearch: scheme.productName ?? '',
      productId: scheme.productId ?? '',
      buyQuantity: String(scheme.buyQuantity),
      bonusQuantity: String(scheme.bonusQuantity),
      bannerText: scheme.bannerText ?? '',
      isActive: scheme.isActive,
      sortOrder: String(scheme.sortOrder),
    })
  }

  async function submitForm(e: FormEvent) {
    e.preventDefault()
    if (!mp.token) return

    const buyQuantity = Number.parseInt(form.buyQuantity, 10)
    const bonusQuantity = Number.parseInt(form.bonusQuantity, 10)
    const sortOrder = Number.parseInt(form.sortOrder, 10) || 0

    if (!form.title.trim()) {
      setError('Title is required.')
      return
    }
    if (!form.productId) {
      setError('Select a product — bonus applies to one SKU only.')
      return
    }

    const body = {
      title: form.title.trim(),
      manufacturer: null,
      productId: form.productId,
      buyQuantity,
      bonusQuantity,
      bannerText: form.bannerText.trim() || null,
      isActive: form.isActive,
      sortOrder,
      validFromUtc: null,
      validToUtc: null,
    }

    setError(null)
    setMsg(null)
    setBusy('save')
    try {
      const url = editingId
        ? `/api/admin/bonus-schemes/${editingId}`
        : '/api/admin/bonus-schemes'
      const res = await apiFetch(url, {
        method: editingId ? 'PUT' : 'POST',
        accessToken: mp.token,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body),
      })
      const text = await res.text()
      if (!res.ok) {
        setError(parseErrorDetail(text) || `HTTP ${res.status}`)
        return
      }
      setMsg(editingId ? 'Bonus scheme updated.' : 'Bonus scheme created.')
      resetForm()
      await loadSchemes()
    } finally {
      setBusy(null)
    }
  }

  async function removeScheme(id: string) {
    if (!mp.token || !window.confirm('Delete this bonus scheme?')) return
    setBusy(`delete-${id}`)
    setError(null)
    try {
      const res = await apiFetch(`/api/admin/bonus-schemes/${id}`, {
        method: 'DELETE',
        accessToken: mp.token,
      })
      if (!res.ok) {
        const text = await res.text()
        setError(parseErrorDetail(text) || `HTTP ${res.status}`)
        return
      }
      setMsg('Bonus scheme removed.')
      if (editingId === id) resetForm()
      await loadSchemes()
    } finally {
      setBusy(null)
    }
  }

  async function seedDemo() {
    if (!mp.token) return
    setBusy('seed')
    setError(null)
    setMsg(null)
    try {
      const res = await apiFetch('/api/admin/bonus-schemes/seed-demo', {
        method: 'POST',
        accessToken: mp.token,
      })
      const text = await res.text()
      if (!res.ok) {
        setError(parseErrorDetail(text) || `HTTP ${res.status}`)
        return
      }
      const j = JSON.parse(text) as { message?: string }
      setMsg(j.message ?? 'Demo bonus schemes ready.')
      await loadSchemes()
    } finally {
      setBusy(null)
    }
  }

  return (
    <main className="main single-page">
      <header className="page-header">
        <h1 className="page-title">Bonus &amp; offers</h1>
        <p className="page-lead">
          Set product-specific schemes like <strong>10+1</strong>, <strong>5+1</strong>, or{' '}
          <strong>20+2</strong> on individual SKUs. Stores see banners and badges on the catalogue.
        </p>
      </header>

      <section className="panel">
        <div className="panel-head">
          <h2>{editingId ? 'Edit scheme' : 'New scheme'}</h2>
          <div className="panel-head-actions">
            <button
              type="button"
              className="btn btn-ghost small"
              disabled={busy === 'seed'}
              onClick={() => void seedDemo()}
            >
              {busy === 'seed' ? 'Seeding…' : 'Seed demo offers'}
            </button>
            <Link to="/admin" className="btn btn-ghost">
              ← Distributor console
            </Link>
          </div>
        </div>

        <form className="bonus-scheme-form" onSubmit={(e) => void submitForm(e)}>
          <div className="field">
            <label htmlFor="bs-title">Title</label>
            <input
              id="bs-title"
              value={form.title}
              onChange={(e) => setForm((f) => ({ ...f, title: e.target.value }))}
              placeholder="e.g. Panadol 10+1 promo"
            />
          </div>

          <div className="field">
            <label htmlFor="bs-product">Product *</label>
            <input
              id="bs-product"
              type="search"
              value={form.productSearch}
              onChange={(e) =>
                setForm((f) => ({ ...f, productSearch: e.target.value, productId: '' }))
              }
              placeholder="Search by name or SKU…"
              autoComplete="off"
            />
            {productHits.length > 0 && !form.productId && (
              <ul className="product-search-hits">
                {productHits.map((p) => (
                  <li key={p.id}>
                    <button
                      type="button"
                      className="linkish"
                      onClick={() =>
                        setForm((f) => ({
                          ...f,
                          productId: p.id,
                          productSearch: `${p.name} (${p.skuCode})`,
                        }))
                      }
                    >
                      {p.name} · {p.skuCode} · {p.manufacturer}
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </div>

          <div className="bonus-qty-row">
            <div className="field">
              <label htmlFor="bs-buy">Buy quantity</label>
              <input
                id="bs-buy"
                type="number"
                min={1}
                value={form.buyQuantity}
                onChange={(e) => setForm((f) => ({ ...f, buyQuantity: e.target.value }))}
              />
            </div>
            <div className="field">
              <label htmlFor="bs-bonus">Bonus (free) qty</label>
              <input
                id="bs-bonus"
                type="number"
                min={1}
                value={form.bonusQuantity}
                onChange={(e) => setForm((f) => ({ ...f, bonusQuantity: e.target.value }))}
              />
            </div>
            <div className="bonus-preview-pill" aria-live="polite">
              Preview: <strong>{previewLabel}</strong>
            </div>
          </div>

          <div className="field">
            <label htmlFor="bs-banner">Banner text (optional)</label>
            <input
              id="bs-banner"
              value={form.bannerText}
              onChange={(e) => setForm((f) => ({ ...f, bannerText: e.target.value }))}
              placeholder="Shown on store catalogue banner"
            />
          </div>

          <div className="field checkbox-field">
            <label>
              <input
                type="checkbox"
                checked={form.isActive}
                onChange={(e) => setForm((f) => ({ ...f, isActive: e.target.checked }))}
              />
              Active (visible to stores)
            </label>
          </div>

          <div className="actions">
            <button type="submit" className="btn-primary" disabled={busy === 'save'}>
              {busy === 'save' ? 'Saving…' : editingId ? 'Update scheme' : 'Create scheme'}
            </button>
            {editingId && (
              <button type="button" className="secondary" onClick={() => resetForm()}>
                Cancel edit
              </button>
            )}
          </div>
        </form>

        {msg && <p className="help">{msg}</p>}
        {error && (
          <p className="error" role="alert">
            {error}
          </p>
        )}
      </section>

      <section className="panel">
        <h2>Active schemes ({schemes.length})</h2>
        {busy === 'load' && <p className="meta">Loading…</p>}
        {schemes.length === 0 && busy !== 'load' && (
          <p className="meta">No bonus schemes yet. Create one or use “Seed demo offers”.</p>
        )}
        {schemes.length > 0 && (
          <div className="store-table-wrap">
            <table className="store-table">
              <thead>
                <tr>
                  <th>Label</th>
                  <th>Title</th>
                  <th>Product</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {schemes.map((s) => (
                  <tr key={s.id}>
                    <td>
                      <span className="bonus-badge">{s.label}</span>
                    </td>
                    <td>{s.title}</td>
                    <td>
                      {s.productId ? (
                        <>
                          <span className="sku">{s.productName ?? s.productId}</span>
                          {s.manufacturer && (
                            <>
                              <br />
                              <span className="meta">{s.manufacturer}</span>
                            </>
                          )}
                        </>
                      ) : (
                        <span className="meta">Legacy — delete and recreate on a product</span>
                      )}
                    </td>
                    <td>{s.isActive ? 'Active' : 'Off'}</td>
                    <td>
                      <button
                        type="button"
                        className="small secondary"
                        disabled={busy === `delete-${s.id}`}
                        onClick={() => startEdit(s)}
                      >
                        Edit
                      </button>{' '}
                      <button
                        type="button"
                        className="small secondary"
                        disabled={busy === `delete-${s.id}`}
                        onClick={() => void removeScheme(s.id)}
                      >
                        Delete
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </main>
  )
}
