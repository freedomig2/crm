import { Outlet } from 'react-router-dom'
import { useState } from 'react'
import { navGroups } from '../navigation'
import { useAuth } from '../../auth/AuthContext'
import { Sidebar } from './Sidebar'
import { TopBar } from './TopBar'
import styles from './AppShell.module.css'

export function AppShell({ darkMode, onToggleDarkMode }: { darkMode: boolean; onToggleDarkMode: (value: boolean) => void }) {
  const [collapsed, setCollapsed] = useState(false)
  const { hasPermission, user } = useAuth()

  const canAccess = (permission?: string) => {
    if (!permission) {
      return true
    }
    return hasPermission(permission)
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
