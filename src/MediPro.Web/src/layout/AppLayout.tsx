import { NavLink, Outlet } from 'react-router-dom'
import { useMediPro } from '../medipro/MediProProvider'

function navClass({ isActive }: { isActive: boolean }) {
  return 'app-navlink' + (isActive ? ' app-navlink-active' : '')
}

function dockClass({ isActive }: { isActive: boolean }) {
  return 'app-dock-link' + (isActive ? ' app-dock-link-active' : '')
}

export function AppLayout() {
  const { token, authInfo, isAdmin, logout } = useMediPro()

  return (
    <div className="app">
      <header className="app-topbar">
        <div className="app-topbar-inner">
          <NavLink to="/" className="app-brand" end>
            <span className="app-brand-mark" aria-hidden />
            MediPro
          </NavLink>
          <nav className="app-nav" aria-label="Main">
            <NavLink to="/" className={navClass} end>
              Home
            </NavLink>
            <NavLink to="/catalog" className={navClass}>
              Catalogue
            </NavLink>
            <NavLink to="/cart" className={navClass}>
              Cart
            </NavLink>
            <NavLink to="/orders" className={navClass}>
              Orders
            </NavLink>
            {token && isAdmin && (
              <>
                <NavLink to="/admin" className={navClass}>
                  Admin
                </NavLink>
                <NavLink to="/admin/orders" className={navClass}>
                  All orders
                </NavLink>
              </>
            )}
          </nav>
          <div className="app-topbar-actions">
            {token && authInfo && (
              <span className="app-rolechip" title="Current session">
                {authInfo.role === 'StoreUser' ? 'Pharmacy' : 'Distributor'}
                {authInfo.storeApprovalStatus
                  ? ` · ${authInfo.storeApprovalStatus}`
                  : ''}
              </span>
            )}
            {token && (
              <button type="button" className="secondary small" onClick={logout}>
                Sign out
              </button>
            )}
          </div>
        </div>
      </header>

      <div className="app-body">
        <Outlet />
      </div>

      <nav className="app-dock" aria-label="Primary">
        <NavLink to="/" className={dockClass} end>
          Home
        </NavLink>
        <NavLink to="/catalog" className={dockClass}>
          Shop
        </NavLink>
        <NavLink to="/cart" className={dockClass}>
          Cart
        </NavLink>
        <NavLink to="/orders" className={dockClass}>
          Orders
        </NavLink>
        {token && isAdmin && (
          <>
            <NavLink to="/admin" className={dockClass}>
              Admin
            </NavLink>
            <NavLink to="/admin/orders" className={dockClass}>
              Orders
            </NavLink>
          </>
        )}
      </nav>
    </div>
  )
}
