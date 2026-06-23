import { ChevronDown16Regular, ChevronUp16Regular } from '@fluentui/react-icons'
import { NavLink, useLocation } from 'react-router-dom'
import { useMemo, useState } from 'react'
import type { NavGroup } from '../navigation'
import styles from './Sidebar.module.css'

const expandedGroupsStorageKey = 'crm.sidebar.expanded-groups.v2'
const legacyExpandedGroupsStorageKey = 'crm.sidebar.expanded-groups'

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

  const activeGroupKey = useMemo(
    () =>
      visibleGroups.find((group) =>
        group.items.some((item) => pathname === item.to || pathname.startsWith(`${item.to}/`)),
      )?.key,
    [pathname, visibleGroups],
  )

  const defaultExpandedState = useMemo(
    () =>
      Object.fromEntries(
        visibleGroups.map((group) => [group.key, group.key === activeGroupKey]),
      ) as Record<string, boolean>,
    [activeGroupKey, visibleGroups],
  )

  const storedExpandedState = useMemo(() => {
    const parseStored = (raw: string): Record<string, boolean> | null => {
      try {
        const stored = JSON.parse(raw) as Record<string, boolean>
        return Object.fromEntries(
          visibleGroups.map((group) => [group.key, Boolean(stored[group.key])]),
        ) as Record<string, boolean>
      } catch {
        return null
      }
    }

    const currentRaw = localStorage.getItem(expandedGroupsStorageKey)
    if (currentRaw) {
      const parsed = parseStored(currentRaw)
      if (parsed) {
        return parsed
      }
      localStorage.removeItem(expandedGroupsStorageKey)
    }

    const legacyRaw = localStorage.getItem(legacyExpandedGroupsStorageKey)
    if (legacyRaw) {
      const parsed = parseStored(legacyRaw)
      localStorage.removeItem(legacyExpandedGroupsStorageKey)
      if (parsed) {
        localStorage.setItem(expandedGroupsStorageKey, JSON.stringify(parsed))
        return parsed
      }
    }

    return null
  }, [visibleGroups])

  const [expanded, setExpanded] = useState<Record<string, boolean>>(
    () => storedExpandedState ?? defaultExpandedState,
  )
  const [hasCustomizedExpansion, setHasCustomizedExpansion] = useState(
    () => storedExpandedState !== null,
  )

  const effectiveExpanded = hasCustomizedExpansion ? expanded : defaultExpandedState

  const toggleGroup = (key: string) => {
    setHasCustomizedExpansion(true)
    setExpanded((current) => {
      const base = hasCustomizedExpansion ? current : defaultExpandedState
      const next = { ...base, [key]: !base[key] }
      localStorage.setItem(expandedGroupsStorageKey, JSON.stringify(next))
      return next
    })
  }

  return (
    <aside className={`${styles.sidebar} ${collapsed ? styles.collapsed : ''}`}>
      <div className={styles.brand}>
        <span>CRM</span>
        <span className={styles.brandText}>Admin Console</span>
      </div>

      {visibleGroups.map((group) => {
        const isExpanded = collapsed ? false : effectiveExpanded[group.key]
        const isGroupActive = group.items.some((item) => pathname === item.to || pathname.startsWith(`${item.to}/`))
        return (
          <section className={styles.group} key={group.key}>
            <button className={`${styles.groupHeader} ${isGroupActive ? styles.groupHeaderActive : ''}`} type="button" onClick={() => toggleGroup(group.key)} title={group.label}>
              <span className={styles.groupHeaderLeft}>
                <span className={styles.groupIcon}>{group.icon}</span>
                <span className={styles.groupTitle}>{group.label}</span>
              </span>
              <span className={styles.toggleIcon}>{isExpanded ? <ChevronUp16Regular /> : <ChevronDown16Regular />}</span>
            </button>
            {isExpanded && (
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
