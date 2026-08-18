import { useEffect, useState } from 'react'
import { Link, useLocation, useNavigate, useSearchParams } from 'react-router-dom'
import { defaultAdminEmail, defaultAdminPassword } from '../api/constants'
import { Icon } from '../components/Icon'
import { useMediPro } from '../medipro/MediProProvider'

const isDev = import.meta.env.DEV

/** Only allow in-app destinations so `next` cannot bounce to another site. */
function safeNext(raw: string | null): string {
  if (!raw || !raw.startsWith('/') || raw.startsWith('//')) return '/'
  return raw
}

export function LoginPage() {
  const mp = useMediPro()
  const navigate = useNavigate()
  const [params] = useSearchParams()
  const next = safeNext(params.get('next'))
  const { token, registerMsg } = mp
  const { hash } = useLocation()
  const [registerOpen, setRegisterOpen] = useState(hash === '#register')

  useEffect(() => {
    if (token && !registerMsg) navigate(next, { replace: true })
  }, [token, registerMsg, next, navigate])

  return (
    <main className="main single-page auth-page">
      <header className="page-header">
        <h1 className="page-title">Sign in</h1>
        <p className="page-lead">
          Use the email and password your distributor gave you.
        </p>
      </header>

      {mp.healthError && (
        <p className="notice notice-bad" role="alert">
          MediPro is not responding right now. Check your internet, then try again.
        </p>
      )}

      <section className="panel sign-in-panel">
        <div className="field">
          <label htmlFor="email">Email</label>
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
        <button
          type="button"
          className="btn-primary btn-block"
          disabled={mp.busy === 'login'}
          onClick={() => void mp.login()}
        >
          <Icon name="signIn" />
          {mp.busy === 'login' ? 'Signing in…' : 'Sign in'}
        </button>
        {mp.authError && (
          <p className="error" role="alert">
            {mp.authError}
          </p>
        )}
        {isDev && (
          <p className="dev-hint">
            Test login: <code>{defaultAdminEmail}</code> /{' '}
            <code>{defaultAdminPassword}</code>
          </p>
        )}
      </section>

      <details
        className="register-disclosure"
        id="register"
        open={registerOpen}
        onToggle={(e) => setRegisterOpen(e.currentTarget.open)}
      >
        <summary>New pharmacy? Register here</summary>
        <p className="help">
          Fill in your details. You can sign in straight away, and ordering opens once
          the distributor approves your pharmacy.
        </p>
        <div className="field-grid">
          <div className="field">
            <label htmlFor="reg-email">Email</label>
            <input
              id="reg-email"
              type="email"
              autoComplete="email"
              value={mp.regEmail}
              onChange={(e) => mp.setRegEmail(e.target.value)}
            />
          </div>
          <div className="field">
            <label htmlFor="reg-password">Password (8 letters or more)</label>
            <input
              id="reg-password"
              type="password"
              autoComplete="new-password"
              value={mp.regPassword}
              onChange={(e) => mp.setRegPassword(e.target.value)}
            />
          </div>
          <div className="field span-2">
            <label htmlFor="reg-business">Pharmacy name</label>
            <input
              id="reg-business"
              type="text"
              value={mp.regBusinessName}
              onChange={(e) => mp.setRegBusinessName(e.target.value)}
            />
          </div>
          <div className="field span-2">
            <label htmlFor="reg-address">Address</label>
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
            <label htmlFor="reg-contact">Your name</label>
            <input
              id="reg-contact"
              type="text"
              value={mp.regContactName}
              onChange={(e) => mp.setRegContactName(e.target.value)}
            />
          </div>
          <div className="field">
            <label htmlFor="reg-license">Drug sale licence number</label>
            <input
              id="reg-license"
              type="text"
              required
              value={mp.regLicenseNumber}
              onChange={(e) => mp.setRegLicenseNumber(e.target.value)}
              placeholder="Needed for approval"
              autoComplete="off"
            />
          </div>
        </div>
        <button
          type="button"
          className="btn-primary btn-block"
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
        {mp.registerMsg && (
          <>
            <p className="ok ok-block">{mp.registerMsg}</p>
            <Link to="/" className="btn btn-primary btn-block">
              Continue
            </Link>
          </>
        )}
        {mp.registerError && (
          <p className="error" role="alert">
            {mp.registerError}
          </p>
        )}
      </details>
    </main>
  )
}
