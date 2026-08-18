import { useEffect, useRef, useState } from 'react'
import type { ProductDto } from '../api/types'

function formatPkr(n: number): string {
  return n.toLocaleString('en-PK', {
    minimumFractionDigits: 0,
    maximumFractionDigits: 2,
  })
}

/** Free packs earned for a bonus scheme like 10+1 at the chosen quantity. */
function freePacks(product: ProductDto, quantity: number): number {
  const buy = product.bonusBuyQuantity ?? 0
  const free = product.bonusFreeQuantity ?? 0
  if (buy <= 0 || free <= 0) return 0
  return Math.floor(quantity / buy) * free
}

export function AddToCartDialog({
  product,
  busy,
  onCancel,
  onConfirm,
}: {
  product: ProductDto
  busy: boolean
  onCancel: () => void
  onConfirm: (quantity: number) => void
}) {
  const [quantity, setQuantity] = useState(1)
  const inputRef = useRef<HTMLInputElement>(null)

  useEffect(() => {
    inputRef.current?.focus()
    inputRef.current?.select()
  }, [])

  useEffect(() => {
    function onKey(e: KeyboardEvent) {
      if (e.key === 'Escape') onCancel()
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [onCancel])

  const bonus = freePacks(product, quantity)
  const total = Number(product.tradePrice) * quantity

  function commit() {
    if (quantity >= 1) onConfirm(quantity)
  }

  return (
    <div
      className="qty-dialog-backdrop"
      role="presentation"
      onClick={(e) => {
        if (e.target === e.currentTarget) onCancel()
      }}
    >
      <div
        className="qty-dialog"
        role="dialog"
        aria-modal="true"
        aria-labelledby="qty-dialog-title"
      >
        <h2 className="qty-dialog-title" id="qty-dialog-title">
          How many?
        </h2>
        <p className="qty-dialog-product">{product.name}</p>
        <p className="qty-dialog-meta">
          {product.pack} · {formatPkr(Number(product.tradePrice))} PKR each
        </p>

        <div className="qty-dialog-stepper">
          <button
            type="button"
            className="qty-stepper-btn"
            aria-label="Less"
            disabled={quantity <= 1}
            onClick={() => setQuantity((q) => Math.max(1, q - 1))}
          >
            −
          </button>
          <input
            ref={inputRef}
            className="qty-dialog-input"
            type="number"
            inputMode="numeric"
            min={1}
            max={9999}
            value={quantity}
            onChange={(e) => {
              const n = Number(e.target.value)
              setQuantity(Number.isFinite(n) && n >= 1 ? Math.trunc(n) : 1)
            }}
            onKeyDown={(e) => {
              if (e.key === 'Enter') {
                e.preventDefault()
                commit()
              }
            }}
          />
          <button
            type="button"
            className="qty-stepper-btn"
            aria-label="More"
            onClick={() => setQuantity((q) => Math.min(9999, q + 1))}
          >
            +
          </button>
        </div>

        <div className="qty-dialog-quick">
          {[5, 10, 20, 50].map((n) => (
            <button
              key={n}
              type="button"
              className="btn btn-ghost btn-compact"
              onClick={() => setQuantity(n)}
            >
              {n}
            </button>
          ))}
        </div>

        {bonus > 0 && (
          <p className="qty-dialog-bonus">
            {product.bonusLabel} offer — you also get {bonus} free.
          </p>
        )}

        <p className="qty-dialog-total">
          Total <strong>{formatPkr(total)} PKR</strong>
        </p>

        <div className="qty-dialog-actions">
          <button
            type="button"
            className="btn-primary btn-block"
            disabled={busy || quantity < 1}
            onClick={commit}
          >
            {busy ? 'Adding…' : `Add ${quantity} to cart`}
          </button>
          <button
            type="button"
            className="secondary btn-block"
            disabled={busy}
            onClick={onCancel}
          >
            Cancel
          </button>
        </div>
      </div>
    </div>
  )
}
