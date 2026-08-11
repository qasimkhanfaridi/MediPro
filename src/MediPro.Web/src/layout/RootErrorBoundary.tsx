import { Component, type ErrorInfo, type ReactNode } from 'react'

type Props = { children: ReactNode }
type State = { error: Error | null }

export class RootErrorBoundary extends Component<Props, State> {
  state: State = { error: null }

  static getDerivedStateFromError(error: Error): State {
    return { error }
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error('[MediPro]', error, info.componentStack)
  }

  render() {
    if (this.state.error) {
      return (
        <div
          style={{
            fontFamily: 'system-ui, sans-serif',
            padding: '1.25rem',
            maxWidth: '40rem',
            margin: '0 auto',
          }}
        >
          <h1 style={{ fontSize: '1.1rem' }}>Something went wrong</h1>
          <p style={{ color: '#555', lineHeight: 1.5 }}>
            The app hit a runtime error. Check the browser console (F12) for
            details. Common causes: opening the built files with{' '}
            <code>file://</code> (use <code>npm run dev</code> or{' '}
            <code>npm run preview</code> instead), or blocked storage in a
            locked-down browser profile.
          </p>
          <pre
            style={{
              background: '#f4f4f4',
              padding: '0.75rem',
              overflow: 'auto',
              fontSize: '0.85rem',
            }}
          >
            {this.state.error.message}
          </pre>
          <button
            type="button"
            style={{ marginTop: '1rem', padding: '0.5rem 1rem' }}
            onClick={() => window.location.reload()}
          >
            Reload
          </button>
        </div>
      )
    }
    return this.props.children
  }
}
