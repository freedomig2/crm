import { Outlet } from 'react-router-dom'
import { useEffect, useState } from 'react'
import { navGroups } from '../navigation'
import { useAuth } from '../../auth/AuthContext'
import { Sidebar } from './Sidebar'
import { TopBar } from './TopBar'
import styles from './AppShell.module.css'

export function AppShell({ darkMode, onToggleDarkMode }: { darkMode: boolean; onToggleDarkMode: (value: boolean) => void }) {
  const [collapsed, setCollapsed] = useState<boolean>(() => localStorage.getItem('crm.sidebar.collapsed') === '1')
  const { hasPermission, user } = useAuth()

  useEffect(() => {
    localStorage.setItem('crm.sidebar.collapsed', collapsed ? '1' : '0')
  }, [collapsed])

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

  return (
    <div className={`${styles.shell} ${collapsed ? styles.shellCollapsed : ''}`}>
      <Sidebar collapsed={collapsed} groups={navGroups} hasPermission={canAccess} />

      <div className={styles.mainArea}>
        <div className={styles.topRow}>
          <TopBar
            collapsed={collapsed}
            onToggleSidebar={() => setCollapsed((v) => !v)}
            darkMode={darkMode}
            onToggleDarkMode={onToggleDarkMode}
            userEmail={user?.email}
          />
        </div>
        <main className={styles.contentArea}>
          <div className={styles.pageCard}>
            <Outlet />
          </div>
        </main>
      </div>
    </div>
  )
}
