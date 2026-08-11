import { Link } from 'react-router-dom'
import {
  defaultAdminEmail,
  defaultAdminPassword,
} from '../api/constants'
import { useMediPro } from '../medipro/MediProProvider'

const isDev = import.meta.env.DEV

export function HomePage() {
  const mp = useMediPro()

  return (
    <>
      <section className="hero">
        <p className="hero-eyebrow">B2B wholesale · Pakistan</p>
        <h1 className="hero-title">Order medicines for your pharmacy with confidence</h1>
        <p className="hero-lead">
          MediPro connects licensed pharmacies with distributors: browse approved
          catalogues, build carts in PKR, and submit orders in one place.
        </p>
        <div className="hero-actions">
          {mp.token ? (
            <>
              <Link to="/catalog" className="btn btn-primary">
                Browse catalogue
              </Link>
              <Link to="/cart" className="btn btn-ghost">
                View cart
              </Link>
            </>
          ) : (
            <>
              <a href="#sign-in" className="btn btn-primary">
                Sign in
              </a>
              <a href="#register" className="btn btn-ghost">
                Register pharmacy
              </a>
            </>
          )}
        </div>
      </section>

      <main className="main main-public">
        <section className="panel panel-muted" aria-live="polite">
          <div className="panel-head">
            <h2 className="panel-title-inline">Service status</h2>
            {mp.health && !mp.healthError && (
              <span className="status-pill status-pill-ok">Online</span>
            )}
            {mp.healthError && (
              <span className="status-pill status-pill-bad">Unavailable</span>
            )}
          </div>
          <p className="help help-tight">
            {mp.healthError
              ? 'We cannot reach the MediPro servers right now. Check your connection, or ask your distributor or IT team if the service is running.'
              : 'You are connected to MediPro. Sign in below to access your catalogue and orders.'}
          </p>
          {mp.health && !mp.healthError && (
            <dl className="kv kv-compact">
              <dt>Service</dt>
              <dd>{mp.health.service}</dd>
              <dt>Last check (UTC)</dt>
              <dd>{mp.health.timestampUtc}</dd>
            </dl>
          )}
          {mp.healthError && (
            <p className="error" role="alert">
              {mp.healthError}
            </p>
          )}
        </section>

        <section className="panel" id="sign-in">
          <p className="panel-kicker">Account</p>
          <div className="panel-head">
            <h2>Sign in</h2>
          </div>
          <p className="help">
            Use the email and password provided by your distributor. New pharmacies
            can register below — your account stays pending until a distributor
            approves your store.
          </p>
          {isDev && (
            <details className="dev-disclosure">
              <summary>Development tester sign-in</summary>
              <p className="help help-tight">
                Seeded admin: <code>{defaultAdminEmail}</code> /{' '}
                <code>{defaultAdminPassword}</code>
              </p>
            </details>
          )}
          <div className="field">
            <label htmlFor="email">Work email</label>
            <input
              id="email"
              type="email"
              autoComplete="username"
              value={mp.email}
              onChange={(e) => mp.setEmail(e.target.value)}
            />
          </div>
          <div className="field">
            <label htmlFor="password">Password</label>
            <input
              id="password"
              type="password"
              autoComplete="current-password"
              value={mp.password}
              onChange={(e) => mp.setPassword(e.target.value)}
            />
          </div>
          <div className="actions">
            <button
              type="button"
              className="btn-primary"
              disabled={mp.busy === 'login'}
              onClick={() => void mp.login()}
            >
              {mp.busy === 'login' ? 'Signing in…' : 'Sign in'}
            </button>
          </div>
          {mp.authInfo && (
            <p className="ok ok-block">
              Signed in as <strong>{mp.authInfo.role}</strong>
              {mp.authInfo.storeApprovalStatus
                ? ` · Store: ${mp.authInfo.storeApprovalStatus}`
                : ''}
            </p>
          )}
          {mp.authError && (
            <p className="error" role="alert">
              {mp.authError}
            </p>
          )}
        </section>

        <section className="panel" id="register">
          <p className="panel-kicker">Pharmacy</p>
          <div className="panel-head">
            <h2>Register your pharmacy</h2>
          </div>
          <p className="help">
            Create a MediPro account for your business. You will be signed in
            immediately; ordering opens after your distributor approves your
            registration. Signing up while already signed in will switch this
            browser to the new pharmacy account.
          </p>
          <div className="field-grid">
            <div className="field">
              <label htmlFor="reg-email">Login email</label>
              <input
                id="reg-email"
                type="email"
                autoComplete="email"
                value={mp.regEmail}
                onChange={(e) => mp.setRegEmail(e.target.value)}
              />
            </div>
            <div className="field">
              <label htmlFor="reg-password">Password (at least 8 characters)</label>
              <input
                id="reg-password"
                type="password"
                autoComplete="new-password"
                value={mp.regPassword}
                onChange={(e) => mp.setRegPassword(e.target.value)}
              />
            </div>
            <div className="field span-2">
              <label htmlFor="reg-business">Pharmacy / business name</label>
              <input
                id="reg-business"
                type="text"
                value={mp.regBusinessName}
                onChange={(e) => mp.setRegBusinessName(e.target.value)}
              />
            </div>
            <div className="field span-2">
              <label htmlFor="reg-address">Street address</label>
              <input
                id="reg-address"
                type="text"
                value={mp.regAddressLine}
                onChange={(e) => mp.setRegAddressLine(e.target.value)}
              />
            </div>
            <div className="field">
              <label htmlFor="reg-city">City</label>
              <input
                id="reg-city"
                type="text"
                value={mp.regCity}
                onChange={(e) => mp.setRegCity(e.target.value)}
              />
            </div>
            <div className="field">
              <label htmlFor="reg-mobile">Mobile</label>
              <input
                id="reg-mobile"
                type="tel"
                value={mp.regMobile}
                onChange={(e) => mp.setRegMobile(e.target.value)}
              />
            </div>
            <div className="field">
              <label htmlFor="reg-contact">Contact person</label>
              <input
                id="reg-contact"
                type="text"
                value={mp.regContactName}
                onChange={(e) => mp.setRegContactName(e.target.value)}
              />
            </div>
            <div className="field">
              <label htmlFor="reg-license">Drug sale licence number *</label>
              <input
                id="reg-license"
                type="text"
                required
                value={mp.regLicenseNumber}
                onChange={(e) => mp.setRegLicenseNumber(e.target.value)}
                placeholder="Required for approval"
                autoComplete="off"
              />
            </div>
          </div>
          <div className="actions">
            <button
              type="button"
              className="btn-primary"
              disabled={
                mp.busy === 'register-store' ||
                mp.regPassword.length < 8 ||
                !mp.regEmail.trim() ||
                !mp.regBusinessName.trim() ||
                !mp.regAddressLine.trim() ||
                !mp.regCity.trim() ||
                !mp.regMobile.trim() ||
                !mp.regContactName.trim() ||
                !mp.regLicenseNumber.trim()
              }
              onClick={() => void mp.registerStore()}
            >
              {mp.busy === 'register-store' ? 'Creating account…' : 'Create account'}
            </button>
          </div>
          {mp.registerMsg && <p className="ok ok-block">{mp.registerMsg}</p>}
          {mp.registerError && (
            <p className="error" role="alert">
              {mp.registerError}
            </p>
          )}
        </section>

        {mp.token && mp.isAdmin && (
          <div className="callout callout-soft" role="status">
            <strong>Distributor:</strong> use <strong>Admin</strong> in the menu
            to approve pharmacies, update the product file, and read notifications.
          </div>
        )}

        {mp.token && mp.authInfo?.role === 'StoreUser' && (
          <div className="callout callout-soft" role="status">
            <strong>Your pharmacy:</strong> catalogue and cart unlock when your
            distributor marks your account as <strong>Approved</strong>.
          </div>
        )}

        {isDev && (
          <p className="footer-hint">
            Seeded tester credentials appear in the <strong>Account</strong> section
            when you run <code>npm run dev</code>.
          </p>
        )}
      </main>
    </>
  )
}
