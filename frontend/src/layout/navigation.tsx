import type { JSX } from 'react'
import {
  ChartMultipleRegular,
  PeopleRegular,
  ShieldRegular,
  SettingsRegular,
  BookDatabaseRegular,
  DataPieRegular,
  LockClosedRegular,
  ClockRegular,
  WarningRegular,
  KeyRegular,
  DarkThemeRegular,
  ClipboardTaskRegular,
  DocumentSearchRegular,
  BranchRequestRegular,
  BuildingRegular,
  AppsListRegular,
  BuildingBankRegular,
  PersonCallRegular,
  LocationRegular,
  ClipboardTextLtrRegular,
  NetworkCheckRegular,
  CalendarAgendaRegular,
} from '@fluentui/react-icons'

export type NavItem = {
  key: string
  label: string
  to: string
  icon: JSX.Element
  permission?: string
}

export type NavGroup = {
  key: string
  label: string
  icon: JSX.Element
  items: NavItem[]
}

export const navGroups: NavGroup[] = [
  {
    key: 'dashboard',
    label: 'Dashboard',
    icon: <ChartMultipleRegular />,
    items: [{ key: 'dashboard-home', label: 'Dashboard', to: '/dashboard', icon: <ChartMultipleRegular />, permission: 'Users.View' }],
  },
  {
    key: 'administration',
    label: 'Administration',
    icon: <PeopleRegular />,
    items: [
      { key: 'users', label: 'Users', to: '/admin/users', icon: <PeopleRegular />, permission: 'Users.View' },
      { key: 'roles', label: 'Roles', to: '/admin/roles', icon: <ShieldRegular />, permission: 'Roles.View' },
      { key: 'permissions', label: 'Permissions', to: '/admin/permissions', icon: <KeyRegular />, permission: 'Roles.View' },
      { key: 'teams', label: 'Teams', to: '/admin/teams', icon: <BuildingRegular />, permission: 'Teams.View' },
      { key: 'departments', label: 'Departments', to: '/admin/departments', icon: <BranchRequestRegular />, permission: 'Departments.View' },
    ],
  },
  {
    key: 'security',
    label: 'Security',
    icon: <LockClosedRegular />,
    items: [
      { key: 'login-history', label: 'Login History', to: '/security/login-history', icon: <ClockRegular />, permission: 'AuditLogs.View' },
      { key: 'active-sessions', label: 'Active Sessions', to: '/security/active-sessions', icon: <AppsListRegular />, permission: 'Users.View' },
      { key: 'failed-logins', label: 'Failed Login Attempts', to: '/security/failed-logins', icon: <WarningRegular />, permission: 'AuditLogs.View' },
      { key: 'password-policies', label: 'Password Policies', to: '/security/password-policies', icon: <KeyRegular />, permission: 'Settings.Update' },
      { key: 'mfa-settings', label: 'MFA Settings', to: '/security/mfa-settings', icon: <DarkThemeRegular />, permission: 'Settings.Update' },
    ],
  },
  {
    key: 'configuration',
    label: 'Configuration',
    icon: <SettingsRegular />,
    items: [
      { key: 'system-settings', label: 'System Settings', to: '/admin/system-settings', icon: <SettingsRegular />, permission: 'Settings.View' },
      { key: 'lookup-categories', label: 'Lookup Categories', to: '/admin/lookup-categories', icon: <BookDatabaseRegular />, permission: 'ReferenceData.View' },
      { key: 'lookup-values', label: 'Lookup Values', to: '/admin/lookup-values', icon: <BookDatabaseRegular />, permission: 'ReferenceData.View' },
      { key: 'number-sequences', label: 'Number Sequences', to: '/configuration/number-sequences', icon: <ClipboardTaskRegular />, permission: 'Settings.View' },
    ],
  },
  {
    key: 'audit',
    label: 'Audit',
    icon: <DocumentSearchRegular />,
    items: [
      { key: 'audit-logs', label: 'Audit Logs', to: '/admin/audit-logs', icon: <DocumentSearchRegular />, permission: 'AuditLogs.View' },
      { key: 'data-changes', label: 'Data Changes', to: '/audit/data-changes', icon: <DataPieRegular />, permission: 'AuditLogs.View' },
      { key: 'security-events', label: 'Security Events', to: '/audit/security-events', icon: <WarningRegular />, permission: 'AuditLogs.View' },
    ],
  },
  {
    key: 'crm-setup',
    label: 'CRM Setup',
    icon: <BookDatabaseRegular />,
    items: [
      { key: 'lead-sources', label: 'Lead Sources', to: '/crm-setup/lead-sources', icon: <BookDatabaseRegular />, permission: 'ReferenceData.View' },
      { key: 'industries', label: 'Industries', to: '/crm-setup/industries', icon: <BookDatabaseRegular />, permission: 'ReferenceData.View' },
      { key: 'case-statuses', label: 'Case Statuses', to: '/crm-setup/case-statuses', icon: <BookDatabaseRegular />, permission: 'ReferenceData.View' },
      { key: 'opportunity-stages', label: 'Opportunity Stages', to: '/crm-setup/opportunity-stages', icon: <BookDatabaseRegular />, permission: 'ReferenceData.View' },
    ],
  },
  {
    key: 'account-management',
    label: 'Account Management',
    icon: <BuildingBankRegular />,
    items: [
      { key: 'accounts', label: 'Accounts', to: '/crm/accounts', icon: <BuildingBankRegular />, permission: 'Accounts.View' },
      { key: 'contacts', label: 'Contacts', to: '/crm/contacts', icon: <PersonCallRegular />, permission: 'Contacts.View' },
      { key: 'account-addresses', label: 'Account Addresses', to: '/crm/account-addresses', icon: <LocationRegular />, permission: 'AccountAddresses.View' },
      { key: 'customer-profiles', label: 'Customer Profiles', to: '/crm/customer-profiles', icon: <ClipboardTextLtrRegular />, permission: 'CustomerProfiles.View' },
      { key: 'account-relationships', label: 'Account Relationships', to: '/crm/account-relationships', icon: <NetworkCheckRegular />, permission: 'AccountRelationships.View' },
      { key: 'account-activities', label: 'Account Activities', to: '/crm/account-activities', icon: <CalendarAgendaRegular />, permission: 'AccountActivities.View' },
    ],
  },
]
