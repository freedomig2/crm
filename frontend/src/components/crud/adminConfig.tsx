import { statusCell, type DenseColumn } from '../grid/DenseDataGrid'
import type {
  AuditLog,
  Department,
  LookupCategory,
  LookupValue,
  Permission,
  Role,
  SystemSetting,
  Team,
  User,
} from '../../types/models'

export type FieldKind = 'text' | 'textarea' | 'number' | 'checkbox' | 'select'

export type EntityField = {
  key: string
  label: string
  kind: FieldKind
  required?: boolean
  options?: Array<{ value: string; label: string }>
  readOnlyOnEdit?: boolean
}

export type FormState = Record<string, string | number | boolean>

export type EntityConfig<TItem extends { id: string }> = {
  key: string
  title: string
  subtitle: string
  endpoint: string
  permissions: { view: string; create?: string; update?: string; delete?: string }
  listPath: string
  createPath: string
  detailsPath: (id: string) => string
  editPath: (id: string) => string
  columns: DenseColumn<TItem>[]
  fields: EntityField[]
  defaultForm: FormState
  mapItemToForm: (item: TItem) => FormState
  mapCreatePayload: (form: FormState) => unknown
  mapUpdatePayload: (form: FormState, item: TItem) => unknown
  details: (item: TItem) => Array<{ label: string; value: string }>
  readOnly?: boolean
}

export const settingDataTypeOptions = [
  { value: '1', label: 'String' },
  { value: '2', label: 'Number' },
  { value: '3', label: 'Boolean' },
  { value: '4', label: 'Json' },
]

export const usersConfig: EntityConfig<User> = {
  key: 'users',
  title: 'Users',
  subtitle: 'Manage user accounts, lock status, and security assignments.',
  endpoint: 'api/users',
  permissions: { view: 'Users.View', create: 'Users.Create', update: 'Users.Update', delete: 'Users.Delete' },
  listPath: '/admin/users',
  createPath: '/admin/users/create',
  detailsPath: (id) => `/admin/users/${id}`,
  editPath: (id) => `/admin/users/${id}/edit`,
  columns: [
    { key: 'email', label: 'Email', sortable: true },
    { key: 'firstName', label: 'First Name', sortable: true },
    { key: 'lastName', label: 'Last Name', sortable: true },
    { key: 'isEnabled', label: 'Status', sortable: true, render: (row) => statusCell(row.isEnabled ? 'Active' : 'Disabled') },
  ],
  fields: [
    { key: 'email', label: 'Email', kind: 'text', required: true, readOnlyOnEdit: true },
    { key: 'password', label: 'Password', kind: 'text', required: true },
    { key: 'firstName', label: 'First Name', kind: 'text' },
    { key: 'lastName', label: 'Last Name', kind: 'text' },
    { key: 'isEnabled', label: 'Enabled', kind: 'checkbox' },
  ],
  defaultForm: { email: '', password: '', firstName: '', lastName: '', isEnabled: true },
  mapItemToForm: (item) => ({ email: item.email, password: '', firstName: item.firstName ?? '', lastName: item.lastName ?? '', isEnabled: item.isEnabled }),
  mapCreatePayload: (form) => ({
    email: String(form.email ?? ''),
    password: String(form.password ?? ''),
    firstName: String(form.firstName ?? ''),
    lastName: String(form.lastName ?? ''),
    isEnabled: Boolean(form.isEnabled),
  }),
  mapUpdatePayload: (form) => ({
    firstName: String(form.firstName ?? ''),
    lastName: String(form.lastName ?? ''),
    isEnabled: Boolean(form.isEnabled),
  }),
  details: (item) => [
    { label: 'Email', value: item.email },
    { label: 'First Name', value: item.firstName ?? '' },
    { label: 'Last Name', value: item.lastName ?? '' },
    { label: 'Enabled', value: item.isEnabled ? 'Yes' : 'No' },
    { label: 'Locked', value: item.isLocked ? 'Yes' : 'No' },
    { label: 'Roles', value: item.roles.join(', ') },
  ],
}

