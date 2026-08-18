import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import type { OrderDetailDto } from '../api/types'
import { useMediPro } from '../medipro/MediProProvider'

export function OrdersPage() {
  const mp = useMediPro()
  const [expandedId, setExpandedId] = useState<string | null>(null)
  const [details, setDetails] = useState<Record<string, OrderDetailDto>>({})

  useEffect(() => {
    if (mp.token) void mp.loadOrders({ pageSize: 20 })
  }, [mp.token, mp.loadOrders])

  async function toggleDetail(orderId: string) {
    if (expandedId === orderId) {
      setExpandedId(null)
      return
    }
    setExpandedId(orderId)
    if (details[orderId]) return
    const d = await mp.loadOrderDetail(orderId)
    if (d) setDetails((prev) => ({ ...prev, [orderId]: d }))
  }

  if (!mp.token) {
    return (
      <main className="main single-page">
        <section className="panel panel-center">
          <h1 className="page-title">Orders</h1>
          <p className="page-lead">
            Sign in to see orders placed under your pharmacy or distributor account.
          </p>
          <Link to="/login?next=%2Forders" className="btn btn-primary">
            Sign in
          </Link>
        </section>
      </main>
    )
  }

  return (
    <main className="main single-page">
      <header className="page-header">
        <h1 className="page-title">Orders</h1>
        <p className="page-lead">
          Track submissions and status. Place new orders from your cart.
        </p>
      </header>

      <section className="panel">
        <div className="cart-toolbar">
          <button
            type="button"
            className="secondary"
            disabled={mp.busy === 'orders'}
            onClick={() => void mp.loadOrders()}
          >
            {mp.busy === 'orders' ? 'Loading…' : 'Refresh list'}
          </button>
          <Link to="/cart" className="btn btn-ghost">
            Open cart
          </Link>
        </div>
        <p className="help help-tight">
          Status labels reflect how your distributor is processing each order.
        </p>
        {mp.orders && mp.orders.items.length === 0 && (
          <div className="empty-state">
            <p className="empty-title">No orders yet</p>
            <p className="empty-text">Submit your first order from the cart page.</p>
            <Link to="/cart" className="btn btn-primary">
              Go to cart
            </Link>
          </div>
        )}
        {mp.orders && mp.orders.items.length > 0 && (
          <ul className="order-history">
            {mp.orders.items.map((o) => (
              <li key={o.id} className="order-history-card">
                <div className="order-history-top">
                  <span className="order-status-pill">{o.status}</span>
                  <time dateTime={o.submittedAtUtc}>
                    {new Date(o.submittedAtUtc).toLocaleString()}
                  </time>
                </div>
                {!mp.isAdmin && (
                  <p className="order-history-store">{o.storeName}</p>
                )}
                <p className="order-history-total">
                  {o.totalAmount.toFixed(2)} {o.currency}
                </p>
                <button
                  type="button"
                  className="small secondary"
                  style={{ marginTop: '0.5rem' }}
                  onClick={() => void toggleDetail(o.id)}
                >
                  {expandedId === o.id ? 'Hide items' : 'View items'}
                </button>
                {expandedId === o.id && details[o.id] && (
                  <ul className="plist" style={{ marginTop: '0.65rem' }}>
                    {details[o.id].lines.map((line, i) => (
                      <li key={`${line.productId}-${i}`}>
                        {line.productNameSnapshot} · {line.packSnapshot} · qty{' '}
                        {line.quantity} · {line.lineTotal.toFixed(2)} PKR
                      </li>
                    ))}
                  </ul>
                )}
              </li>
            ))}
          </ul>
        )}
        {mp.ordersError && (
          <p className="error" role="alert">
            {mp.ordersError}
          </p>
        )}
      </section>
    </main>
  )
}
