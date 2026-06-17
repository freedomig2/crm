import { useMemo, useState } from 'react'
import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'

type MenuItem = {
  label: string
  to: string
  permission: string
}

type MenuGroup = {
  id: string
  title: string
  items: MenuItem[]
}

const menuGroups: MenuGroup[] = [
  {
    id: 'security',
    title: 'Security',
    items: [
      { label: 'Users', to: '/users', permission: 'Users.View' },
      { label: 'Roles', to: '/roles', permission: 'Roles.View' },
      { label: 'Permission Assignment', to: '/permissions', permission: 'Roles.View' },
    ],
  },
  {
    id: 'organization',
    title: 'Organization',
    items: [
      { label: 'Teams', to: '/teams', permission: 'Teams.View' },
      { label: 'Departments', to: '/departments', permission: 'Departments.View' },
    ],
  },
  {
    id: 'configuration',
    title: 'Configuration',
    items: [
      { label: 'System Settings', to: '/system-settings', permission: 'Settings.View' },
      { label: 'Reference Data', to: '/reference-data', permission: 'ReferenceData.View' },
    ],
  },
  {
    id: 'monitoring',
    title: 'Monitoring',
    items: [{ label: 'Audit Logs', to: '/audit-logs', permission: 'AuditLogs.View' }],
  },
]

export function AdminLayout() {
  const { logout, user, hasPermission } = useAuth()
  const navigate = useNavigate()
  const [expandedGroups, setExpandedGroups] = useState<Record<string, boolean>>({
    security: true,
    organization: true,
    configuration: true,
    monitoring: true,
  })

  const visibleGroups = useMemo(
    () =>
      menuGroups
        .map((group) => ({
          ...group,
          items: group.items.filter((item) => hasPermission(item.permission)),
        }))
        .filter((group) => group.items.length > 0),
    [hasPermission],
  )

  const handleLogout = async () => {
    await logout()
    navigate('/login')
  }

  const toggleGroup = (groupId: string) => {
    setExpandedGroups((current) => ({
      ...current,
      [groupId]: !current[groupId],
    }))
  }

  return (
    <div className="crm-shell">
      <aside className="crm-sidebar">
        <div className="crm-brand">
          <p className="crm-brand-kicker">Administration</p>
          <h2>CRM Dashboard</h2>
          <p className="crm-brand-subtitle">Operations Control Center</p>
        </div>

        <nav className="crm-menu-groups">
          {visibleGroups.map((group) => {
            const expanded = expandedGroups[group.id]

            return (
              <div className="crm-menu-group" key={group.id}>
                <button type="button" className="crm-group-toggle" onClick={() => toggleGroup(group.id)}>
                  <span>{group.title}</span>
                  <span className="crm-group-toggle-icon">{expanded ? '-' : '+'}</span>
                </button>
                {expanded && (
                  <div className="crm-group-links">
                    {group.items.map((item) => (
                      <NavLink
                        key={item.to}
                        to={item.to}
                        className={({ isActive }) =>
                          isActive ? 'crm-nav-link crm-nav-link-active' : 'crm-nav-link'
                        }
                      >
                        {item.label}
                      </NavLink>
                    ))}
                  </div>
                )}
              </div>
            )
          })}
        </nav>
      </aside>

      <main className="crm-main">
        <header className="crm-header">
          <div>
            <p className="crm-header-title">Administration Workspace</p>
            <p className="crm-header-subtitle">Signed in as {user?.email}</p>
          </div>
          <button type="button" onClick={handleLogout}>Logout</button>
        </header>
        <Outlet />
      </main>
    </div>
  )
}
