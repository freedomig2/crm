import { createContext, useContext, useEffect, useMemo, useState } from 'react'
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

function parseStoredUser(): User | null {
  const raw = localStorage.getItem('crm.user')
  if (!raw) {
    return null
  }

  try {
    return JSON.parse(raw) as User
  } catch {
    localStorage.removeItem('crm.user')
    return null
  }
}

const initialUser = parseStoredUser()

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(initialUser)
  const [accessToken, setAccessToken] = useState<string | null>(localStorage.getItem('crm.accessToken'))

  useEffect(() => {
    const syncFromStorage = () => {
      setAccessToken(localStorage.getItem('crm.accessToken'))
      setUser(parseStoredUser())
    }

    const refreshToken = localStorage.getItem('crm.refreshToken')
    if (refreshToken) {
      void (async () => {
        try {
          const { data } = await api.post<AuthResponse>('api/auth/refresh-token', { refreshToken })
          localStorage.setItem('crm.accessToken', data.accessToken)
          localStorage.setItem('crm.refreshToken', data.refreshToken)
          localStorage.setItem('crm.user', JSON.stringify(data.user))
          syncFromStorage()
        } catch {
          localStorage.removeItem('crm.accessToken')
          localStorage.removeItem('crm.refreshToken')
          localStorage.removeItem('crm.user')
          syncFromStorage()
        }
      })()
    }

    window.addEventListener('crm-auth-updated', syncFromStorage)
    window.addEventListener('storage', syncFromStorage)

    return () => {
      window.removeEventListener('crm-auth-updated', syncFromStorage)
      window.removeEventListener('storage', syncFromStorage)
    }
  }, [])

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
      try {
        await api.post('api/auth/logout', { refreshToken })
      } catch {
        // Keep local logout behavior even if API revocation fails.
      }
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
