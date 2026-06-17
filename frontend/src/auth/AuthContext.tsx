import { createContext, useContext, useMemo, useState } from 'react'
import { api } from '../api/client'
import type { AuthResponse, User } from '../types/models'

type AuthContextValue = {
  user: User | null
  accessToken: string | null
  login: (email: string, password: string) => Promise<void>
  logout: () => Promise<void>
  isAuthenticated: boolean
  hasPermission: (permission?: string) => boolean
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

const initialUserRaw = localStorage.getItem('crm.user')
const initialUser = initialUserRaw ? (JSON.parse(initialUserRaw) as User) : null

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(initialUser)
  const [accessToken, setAccessToken] = useState<string | null>(localStorage.getItem('crm.accessToken'))

  const login = async (email: string, password: string) => {
    const { data } = await api.post<AuthResponse>('api/auth/login', { email, password })
    localStorage.setItem('crm.accessToken', data.accessToken)
    localStorage.setItem('crm.refreshToken', data.refreshToken)
    localStorage.setItem('crm.user', JSON.stringify(data.user))
    setAccessToken(data.accessToken)
    setUser(data.user)
  }

  const logout = async () => {
    const refreshToken = localStorage.getItem('crm.refreshToken')
    if (refreshToken) {
      await api.post('api/auth/logout', { refreshToken })
    }

    localStorage.removeItem('crm.accessToken')
    localStorage.removeItem('crm.refreshToken')
    localStorage.removeItem('crm.user')
    setAccessToken(null)
    setUser(null)
  }

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      accessToken,
      login,
      logout,
      isAuthenticated: Boolean(accessToken && user),
      hasPermission: (permission?: string) => !permission || Boolean(user?.permissions?.includes(permission)),
    }),
    [user, accessToken],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider')
  }

  return context
}
