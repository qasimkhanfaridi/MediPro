import { useEffect } from 'react'
import { Link } from 'react-router-dom'
import { Icon, type IconName } from '../components/Icon'
import { useMediPro } from '../medipro/MediProProvider'

type Tile = {
  to: string
  icon: IconName
  label: string
  hint: string
  note?: string
  locked?: boolean
}

function TileLink({ tile }: { tile: Tile }) {
  return (
    <Link
      to={tile.locked ? `/login?next=${encodeURIComponent(tile.to)}` : tile.to}
      className={tile.locked ? 'tile tile-locked' : 'tile'}
    >
      <span className="tile-icon">
        <Icon name={tile.icon} />
      </span>
      <span className="tile-label">{tile.label}</span>
      <span className="tile-hint">{tile.hint}</span>
      {tile.note && <span className="tile-note">{tile.note}</span>}
      {tile.locked && (
        <span className="tile-lock">
          <Icon name="lock" />
          Sign in needed
        </span>
      )}
    </Link>
  )
}

export function HomePage() {
  const mp = useMediPro()
  const { token, isAdmin, stores, loadStores } = mp

  useEffect(() => {
    if (token && isAdmin && !stores) void loadStores({ quiet: true })
  }, [token, isAdmin, stores, loadStores])

  const cartCount = mp.cart?.lines.length ?? 0
  const pendingPharmacies =
    mp.stores?.filter((s) => s.approvalStatus === 'Pending').length ?? 0
  const signedOut = !mp.token

  const tiles: Tile[] = [
    {
      to: '/catalog',
      icon: 'medicine',
      label: 'Medicines',
      hint: signedOut
        ? 'Browse the price list'
        : mp.canUseCart
          ? 'Search and add to your cart'
          : 'See the price list',
      locked: signedOut,
    },
  ]

  if (signedOut || mp.canUseCart) {
    tiles.push({
      to: '/cart',
      icon: 'cart',
      label: 'My cart',
      hint: 'Check items and place the order',
      note: cartCount > 0 ? `${cartCount} item${cartCount === 1 ? '' : 's'}` : undefined,
      locked: signedOut,
    })
  }

  tiles.push({
    to: isAdmin ? '/admin/orders' : '/orders',
    icon: 'orders',
    label: isAdmin ? 'Orders' : 'My orders',
    hint: isAdmin ? 'See and update every order' : 'See what you ordered before',
    locked: signedOut,
  })

  if (isAdmin) {
    tiles.push(
      {
        to: '/admin',
        icon: 'pharmacy',
        label: 'Pharmacies',
        hint: 'Approve new pharmacies',
        note: pendingPharmacies > 0 ? `${pendingPharmacies} waiting` : undefined,
      },
      {
        to: '/admin/products',
        icon: 'box',
        label: 'Products',
        hint: 'Add products and set stock',
      },
      {
        to: '/admin/bonus-schemes',
        icon: 'offer',
        label: 'Offers',
        hint: 'Bonus deals like 10 + 1 free',
      },
    )
  }

  const steps: { icon: IconName; title: string; text: string }[] = [
    {
      icon: 'search',
      title: 'Find your medicines',
      text: 'Search by brand, salt or company and see trade prices for your pharmacy.',
    },
    {
      icon: 'cart',
      title: 'Fill the cart',
      text: 'Add packs, check bonus offers like 10 + 1 free, and see the total before you send it.',
    },
    {
      icon: 'truck',
      title: 'Send and track',
      text: 'Your distributor confirms the order and you follow it until it reaches your counter.',
    },
  ]

  return (
    <main className="main home">
      {mp.healthError && (
        <p className="notice notice-bad" role="alert">
          MediPro is not responding right now. Check your internet, then try again.
        </p>
      )}

      <header className="home-head">
        <h1 className="home-title">
          {signedOut ? 'Order medicines for your pharmacy' : 'What would you like to do?'}
        </h1>
        <p className="home-sub">
          {signedOut
            ? 'Have a look around. Sign in when you are ready to order.'
            : isAdmin
              ? 'You are signed in as the distributor.'
              : 'Tap any option below.'}
        </p>
      </header>

      {mp.authInfo?.role === 'StoreUser' && !mp.canUseCart && (
        <p className="notice" role="status">
          Your pharmacy is waiting for approval. You can look at medicines and prices
          now — ordering opens once the distributor approves you.
        </p>
      )}

      <nav className="tile-grid" aria-label="Main options">
        {tiles.map((t) => (
          <TileLink key={t.to} tile={t} />
        ))}
      </nav>

      {signedOut && (
        <>
          <section className="home-cta">
            <Link to="/login" className="btn btn-primary btn-lg">
              <Icon name="signIn" />
              Sign in
            </Link>
            <Link to="/login#register" className="btn btn-ghost btn-lg">
              Register your pharmacy
            </Link>
          </section>

          <section className="home-steps" aria-labelledby="how-it-works">
            <h2 className="home-section-title" id="how-it-works">
              How it works
            </h2>
            <ol className="step-list">
              {steps.map((s, i) => (
                <li key={s.title} className="step">
                  <span className="step-num" aria-hidden>
                    {i + 1}
                  </span>
                  <span className="step-icon">
                    <Icon name={s.icon} />
                  </span>
                  <span className="step-body">
                    <span className="step-title">{s.title}</span>
                    <span className="step-text">{s.text}</span>
                  </span>
                </li>
              ))}
            </ol>
          </section>
        </>
      )}
    </main>
  )
}