export const rolesConfig: EntityConfig<Role> = {
  key: 'roles',
  title: 'Roles',
  subtitle: 'Create and maintain role definitions.',
  endpoint: 'api/roles',
  permissions: { view: 'Roles.View', create: 'Roles.Create', update: 'Roles.Update', delete: 'Roles.Delete' },
  listPath: '/admin/roles',
  createPath: '/admin/roles/create',
  detailsPath: (id) => `/admin/roles/${id}`,
  editPath: (id) => `/admin/roles/${id}/edit`,
  columns: [
    { key: 'name', label: 'Name', sortable: true },
    { key: 'description', label: 'Description', sortable: true },
  ],
  fields: [
    { key: 'name', label: 'Name', kind: 'text', required: true },
    { key: 'description', label: 'Description', kind: 'textarea' },
  ],
  defaultForm: { name: '', description: '' },
  mapItemToForm: (item) => ({ name: item.name, description: item.description ?? '' }),
  mapCreatePayload: (form) => ({ name: String(form.name ?? ''), description: String(form.description ?? '') || null }),
  mapUpdatePayload: (form) => ({ name: String(form.name ?? ''), description: String(form.description ?? '') || null }),
  details: (item) => [
    { label: 'Name', value: item.name },
    { label: 'Description', value: item.description ?? '' },
  ],
}

export const permissionsConfig: EntityConfig<Permission> = {
  key: 'permissions',
  title: 'Permissions',
  subtitle: 'Manage module/action permissions.',
  endpoint: 'api/permissions',
  permissions: { view: 'Roles.View', create: 'Roles.Create', update: 'Roles.Update', delete: 'Roles.Delete' },
  listPath: '/admin/permissions',
  createPath: '/admin/permissions/create',
  detailsPath: (id) => `/admin/permissions/${id}`,
  editPath: (id) => `/admin/permissions/${id}/edit`,
  columns: [
    { key: 'name', label: 'Permission', sortable: true },
    { key: 'module', label: 'Module', sortable: true },
    { key: 'action', label: 'Action', sortable: true },
  ],
  fields: [
    { key: 'module', label: 'Module', kind: 'text', required: true },
    { key: 'action', label: 'Action', kind: 'text', required: true },
  ],
  defaultForm: { module: '', action: '' },
  mapItemToForm: (item) => ({ module: item.module, action: item.action }),
  mapCreatePayload: (form) => ({ module: String(form.module ?? ''), action: String(form.action ?? '') }),
  mapUpdatePayload: (form) => ({ module: String(form.module ?? ''), action: String(form.action ?? '') }),
  details: (item) => [
    { label: 'Name', value: item.name },
    { label: 'Module', value: item.module },
    { label: 'Action', value: item.action },
  ],
}

export const teamsConfig: EntityConfig<Team> = {
  key: 'teams',
  title: 'Teams',
  subtitle: 'Configure team ownership and state.',
  endpoint: 'api/teams',
  permissions: { view: 'Teams.View', create: 'Teams.Create', update: 'Teams.Update', delete: 'Teams.Delete' },
  listPath: '/admin/teams',
  createPath: '/admin/teams/create',
  detailsPath: (id) => `/admin/teams/${id}`,
  editPath: (id) => `/admin/teams/${id}/edit`,
  columns: [
    { key: 'name', label: 'Name', sortable: true },
    { key: 'description', label: 'Description', sortable: true },
    { key: 'ownerUserId', label: 'Owner User Id', sortable: true },
    { key: 'isActive', label: 'Status', sortable: true, render: (row) => statusCell(row.isActive ? 'Active' : 'Disabled') },
  ],
  fields: [
    { key: 'name', label: 'Name', kind: 'text', required: true },
    { key: 'description', label: 'Description', kind: 'textarea' },
    { key: 'ownerUserId', label: 'Owner User Id', kind: 'text' },
    { key: 'isActive', label: 'Active', kind: 'checkbox' },
  ],
  defaultForm: { name: '', description: '', ownerUserId: '', isActive: true },
  mapItemToForm: (item) => ({ name: item.name, description: item.description ?? '', ownerUserId: item.ownerUserId ?? '', isActive: item.isActive }),
  mapCreatePayload: (form) => ({
    name: String(form.name ?? ''),
    description: String(form.description ?? '') || null,
    ownerUserId: String(form.ownerUserId ?? '') || null,
    isActive: Boolean(form.isActive),
  }),
  mapUpdatePayload: (form) => ({
    name: String(form.name ?? ''),
    description: String(form.description ?? '') || null,
    ownerUserId: String(form.ownerUserId ?? '') || null,
    isActive: Boolean(form.isActive),
  }),
  details: (item) => [
    { label: 'Name', value: item.name },
    { label: 'Description', value: item.description ?? '' },
    { label: 'Owner User Id', value: item.ownerUserId ?? '' },
    { label: 'Active', value: item.isActive ? 'Yes' : 'No' },
  ],
}

