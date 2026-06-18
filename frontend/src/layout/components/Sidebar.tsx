import { ChevronDown16Regular, ChevronUp16Regular } from '@fluentui/react-icons'
import { NavLink } from 'react-router-dom'
import { useMemo, useState } from 'react'
import type { NavGroup } from '../navigation'
import styles from './Sidebar.module.css'

export function Sidebar({
  collapsed,
  groups,
  hasPermission,
}: {
  collapsed: boolean
  groups: NavGroup[]
  hasPermission: (permission?: string) => boolean
}) {
  const initialState = useMemo(() => {
    const raw = localStorage.getItem('crm.sidebar.expanded-groups')
    if (raw) {
      try {
        return JSON.parse(raw) as Record<string, boolean>
      } catch {
        localStorage.removeItem('crm.sidebar.expanded-groups')
      }
    }

    return Object.fromEntries(groups.map((g) => [g.key, true])) as Record<string, boolean>
  }, [groups])
  const [expanded, setExpanded] = useState<Record<string, boolean>>(initialState)

  const toggleGroup = (key: string) => {
    setExpanded((current) => {
      const next = { ...current, [key]: !current[key] }
      localStorage.setItem('crm.sidebar.expanded-groups', JSON.stringify(next))
      return next
    })
  }

  const visibleGroups = groups
    .filter((group) => group.enabled !== false)
    .map((group) => ({
      ...group,
      items: group.items.filter((item) => item.enabled !== false && hasPermission(item.permission)),
    }))
    .filter((group) => group.items.length > 0)

  return (
    <aside className={`${styles.sidebar} ${collapsed ? styles.collapsed : ''}`}>
      <div className={styles.brand}>
        <span>CRM</span>
        <span className={styles.brandText}>Admin Console</span>
      </div>

      {visibleGroups.map((group) => {
        const isExpanded = collapsed ? false : expanded[group.key]
        return (
          <section className={styles.group} key={group.key}>
            <button className={styles.groupHeader} type="button" onClick={() => toggleGroup(group.key)}>
              <span className={styles.groupTitle}>{group.label}</span>
              <span className={styles.toggleIcon}>{isExpanded ? <ChevronUp16Regular /> : <ChevronDown16Regular />}</span>
            </button>
            {(isExpanded || collapsed) && (
              <div className={styles.links}>
                {group.items.map((item) => (
                  <NavLink
                    key={item.key}
                    to={item.to}
                    className={({ isActive }) => `${styles.link} ${isActive ? styles.linkActive : ''}`}
                    title={item.label}
                  >
                    {item.icon}
                    <span className={styles.label}>{item.label}</span>
                  </NavLink>
                ))}
              </div>
            )}
          </section>
        )
      })}
    </aside>
  )
}
