import { useAuth } from '../auth/AuthContext'

export function PermissionGuard({ permission, children }: { permission: string; children: React.ReactNode }) {
  const { hasPermission } = useAuth()
  if (!hasPermission(permission)) {
    return null
  }
  return <>{children}</>
}
