export type PagedResult<T> = {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}

export type User = {
  id: string
  email: string
  firstName?: string
  lastName?: string
  isEnabled: boolean
  isLocked: boolean
  roles: string[]
  permissions: string[]
}

export type AuthResponse = {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: User
}

export type Role = { id: string; name: string; description?: string }
export type Permission = { id: string; name: string; module: string; action: string }
export type Team = { id: string; name: string; description?: string; ownerUserId?: string; isActive: boolean }
export type Department = { id: string; name: string; description?: string; parentDepartmentId?: string; isActive: boolean }
export type SystemSetting = { id: string; category: string; key: string; value: string; dataType: number; description?: string }
export type AuditLog = { id: string; entityName: string; entityId: string; action: string; oldValues?: string; newValues?: string; userId?: string; ipAddress?: string; userAgent?: string; createdAt: string }
export type LookupCategory = { id: string; name: string; code: string; description?: string; isActive: boolean }
export type LookupValue = { id: string; lookupCategoryId: string; name: string; code: string; sortOrder: number; isDefault: boolean; isActive: boolean }
