import type { OrderDetailDto } from '../api/types'

function money(value: number): string {
  return value.toLocaleString('en-PK', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  })
}

function orderReference(id: string): string {
  return `MP-${id.replaceAll('-', '').slice(0, 10).toUpperCase()}`
}

/** "10+1" -> { buy: 10, free: 1 } so the receipt can explain a missed threshold. */
function parseBonusLabel(label: string | null): { buy: number; free: number } | null {
  if (!label) return null
  const [buy, free] = label.split('+').map(Number)
  if (!Number.isFinite(buy) || !Number.isFinite(free) || buy <= 0) return null
  return { buy, free }
}

export function OrderReceipt({ order }: { order: OrderDetailDto }) {
  const orderedPacks = order.lines.reduce((sum, line) => sum + line.quantity, 0)
  const freePacks = order.lines.reduce(
    (sum, line) => sum + line.bonusQuantitySnapshot,
    0,
  )

  return (
    <article className="order-receipt">
      <header className="receipt-head">
        <div>
          <p className="receipt-brand">MediPro</p>
          <h3>Order receipt</h3>
        </div>
        <div className="receipt-reference">
          <strong>{orderReference(order.id)}</strong>
          <span>{new Date(order.submittedAtUtc).toLocaleString()}</span>
          <span className="order-status-pill">{order.status}</span>
        </div>
      </header>

      <section className="receipt-store">
        <strong>{order.storeName}</strong>
        <span>
          {[order.storeAddress, order.storeArea, order.storeCity]
            .filter(Boolean)
            .join(', ')}
        </span>
        {order.storeMobile && <span>{order.storeMobile}</span>}
      </section>

      <div className="receipt-table-wrap">
        <table className="receipt-table">
          <thead>
            <tr>
              <th>Product</th>
              <th>Pack</th>
              <th className="receipt-number">Qty</th>
              <th className="receipt-number">Free</th>
              <th className="receipt-number">Unit</th>
              <th className="receipt-number">Amount</th>
            </tr>
          </thead>
          <tbody>
            {order.lines.map((line, i) => {
              const scheme = parseBonusLabel(line.bonusLabelSnapshot)
              const shortBy = scheme
                ? scheme.buy - (line.quantity % scheme.buy)
                : 0
              return (
              <tr key={`${line.productId}-${i}`}>
                <td>
                  <strong>{line.productNameSnapshot}</strong>
                  {line.bonusQuantitySnapshot > 0 ? (
                    <span className="receipt-bonus">
                      {line.bonusLabelSnapshot} offer applied
                    </span>
                  ) : (
                    scheme && (
                      <span className="receipt-bonus receipt-bonus-missed">
                        {line.bonusLabelSnapshot} offer — {shortBy} more pack
                        {shortBy === 1 ? '' : 's'} for {scheme.free} free
                      </span>
                    )
                  )}
                </td>
                <td>{line.packSnapshot}</td>
                <td className="receipt-number">{line.quantity}</td>
                <td className="receipt-number">
                  {line.bonusQuantitySnapshot > 0 ? (
                    <strong className="receipt-free">
                      +{line.bonusQuantitySnapshot}
                    </strong>
                  ) : (
                    '—'
                  )}
                </td>
                <td className="receipt-number">{money(line.unitPriceSnapshot)}</td>
                <td className="receipt-number">{money(line.lineTotal)}</td>
              </tr>
              )
            })}
          </tbody>
        </table>
      </div>

      <footer className="receipt-footer">
        <div className="receipt-summary">
          <span>{order.lines.length} products</span>
          <span>{orderedPacks} ordered packs</span>
          {freePacks > 0 && (
            <strong className="receipt-free">{freePacks} free packs</strong>
          )}
        </div>
        <div className="receipt-total">
          <span>Order total</span>
          <strong>
            {money(order.totalAmount)} {order.currency}
          </strong>
          <small>Free packs are not charged.</small>
        </div>
      </footer>

      {(order.notes || order.statusNotes) && (
        <section className="receipt-notes">
          {order.notes && (
            <p>
              <strong>Store notes:</strong> {order.notes}
            </p>
          )}
          {order.statusNotes && (
            <p>
              <strong>Status notes:</strong> {order.statusNotes}
            </p>
          )}
        </section>
      )}

      <button
        type="button"
        className="secondary btn-compact receipt-print"
        onClick={() => window.print()}
      >
        Print receipt
      </button>
    </article>
  )
}
