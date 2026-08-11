import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter, HashRouter } from 'react-router-dom'
import './index.css'
import App from './App.tsx'
import { RootErrorBoundary } from './layout/RootErrorBoundary.tsx'
import { MediProProvider } from './medipro/MediProProvider.tsx'

/** Vite default is `/`. Set `base` in vite.config if the app lives under a subpath. */
function routerBasename(): string | undefined {
  const raw = import.meta.env.BASE_URL ?? '/'
  if (raw === '/' || raw === '') return undefined
  return raw.endsWith('/') ? raw.slice(0, -1) : raw
}

const useHashRouter = import.meta.env.VITE_HASH_ROUTER === '1'
const Router = useHashRouter ? HashRouter : BrowserRouter

const rootEl = document.getElementById('root')
if (!rootEl) {
  document.body.innerHTML =
    '<p style="font-family:system-ui;padding:1rem">Missing #root in index.html</p>'
} else {
  createRoot(rootEl).render(
    <StrictMode>
      <RootErrorBoundary>
        <Router basename={routerBasename()}>
          <MediProProvider>
            <App />
          </MediProProvider>
        </Router>
      </RootErrorBoundary>
    </StrictMode>,
  )
}
