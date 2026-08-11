import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { apiFetch, parseErrorDetail } from '../api/client'
import type { ProductDto } from '../api/types'
import { useMediPro } from '../medipro/MediProProvider'

function formatPkr(n: number): string {
  return n.toLocaleString('en-PK', {
    minimumFractionDigits: 0,
    maximumFractionDigits: 2,
  })
}

export function ProductDetailPage() {
  const { productId } = useParams<{ productId: string }>()
  const mp = useMediPro()
  const [product, setProduct] = useState<ProductDto | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    if (!mp.token || !productId) {
      setProduct(null)
      setError(null)
      return
    }
    let cancelled = false
    setLoading(true)
    setError(null)
    setProduct(null)
    void (async () => {
      try {
        const res = await apiFetch(`/api/products/${productId}`, {
          accessToken: mp.token,
        })
        const text = await res.text()
        if (cancelled) return
        if (!res.ok) {
          setError(parseErrorDetail(text) || `HTTP ${res.status}`)
          setProduct(null)
          return
        }
        setProduct(JSON.parse(text) as ProductDto)
      } finally {
        if (!cancelled) setLoading(false)
      }
    })()
    return () => {
      cancelled = true
    }
  }, [mp.token, productId])

  if (!mp.token) {
    return (
      <main className="main single-page">
        <section className="panel panel-center">
          <h1 className="page-title">Product</h1>
          <p className="page-lead">Sign in to view trade details.</p>
          <Link to="/" className="btn btn-primary">
            Sign in
          </Link>
        </section>
      </main>
    )
  }

  return (
    <main className="main single-page">
      <nav className="breadcrumb">
        <Link to="/catalog">Catalogue</Link>
        <span aria-hidden="true"> / </span>
        <span className="breadcrumb-current">Product</span>
      </nav>

      <header className="page-header">
        <h1 className="page-title">Product details</h1>
        <p className="page-lead">
          Trade pricing and pack information for ordering. Prices are PKR before any
          distributor discounts.
        </p>
      </header>

      <section className="panel">
        {loading && <p className="meta">Loading…</p>}
        {error && (
          <p className="error" role="alert">
            {error}
          </p>
        )}
        {!loading && !error && product && (
          <div className="product-detail">
            {product.imageUrl ? (
              <div className="product-detail-hero">
                <img
                  className="product-detail-hero-img"
                  src={product.imageUrl}
                  alt=""
                  loading="eager"
                  decoding="async"
                />
              </div>
            ) : null}
            <div className="product-detail-head">
              <span className="product-sku">{product.skuCode}</span>
              {product.bonusLabel && (
                <span className="bonus-badge bonus-badge-lg">{product.bonusLabel} bonus</span>
              )}
              {product.mrp != null && (
                <span className="product-mrp">MRP {formatPkr(product.mrp)}</span>
              )}
            </div>
            <h2 className="product-detail-title">{product.name}</h2>
            {product.bonusLabel && (
              <p className="bonus-offer-detail-banner">
                <strong>{product.bonusTitle ?? 'Bonus offer'}:</strong> order{' '}
                {product.bonusBuyQuantity ?? product.bonusLabel.split('+')[0]} and get{' '}
                {product.bonusFreeQuantity ?? product.bonusLabel.split('+')[1]} extra free (
                {product.bonusLabel})
                {product.bonusBannerText ? ` — ${product.bonusBannerText}` : ''}
              </p>
            )}
            {mp.isAdmin && !product.isActive && (
              <p className="help help-banner" role="status">
                This SKU is <strong>inactive</strong> and is hidden from pharmacy catalogues.
              </p>
            )}
            <dl className="product-detail-dl">
              <div>
                <dt>Pack</dt>
                <dd>{product.pack}</dd>
              </div>
              <div>
                <dt>Manufacturer</dt>
                <dd>{product.manufacturer}</dd>
              </div>
              <div>
                <dt>Salt / composition</dt>
                <dd>{product.saltComposition}</dd>
              </div>
              {product.category && (
                <div>
                  <dt>Category</dt>
                  <dd>{product.category}</dd>
                </div>
              )}
              <div>
                <dt>Trade price</dt>
                <dd className="product-detail-price">
                  {formatPkr(Number(product.tradePrice))} <span className="pkr">PKR</span>
                </dd>
              </div>
              <div>
                <dt>Availability</dt>
                <dd
                  className={
                    product.inStock === false
                      ? 'product-stock product-stock-out'
                      : 'product-stock product-stock-in'
                  }
                >
                  {product.inStock === false ? 'Out of stock' : 'In stock'}
                </dd>
              </div>
            </dl>
            <div className="product-detail-actions">
              {mp.canUseCart ? (
                <button
                  type="button"
                  className="btn-primary"
                  disabled={mp.busy === `cart-add-${product.id}` || product.inStock === false}
                  onClick={() => void mp.addOneToCart(product.id)}
                >
                  {product.inStock === false
                    ? 'Unavailable'
                    : mp.busy === `cart-add-${product.id}`
                      ? 'Adding…'
                      : 'Add to cart'}
                </button>
              ) : mp.isAdmin ? (
                <p className="help">
                  Signed in as distributor admin — cart and checkout are for approved
                  pharmacy accounts only.
                </p>
              ) : (
                <p className="help">
                  {mp.authInfo?.role === 'StoreUser'
                    ? 'Ordering is available after your distributor approves your pharmacy.'
                    : 'Sign in with a pharmacy account to add items to a cart.'}
                </p>
              )}
              {mp.canUseCart && (
                <Link to="/cart" className="btn btn-ghost">
                  View cart
                </Link>
              )}
            </div>
          </div>
        )}
      </section>
    </main>
  )
}
