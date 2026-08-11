import { type FormEvent, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { apiFetch, parseErrorDetail } from '../api/client'
import type { LowStockList } from '../api/types'
import { isStorePendingApproval, storeBadgeClass } from '../api/storeBadge'
import { useMediPro } from '../medipro/MediProProvider'

export function AdminPage() {
  const mp = useMediPro()
  const [stockSku, setStockSku] = useState('')
  const [stockDelta, setStockDelta] = useState('')
  const [lowStock, setLowStock] = useState<LowStockList | null>(null)
  const [lowStockError, setLowStockError] = useState<string | null>(null)
  const [lowStockBusy, setLowStockBusy] = useState(false)
  const [storeListLoading, setStoreListLoading] = useState(false)

  useEffect(() => {
    if (!mp.token || !mp.isAdmin) return
    let cancelled = false
    setStoreListLoading(true)
    void mp
      .loadStores({ quiet: true })
      .finally(() => {
        if (!cancelled) setStoreListLoading(false)
      })
    return () => {
      cancelled = true
    }
  }, [mp.token, mp.isAdmin, mp.loadStores])

  useEffect(() => {
    if (!mp.token) {
      setLowStock(null)
      setLowStockError(null)
      return
    }
    let cancelled = false
    setLowStockBusy(true)
    setLowStockError(null)
    void (async () => {
      try {
        const res = await apiFetch(
          '/api/admin/catalog/low-stock?threshold=20&max=50',
          { accessToken: mp.token },
        )
        const text = await res.text()
        if (cancelled) return
        if (!res.ok) {
          setLowStockError(parseErrorDetail(text) || `HTTP ${res.status}`)
          setLowStock(null)
          return
        }
        setLowStock(JSON.parse(text) as LowStockList)
      } catch {
        if (!cancelled) setLowStockError('Could not load low-stock list.')
      } finally {
        if (!cancelled) setLowStockBusy(false)
      }
    })()
    return () => {
      cancelled = true
    }
  }, [mp.token, mp.stockMsg, mp.catalogDemoMsg, mp.storesDemoMsg])

  function submitStock(e: FormEvent) {
    e.preventDefault()
    const delta = Number.parseInt(stockDelta, 10)
    if (!stockSku.trim()) return
    if (Number.isNaN(delta)) return
    void mp.adjustStock(stockSku, delta)
  }

  return (
    <main className="main single-page">
      <header className="page-header">
        <h1 className="page-title">Distributor console</h1>
        <p className="page-lead">
          Onboard pharmacies, maintain your product file, and stay on top of new
          orders and registrations.
        </p>
        <div className="actions page-header-actions">
          <Link to="/admin/orders" className="btn-primary">
            Open orders console →
          </Link>
          <Link to="/admin/bonus-schemes" className="btn btn-ghost">
            Bonus &amp; offers →
          </Link>
          <Link to="/admin/products" className="btn btn-ghost">
            Products →
          </Link>
        </div>
      </header>

      <section className="panel">
        <p className="panel-kicker">Stores</p>
        <div className="panel-head">
          <h2>Registered stores</h2>
        </div>
        <p className="help">
          New pharmacies appear as <strong>Pending</strong>. Check the{' '}
          <strong>licence number</strong> before you approve so only licensed
          pharmacies can order. The list loads automatically; use refresh after new
          registrations if needed.
        </p>
        <div className="actions">
          <button
            type="button"
            className="btn-primary"
            disabled={mp.busy === 'stores'}
            onClick={() => void mp.loadStores()}
          >
            {mp.busy === 'stores' ? 'Loading…' : 'Refresh store list'}
          </button>
        </div>
        {storeListLoading && mp.stores === null && !mp.storesError && (
          <p className="meta" aria-live="polite">
            Loading registered pharmacies…
          </p>
        )}
        {mp.stores && mp.stores.length > 0 && (
          <div className="store-table-wrap">
            <table className="store-table">
              <thead>
                <tr>
                  <th>Business</th>
                  <th>Licence no.</th>
                  <th>City / mobile</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {mp.stores.map((s) => (
                  <tr key={s.id}>
                    <td>{s.businessName}</td>
                    <td>
                      {s.licenseNumber ? (
                        <span className="sku" title="Verify against drug sale licence">
                          {s.licenseNumber}
                        </span>
                      ) : (
                        <span className="meta">—</span>
                      )}
                    </td>
                    <td>
                      {s.city}
                      {s.area ? (
                        <>
                          <br />
                          <span className="meta">{s.area}</span>
                        </>
                      ) : null}
                      <br />
                      <span className="sku">{s.mobile}</span>
                    </td>
                    <td>
                      <span className={storeBadgeClass(s.approvalStatus)}>
                        {s.approvalStatus}
                      </span>
                    </td>
                    <td>
                      {isStorePendingApproval(s.approvalStatus) ? (
                        <div className="row-actions">
                          <button
                            type="button"
                            className="small"
                            disabled={mp.busy === `approve-${s.id}`}
                            onClick={() =>
                              void mp.setStoreApproval(s.id, 'Approved')
                            }
                          >
                            {mp.busy === `approve-${s.id}` ? 'Saving…' : 'Approve'}
                          </button>
                          <button
                            type="button"
                            className="small danger"
                            disabled={mp.busy === `approve-${s.id}`}
                            onClick={() =>
                              void mp.setStoreApproval(s.id, 'Rejected')
                            }
                          >
                            Reject
                          </button>
                        </div>
                      ) : (
                        <span className="meta">—</span>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
        {mp.stores && mp.stores.length === 0 && (
          <p className="meta">No stores registered yet.</p>
        )}
        {mp.storesError && (
          <p className="error" role="alert">
            {mp.storesError}
          </p>
        )}

        <h3 className="subh">Low stock (tracked SKUs)</h3>
        <p className="help">
          Active products with on-hand quantity at or below <strong>20</strong> units
          (null / untracked stock is excluded). The list refreshes after you apply a
          stock adjustment.
        </p>
        {lowStockBusy && <p className="meta">Loading low-stock…</p>}
        {lowStock && lowStock.items.length > 0 && (
          <div className="store-table-wrap">
            <table className="store-table low-stock-table">
              <thead>
                <tr>
                  <th>SKU</th>
                  <th>Product</th>
                  <th>On hand</th>
                </tr>
              </thead>
              <tbody>
                {lowStock.items.map((row) => (
                  <tr key={row.skuCode}>
                    <td>
                      <span className="sku">{row.skuCode}</span>
                    </td>
                    <td>{row.name}</td>
                    <td>
                      <span
                        className={
                          row.stockQuantity <= 0
                            ? 'low-stock-cell low-stock-out'
                            : 'low-stock-cell'
                        }
                      >
                        {row.stockQuantity}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            {lowStock.totalMatching > lowStock.items.length && (
              <p className="meta">
                Showing {lowStock.items.length} of {lowStock.totalMatching} SKUs at or
                below threshold.
              </p>
            )}
          </div>
        )}
        {lowStock && lowStock.items.length === 0 && !lowStockBusy && (
          <p className="meta">No active SKUs are at or below the threshold.</p>
        )}
        {lowStockError && (
          <p className="error" role="alert">
            {lowStockError}
          </p>
        )}

        <h3 className="subh">Stock adjustment</h3>
        <p className="help">
          Increase or decrease on-hand units for a SKU. Use a negative number to
          correct stock (e.g. damage). Pharmacies see updated quantities on the
          catalogue; cart quantities and submitted orders cannot exceed available
          stock when stock is tracked.
        </p>
        <form className="stock-adjust-form" onSubmit={submitStock}>
          <div className="field">
            <label htmlFor="stock-sku">SKU code</label>
            <input
              id="stock-sku"
              type="text"
              autoComplete="off"
              value={stockSku}
              onChange={(e) => setStockSku(e.target.value)}
              placeholder="e.g. DEMO-001"
            />
          </div>
          <div className="field">
            <label htmlFor="stock-delta">Units to add (use − for reduction)</label>
            <input
              id="stock-delta"
              type="text"
              inputMode="numeric"
              value={stockDelta}
              onChange={(e) => setStockDelta(e.target.value)}
              placeholder="e.g. 100 or -5"
            />
          </div>
          <div className="actions">
            <button
              type="submit"
              className="btn-primary"
              disabled={
                mp.busy === 'stock' ||
                !stockSku.trim() ||
                Number.isNaN(Number.parseInt(stockDelta, 10))
              }
            >
              {mp.busy === 'stock' ? 'Updating…' : 'Apply stock change'}
            </button>
          </div>
        </form>
        {mp.stockMsg && <p className="ok ok-block">{mp.stockMsg}</p>}
        {mp.stockError && (
          <p className="error" role="alert">
            {mp.stockError}
          </p>
        )}

        <h3 className="subh">Demo catalogue (testing)</h3>
        <p className="help">
          Inserts <strong>87</strong> sample SKUs (<span className="sku">TEST-MED-001</span> …{' '}
          <span className="sku">TEST-MED-087</span>) with varied forms (tablets, capsules, syrups,
          injections, inhalers, nebuliser solutions, creams, drops), strengths, and placeholder pack
          images (requires internet). Safe to run again — existing demo SKUs are skipped.
        </p>
        <div className="actions">
          <button
            type="button"
            className="btn-primary"
            disabled={mp.busy === 'demo-catalog'}
            onClick={() => void mp.seedDemoCatalog()}
          >
            {mp.busy === 'demo-catalog' ? 'Loading…' : 'Load demo products'}
          </button>
        </div>
        {mp.catalogDemoMsg && <p className="ok ok-block">{mp.catalogDemoMsg}</p>}
        {mp.catalogDemoError && (
          <p className="error" role="alert">
            {mp.catalogDemoError}
          </p>
        )}

        <h3 className="subh">Demo pharmacy accounts (testing)</h3>
        <p className="help">
          Creates several <span className="sku">*@demo.medipro.local</span> store users under your
          tenant (approved, pending, and one rejected sample). Password is{' '}
          <strong>DevSeed:DemoStorePassword</strong> or, if blank, <strong>AdminPassword</strong>{' '}
          (minimum 8 characters). Idempotent — skips emails that already exist.
        </p>
        <p className="help meta" style={{ marginTop: '0.35rem' }}>
          Examples: <span className="sku">store.approved1@demo.medipro.local</span>,{' '}
          <span className="sku">store.pending1@demo.medipro.local</span>,{' '}
          <span className="sku">store.rejected1@demo.medipro.local</span>.
        </p>
        <div className="actions">
          <button
            type="button"
            className="btn-primary"
            disabled={mp.busy === 'demo-stores'}
            onClick={() => void mp.seedDemoStores()}
          >
            {mp.busy === 'demo-stores' ? 'Loading…' : 'Load demo stores'}
          </button>
        </div>
        {mp.storesDemoMsg && <p className="ok ok-block">{mp.storesDemoMsg}</p>}
        {mp.storesDemoError && (
          <p className="error" role="alert">
            {mp.storesDemoError}
          </p>
        )}

        <h3 className="subh">Catalogue import (Excel)</h3>
        <p className="help">
          <a href="/api/admin/catalog/import-template" download>
            Download the official column template
          </a>
          , fill it in Excel, then upload here. Products appear in connected
          pharmacies after a successful import.
        </p>
        <input
          ref={mp.fileInputRef}
          type="file"
          accept=".xlsx"
          className="file-input"
          onChange={(e) => {
            void mp.handleImportFiles(e.target.files)
            e.target.value = ''
          }}
        />
        <div className="actions">
          <button
            type="button"
            className="secondary"
            disabled={mp.busy === 'import'}
            onClick={() => mp.fileInputRef.current?.click()}
          >
            {mp.busy === 'import' ? 'Importing…' : 'Choose Excel file…'}
          </button>
        </div>
        {mp.importResult && (
          <p className="ok" style={{ marginTop: '0.65rem' }}>
            Inserted {mp.importResult.insertedCount} · Rows attempted{' '}
            {mp.importResult.totalRowsAttempted} · Issues{' '}
            {mp.importResult.skippedOrFailedCount}
          </p>
        )}
        {mp.importResult && mp.importResult.errors.length > 0 && (
          <ul className="plist">
            {mp.importResult.errors.map((err, i) => (
              <li key={`${err.rowNumber}-${i}`}>
                Row {err.rowNumber}: {err.message}
              </li>
            ))}
          </ul>
        )}
        {mp.importError && (
          <p className="error" role="alert">
            {mp.importError}
          </p>
        )}

        <h3 className="subh">Notifications</h3>
        <p className="help">
          New pharmacy registrations and incoming orders surface here so your team
          can respond quickly.
        </p>
        <div className="actions">
          <button
            type="button"
            disabled={mp.busy === 'notifications'}
            onClick={() => void mp.loadNotifications()}
          >
            {mp.busy === 'notifications' ? 'Loading…' : 'Load notifications'}
          </button>
        </div>
        {mp.notifications && mp.notifications.items.length > 0 && (
          <ul className="plist">
            {mp.notifications.items.map((n) => (
              <li key={n.id}>
                <strong>{n.title}</strong> — {n.body}{' '}
                <span className="meta" style={{ display: 'inline', margin: 0 }}>
                  ({new Date(n.createdAtUtc).toLocaleString()})
                </span>
                {!n.isRead && (
                  <button
                    type="button"
                    className="small secondary"
                    disabled={mp.busy === `notif-read-${n.id}`}
                    onClick={() => void mp.markNotificationRead(n.id)}
                  >
                    {mp.busy === `notif-read-${n.id}` ? '…' : 'Mark read'}
                  </button>
                )}
              </li>
            ))}
          </ul>
        )}
        {mp.notifications && mp.notifications.items.length === 0 && (
          <p className="meta">No notifications.</p>
        )}
        {mp.notifError && (
          <p className="error" role="alert">
            {mp.notifError}
          </p>
        )}
      </section>
    </main>
  )
}
