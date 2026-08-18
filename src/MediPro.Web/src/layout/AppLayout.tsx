import { NavLink, Outlet } from 'react-router-dom'
import { Icon, type IconName } from '../components/Icon'
import { useMediPro } from '../medipro/MediProProvider'

type NavItem = { to: string; label: string; icon: IconName; end?: boolean }

function navClass({ isActive }: { isActive: boolean }) {
  return 'app-navlink' + (isActive ? ' app-navlink-active' : '')
}

function dockClass({ isActive }: { isActive: boolean }) {
  return 'app-dock-link' + (isActive ? ' app-dock-link-active' : '')
}

export function AppLayout() {
  const { token, authInfo, isAdmin, canUseCart, cart, logout } = useMediPro()

  const items: NavItem[] = [{ to: '/', label: 'Home', icon: 'home', end: true }]
  items.push({ to: '/catalog', label: 'Medicines', icon: 'medicine' })
  if (!isAdmin) items.push({ to: '/cart', label: 'Cart', icon: 'cart' })
  items.push({
    to: isAdmin ? '/admin/orders' : '/orders',
    label: 'Orders',
    icon: 'orders',
  })
  if (token && isAdmin) items.push({ to: '/admin', label: 'Admin', icon: 'console' })

  const cartCount = canUseCart ? (cart?.lines.length ?? 0) : 0

  return (
    <div className="app">
      <header className="app-topbar">
        <div className="app-topbar-inner">
          <NavLink to="/" className="app-brand" end>
            <span className="app-brand-mark" aria-hidden />
            MediPro
          </NavLink>
          <nav className="app-nav" aria-label="Main">
            {items.map((it) => (
              <NavLink key={it.to} to={it.to} className={navClass} end={it.end}>
                <Icon name={it.icon} />
                {it.label}
                {it.to === '/cart' && cartCount > 0 && (
                  <span className="nav-count">{cartCount}</span>
                )}
              </NavLink>
            ))}
          </nav>
          <div className="app-topbar-actions">
            {token && authInfo && (
              <span className="app-rolechip" title="Current session">
                {authInfo.role === 'StoreUser' ? 'Pharmacy' : 'Distributor'}
              </span>
            )}
            {token ? (
              <button type="button" className="secondary small" onClick={logout}>
                <Icon name="signOut" />
                Sign out
              </button>
            ) : (
              <NavLink to="/login" className="btn btn-primary btn-compact">
                <Icon name="signIn" />
                Sign in
              </NavLink>
            )}
          </div>
        </div>
      </header>

      <div className="app-body">
        <Outlet />
      </div>

      <nav className="app-dock" aria-label="Primary">
        {items.map((it) => (
          <NavLink key={it.to} to={it.to} className={dockClass} end={it.end}>
            <span className="app-dock-icon">
              <Icon name={it.icon} />
              {it.to === '/cart' && cartCount > 0 && (
                <span className="dock-count">{cartCount}</span>
              )}
            </span>
            {it.label}
          </NavLink>
        ))}
      </nav>
    </div>
  )
}
