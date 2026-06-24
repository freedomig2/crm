import { ChevronDown16Regular, ChevronUp16Regular } from '@fluentui/react-icons'
import { NavLink, useLocation } from 'react-router-dom'
import { useMemo, useState } from 'react'
import type { NavGroup } from '../navigation'
import { readMenuState, writeExpandedGroups } from '../menuState'
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
  const { pathname } = useLocation()

  const visibleGroups = useMemo(
    () =>
      groups
        .filter((group) => group.enabled !== false)
        .map((group) => ({
          ...group,
          items: group.items.filter((item) => item.enabled !== false && hasPermission(item.permission)),
        }))
        .filter((group) => group.items.length > 0),
    [groups, hasPermission],
  )

  const visibleGroupKeys = useMemo(() => visibleGroups.map((group) => group.key), [visibleGroups])

  const [expandedGroups, setExpandedGroups] = useState<string[]>(() => readMenuState(visibleGroupKeys).expandedGroups)

  const effectiveExpanded = useMemo(
    () => new Set(expandedGroups.filter((groupKey) => visibleGroupKeys.includes(groupKey))),
    [expandedGroups, visibleGroupKeys],
  )

  const toggleGroup = (key: string) => {
    setExpandedGroups((current) => {
      const exists = current.includes(key)
      const next = exists ? current.filter((groupKey) => groupKey !== key) : [...current, key]
      writeExpandedGroups(next)
      return next
    })
  }

  return (
    <aside className={`${styles.sidebar} ${collapsed ? styles.collapsed : ''}`} data-testid="crm-sidebar" data-collapsed={collapsed ? 'true' : 'false'}>
      <div className={styles.brand}>
        <span>CRM</span>
        <span className={styles.brandText}>Admin Console</span>
      </div>

      {visibleGroups.map((group) => {
        const isExpanded = collapsed ? false : effectiveExpanded.has(group.key)
        const isGroupActive = group.items.some((item) => pathname === item.to || pathname.startsWith(`${item.to}/`))
        return (
          <section className={styles.group} key={group.key}>
            <button className={`${styles.groupHeader} ${isGroupActive ? styles.groupHeaderActive : ''}`} type="button" onClick={() => toggleGroup(group.key)} title={group.label} data-testid={`nav-group-${group.key}`}>
              <span className={styles.groupHeaderLeft}>
                <span className={styles.groupIcon}>{group.icon}</span>
                <span className={styles.groupTitle}>{group.label}</span>
              </span>
              <span className={styles.toggleIcon}>{isExpanded ? <ChevronUp16Regular /> : <ChevronDown16Regular />}</span>
            </button>
            {isExpanded && (
              <div className={styles.links} data-testid={`nav-group-links-${group.key}`}>
                {group.items.map((item) => (
                  <NavLink
                    key={item.key}
                    to={item.to}
                    data-testid={`nav-item-${item.key}`}
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
