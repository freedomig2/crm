const MENU_STATE_VERSION = 'crm-menu-state-v3'
const MENU_STATE_VERSION_STORAGE_KEY = 'crm.menu.state.version'

export const SIDEBAR_COLLAPSED_STORAGE_KEY = 'crm.sidebar.collapsed'
export const EXPANDED_GROUPS_STORAGE_KEY = 'crm.sidebar.expanded-groups.v3'

export type MenuState = {
  sidebarCollapsed: boolean
  expandedGroups: string[]
}

const defaultState: MenuState = {
  sidebarCollapsed: false,
  expandedGroups: [],
}

const parseExpandedGroups = (raw: string | null): string[] => {
  if (!raw) {
    return []
  }

  try {
    const parsed = JSON.parse(raw)
    if (!Array.isArray(parsed)) {
      return []
    }

    return parsed.filter((value): value is string => typeof value === 'string')
  } catch {
    return []
  }
}

export const readMenuState = (visibleGroupKeys: string[]): MenuState => {
  const storedVersion = localStorage.getItem(MENU_STATE_VERSION_STORAGE_KEY)
  if (storedVersion !== MENU_STATE_VERSION) {
    localStorage.setItem(MENU_STATE_VERSION_STORAGE_KEY, MENU_STATE_VERSION)
    localStorage.setItem(SIDEBAR_COLLAPSED_STORAGE_KEY, defaultState.sidebarCollapsed ? '1' : '0')
    localStorage.setItem(EXPANDED_GROUPS_STORAGE_KEY, JSON.stringify(defaultState.expandedGroups))
    return defaultState
  }

  const sidebarCollapsed = localStorage.getItem(SIDEBAR_COLLAPSED_STORAGE_KEY) === '1'
  const expandedGroups = parseExpandedGroups(localStorage.getItem(EXPANDED_GROUPS_STORAGE_KEY)).filter((key) =>
    visibleGroupKeys.includes(key),
  )

  const staleExpandAll = visibleGroupKeys.length > 0 && expandedGroups.length === visibleGroupKeys.length
  if (staleExpandAll) {
    localStorage.setItem(EXPANDED_GROUPS_STORAGE_KEY, JSON.stringify([]))
    return { sidebarCollapsed, expandedGroups: [] }
  }

  return { sidebarCollapsed, expandedGroups }
}

export const writeSidebarCollapsed = (collapsed: boolean) => {
  localStorage.setItem(SIDEBAR_COLLAPSED_STORAGE_KEY, collapsed ? '1' : '0')
}

export const writeExpandedGroups = (expandedGroups: string[]) => {
  localStorage.setItem(EXPANDED_GROUPS_STORAGE_KEY, JSON.stringify(expandedGroups))
}

export const menuStateVersion = MENU_STATE_VERSION