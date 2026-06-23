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
  enabled?: boolean
}

export type NavGroup = {
  key: string
  label: string
  icon: JSX.Element
  items: NavItem[]
  enabled?: boolean
}

const parseCsv = (value: string | undefined): Set<string> | null => {
  if (!value || !value.trim()) {
    return null
  }

  return new Set(
    value
      .split(',')
      .map((part) => part.trim())
      .filter((part) => part.length > 0),
  )
}

const defaultEnabledGroups = new Set([
  'dashboard',
  'customers',
  'sales',
  'administration',
  'security',
  'configuration',
  'audit',
])

const defaultEnabledItems = new Set([
  'dashboard',
  'my-work',
  'my-activities',
  'my-open-tasks',
  'accounts',
  'contacts',
  'leads',
  'lead-score-rules',
  'opportunities',
  'sales-pipeline',
  'sales-forecasts',
  'sales-targets',
  'products',
  'product-categories',
  'price-lists',
  'product-bundles',
  'unit-of-measures',
  'discounts',
  'account-activities',
  'relationships',
  'users',
  'roles',
  'permissions',
  'teams',
  'departments',
  'login-history',
  'active-sessions',
  'failed-login-attempts',
  'password-policies',
  'mfa-settings',
  'system-settings',
  'lookup-categories',
  'lookup-values',
  'number-sequences',
  'audit-logs',
  'data-changes',
  'security-events',
  'my-profile',
  'my-preferences',
  'my-notifications',
  'my-saved-views',
])

const enabledGroupsOverride = parseCsv(import.meta.env.VITE_ENABLED_NAV_GROUPS)
const enabledItemsOverride = parseCsv(import.meta.env.VITE_ENABLED_NAV_ITEMS)

const groupOrder: Record<string, number> = {
  dashboard: 1,
  customers: 2,
  sales: 3,
  marketing: 4,
  service: 5,
  projects: 6,
  documents: 7,
  activities: 8,
  finance: 9,
  reporting: 10,
  administration: 11,
  security: 12,
  configuration: 13,
  'data-management': 14,
  audit: 15,
  integrations: 16,
  'ai-copilot': 17,
  personal: 18,
}

const isGroupEnabled = (key: string) => (enabledGroupsOverride ? enabledGroupsOverride.has(key) : defaultEnabledGroups.has(key))
const isItemEnabled = (key: string) => (enabledItemsOverride ? enabledItemsOverride.has(key) : defaultEnabledItems.has(key))

