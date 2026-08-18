import type { ReactNode } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { useMediPro } from '../medipro/MediProProvider'

export function RequireAdmin({ children }: { children: ReactNode }) {
  const { token, isAdmin } = useMediPro()
  const location = useLocation()
  if (!token)
    return (
      <Navigate
        to={`/login?next=${encodeURIComponent(location.pathname)}`}
        replace
      />
    )
  if (!isAdmin) return <Navigate to="/catalog" replace />
  return children
}
