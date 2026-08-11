import { type FormEvent, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { apiFetch, parseErrorDetail } from '../api/client'
import type { PagedProducts, ProductDto } from '../api/types'
import { useMediPro } from '../medipro/MediProProvider'

const emptyCreate = {
  skuCode: '',
  name: '',
  pack: '',
  manufacturer: '',
  saltComposition: '',
  category: '',
  tradePrice: '',
  mrp: '',
  imageUrl: '',
}

export function AdminProductsPage() {
  const mp = useMediPro()
  const [products, setProducts] = useState<PagedProducts | null>(null)
  const [search, setSearch] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [msg, setMsg] = useState<string | null>(null)
  const [busy, setBusy] = useState<string | null>(null)
  const [create, setCreate] = useState(emptyCreate)
  const [stockSku, setStockSku] = useState('')
  const [stockIn, setStockIn] = useState(true)

  const load = async (q?: string) => {
    if (!mp.token) return
    setBusy('load')
    setError(null)
    try {
      const params = new URLSearchParams({ pageSize: '100' })
      if (q?.trim()) params.set('search', q.trim())
      const res = await apiFetch(`/api/products?${params}`, { accessToken: mp.token })
      const text = await res.text()
      if (!res.ok) {
        setError(parseErrorDetail(text) || `HTTP ${res.status}`)
        setProducts(null)
        return
      }
      setProducts(JSON.parse(text) as PagedProducts)
    } finally {
      setBusy(null)
    }
  }

  useEffect(() => {
    if (mp.token && mp.isAdmin) void load()
  }, [mp.token, mp.isAdmin])

  async function submitCreate(e: FormEvent) {
    e.preventDefault()
    if (!mp.token) return
    setBusy('create')
    setError(null)
    setMsg(null)
    try {
      const res = await apiFetch('/api/products', {
        method: 'POST',
        accessToken: mp.token,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          skuCode: create.skuCode.trim(),
          name: create.name.trim(),
          pack: create.pack.trim(),
          manufacturer: create.manufacturer.trim(),
          saltComposition: create.saltComposition.trim(),
          category: create.category.trim() || null,
          tradePrice: Number(create.tradePrice),
          mrp: create.mrp.trim() ? Number(create.mrp) : null,
          imageUrl: create.imageUrl.trim() || null,
          stockQuantity: null,
        }),
      })
      const text = await res.text()
      if (!res.ok) {
        setError(parseErrorDetail(text) || `HTTP ${res.status}`)
        return
      }
      setMsg('Product created.')
      setCreate(emptyCreate)
      await load(search)
    } finally {
      setBusy(null)
    }
  }

  async function setStock(e: FormEvent) {
    e.preventDefault()
    if (!mp.token || !stockSku.trim()) return
    setBusy('stock')
    setError(null)
    try {
      const res = await apiFetch('/api/admin/catalog/stock-status', {
        method: 'POST',
        accessToken: mp.token,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ skuCode: stockSku.trim(), inStock: stockIn }),
      })
      const text = await res.text()
      if (!res.ok) {
        setError(parseErrorDetail(text) || `HTTP ${res.status}`)
        return
      }
      setMsg(stockIn ? `${stockSku} marked in stock.` : `${stockSku} marked out of stock.`)
      await load(search)
    } finally {
      setBusy(null)
    }
  }

  async function toggleActive(p: ProductDto) {
    if (!mp.token) return
    const path = p.isActive ? 'deactivate' : 'activate'
    setBusy(`active-${p.id}`)
    setError(null)
    try {
      const res = await apiFetch(`/api/products/${p.id}/${path}`, {
        method: 'POST',
        accessToken: mp.token,
      })
      if (!res.ok) {
        const text = await res.text()
        setError(parseErrorDetail(text) || `HTTP ${res.status}`)
        return
      }
      setMsg(p.isActive ? `${p.skuCode} removed from catalogue.` : `${p.skuCode} reactivated.`)
      await load(search)
    } finally {
      setBusy(null)
    }
  }

  return (
    <main className="main single-page">
      <header className="page-header">
        <h1 className="page-title">Product catalogue</h1>
        <p className="page-lead">
          Add SKUs, set in stock / out of stock, or deactivate products without using Excel.
        </p>
        <div className="actions page-header-actions">
          <Link to="/admin" className="btn btn-ghost">
            ← Distributor console
          </Link>
        </div>
      </header>

      <section className="panel">
        <h2>Quick stock (in / out)</h2>
        <p className="help">
          Pharmacies see <strong>In stock</strong> or <strong>Out of stock</strong> only — no
          unit counts on the store portal.
        </p>
        <form className="stock-adjust-form" onSubmit={(e) => void setStock(e)}>
          <div className="field">
            <label htmlFor="ps-sku">SKU code</label>
            <input
              id="ps-sku"
              value={stockSku}
              onChange={(e) => setStockSku(e.target.value)}
              autoComplete="off"
            />
          </div>
          <div className="field">
            <label htmlFor="ps-in">Availability</label>
            <select
              id="ps-in"
              value={stockIn ? 'in' : 'out'}
              onChange={(e) => setStockIn(e.target.value === 'in')}
            >
              <option value="in">In stock</option>
              <option value="out">Out of stock</option>
            </select>
          </div>
          <button type="submit" className="btn-primary" disabled={busy === 'stock'}>
            {busy === 'stock' ? 'Saving…' : 'Apply'}
          </button>
        </form>
      </section>

      <section className="panel">
        <h2>Add product</h2>
        <form className="bonus-scheme-form" onSubmit={(e) => void submitCreate(e)}>
          <div className="field">
            <label htmlFor="np-sku">SKU *</label>
            <input
              id="np-sku"
              required
              value={create.skuCode}
              onChange={(e) => setCreate((c) => ({ ...c, skuCode: e.target.value }))}
            />
          </div>
          <div className="field">
            <label htmlFor="np-name">Name *</label>
            <input
              id="np-name"
              required
              value={create.name}
              onChange={(e) => setCreate((c) => ({ ...c, name: e.target.value }))}
            />
          </div>
          <div className="field">
            <label htmlFor="np-pack">Pack *</label>
            <input
              id="np-pack"
              required
              value={create.pack}
              onChange={(e) => setCreate((c) => ({ ...c, pack: e.target.value }))}
            />
          </div>
          <div className="field">
            <label htmlFor="np-mfg">Manufacturer *</label>
            <input
              id="np-mfg"
              required
              value={create.manufacturer}
              onChange={(e) => setCreate((c) => ({ ...c, manufacturer: e.target.value }))}
            />
          </div>
          <div className="field">
            <label htmlFor="np-salt">Salt / composition *</label>
            <input
              id="np-salt"
              required
              value={create.saltComposition}
              onChange={(e) => setCreate((c) => ({ ...c, saltComposition: e.target.value }))}
            />
          </div>
          <div className="field">
            <label htmlFor="np-price">Trade price (PKR) *</label>
            <input
              id="np-price"
              type="number"
              min={0}
              step="0.01"
              required
              value={create.tradePrice}
              onChange={(e) => setCreate((c) => ({ ...c, tradePrice: e.target.value }))}
            />
          </div>
          <button type="submit" className="btn-primary" disabled={busy === 'create'}>
            {busy === 'create' ? 'Creating…' : 'Create product'}
          </button>
        </form>
      </section>

      <section className="panel">
        <div className="panel-head">
          <h2>All products</h2>
          <form
            className="catalog-search-row"
            onSubmit={(e) => {
              e.preventDefault()
              void load(search)
            }}
          >
            <input
              type="search"
              placeholder="Search SKU or name…"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
            <button type="submit" className="secondary" disabled={busy === 'load'}>
              Search
            </button>
          </form>
        </div>

        {msg && <p className="help">{msg}</p>}
        {error && (
          <p className="error" role="alert">
            {error}
          </p>
        )}

        {busy === 'load' && <p className="meta">Loading…</p>}
        {products && products.items.length > 0 && (
          <div className="store-table-wrap">
            <table className="store-table">
              <thead>
                <tr>
                  <th>SKU</th>
                  <th>Name</th>
                  <th>Company</th>
                  <th>Trade</th>
                  <th>Stock</th>
                  <th>Active</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {products.items.map((p) => (
                  <tr key={p.id} className={!p.isActive ? 'row-muted' : undefined}>
                    <td>{p.skuCode}</td>
                    <td>{p.name}</td>
                    <td>{p.manufacturer}</td>
                    <td>{p.tradePrice}</td>
                    <td>
                      {p.inStock === false ? (
                        <span className="product-stock-out">Out</span>
                      ) : (
                        <span className="product-stock-in">In</span>
                      )}
                      {p.stockQuantity != null && (
                        <span className="sku"> · qty {p.stockQuantity}</span>
                      )}
                    </td>
                    <td>{p.isActive ? 'Yes' : 'No'}</td>
                    <td>
                      <button
                        type="button"
                        className="small secondary"
                        disabled={busy === `active-${p.id}`}
                        onClick={() => void toggleActive(p)}
                      >
                        {p.isActive ? 'Deactivate' : 'Activate'}
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