export const navGroups: NavGroup[] = [
  {
    key: 'dashboard',
    label: 'Dashboard',
    icon: <ChartMultipleRegular />,
    enabled: isGroupEnabled('dashboard'),
    items: [
      { key: 'dashboard', label: 'Dashboard', to: '/dashboard', icon: <ChartMultipleRegular />, permission: 'Dashboard.View', enabled: isItemEnabled('dashboard') },
      { key: 'my-work', label: 'My Work', to: '/dashboard/my-work', icon: <ClipboardTaskRegular />, permission: 'Dashboard.View', enabled: isItemEnabled('my-work') },
      { key: 'my-activities', label: 'My Activities', to: '/dashboard/my-activities', icon: <CalendarAgendaRegular />, permission: 'Activities.View', enabled: isItemEnabled('my-activities') },
      { key: 'my-open-tasks', label: 'My Open Tasks', to: '/dashboard/my-open-tasks', icon: <ClipboardTaskRegular />, permission: 'Activities.View', enabled: isItemEnabled('my-open-tasks') },
    ],
  },
  {
    key: 'customers',
    label: 'Customers',
    icon: <BuildingBankRegular />,
    enabled: isGroupEnabled('customers'),
    items: [
      { key: 'accounts', label: 'Accounts', to: '/crm/accounts', icon: <BuildingBankRegular />, permission: 'Accounts.View', enabled: isItemEnabled('accounts') },
      { key: 'contacts', label: 'Contacts', to: '/contacts', icon: <PersonCallRegular />, permission: 'Contacts.View', enabled: isItemEnabled('contacts') },
      { key: 'account-activities', label: 'Account Activities', to: '/crm/account-activities', icon: <CalendarAgendaRegular />, permission: 'AccountActivities.View', enabled: isItemEnabled('account-activities') },
      { key: 'relationships', label: 'Relationships', to: '/crm/account-relationships', icon: <NetworkCheckRegular />, permission: 'AccountRelationships.View', enabled: isItemEnabled('relationships') },
    ],
  },
  {
    key: 'sales',
    label: 'Sales',
    icon: <DataPieRegular />,
    enabled: isGroupEnabled('sales'),
    items: [
      { key: 'leads', label: 'Leads', to: '/leads', icon: <PeopleRegular />, permission: 'Leads.View', enabled: isItemEnabled('leads') },
      { key: 'lead-score-rules', label: 'Lead Score Rules', to: '/lead-score-rules', icon: <ChartMultipleRegular />, permission: 'LeadScoreRules.View', enabled: isItemEnabled('lead-score-rules') },
      { key: 'opportunities', label: 'Opportunities', to: '/opportunities', icon: <DataPieRegular />, permission: 'Opportunities.View', enabled: isItemEnabled('opportunities') },
      { key: 'sales-pipeline', label: 'Pipeline', to: '/sales/pipeline', icon: <BranchRequestRegular />, permission: 'Pipeline.View', enabled: isItemEnabled('sales-pipeline') },
      { key: 'sales-forecasts', label: 'Forecasts', to: '/sales/forecasts', icon: <ChartMultipleRegular />, permission: 'Forecasts.View', enabled: isItemEnabled('sales-forecasts') },
      { key: 'sales-targets', label: 'Sales Targets', to: '/sales/targets', icon: <ClipboardTaskRegular />, permission: 'SalesTargets.View', enabled: isItemEnabled('sales-targets') },
      { key: 'quotes', label: 'Quotes', to: '/sales/quotes', icon: <DocumentSearchRegular />, permission: 'Quotes.View', enabled: isItemEnabled('quotes') },
      { key: 'orders', label: 'Orders', to: '/sales/orders', icon: <ClipboardTaskRegular />, permission: 'Orders.View', enabled: isItemEnabled('orders') },
      { key: 'invoices-sales', label: 'Invoices', to: '/sales/invoices', icon: <DocumentSearchRegular />, permission: 'Invoices.View', enabled: isItemEnabled('invoices-sales') },
      { key: 'products', label: 'Products', to: '/sales/products', icon: <BookDatabaseRegular />, permission: 'Products.View', enabled: isItemEnabled('products') },
      { key: 'product-categories', label: 'Product Categories', to: '/sales/product-categories', icon: <BookDatabaseRegular />, permission: 'ProductCategories.View', enabled: isItemEnabled('product-categories') },
      { key: 'price-lists', label: 'Price Lists', to: '/sales/price-lists', icon: <BookDatabaseRegular />, permission: 'PriceLists.View', enabled: isItemEnabled('price-lists') },
      { key: 'product-bundles', label: 'Product Bundles', to: '/sales/product-bundles', icon: <BookDatabaseRegular />, permission: 'ProductBundles.View', enabled: isItemEnabled('product-bundles') },
      { key: 'unit-of-measures', label: 'Units Of Measure', to: '/sales/unit-of-measures', icon: <BookDatabaseRegular />, permission: 'UnitOfMeasures.View', enabled: isItemEnabled('unit-of-measures') },
      { key: 'discounts', label: 'Discounts', to: '/sales/discounts', icon: <BookDatabaseRegular />, permission: 'Discounts.View', enabled: isItemEnabled('discounts') },
      { key: 'competitors', label: 'Competitors', to: '/sales/competitors', icon: <BranchRequestRegular />, permission: 'Products.View', enabled: isItemEnabled('competitors') },
      { key: 'sales-territories', label: 'Sales Territories', to: '/sales/territories', icon: <LocationRegular />, permission: 'Opportunities.View', enabled: isItemEnabled('sales-territories') },
    ],
  },
  {
    key: 'marketing',
    label: 'Marketing',
    icon: <DataPieRegular />,
    enabled: isGroupEnabled('marketing'),
    items: [
      { key: 'campaigns', label: 'Campaigns', to: '/marketing/campaigns', icon: <DataPieRegular />, permission: 'Campaigns.View', enabled: isItemEnabled('campaigns') },
      { key: 'campaign-activities', label: 'Campaign Activities', to: '/marketing/campaign-activities', icon: <CalendarAgendaRegular />, permission: 'Marketing.View', enabled: isItemEnabled('campaign-activities') },
      { key: 'marketing-lists', label: 'Marketing Lists', to: '/marketing/lists', icon: <PeopleRegular />, permission: 'Marketing.View', enabled: isItemEnabled('marketing-lists') },
      { key: 'email-campaigns', label: 'Email Campaigns', to: '/marketing/email-campaigns', icon: <DocumentSearchRegular />, permission: 'Marketing.View', enabled: isItemEnabled('email-campaigns') },
      { key: 'landing-pages', label: 'Landing Pages', to: '/marketing/landing-pages', icon: <DocumentSearchRegular />, permission: 'Marketing.View', enabled: isItemEnabled('landing-pages') },
      { key: 'event-management', label: 'Event Management', to: '/marketing/events', icon: <CalendarAgendaRegular />, permission: 'Marketing.View', enabled: isItemEnabled('event-management') },
      { key: 'lead-scoring', label: 'Lead Scoring', to: '/marketing/lead-scoring', icon: <ChartMultipleRegular />, permission: 'Marketing.View', enabled: isItemEnabled('lead-scoring') },
      { key: 'customer-journeys', label: 'Customer Journeys', to: '/marketing/customer-journeys', icon: <BranchRequestRegular />, permission: 'Marketing.View', enabled: isItemEnabled('customer-journeys') },
    ],
  },
  {
    key: 'service',
    label: 'Service',
    icon: <PeopleRegular />,
    enabled: isGroupEnabled('service'),
    items: [
      { key: 'cases', label: 'Cases', to: '/service/cases', icon: <ClipboardTaskRegular />, permission: 'Cases.View', enabled: isItemEnabled('cases') },
      { key: 'service-activities', label: 'Service Activities', to: '/service/activities', icon: <CalendarAgendaRegular />, permission: 'Service.View', enabled: isItemEnabled('service-activities') },
      { key: 'knowledge-base', label: 'Knowledge Base', to: '/service/knowledge-base', icon: <BookDatabaseRegular />, permission: 'Service.View', enabled: isItemEnabled('knowledge-base') },
      { key: 'entitlements', label: 'Entitlements', to: '/service/entitlements', icon: <ShieldRegular />, permission: 'Service.View', enabled: isItemEnabled('entitlements') },
      { key: 'slas', label: 'SLAs', to: '/service/slas', icon: <ClockRegular />, permission: 'Service.View', enabled: isItemEnabled('slas') },
      { key: 'queues', label: 'Queues', to: '/service/queues', icon: <AppsListRegular />, permission: 'Service.View', enabled: isItemEnabled('queues') },
      { key: 'escalations', label: 'Escalations', to: '/service/escalations', icon: <WarningRegular />, permission: 'Service.View', enabled: isItemEnabled('escalations') },
      { key: 'customer-feedback', label: 'Customer Feedback', to: '/service/customer-feedback', icon: <DocumentSearchRegular />, permission: 'Service.View', enabled: isItemEnabled('customer-feedback') },
    ],
  },
  {
    key: 'projects',
    label: 'Projects',
    icon: <ClipboardTaskRegular />,
    enabled: isGroupEnabled('projects'),
    items: [
      { key: 'projects-list', label: 'Projects', to: '/projects', icon: <ClipboardTaskRegular />, permission: 'Projects.View', enabled: isItemEnabled('projects-list') },
      { key: 'project-tasks', label: 'Project Tasks', to: '/projects/tasks', icon: <ClipboardTaskRegular />, permission: 'Projects.View', enabled: isItemEnabled('project-tasks') },
      { key: 'milestones', label: 'Milestones', to: '/projects/milestones', icon: <CalendarAgendaRegular />, permission: 'Projects.View', enabled: isItemEnabled('milestones') },
      { key: 'timesheets', label: 'Timesheets', to: '/projects/timesheets', icon: <ClockRegular />, permission: 'Projects.View', enabled: isItemEnabled('timesheets') },
      { key: 'resource-allocation', label: 'Resource Allocation', to: '/projects/resources', icon: <PeopleRegular />, permission: 'Projects.View', enabled: isItemEnabled('resource-allocation') },
      { key: 'project-billing', label: 'Project Billing', to: '/projects/billing', icon: <DocumentSearchRegular />, permission: 'Projects.View', enabled: isItemEnabled('project-billing') },
    ],
  },
  {
    key: 'documents',
    label: 'Documents',
    icon: <DocumentSearchRegular />,
    enabled: isGroupEnabled('documents'),
    items: [
      { key: 'documents-list', label: 'Documents', to: '/documents', icon: <DocumentSearchRegular />, permission: 'Documents.View', enabled: isItemEnabled('documents-list') },
      { key: 'templates', label: 'Templates', to: '/documents/templates', icon: <DocumentSearchRegular />, permission: 'Documents.View', enabled: isItemEnabled('templates') },
      { key: 'document-categories', label: 'Document Categories', to: '/documents/categories', icon: <BookDatabaseRegular />, permission: 'Documents.View', enabled: isItemEnabled('document-categories') },
      { key: 'shared-files', label: 'Shared Files', to: '/documents/shared-files', icon: <AppsListRegular />, permission: 'Documents.View', enabled: isItemEnabled('shared-files') },
    ],
  },
  {
    key: 'activities',
    label: 'Activities',
    icon: <CalendarAgendaRegular />,
    enabled: isGroupEnabled('activities'),
    items: [
      { key: 'tasks', label: 'Tasks', to: '/activities/tasks', icon: <ClipboardTaskRegular />, permission: 'Activities.View', enabled: isItemEnabled('tasks') },
      { key: 'appointments', label: 'Appointments', to: '/activities/appointments', icon: <CalendarAgendaRegular />, permission: 'Activities.View', enabled: isItemEnabled('appointments') },
      { key: 'phone-calls', label: 'Phone Calls', to: '/activities/phone-calls', icon: <PersonCallRegular />, permission: 'Activities.View', enabled: isItemEnabled('phone-calls') },
      { key: 'emails', label: 'Emails', to: '/activities/emails', icon: <DocumentSearchRegular />, permission: 'Activities.View', enabled: isItemEnabled('emails') },
      { key: 'meetings', label: 'Meetings', to: '/activities/meetings', icon: <PeopleRegular />, permission: 'Activities.View', enabled: isItemEnabled('meetings') },
      { key: 'notes', label: 'Notes', to: '/activities/notes', icon: <ClipboardTextLtrRegular />, permission: 'Activities.View', enabled: isItemEnabled('notes') },
    ],
  },
  {
    key: 'finance',
    label: 'Finance',
    icon: <DataPieRegular />,
    enabled: isGroupEnabled('finance'),
    items: [
      { key: 'invoices-finance', label: 'Invoices', to: '/finance/invoices', icon: <DocumentSearchRegular />, permission: 'Finance.View', enabled: isItemEnabled('invoices-finance') },
      { key: 'payments', label: 'Payments', to: '/finance/payments', icon: <DocumentSearchRegular />, permission: 'Finance.View', enabled: isItemEnabled('payments') },
      { key: 'credit-notes', label: 'Credit Notes', to: '/finance/credit-notes', icon: <DocumentSearchRegular />, permission: 'Finance.View', enabled: isItemEnabled('credit-notes') },
      { key: 'revenue-reports', label: 'Revenue Reports', to: '/finance/revenue-reports', icon: <ChartMultipleRegular />, permission: 'Finance.View', enabled: isItemEnabled('revenue-reports') },
      { key: 'payment-terms', label: 'Payment Terms', to: '/finance/payment-terms', icon: <SettingsRegular />, permission: 'Finance.View', enabled: isItemEnabled('payment-terms') },
      { key: 'currencies', label: 'Currencies', to: '/finance/currencies', icon: <SettingsRegular />, permission: 'Finance.View', enabled: isItemEnabled('currencies') },
    ],
  },
  {
    key: 'reporting',
    label: 'Reporting',
    icon: <ChartMultipleRegular />,
    enabled: isGroupEnabled('reporting'),
    items: [
      { key: 'dashboards-reporting', label: 'Dashboards', to: '/reporting/dashboards', icon: <ChartMultipleRegular />, permission: 'Reports.View', enabled: isItemEnabled('dashboards-reporting') },
      { key: 'reports', label: 'Reports', to: '/reporting/reports', icon: <DocumentSearchRegular />, permission: 'Reports.View', enabled: isItemEnabled('reports') },
      { key: 'kpi-monitoring', label: 'KPI Monitoring', to: '/reporting/kpi-monitoring', icon: <ChartMultipleRegular />, permission: 'Reports.View', enabled: isItemEnabled('kpi-monitoring') },
      { key: 'sales-analytics', label: 'Sales Analytics', to: '/reporting/sales-analytics', icon: <ChartMultipleRegular />, permission: 'Reports.View', enabled: isItemEnabled('sales-analytics') },
      { key: 'customer-analytics', label: 'Customer Analytics', to: '/reporting/customer-analytics', icon: <ChartMultipleRegular />, permission: 'Reports.View', enabled: isItemEnabled('customer-analytics') },
      { key: 'activity-analytics', label: 'Activity Analytics', to: '/reporting/activity-analytics', icon: <ChartMultipleRegular />, permission: 'Reports.View', enabled: isItemEnabled('activity-analytics') },
      { key: 'service-analytics', label: 'Service Analytics', to: '/reporting/service-analytics', icon: <ChartMultipleRegular />, permission: 'Reports.View', enabled: isItemEnabled('service-analytics') },
      { key: 'scheduled-reports', label: 'Scheduled Reports', to: '/reporting/scheduled-reports', icon: <ClockRegular />, permission: 'Reports.View', enabled: isItemEnabled('scheduled-reports') },
    ],
  },
  {
    key: 'administration',
    label: 'Administration',
    icon: <PeopleRegular />,
    enabled: isGroupEnabled('administration'),
    items: [
      { key: 'users', label: 'Users', to: '/admin/users', icon: <PeopleRegular />, permission: 'Users.View', enabled: isItemEnabled('users') },
      { key: 'roles', label: 'Roles', to: '/admin/roles', icon: <ShieldRegular />, permission: 'Roles.View', enabled: isItemEnabled('roles') },
      { key: 'permissions', label: 'Permissions', to: '/admin/permissions', icon: <KeyRegular />, permission: 'Permissions.View', enabled: isItemEnabled('permissions') },
      { key: 'teams', label: 'Teams', to: '/admin/teams', icon: <BuildingRegular />, permission: 'Teams.View', enabled: isItemEnabled('teams') },
      { key: 'departments', label: 'Departments', to: '/admin/departments', icon: <BranchRequestRegular />, permission: 'Departments.View', enabled: isItemEnabled('departments') },
    ],
  },
  {
    key: 'security',
    label: 'Security',
    icon: <LockClosedRegular />,
    enabled: isGroupEnabled('security'),
    items: [
      { key: 'login-history', label: 'Login History', to: '/security/login-history', icon: <ClockRegular />, permission: 'Security.View', enabled: isItemEnabled('login-history') },
      { key: 'active-sessions', label: 'Active Sessions', to: '/security/active-sessions', icon: <AppsListRegular />, permission: 'Security.View', enabled: isItemEnabled('active-sessions') },
      { key: 'failed-login-attempts', label: 'Failed Login Attempts', to: '/security/failed-logins', icon: <WarningRegular />, permission: 'Security.View', enabled: isItemEnabled('failed-login-attempts') },
      { key: 'password-policies', label: 'Password Policies', to: '/security/password-policies', icon: <KeyRegular />, permission: 'Security.View', enabled: isItemEnabled('password-policies') },
      { key: 'mfa-settings', label: 'MFA Settings', to: '/security/mfa-settings', icon: <DarkThemeRegular />, permission: 'Security.View', enabled: isItemEnabled('mfa-settings') },
      { key: 'api-keys', label: 'API Keys', to: '/security/api-keys', icon: <KeyRegular />, permission: 'Security.View', enabled: isItemEnabled('api-keys') },
      { key: 'oauth-clients', label: 'OAuth Clients', to: '/security/oauth-clients', icon: <ShieldRegular />, permission: 'Security.View', enabled: isItemEnabled('oauth-clients') },
      { key: 'security-alerts', label: 'Security Alerts', to: '/security/security-alerts', icon: <WarningRegular />, permission: 'Security.View', enabled: isItemEnabled('security-alerts') },
    ],
  },
  {
    key: 'configuration',
    label: 'Configuration',
    icon: <SettingsRegular />,
    enabled: isGroupEnabled('configuration'),
    items: [
      { key: 'system-settings', label: 'System Settings', to: '/admin/system-settings', icon: <SettingsRegular />, permission: 'Configuration.View', enabled: isItemEnabled('system-settings') },
      { key: 'lookup-categories', label: 'Lookup Categories', to: '/admin/lookup-categories', icon: <BookDatabaseRegular />, permission: 'Configuration.View', enabled: isItemEnabled('lookup-categories') },
      { key: 'lookup-values', label: 'Lookup Values', to: '/admin/lookup-values', icon: <BookDatabaseRegular />, permission: 'Configuration.View', enabled: isItemEnabled('lookup-values') },
      { key: 'number-sequences', label: 'Number Sequences', to: '/configuration/number-sequences', icon: <ClipboardTaskRegular />, permission: 'NumberSequences.View', enabled: isItemEnabled('number-sequences') },
      { key: 'business-rules', label: 'Business Rules', to: '/configuration/business-rules', icon: <BranchRequestRegular />, permission: 'Configuration.View', enabled: isItemEnabled('business-rules') },
      { key: 'workflows', label: 'Workflows', to: '/configuration/workflows', icon: <BranchRequestRegular />, permission: 'Configuration.View', enabled: isItemEnabled('workflows') },
      { key: 'email-templates', label: 'Email Templates', to: '/configuration/email-templates', icon: <DocumentSearchRegular />, permission: 'Configuration.View', enabled: isItemEnabled('email-templates') },
      { key: 'notification-templates', label: 'Notification Templates', to: '/configuration/notification-templates', icon: <DocumentSearchRegular />, permission: 'Configuration.View', enabled: isItemEnabled('notification-templates') },
      { key: 'record-statuses', label: 'Record Statuses', to: '/configuration/record-statuses', icon: <SettingsRegular />, permission: 'Configuration.View', enabled: isItemEnabled('record-statuses') },
      { key: 'custom-fields', label: 'Custom Fields', to: '/configuration/custom-fields', icon: <SettingsRegular />, permission: 'Configuration.View', enabled: isItemEnabled('custom-fields') },
    ],
  },
  {
    key: 'data-management',
    label: 'Data Management',
    icon: <DataPieRegular />,
    enabled: isGroupEnabled('data-management'),
    items: [
      { key: 'imports', label: 'Imports', to: '/data-management/imports', icon: <DocumentSearchRegular />, permission: 'DataManagement.View', enabled: isItemEnabled('imports') },
      { key: 'exports', label: 'Exports', to: '/data-management/exports', icon: <DocumentSearchRegular />, permission: 'DataManagement.View', enabled: isItemEnabled('exports') },
      { key: 'duplicate-detection', label: 'Duplicate Detection', to: '/data-management/duplicate-detection', icon: <WarningRegular />, permission: 'DataManagement.View', enabled: isItemEnabled('duplicate-detection') },
      { key: 'data-quality-rules', label: 'Data Quality Rules', to: '/data-management/data-quality-rules', icon: <SettingsRegular />, permission: 'DataManagement.View', enabled: isItemEnabled('data-quality-rules') },
      { key: 'data-cleanup', label: 'Data Cleanup', to: '/data-management/data-cleanup', icon: <SettingsRegular />, permission: 'DataManagement.View', enabled: isItemEnabled('data-cleanup') },
      { key: 'bulk-operations', label: 'Bulk Operations', to: '/data-management/bulk-operations', icon: <ClipboardTaskRegular />, permission: 'DataManagement.View', enabled: isItemEnabled('bulk-operations') },
    ],
  },
  {
    key: 'audit',
    label: 'Audit',
    icon: <DocumentSearchRegular />,
    enabled: isGroupEnabled('audit'),
    items: [
      { key: 'audit-logs', label: 'Audit Logs', to: '/admin/audit-logs', icon: <DocumentSearchRegular />, permission: 'AuditLogs.View', enabled: isItemEnabled('audit-logs') },
      { key: 'data-changes', label: 'Data Changes', to: '/audit/data-changes', icon: <DataPieRegular />, permission: 'AuditLogs.View', enabled: isItemEnabled('data-changes') },
      { key: 'security-events', label: 'Security Events', to: '/audit/security-events', icon: <WarningRegular />, permission: 'AuditLogs.View', enabled: isItemEnabled('security-events') },
      { key: 'user-activity', label: 'User Activity', to: '/audit/user-activity', icon: <PeopleRegular />, permission: 'AuditLogs.View', enabled: isItemEnabled('user-activity') },
      { key: 'integration-logs', label: 'Integration Logs', to: '/audit/integration-logs', icon: <DocumentSearchRegular />, permission: 'AuditLogs.View', enabled: isItemEnabled('integration-logs') },
    ],
  },
  {
    key: 'integrations',
    label: 'Integrations',
    icon: <AppsListRegular />,
    enabled: isGroupEnabled('integrations'),
    items: [
      { key: 'microsoft-365', label: 'Microsoft 365', to: '/integrations/microsoft-365', icon: <AppsListRegular />, permission: 'Integrations.View', enabled: isItemEnabled('microsoft-365') },
      { key: 'outlook', label: 'Outlook', to: '/integrations/outlook', icon: <AppsListRegular />, permission: 'Integrations.View', enabled: isItemEnabled('outlook') },
      { key: 'exchange', label: 'Exchange', to: '/integrations/exchange', icon: <AppsListRegular />, permission: 'Integrations.View', enabled: isItemEnabled('exchange') },
      { key: 'sharepoint', label: 'SharePoint', to: '/integrations/sharepoint', icon: <AppsListRegular />, permission: 'Integrations.View', enabled: isItemEnabled('sharepoint') },
      { key: 'teams-integration', label: 'Teams', to: '/integrations/teams', icon: <AppsListRegular />, permission: 'Integrations.View', enabled: isItemEnabled('teams-integration') },
      { key: 'azure-storage', label: 'Azure Storage', to: '/integrations/azure-storage', icon: <AppsListRegular />, permission: 'Integrations.View', enabled: isItemEnabled('azure-storage') },
      { key: 'webhooks', label: 'Webhooks', to: '/integrations/webhooks', icon: <BranchRequestRegular />, permission: 'Integrations.View', enabled: isItemEnabled('webhooks') },
      { key: 'api-management', label: 'API Management', to: '/integrations/api-management', icon: <SettingsRegular />, permission: 'Integrations.View', enabled: isItemEnabled('api-management') },
    ],
  },
  {
    key: 'ai-copilot',
    label: 'AI & Copilot',
    icon: <ChartMultipleRegular />,
    enabled: isGroupEnabled('ai-copilot'),
    items: [
      { key: 'ai-dashboard', label: 'AI Dashboard', to: '/ai/dashboard', icon: <ChartMultipleRegular />, permission: 'AI.View', enabled: isItemEnabled('ai-dashboard') },
      { key: 'lead-insights', label: 'Lead Insights', to: '/ai/lead-insights', icon: <ChartMultipleRegular />, permission: 'AI.View', enabled: isItemEnabled('lead-insights') },
      { key: 'opportunity-insights', label: 'Opportunity Insights', to: '/ai/opportunity-insights', icon: <ChartMultipleRegular />, permission: 'AI.View', enabled: isItemEnabled('opportunity-insights') },
      { key: 'customer-insights', label: 'Customer Insights', to: '/ai/customer-insights', icon: <ChartMultipleRegular />, permission: 'AI.View', enabled: isItemEnabled('customer-insights') },
      { key: 'case-recommendations', label: 'Case Recommendations', to: '/ai/case-recommendations', icon: <ChartMultipleRegular />, permission: 'AI.View', enabled: isItemEnabled('case-recommendations') },
      { key: 'email-generation', label: 'Email Generation', to: '/ai/email-generation', icon: <DocumentSearchRegular />, permission: 'AI.View', enabled: isItemEnabled('email-generation') },
      { key: 'meeting-summaries', label: 'Meeting Summaries', to: '/ai/meeting-summaries', icon: <CalendarAgendaRegular />, permission: 'AI.View', enabled: isItemEnabled('meeting-summaries') },
      { key: 'next-best-actions', label: 'Next Best Actions', to: '/ai/next-best-actions', icon: <BranchRequestRegular />, permission: 'AI.View', enabled: isItemEnabled('next-best-actions') },
      { key: 'predictive-analytics', label: 'Predictive Analytics', to: '/ai/predictive-analytics', icon: <ChartMultipleRegular />, permission: 'AI.View', enabled: isItemEnabled('predictive-analytics') },
    ],
  },
  {
    key: 'personal',
    label: 'Personal',
    icon: <PeopleRegular />,
    enabled: isGroupEnabled('personal'),
    items: [
      { key: 'my-profile', label: 'My Profile', to: '/personal/profile', icon: <PeopleRegular />, enabled: isItemEnabled('my-profile') },
      { key: 'my-preferences', label: 'My Preferences', to: '/personal/preferences', icon: <SettingsRegular />, enabled: isItemEnabled('my-preferences') },
      { key: 'my-notifications', label: 'My Notifications', to: '/personal/notifications', icon: <WarningRegular />, enabled: isItemEnabled('my-notifications') },
      { key: 'my-saved-views', label: 'My Saved Views', to: '/personal/saved-views', icon: <DocumentSearchRegular />, enabled: isItemEnabled('my-saved-views') },
    ],
  },
].sort((a, b) => (groupOrder[a.key] ?? Number.MAX_SAFE_INTEGER) - (groupOrder[b.key] ?? Number.MAX_SAFE_INTEGER))
