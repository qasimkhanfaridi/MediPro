import { Link } from 'react-router-dom'
import { useMediPro } from '../medipro/MediProProvider'

export function CartPage() {
  const mp = useMediPro()

  if (!mp.token) {
    return (
      <main className="main single-page">
        <section className="panel panel-center">
          <h1 className="page-title">Your cart</h1>
          <p className="page-lead">
            Sign in with an approved pharmacy account to build an order and submit
            it to your distributor.
          </p>
          <Link to="/login?next=%2Fcart" className="btn btn-primary">
            Sign in
          </Link>
        </section>
      </main>
    )
  }

  return (
    <main className="main single-page">
      <header className="page-header">
        <h1 className="page-title">Cart</h1>
        <p className="page-lead">
          Review quantities and submit your order. Your distributor will confirm
          fulfilment according to your agreement.
        </p>
      </header>

      <section className="panel">
        {!mp.canUseCart && (
          <div className="empty-state">
            <p className="empty-title">Ordering not enabled yet</p>
            <p className="empty-text">
              Your pharmacy account must be <strong>Approved</strong> before you
              can add products. Contact your distributor if you are still waiting.
            </p>
            <Link to="/catalog" className="btn btn-primary">
              Back to catalogue
            </Link>
          </div>
        )}
        {mp.canUseCart && (
          <>
            <div className="cart-toolbar">
              <button
                type="button"
                className="secondary"
                disabled={mp.busy === 'cart-refresh'}
                onClick={() => void mp.refreshCartClick()}
              >
                {mp.busy === 'cart-refresh' ? 'Refreshing…' : 'Refresh'}
              </button>
              <Link to="/catalog" className="btn btn-ghost">
                Continue shopping
              </Link>
            </div>

            {mp.cart && mp.cart.lines.length === 0 && (
              <div className="empty-state">
                <p className="empty-title">Your cart is empty</p>
                <p className="empty-text">Browse the catalogue and tap Add on items you need.</p>
                <Link to="/catalog" className="btn btn-primary">
                  Browse catalogue
                </Link>
              </div>
            )}

            {mp.cart && mp.cart.lines.length > 0 && (
              <>
                <ul className="order-line-list">
                  {mp.cart.lines.map((l) => {
                    const lineBusy =
                      mp.busy === `cart-qty-${l.productId}` ||
                      mp.busy === `cart-remove-${l.productId}`
                    return (
                      <li key={l.lineId} className="order-line-card">
                        <div className="order-line-main">
                          <Link
                            to={`/catalog/${l.productId}`}
                            className="order-line-sku order-line-sku-link"
                          >
                            {l.skuCode}
                          </Link>
                          <span className="order-line-name">{l.name}</span>
                          <span className="order-line-pack">{l.pack}</span>
                        </div>
                        <div className="order-line-actions">
                          <div className="qty-stepper" role="group" aria-label="Quantity">
                            <button
                              type="button"
                              className="qty-stepper-btn"
                              disabled={lineBusy || mp.busy === 'order-submit'}
                              aria-label="Decrease quantity"
                              onClick={() =>
                                void mp.setCartLineQuantity(
                                  l.productId,
                                  l.quantity <= 1 ? 0 : l.quantity - 1,
                                )
                              }
                            >
                              −
                            </button>
                            <span className="qty-stepper-value">{l.quantity}</span>
                            <button
                              type="button"
                              className="qty-stepper-btn"
                              disabled={lineBusy || mp.busy === 'order-submit'}
                              aria-label="Increase quantity"
                              onClick={() =>
                                void mp.setCartLineQuantity(l.productId, l.quantity + 1)
                              }
                            >
                              +
                            </button>
                          </div>
                          <button
                            type="button"
                            className="small danger"
                            disabled={lineBusy || mp.busy === 'order-submit'}
                            onClick={() => void mp.removeCartLine(l.productId)}
                          >
                            Remove
                          </button>
                          <div className="order-line-meta">
                            <span>
                              {l.quantity} × {l.tradePrice.toFixed(2)} PKR
                            </span>
                            <strong>{l.lineTotal.toFixed(2)} PKR</strong>
                          </div>
                        </div>
                      </li>
                    )
                  })}
                </ul>
                <div className="cart-summary">
                  <div>
                    <span className="cart-summary-label">Subtotal</span>
                    <strong className="cart-summary-total">
                      {mp.cart.subtotal.toFixed(2)} PKR
                    </strong>
                  </div>
                  <button
                    type="button"
                    className="btn-primary btn-lg"
                    disabled={
                      mp.busy === 'order-submit' ||
                      !mp.cart ||
                      mp.cart.lines.length === 0
                    }
                    onClick={() => void mp.submitOrder()}
                  >
                    {mp.busy === 'order-submit' ? 'Submitting…' : 'Submit order'}
                  </button>
                </div>
              </>
            )}
            {mp.orderMsg && <p className="ok ok-block">{mp.orderMsg}</p>}
            {mp.cartError && (
              <p className="error" role="alert">
                {mp.cartError}
              </p>
            )}
          </>
        )}
      </section>
    </main>
  )
}