export const departmentsConfig: EntityConfig<Department> = {
  key: 'departments',
  title: 'Departments',
  subtitle: 'Maintain department hierarchy and status.',
  endpoint: 'api/departments',
  permissions: { view: 'Departments.View', create: 'Departments.Create', update: 'Departments.Update', delete: 'Departments.Delete' },
  listPath: '/admin/departments',
  createPath: '/admin/departments/create',
  detailsPath: (id) => `/admin/departments/${id}`,
  editPath: (id) => `/admin/departments/${id}/edit`,
  columns: [
    { key: 'name', label: 'Name', sortable: true },
    { key: 'description', label: 'Description', sortable: true },
    { key: 'parentDepartmentId', label: 'Parent Department Id', sortable: true },
    { key: 'isActive', label: 'Status', sortable: true, render: (row) => statusCell(row.isActive ? 'Active' : 'Disabled') },
  ],
  fields: [
    { key: 'name', label: 'Name', kind: 'text', required: true },
    { key: 'description', label: 'Description', kind: 'textarea' },
    { key: 'parentDepartmentId', label: 'Parent Department Id', kind: 'text' },
    { key: 'isActive', label: 'Active', kind: 'checkbox' },
  ],
  defaultForm: { name: '', description: '', parentDepartmentId: '', isActive: true },
  mapItemToForm: (item) => ({ name: item.name, description: item.description ?? '', parentDepartmentId: item.parentDepartmentId ?? '', isActive: item.isActive }),
  mapCreatePayload: (form) => ({
    name: String(form.name ?? ''),
    description: String(form.description ?? '') || null,
    parentDepartmentId: String(form.parentDepartmentId ?? '') || null,
    isActive: Boolean(form.isActive),
  }),
  mapUpdatePayload: (form) => ({
    name: String(form.name ?? ''),
    description: String(form.description ?? '') || null,
    parentDepartmentId: String(form.parentDepartmentId ?? '') || null,
    isActive: Boolean(form.isActive),
  }),
  details: (item) => [
    { label: 'Name', value: item.name },
    { label: 'Description', value: item.description ?? '' },
    { label: 'Parent Department Id', value: item.parentDepartmentId ?? '' },
    { label: 'Active', value: item.isActive ? 'Yes' : 'No' },
  ],
}

