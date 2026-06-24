import { Fragment } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { Breadcrumb, BreadcrumbButton, BreadcrumbDivider, BreadcrumbItem } from '@fluentui/react-components'
import styles from './PageChrome.module.css'

const labelMap: Record<string, string> = {
  admin: 'Administration',
  dashboard: 'Dashboard',
  users: 'Users',
  roles: 'Roles',
  permissions: 'Permissions',
  teams: 'Teams',
  departments: 'Departments',
  create: 'Create',
  edit: 'Edit',
  'system-settings': 'System Settings',
  'lookup-categories': 'Lookup Categories',
  'lookup-values': 'Lookup Values',
  'audit-logs': 'Audit Logs',
  'change-password': 'Change Password',
  configuration: 'Configuration',
  security: 'Security',
  audit: 'Audit',
  'crm-setup': 'CRM Setup',
  crm: 'CRM',
  accounts: 'Accounts',
  contacts: 'Contacts',
  'account-addresses': 'Account Addresses',
  'customer-profiles': 'Customer Profiles',
  'account-relationships': 'Account Relationships',
  'account-activities': 'Account Activities',
}

export function Breadcrumbs() {
  const location = useLocation()
  const navigate = useNavigate()
  const parts = location.pathname.split('/').filter(Boolean)
  const rawCrumbs = [
    { path: '/dashboard', label: 'Home' },
    ...parts.map((part, index) => {
      const path = `/${parts.slice(0, index + 1).join('/')}`
      const isIdLike = /^[0-9a-f-]{8,}$/i.test(part)
      const label = isIdLike ? 'Details' : labelMap[part] ?? part.replace(/-/g, ' ')
      return { path, label }
    }),
  ]

  const seen = new Set<string>()
  const crumbs = rawCrumbs.filter((crumb) => {
    if (seen.has(crumb.path)) {
      return false
    }

    seen.add(crumb.path)
    return true
  })

  return (
    <Breadcrumb className={styles.breadcrumbs} aria-label="Breadcrumb">
      {crumbs.map((crumb, index) => {
        const isLast = index === crumbs.length - 1

        return (
          <Fragment key={crumb.path}>
            {index > 0 ? <BreadcrumbDivider /> : null}
            <BreadcrumbItem>
              <BreadcrumbButton current={isLast} onClick={isLast ? undefined : () => navigate(crumb.path)}>
                {crumb.label}
              </BreadcrumbButton>
            </BreadcrumbItem>
          </Fragment>
        )
      })}
    </Breadcrumb>
  )
}
