import { Fragment, type FormEvent, useCallback, useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import type { OrderDetailDto } from '../api/types'
import { useMediPro } from '../medipro/MediProProvider'

const ORDER_STATUSES = [
  '',
  'Submitted',
  'Confirmed',
  'OnHold',
  'Processing',
  'Dispatched',
  'Delivered',
  'Rejected',
  'Cancelled',
] as const

const NEXT_STATUS: Record<string, string[]> = {
  Submitted: ['Confirmed', 'OnHold', 'Rejected', 'Cancelled'],
  Confirmed: ['Processing', 'OnHold', 'Cancelled'],
  OnHold: ['Confirmed', 'Rejected', 'Cancelled'],
  Processing: ['Dispatched', 'OnHold', 'Cancelled'],
  Dispatched: ['Delivered', 'Cancelled'],
}

function formatPkr(n: number): string {
  return n.toLocaleString('en-PK', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

export function AdminOrdersPage() {
  const mp = useMediPro()
  const [city, setCity] = useState('')
  const [area, setArea] = useState('')
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState('')
  const [from, setFrom] = useState('')
  const [to, setTo] = useState('')
  const [expandedId, setExpandedId] = useState<string | null>(null)
  const [details, setDetails] = useState<Record<string, OrderDetailDto>>({})
  const [detailLoading, setDetailLoading] = useState<string | null>(null)

  useEffect(() => {
    if (mp.token && mp.isAdmin) {
      void mp.loadOrderLocationOptions()
      void mp.loadOrders({ pageSize: 50 })
    }
  }, [mp.token, mp.isAdmin, mp.loadOrderLocationOptions, mp.loadOrders])

  const areaOptions = useMemo(() => {
    if (!city || !mp.orderLocationOptions) return []
    return mp.orderLocationOptions.areasByCity[city] ?? []
  }, [city, mp.orderLocationOptions])

  function applyFilters(e: FormEvent) {
    e.preventDefault()
    void mp.loadOrders({ city, area, search, status, from, to, pageSize: 50 })
  }

  function clearFilters() {
    setCity('')
    setArea('')
    setSearch('')
    setStatus('')
    setFrom('')
    setTo('')
    void mp.loadOrders({ pageSize: 50 })
  }

  function onCityChange(nextCity: string) {
    setCity(nextCity)
    setArea('')
  }

  const toggleDetail = useCallback(
    async (orderId: string) => {
      if (expandedId === orderId) {
        setExpandedId(null)
        return
      }
      setExpandedId(orderId)
      if (details[orderId]) return
      setDetailLoading(orderId)
      const d = await mp.loadOrderDetail(orderId)
      setDetailLoading(null)
      if (d) setDetails((prev) => ({ ...prev, [orderId]: d }))
    },
    [details, expandedId, mp],
  )

  return (
    <main className="main single-page">
      <header className="page-header">
        <h1 className="page-title">Orders console</h1>
        <p className="page-lead">
          All pharmacy orders for your distributor. Filter by city and area (e.g. Rawalpindi → Moti
          Mehel), store, status, or date range. Expand a row to see each product and quantity.
        </p>
      </header>

      <section className="panel">
        <div className="panel-head">
          <h2>Filters</h2>
          <div className="panel-head-actions">
            <button
              type="button"
              className="btn btn-ghost small"
              disabled={mp.busy === 'demo-orders'}
              onClick={() => void mp.seedDemoOrders()}
            >
              {mp.busy === 'demo-orders' ? 'Seeding…' : 'Seed demo orders'}
            </button>
            <Link to="/admin" className="btn btn-ghost">
              ← Distributor console
            </Link>
          </div>
        </div>
        <form className="catalog-filters admin-order-filters" onSubmit={applyFilters}>
          <div className="field catalog-filter-field">
            <label htmlFor="ord-city">City</label>
            <select
              id="ord-city"
              value={city}
              onChange={(e) => onCityChange(e.target.value)}
            >
              <option value="">All cities</option>
              {mp.orderLocationOptions?.cities.map((c) => (
                <option key={c} value={c}>
                  {c}
                </option>
              ))}
            </select>
          </div>
          <div className="field catalog-filter-field">
            <label htmlFor="ord-area">Area</label>
            <select
              id="ord-area"
              value={area}
              disabled={!city}
              onChange={(e) => setArea(e.target.value)}
            >
              <option value="">{city ? 'All areas in city' : 'Select city first'}</option>
              {areaOptions.map((a) => (
                <option key={a} value={a}>
                  {a}
                </option>
              ))}
            </select>
          </div>
          <div className="field catalog-filter-field">
            <label htmlFor="ord-search">Store search</label>
            <input
              id="ord-search"
              type="search"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Business name or mobile"
              autoComplete="off"
            />
          </div>
          <div className="field catalog-filter-field">
            <label htmlFor="ord-status">Status</label>
            <select id="ord-status" value={status} onChange={(e) => setStatus(e.target.value)}>
              {ORDER_STATUSES.map((s) => (
                <option key={s || 'all'} value={s}>
                  {s || 'All statuses'}
                </option>
              ))}
            </select>
          </div>
          <div className="field catalog-filter-field">
            <label htmlFor="ord-from">From date</label>
            <input
              id="ord-from"
              type="date"
              value={from}
              onChange={(e) => setFrom(e.target.value)}
            />
          </div>
          <div className="field catalog-filter-field">
            <label htmlFor="ord-to">To date</label>
            <input
              id="ord-to"
              type="date"
              value={to}
              onChange={(e) => setTo(e.target.value)}
            />
          </div>
          <div className="actions catalog-filter-actions">
            <button type="submit" className="btn-primary" disabled={mp.busy === 'orders'}>
              {mp.busy === 'orders' ? 'Loading…' : 'Apply filters'}
            </button>
            <button type="button" className="secondary" onClick={() => clearFilters()}>
              Clear
            </button>
          </div>
        </form>

        {mp.orderLocationError && (
          <p className="error" role="alert">
            {mp.orderLocationError}
          </p>
        )}
        {mp.ordersDemoMsg && <p className="help">{mp.ordersDemoMsg}</p>}
        {mp.ordersDemoError && (
          <p className="error" role="alert">
            {mp.ordersDemoError}
          </p>
        )}

        {mp.orders && (
          <p className="catalog-meta">
            {mp.orders.totalCount} order{mp.orders.totalCount === 1 ? '' : 's'}
            {mp.orders.totalCount > mp.orders.items.length
              ? ` · showing ${mp.orders.items.length} on this page`
              : ''}
          </p>
        )}

        {mp.orders && mp.orders.items.length === 0 && (
          <p className="meta">No orders match these filters.</p>
        )}

        {mp.orders && mp.orders.items.length > 0 && (
          <div className="store-table-wrap">
            <table className="store-table admin-orders-table">
              <thead>
                <tr>
                  <th aria-label="Expand" />
                  <th>When</th>
                  <th>Store</th>
                  <th>City</th>
                  <th>Area</th>
                  <th>Status</th>
                  <th>Total</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {mp.orders.items.map((o) => {
                  const open = expandedId === o.id
                  const detail = details[o.id]
                  const nextStatuses = NEXT_STATUS[o.status] ?? []
                  return (
                    <Fragment key={o.id}>
                      <tr>
                        <td>
                          <button
                            type="button"
                            className="small secondary"
                            aria-expanded={open}
                            onClick={() => void toggleDetail(o.id)}
                          >
                            {detailLoading === o.id ? '…' : open ? '−' : '+'}
                          </button>
                        </td>
                        <td>
                          <time dateTime={o.submittedAtUtc}>
                            {new Date(o.submittedAtUtc).toLocaleString()}
                          </time>
                        </td>
                        <td>
                          {o.storeName}
                          <br />
                          <span className="sku">{o.storeMobile}</span>
                        </td>
                        <td>{o.storeCity || '—'}</td>
                        <td>{o.storeArea || '—'}</td>
                        <td>
                          <span className="order-status-pill">{o.status}</span>
                        </td>
                        <td>
                          {formatPkr(Number(o.totalAmount))} {o.currency}
                        </td>
                        <td>
                          {nextStatuses.length > 0 ? (
                            <select
                              className="order-status-select"
                              defaultValue=""
                              disabled={mp.busy === `order-status-${o.id}`}
                              onChange={(e) => {
                                const v = e.target.value
                                if (!v) return
                                void mp.updateOrderStatus(o.id, v).then((ok) => {
                                  if (ok && open) {
                                    void mp.loadOrderDetail(o.id).then((d) => {
                                      if (d) setDetails((prev) => ({ ...prev, [o.id]: d }))
                                    })
                                  }
                                  e.target.value = ''
                                })
                              }}
                            >
                              <option value="">Update status…</option>
                              {nextStatuses.map((s) => (
                                <option key={s} value={s}>
                                  {s}
                                </option>
                              ))}
                            </select>
                          ) : (
                            <span className="meta">—</span>
                          )}
                        </td>
                      </tr>
                      {open && (
                        <tr className="order-detail-row">
                          <td colSpan={8}>
                            {detailLoading === o.id && !detail && (
                              <p className="meta">Loading line items…</p>
                            )}
                            {detail && (
                              <>
                                {detail.notes && (
                                  <p className="help">
                                    <strong>Store notes:</strong> {detail.notes}
                                  </p>
                                )}
                                <table className="store-table order-lines-table">
                                  <thead>
                                    <tr>
                                      <th>Product</th>
                                      <th>Pack</th>
                                      <th>Qty</th>
                                      <th>Unit (PKR)</th>
                                      <th>Line total</th>
                                    </tr>
                                  </thead>
                                  <tbody>
                                    {detail.lines.map((line, i) => (
                                      <tr key={`${line.productId}-${i}`}>
                                        <td>{line.productNameSnapshot}</td>
                                        <td>{line.packSnapshot}</td>
                                        <td>{line.quantity}</td>
                                        <td>{formatPkr(Number(line.unitPriceSnapshot))}</td>
                                        <td>{formatPkr(Number(line.lineTotal))}</td>
                                      </tr>
                                    ))}
                                  </tbody>
                                </table>
                                <p className="meta">
                                  {detail.lines.length} line
                                  {detail.lines.length === 1 ? '' : 's'} · Order total{' '}
                                  {formatPkr(Number(detail.totalAmount))} {detail.currency}
                                </p>
                              </>
                            )}
                          </td>
                        </tr>
                      )}
                    </Fragment>
                  )
                })}
              </tbody>
            </table>
          </div>
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