export const settingsConfig: EntityConfig<SystemSetting> = {
  key: 'system-settings',
  title: 'System Settings',
  subtitle: 'Configure global key/value settings.',
  endpoint: 'api/system-settings',
  permissions: { view: 'Settings.View', create: 'Settings.Update', update: 'Settings.Update', delete: 'Settings.Update' },
  listPath: '/admin/system-settings',
  createPath: '/admin/system-settings/create',
  detailsPath: (id) => `/admin/system-settings/${id}`,
  editPath: (id) => `/admin/system-settings/${id}/edit`,
  columns: [
    { key: 'category', label: 'Category', sortable: true },
    { key: 'key', label: 'Key', sortable: true },
    { key: 'value', label: 'Value', sortable: true },
    { key: 'dataType', label: 'Data Type', sortable: true, render: (row) => settingDataTypeOptions.find((x) => Number(x.value) === row.dataType)?.label ?? String(row.dataType) },
  ],
  fields: [
    { key: 'category', label: 'Category', kind: 'text', required: true },
    { key: 'key', label: 'Key', kind: 'text', required: true },
    { key: 'value', label: 'Value', kind: 'textarea', required: true },
    { key: 'dataType', label: 'Data Type', kind: 'select', required: true, options: settingDataTypeOptions },
    { key: 'description', label: 'Description', kind: 'textarea' },
  ],
  defaultForm: { category: '', key: '', value: '', dataType: '1', description: '' },
  mapItemToForm: (item) => ({ category: item.category, key: item.key, value: item.value, dataType: String(item.dataType), description: item.description ?? '' }),
  mapCreatePayload: (form) => ({
    category: String(form.category ?? ''),
    key: String(form.key ?? ''),
    value: String(form.value ?? ''),
    dataType: Number(form.dataType ?? 1),
    description: String(form.description ?? '') || null,
  }),
  mapUpdatePayload: (form) => ({
    category: String(form.category ?? ''),
    key: String(form.key ?? ''),
    value: String(form.value ?? ''),
    dataType: Number(form.dataType ?? 1),
    description: String(form.description ?? '') || null,
  }),
  details: (item) => [
    { label: 'Category', value: item.category },
    { label: 'Key', value: item.key },
    { label: 'Value', value: item.value },
    { label: 'Data Type', value: settingDataTypeOptions.find((x) => Number(x.value) === item.dataType)?.label ?? String(item.dataType) },
    { label: 'Description', value: item.description ?? '' },
  ],
}

export const lookupCategoriesConfig: EntityConfig<LookupCategory> = {
  key: 'lookup-categories',
  title: 'Lookup Categories',
  subtitle: 'Manage reference data categories.',
  endpoint: 'api/lookup-categories',
  permissions: { view: 'ReferenceData.View', create: 'ReferenceData.Create', update: 'ReferenceData.Update', delete: 'ReferenceData.Delete' },
  listPath: '/admin/lookup-categories',
  createPath: '/admin/lookup-categories/create',
  detailsPath: (id) => `/admin/lookup-categories/${id}`,
  editPath: (id) => `/admin/lookup-categories/${id}/edit`,
  columns: [
    { key: 'name', label: 'Name', sortable: true },
    { key: 'code', label: 'Code', sortable: true },
    { key: 'description', label: 'Description', sortable: true },
    { key: 'isActive', label: 'Status', sortable: true, render: (row) => statusCell(row.isActive ? 'Active' : 'Disabled') },
  ],
  fields: [
    { key: 'name', label: 'Name', kind: 'text', required: true },
    { key: 'code', label: 'Code', kind: 'text', required: true },
    { key: 'description', label: 'Description', kind: 'textarea' },
    { key: 'isActive', label: 'Active', kind: 'checkbox' },
  ],
  defaultForm: { name: '', code: '', description: '', isActive: true },
  mapItemToForm: (item) => ({ name: item.name, code: item.code, description: item.description ?? '', isActive: item.isActive }),
  mapCreatePayload: (form) => ({ name: String(form.name ?? ''), code: String(form.code ?? ''), description: String(form.description ?? '') || null, isActive: Boolean(form.isActive) }),
  mapUpdatePayload: (form) => ({ name: String(form.name ?? ''), code: String(form.code ?? ''), description: String(form.description ?? '') || null, isActive: Boolean(form.isActive) }),
  details: (item) => [
    { label: 'Name', value: item.name },
    { label: 'Code', value: item.code },
    { label: 'Description', value: item.description ?? '' },
    { label: 'Active', value: item.isActive ? 'Yes' : 'No' },
  ],
}

