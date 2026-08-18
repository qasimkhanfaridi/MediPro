import { type FormEvent, useEffect, useRef, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { apiFetch, parseErrorDetail } from '../api/client'
import type { BonusSchemeSummary, CatalogFilterOptions, ProductDto } from '../api/types'
import { AddToCartDialog } from '../components/AddToCartDialog'
import { useMediPro } from '../medipro/MediProProvider'

function formatPkr(n: number): string {
  return n.toLocaleString('en-PK', {
    minimumFractionDigits: 0,
    maximumFractionDigits: 2,
  })
}

export function CatalogPage() {
  const mp = useMediPro()
  const navigate = useNavigate()
  const [searchInput, setSearchInput] = useState('')
  const [manufacturer, setManufacturer] = useState('')
  const [category, setCategory] = useState('')
  const [salt, setSalt] = useState('')
  const [filterOptions, setFilterOptions] = useState<CatalogFilterOptions | null>(null)
  const [filterOptionsError, setFilterOptionsError] = useState<string | null>(null)
  const [bonusOffers, setBonusOffers] = useState<BonusSchemeSummary[]>([])
  const [suggestions, setSuggestions] = useState<ProductDto[]>([])
  const [suggestOpen, setSuggestOpen] = useState(false)
  const [pending, setPending] = useState<ProductDto | null>(null)
  const searchBoxRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!mp.token) return
    const handle = window.setTimeout(() => {
      void mp.loadProducts({
        search: searchInput,
        manufacturer: manufacturer || undefined,
        category: category || undefined,
        salt: salt || undefined,
      })
    }, 250)
    return () => window.clearTimeout(handle)
  }, [mp.token, mp.loadProducts, searchInput, manufacturer, category, salt])

  // Type-ahead list under the search box, independent of the grid below.
  useEffect(() => {
    const q = searchInput.trim()
    if (!mp.token || q.length < 1) {
      setSuggestions([])
      return
    }
    let cancelled = false
    const handle = window.setTimeout(() => {
      void (async () => {
        try {
          const res = await apiFetch(
            `/api/products?search=${encodeURIComponent(q)}&pageSize=8`,
            { accessToken: mp.token },
          )
          if (!res.ok || cancelled) return
          const data = (await res.json()) as { items: ProductDto[] }
          if (!cancelled) setSuggestions(data.items)
        } catch {
          if (!cancelled) setSuggestions([])
        }
      })()
    }, 200)
    return () => {
      cancelled = true
      window.clearTimeout(handle)
    }
  }, [searchInput, mp.token])

  useEffect(() => {
    function onDocClick(e: MouseEvent) {
      if (!searchBoxRef.current?.contains(e.target as Node)) setSuggestOpen(false)
    }
    document.addEventListener('mousedown', onDocClick)
    return () => document.removeEventListener('mousedown', onDocClick)
  }, [])

  useEffect(() => {
    if (!mp.token) {
      setFilterOptions(null)
      setFilterOptionsError(null)
      return
    }
    let cancelled = false
    setFilterOptionsError(null)
    void (async () => {
      try {
        const res = await apiFetch('/api/products/filters', {
          accessToken: mp.token,
        })
        const text = await res.text()
        if (cancelled) return
        if (!res.ok) {
          setFilterOptionsError(parseErrorDetail(text) || `HTTP ${res.status}`)
          setFilterOptions(null)
          return
        }
        setFilterOptions(JSON.parse(text) as CatalogFilterOptions)
      } catch {
        if (!cancelled) setFilterOptionsError('Could not load filter options.')
      }
    })()
    return () => {
      cancelled = true
    }
  }, [mp.token])

  useEffect(() => {
    if (!mp.token || !mp.canUseCart) {
      setBonusOffers([])
      return
    }
    let cancelled = false
    void (async () => {
      try {
        const res = await apiFetch('/api/bonus-schemes', { accessToken: mp.token })
        if (!res.ok || cancelled) return
        setBonusOffers((await res.json()) as BonusSchemeSummary[])
      } catch {
        if (!cancelled) setBonusOffers([])
      }
    })()
    return () => {
      cancelled = true
    }
  }, [mp.token, mp.canUseCart])

  if (!mp.token) {
    return (
      <main className="main single-page">
        <section className="panel panel-center">
          <h1 className="page-title">Catalogue</h1>
          <p className="page-lead">
            Sign in to see your distributor&apos;s approved product list and trade
            prices.
          </p>
          <Link to="/login?next=%2Fcatalog" className="btn btn-primary">
            Go to sign in
          </Link>
        </section>
      </main>
    )
  }

  function runSearch(e: FormEvent) {
    e.preventDefault()
    void mp.loadProducts({
      search: searchInput,
      manufacturer: manufacturer || undefined,
      category: category || undefined,
      salt: salt || undefined,
    })
  }

  function showAll() {
    setSearchInput('')
    setManufacturer('')
    setCategory('')
    setSalt('')
    setSuggestOpen(false)
  }

  /** Pharmacy accounts get the quantity prompt; anyone else just opens the product. */
  function pickSuggestion(product: ProductDto) {
    setSuggestOpen(false)
    if (mp.canUseCart) setPending(product)
    else navigate(`/catalog/${product.id}`)
  }

  function askQuantity(product: ProductDto) {
    setSuggestOpen(false)
    setPending(product)
  }

  async function confirmAdd(quantity: number) {
    if (!pending) return
    await mp.addOneToCart(pending.id, quantity)
    setPending(null)
  }

  const activeFilterBits: string[] = []
  if (searchInput.trim()) activeFilterBits.push(`search “${searchInput.trim()}”`)
  if (manufacturer) activeFilterBits.push(`company ${manufacturer}`)
  if (category) activeFilterBits.push(`category ${category}`)
  if (salt.trim()) activeFilterBits.push(`salt contains “${salt.trim()}”`)

  return (
    <main className="main single-page">
      <header className="page-header">
        <h1 className="page-title">Catalogue</h1>
        <p className="page-lead">
          Type to search — results update as you go. Combine with company, category, or
          salt filters. Prices are trade (PKR) before any distributor discounts.
        </p>
      </header>

      <section className="panel">
        <form className="catalog-toolbar catalog-toolbar-stack" onSubmit={runSearch}>
          <div className="catalog-search-row" ref={searchBoxRef}>
            <label className="visually-hidden" htmlFor="catalog-search">
              Search products
            </label>
            <div className="catalog-search-box">
              <input
                id="catalog-search"
                className="catalog-search-input"
                type="search"
                placeholder="Type a name, SKU, company or salt…"
                value={searchInput}
                onChange={(e) => {
                  setSearchInput(e.target.value)
                  setSuggestOpen(true)
                }}
                onFocus={() => setSuggestOpen(true)}
                autoComplete="off"
                role="combobox"
                aria-expanded={suggestOpen && suggestions.length > 0}
                aria-controls="catalog-suggestions"
                autoFocus
              />
              {suggestOpen && searchInput.trim() && (
                <ul className="search-suggestions" id="catalog-suggestions" role="listbox">
                  {suggestions.length === 0 ? (
                    <li className="search-suggestion-empty">No match yet — keep typing</li>
                  ) : (
                    suggestions.map((s) => (
                      <li key={s.id}>
                        <button
                          type="button"
                          className="search-suggestion"
                          disabled={mp.canUseCart && s.inStock === false}
                          onClick={() => pickSuggestion(s)}
                        >
                          <span className="search-suggestion-name">{s.name}</span>
                          <span className="search-suggestion-meta">
                            {s.pack} · {s.manufacturer}
                            {s.bonusLabel ? ` · ${s.bonusLabel}` : ''}
                            {s.inStock === false ? ' · out of stock' : ''}
                          </span>
                          <span className="search-suggestion-price">
                            {formatPkr(Number(s.tradePrice))} PKR
                          </span>
                        </button>
                      </li>
                    ))
                  )}
                </ul>
              )}
            </div>
            {(searchInput || manufacturer || category || salt) && (
              <button
                type="button"
                className="secondary"
                disabled={mp.busy === 'products'}
                onClick={() => showAll()}
              >
                Clear
              </button>
            )}
            {mp.canUseCart && (
              <Link to="/cart" className="btn btn-ghost catalog-cart-link">
                Cart →
              </Link>
            )}
          </div>
          <div className="catalog-filters">
            <div className="field catalog-filter-field">
              <label htmlFor="catalog-mfg">Manufacturer</label>
              <select
                id="catalog-mfg"
                value={manufacturer}
                onChange={(e) => setManufacturer(e.target.value)}
              >
                <option value="">All companies</option>
                {(filterOptions?.manufacturers ?? []).map((m) => (
                  <option key={m} value={m}>
                    {m}
                  </option>
                ))}
              </select>
            </div>
            <div className="field catalog-filter-field">
              <label htmlFor="catalog-cat">Category</label>
              <select
                id="catalog-cat"
                value={category}
                onChange={(e) => setCategory(e.target.value)}
              >
                <option value="">All categories</option>
                {(filterOptions?.categories ?? []).map((c) => (
                  <option key={c} value={c}>
                    {c}
                  </option>
                ))}
              </select>
            </div>
            <div className="field catalog-filter-field catalog-filter-salt">
              <label htmlFor="catalog-salt">Salt / composition contains</label>
              <input
                id="catalog-salt"
                type="text"
                autoComplete="off"
                placeholder="e.g. Paracetamol"
                value={salt}
                onChange={(e) => setSalt(e.target.value)}
              />
            </div>
          </div>
        </form>

        {filterOptionsError && (
          <p className="help" role="status">
            {filterOptionsError} You can still search by product name, SKU, manufacturer, or
            salt using the fields above.
          </p>
        )}

        {mp.authInfo?.role === 'StoreUser' &&
          mp.authInfo.storeApprovalStatus !== 'Approved' && (
            <p className="help help-banner">
              Your pharmacy is <strong>{mp.authInfo.storeApprovalStatus}</strong>.
              Ordering is enabled after your distributor approves your account.
            </p>
          )}

        {bonusOffers.length > 0 && (
          <div className="bonus-offers-banner-wrap" role="region" aria-label="Active bonus offers">
            <p className="bonus-offers-kicker">Active offers</p>
            <ul className="bonus-offers-banner-list">
              {bonusOffers.map((o) => (
                <li key={o.id} className="bonus-offers-banner-item">
                  <span className="bonus-badge bonus-badge-lg">{o.label}</span>
                  <span className="bonus-offers-banner-text">{o.bannerText}</span>
                </li>
              ))}
            </ul>
          </div>
        )}

        {mp.products && (
          <p className="catalog-meta" aria-live="polite">
            {mp.busy === 'products' ? 'Updating… · ' : ''}
            {mp.products.totalCount} product
            {mp.products.totalCount === 1 ? '' : 's'}
            {activeFilterBits.length > 0 ? ` · ${activeFilterBits.join(' · ')}` : ''}
          </p>
        )}

        {!mp.products && mp.busy === 'products' && (
          <p className="catalog-meta">Loading products…</p>
        )}

        {mp.products && mp.products.items.length === 0 && (
          <div className="empty-state">
            <p className="empty-title">No products found</p>
            <p className="empty-text">
              Try another letter or filter, or ask your distributor to publish SKUs to your
              catalogue.
            </p>
          </div>
        )}

        {mp.products && mp.products.items.length > 0 && (
          <ul className="product-grid">
            {mp.products.items.map((p) => (
              <li
                key={p.id}
                className={`product-card${p.inStock === false ? ' product-card-out' : ''}`}
              >
                <div className="product-card-top">
                  <span className="product-sku">{p.skuCode}</span>
                  {p.bonusLabel && (
                    <span className="bonus-badge" title={p.bonusTitle ?? p.bonusLabel}>
                      {p.bonusLabel} bonus
                    </span>
                  )}
                  {p.mrp != null && (
                    <span className="product-mrp">MRP {formatPkr(p.mrp)}</span>
                  )}
                </div>
                <h3 className={`product-name${p.inStock === false ? ' product-name-muted' : ''}`}>
                  <Link to={`/catalog/${p.id}`} className="product-name-link">
                    {p.name}
                  </Link>
                </h3>
                <p className="product-pack">{p.pack}</p>
                <p className="product-mfg">{p.manufacturer}</p>
                <p
                  className={
                    p.inStock === false
                      ? 'product-stock product-stock-out'
                      : 'product-stock product-stock-in'
                  }
                >
                  {p.inStock === false ? 'Out of stock' : 'In stock'}
                </p>
                <div className="product-card-foot">
                  <span className="product-price">
                    {formatPkr(Number(p.tradePrice))} <span className="pkr">PKR</span>
                  </span>
                  {mp.canUseCart ? (
                    <button
                      type="button"
                      className="btn-primary btn-compact"
                      disabled={
                        mp.busy === `cart-add-${p.id}` || p.inStock === false
                      }
                      onClick={() => askQuantity(p)}
                    >
                      {p.inStock === false
                        ? 'Unavailable'
                        : mp.busy === `cart-add-${p.id}`
                          ? '…'
                          : 'Add'}
                    </button>
                  ) : (
                    <span className="product-locked">View only</span>
                  )}
                </div>
              </li>
            ))}
          </ul>
        )}

        {mp.productsError && (
          <p className="error" role="alert">
            {mp.productsError}
          </p>
        )}
        {mp.cartError && (
          <p className="error" role="alert">
            {mp.cartError}
          </p>
        )}
      </section>

      {pending && (
        <AddToCartDialog
          product={pending}
          busy={mp.busy === `cart-add-${pending.id}`}
          onCancel={() => setPending(null)}
          onConfirm={(q) => void confirmAdd(q)}
        />
      )}
    </main>
  )
}
