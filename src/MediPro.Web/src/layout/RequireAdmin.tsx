import type { ReactNode } from 'react'
import { Navigate } from 'react-router-dom'
import { useMediPro } from '../medipro/MediProProvider'

export function RequireAdmin({ children }: { children: ReactNode }) {
  const { token, isAdmin } = useMediPro()
  if (!token) return <Navigate to="/" replace />
  if (!isAdmin) return <Navigate to="/catalog" replace />
  return children
}