export const lookupValuesConfig: EntityConfig<LookupValue> = {
  key: 'lookup-values',
  title: 'Lookup Values',
  subtitle: 'Manage values and sort order in reference categories.',
  endpoint: 'api/lookup-values',
  permissions: { view: 'ReferenceData.View', create: 'ReferenceData.Create', update: 'ReferenceData.Update', delete: 'ReferenceData.Delete' },
  listPath: '/admin/lookup-values',
  createPath: '/admin/lookup-values/create',
  detailsPath: (id) => `/admin/lookup-values/${id}`,
  editPath: (id) => `/admin/lookup-values/${id}/edit`,
  columns: [
    { key: 'name', label: 'Name', sortable: true },
    { key: 'code', label: 'Code', sortable: true },
    { key: 'sortOrder', label: 'Sort', sortable: true },
    { key: 'isDefault', label: 'Default', sortable: true, render: (row) => statusCell(row.isDefault ? 'Default' : 'No') },
    { key: 'isActive', label: 'Status', sortable: true, render: (row) => statusCell(row.isActive ? 'Active' : 'Disabled') },
  ],
  fields: [
    { key: 'lookupCategoryId', label: 'Lookup Category Id', kind: 'text', required: true },
    { key: 'name', label: 'Name', kind: 'text', required: true },
    { key: 'code', label: 'Code', kind: 'text', required: true },
    { key: 'sortOrder', label: 'Sort Order', kind: 'number' },
    { key: 'isDefault', label: 'Default', kind: 'checkbox' },
    { key: 'isActive', label: 'Active', kind: 'checkbox' },
  ],
  defaultForm: { lookupCategoryId: '', name: '', code: '', sortOrder: 0, isDefault: false, isActive: true },
  mapItemToForm: (item) => ({ lookupCategoryId: item.lookupCategoryId, name: item.name, code: item.code, sortOrder: item.sortOrder, isDefault: item.isDefault, isActive: item.isActive }),
  mapCreatePayload: (form) => ({
    lookupCategoryId: String(form.lookupCategoryId ?? ''),
    name: String(form.name ?? ''),
    code: String(form.code ?? ''),
    sortOrder: Number(form.sortOrder ?? 0),
    isDefault: Boolean(form.isDefault),
    isActive: Boolean(form.isActive),
  }),
  mapUpdatePayload: (form) => ({
    lookupCategoryId: String(form.lookupCategoryId ?? ''),
    name: String(form.name ?? ''),
    code: String(form.code ?? ''),
    sortOrder: Number(form.sortOrder ?? 0),
    isDefault: Boolean(form.isDefault),
    isActive: Boolean(form.isActive),
  }),
  details: (item) => [
    { label: 'Lookup Category Id', value: item.lookupCategoryId },
    { label: 'Name', value: item.name },
    { label: 'Code', value: item.code },
    { label: 'Sort Order', value: String(item.sortOrder) },
    { label: 'Default', value: item.isDefault ? 'Yes' : 'No' },
    { label: 'Active', value: item.isActive ? 'Yes' : 'No' },
  ],
}

export const auditLogsConfig: EntityConfig<AuditLog> = {
  key: 'audit-logs',
  title: 'Audit Logs',
  subtitle: 'Review immutable audit history records.',
  endpoint: 'api/audit-logs',
  permissions: { view: 'AuditLogs.View' },
  listPath: '/admin/audit-logs',
  createPath: '/admin/audit-logs/create',
  detailsPath: (id) => `/admin/audit-logs/${id}`,
  editPath: (id) => `/admin/audit-logs/${id}/edit`,
  columns: [
    { key: 'entityName', label: 'Entity', sortable: true },
    { key: 'entityId', label: 'Entity Id', sortable: true },
    { key: 'action', label: 'Action', sortable: true },
    { key: 'createdAt', label: 'Created At', sortable: true },
  ],
  fields: [],
  defaultForm: {},
  mapItemToForm: () => ({}),
  mapCreatePayload: () => ({}),
  mapUpdatePayload: () => ({}),
  details: (item) => [
    { label: 'Entity', value: item.entityName },
    { label: 'Entity Id', value: item.entityId },
    { label: 'Action', value: item.action },
    { label: 'Created At', value: item.createdAt },
    { label: 'User Id', value: item.userId ?? '' },
    { label: 'IP Address', value: item.ipAddress ?? '' },
    { label: 'User Agent', value: item.userAgent ?? '' },
    { label: 'Old Values', value: item.oldValues ?? '' },
    { label: 'New Values', value: item.newValues ?? '' },
  ],
  readOnly: true,
}
