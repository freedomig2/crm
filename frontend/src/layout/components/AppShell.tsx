import { Outlet } from 'react-router-dom'
import { useEffect, useState } from 'react'
import { navGroups } from '../navigation'
import { useAuth } from '../../auth/AuthContext'
import { Sidebar } from './Sidebar'
import { TopBar } from './TopBar'
import { readMenuState, writeSidebarCollapsed } from '../menuState'
import styles from './AppShell.module.css'

export function AppShell({ darkMode, onToggleDarkMode }: { darkMode: boolean; onToggleDarkMode: (value: boolean) => void }) {
  const { hasPermission, user } = useAuth()

  const permissionAliases: Record<string, string[]> = {
    'Dashboard.View': ['Users.View'],
    'Permissions.View': ['Roles.View'],
    'Security.View': ['AuditLogs.View', 'Users.View', 'Settings.Update'],
    'Configuration.View': ['Settings.View', 'ReferenceData.View'],
  }

  const canAccess = (permission?: string) => {
    if (!permission) {
      return true
    }

    if (hasPermission(permission)) {
      return true
    }

    return (permissionAliases[permission] ?? []).some((alias) => hasPermission(alias))
  }

  const visibleGroupKeys = navGroups
    .filter((group) => group.enabled !== false)
    .map((group) => group.key)

  const [collapsed, setCollapsed] = useState<boolean>(() => readMenuState(visibleGroupKeys).sidebarCollapsed)

  useEffect(() => {
    writeSidebarCollapsed(collapsed)
  }, [collapsed])

  return (
    <div className={`${styles.shell} ${collapsed ? styles.shellCollapsed : ''}`} data-testid="app-shell" data-collapsed={collapsed ? 'true' : 'false'}>
      <Sidebar collapsed={collapsed} groups={navGroups} hasPermission={canAccess} />

      <div className={styles.mainArea} data-testid="app-main-shell">
        <div className={styles.topRow}>
          <TopBar
            collapsed={collapsed}
            onToggleSidebar={() => setCollapsed((v) => !v)}
            darkMode={darkMode}
            onToggleDarkMode={onToggleDarkMode}
            userEmail={user?.email}
          />
        </div>
        <main className={styles.contentArea} data-testid="app-page-content">
          <div className={styles.pageCard}>
            <Outlet />
          </div>
        </main>
      </div>
    </div>
  )
}
