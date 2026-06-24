import type { DenseColumn } from './DenseDataGrid'
import { statusCell } from './DenseDataGrid'

export type EntityColumnDefinition = {
  key: string
  label: string
  field: string
  sortable?: boolean
  filterable?: boolean
  defaultVisible?: boolean
  width?: number
  type?: 'text' | 'number' | 'date' | 'money' | 'lookup' | 'boolean' | 'status' | 'actions'
  required?: boolean
}

export type EntityFilterDefinition =
  | { type: 'lookup'; key: string; label: string; lookupFieldKey: string }
  | { type: 'boolean'; key: string; label: string; trueLabel?: string; falseLabel?: string }
  | { type: 'text'; key: string; label: string }
  | { type: 'dateRange'; fromKey: string; toKey: string; fromLabel: string; toLabel: string }

const entityColumns: Record<string, EntityColumnDefinition[]> = {
  accounts: [
    { key: 'accountNumber', label: 'Account #', field: 'accountNumber', sortable: true, defaultVisible: true },
    { key: 'name', label: 'Name', field: 'name', sortable: true, defaultVisible: true },
    { key: 'accountTypeName', label: 'Account Type', field: 'accountTypeName', sortable: true, defaultVisible: true, type: 'lookup' },
    { key: 'industryName', label: 'Industry', field: 'industryName', sortable: true, defaultVisible: true, type: 'lookup' },
    { key: 'email', label: 'Email', field: 'email', sortable: true, defaultVisible: true },
    { key: 'mainPhone', label: 'Main Phone', field: 'mainPhone', sortable: true, defaultVisible: true },
    { key: 'customerStatusName', label: 'Status', field: 'customerStatusName', sortable: true, defaultVisible: true, type: 'status' },
    { key: 'legalName', label: 'Legal Name', field: 'legalName', sortable: true, defaultVisible: false },
    { key: 'tradingName', label: 'Trading Name', field: 'tradingName', sortable: true, defaultVisible: false },
    { key: 'customerSegmentName', label: 'Segment', field: 'customerSegmentName', sortable: true, defaultVisible: false, type: 'lookup' },
    { key: 'ownerUserName', label: 'Owner', field: 'ownerUserName', sortable: true, defaultVisible: false, type: 'lookup' },
    { key: 'createdAt', label: 'Created At', field: 'createdAt', sortable: true, defaultVisible: false, type: 'date' },
    { key: 'updatedAt', label: 'Updated At', field: 'updatedAt', sortable: true, defaultVisible: false, type: 'date' },
  ],
  users: [
    { key: 'email', label: 'Email', field: 'email', sortable: true, defaultVisible: true, required: true },
    { key: 'firstName', label: 'First Name', field: 'firstName', sortable: true, defaultVisible: true },
    { key: 'lastName', label: 'Last Name', field: 'lastName', sortable: true, defaultVisible: true },
    { key: 'isEnabled', label: 'Status', field: 'isEnabled', sortable: true, defaultVisible: true, type: 'boolean' },
    { key: 'isLocked', label: 'Locked', field: 'isLocked', sortable: true, defaultVisible: false, type: 'boolean' },
  ],
  roles: [
    { key: 'name', label: 'Name', field: 'name', sortable: true, defaultVisible: true, required: true },
    { key: 'description', label: 'Description', field: 'description', sortable: true, defaultVisible: true },
  ],
  'audit-logs': [
    { key: 'entityName', label: 'Entity', field: 'entityName', sortable: true, defaultVisible: true },
    { key: 'action', label: 'Action', field: 'action', sortable: true, defaultVisible: true, type: 'status' },
    { key: 'createdAt', label: 'Created At', field: 'createdAt', sortable: true, defaultVisible: true, type: 'date' },
    { key: 'userId', label: 'User', field: 'userId', sortable: true, defaultVisible: false },
    { key: 'ipAddress', label: 'IP Address', field: 'ipAddress', sortable: true, defaultVisible: false },
    { key: 'entityId', label: 'Entity Id', field: 'entityId', sortable: true, defaultVisible: false },
  ],
  products: [
    { key: 'productCode', label: 'Product #', field: 'productCode', sortable: true, defaultVisible: true },
    { key: 'name', label: 'Name', field: 'name', sortable: true, defaultVisible: true },
    { key: 'productCategoryName', label: 'Category', field: 'productCategoryName', sortable: true, defaultVisible: true, type: 'lookup' },
    { key: 'productTypeName', label: 'Type', field: 'productTypeName', sortable: true, defaultVisible: true, type: 'lookup' },
    { key: 'unitOfMeasureName', label: 'UOM', field: 'unitOfMeasureName', sortable: true, defaultVisible: true, type: 'lookup' },
    { key: 'standardPrice', label: 'Price', field: 'standardPrice', sortable: true, defaultVisible: true, type: 'money' },
    { key: 'productStatusName', label: 'Status', field: 'productStatusName', sortable: true, defaultVisible: true, type: 'status' },
    { key: 'isActive', label: 'Active', field: 'isActive', sortable: true, defaultVisible: false, type: 'boolean' },
    { key: 'createdAt', label: 'Created At', field: 'createdAt', sortable: true, defaultVisible: false, type: 'date' },
    { key: 'updatedAt', label: 'Updated At', field: 'updatedAt', sortable: true, defaultVisible: false, type: 'date' },
  ],
  quotes: [
    { key: 'quoteNumber', label: 'Quote #', field: 'quoteNumber', sortable: true, defaultVisible: true },
    { key: 'accountName', label: 'Account', field: 'accountName', sortable: true, defaultVisible: true, type: 'lookup' },
    { key: 'quoteStatusName', label: 'Quote Status', field: 'quoteStatusName', sortable: true, defaultVisible: true, type: 'status' },
    { key: 'approvalStatusName', label: 'Approval Status', field: 'approvalStatusName', sortable: true, defaultVisible: true, type: 'status' },
    { key: 'currencyName', label: 'Currency', field: 'currencyName', sortable: true, defaultVisible: true, type: 'lookup' },
    { key: 'totalAmount', label: 'Total', field: 'totalAmount', sortable: true, defaultVisible: true, type: 'money' },
    { key: 'validFrom', label: 'Valid From', field: 'validFrom', sortable: true, defaultVisible: false, type: 'date' },
    { key: 'validTo', label: 'Valid To', field: 'validTo', sortable: true, defaultVisible: true, type: 'date' },
    { key: 'createdAt', label: 'Created At', field: 'createdAt', sortable: true, defaultVisible: false, type: 'date' },
  ],
  orders: [
    { key: 'orderNumber', label: 'Order #', field: 'orderNumber', sortable: true, defaultVisible: true },
    { key: 'accountName', label: 'Account', field: 'accountName', sortable: true, defaultVisible: true, type: 'lookup' },
    { key: 'orderStatusName', label: 'Order Status', field: 'orderStatusName', sortable: true, defaultVisible: true, type: 'status' },
    { key: 'deliveryStatusName', label: 'Fulfilment', field: 'deliveryStatusName', sortable: true, defaultVisible: true, type: 'status' },
    { key: 'billingStatusName', label: 'Billing', field: 'billingStatusName', sortable: true, defaultVisible: true, type: 'status' },
    { key: 'orderDate', label: 'Order Date', field: 'orderDate', sortable: true, defaultVisible: true, type: 'date' },
    { key: 'totalAmount', label: 'Total', field: 'totalAmount', sortable: true, defaultVisible: true, type: 'money' },
    { key: 'createdAt', label: 'Created At', field: 'createdAt', sortable: true, defaultVisible: false, type: 'date' },
  ],
  invoices: [
    { key: 'invoiceNumber', label: 'Invoice #', field: 'invoiceNumber', sortable: true, defaultVisible: true },
    { key: 'accountName', label: 'Account', field: 'accountName', sortable: true, defaultVisible: true, type: 'lookup' },
    { key: 'invoiceStatusName', label: 'Invoice Status', field: 'invoiceStatusName', sortable: true, defaultVisible: true, type: 'status' },
    { key: 'paymentStatusName', label: 'Payment Status', field: 'paymentStatusName', sortable: true, defaultVisible: true, type: 'status' },
    { key: 'dueDate', label: 'Due Date', field: 'dueDate', sortable: true, defaultVisible: true, type: 'date' },
    { key: 'totalAmount', label: 'Total', field: 'totalAmount', sortable: true, defaultVisible: true, type: 'money' },
    { key: 'paidAmount', label: 'Paid', field: 'paidAmount', sortable: true, defaultVisible: true, type: 'money' },
    { key: 'createdAt', label: 'Created At', field: 'createdAt', sortable: true, defaultVisible: false, type: 'date' },
  ],
  cases: [
    { key: 'caseNumber', label: 'Case #', field: 'caseNumber', sortable: true, defaultVisible: true },
    { key: 'subject', label: 'Subject', field: 'subject', sortable: true, defaultVisible: true },
    { key: 'accountName', label: 'Account', field: 'accountName', sortable: true, defaultVisible: true, type: 'lookup' },
    { key: 'categoryName', label: 'Category', field: 'categoryName', sortable: true, defaultVisible: true, type: 'lookup' },
    { key: 'caseStatusName', label: 'Status', field: 'caseStatusName', sortable: true, defaultVisible: true, type: 'status' },
    { key: 'priorityName', label: 'Priority', field: 'priorityName', sortable: true, defaultVisible: true, type: 'status' },
    { key: 'assignedToUserName', label: 'Assigned To', field: 'assignedToUserName', sortable: true, defaultVisible: true, type: 'lookup' },
    { key: 'dueAt', label: 'Due', field: 'dueAt', sortable: true, defaultVisible: true, type: 'date' },
    { key: 'isActive', label: 'Active', field: 'isActive', sortable: true, defaultVisible: false, type: 'boolean' },
    { key: 'createdAt', label: 'Created At', field: 'createdAt', sortable: true, defaultVisible: false, type: 'date' },
  ],
  documents: [
    { key: 'documentNumber', label: 'Document #', field: 'documentNumber', sortable: true, defaultVisible: true },
    { key: 'title', label: 'Title', field: 'title', sortable: true, defaultVisible: true },
    { key: 'documentCategoryName', label: 'Category', field: 'documentCategoryName', sortable: true, defaultVisible: true, type: 'lookup' },
    { key: 'documentStatusName', label: 'Status', field: 'documentStatusName', sortable: true, defaultVisible: true, type: 'status' },
    { key: 'accountName', label: 'Account', field: 'accountName', sortable: true, defaultVisible: false, type: 'lookup' },
    { key: 'currentVersion', label: 'Version', field: 'currentVersion', sortable: true, defaultVisible: true, type: 'number' },
    { key: 'isConfidential', label: 'Confidential', field: 'isConfidential', sortable: true, defaultVisible: true, type: 'boolean' },
    { key: 'isActive', label: 'Active', field: 'isActive', sortable: true, defaultVisible: true, type: 'boolean' },
    { key: 'createdAt', label: 'Created At', field: 'createdAt', sortable: true, defaultVisible: false, type: 'date' },
  ],
}

const entityFilters: Record<string, EntityFilterDefinition[]> = {
  accounts: [
    { type: 'lookup', key: 'accountTypeId', label: 'Account Type', lookupFieldKey: 'accountTypeId' },
    { type: 'lookup', key: 'industryId', label: 'Industry', lookupFieldKey: 'industryId' },
    { type: 'lookup', key: 'customerStatusId', label: 'Customer Status', lookupFieldKey: 'customerStatusId' },
    { type: 'lookup', key: 'customerSegmentId', label: 'Customer Segment', lookupFieldKey: 'customerSegmentId' },
    { type: 'lookup', key: 'ownerUserId', label: 'Owner', lookupFieldKey: 'ownerUserId' },
    { type: 'boolean', key: 'isActive', label: 'Active', trueLabel: 'Active', falseLabel: 'Inactive' },
    { type: 'dateRange', fromKey: 'createdFrom', toKey: 'createdTo', fromLabel: 'Created From', toLabel: 'Created To' },
  ],
  users: [
    { type: 'boolean', key: 'isEnabled', label: 'Enabled', trueLabel: 'Enabled', falseLabel: 'Disabled' },
    { type: 'boolean', key: 'isLocked', label: 'Locked', trueLabel: 'Locked', falseLabel: 'Unlocked' },
  ],
  roles: [{ type: 'text', key: 'name', label: 'Role Name' }],
  'audit-logs': [
    { type: 'text', key: 'entityName', label: 'Entity' },
    { type: 'text', key: 'action', label: 'Action' },
    { type: 'dateRange', fromKey: 'createdFrom', toKey: 'createdTo', fromLabel: 'Created From', toLabel: 'Created To' },
  ],
  products: [
    { type: 'lookup', key: 'productCategoryId', label: 'Product Category', lookupFieldKey: 'productCategoryId' },
    { type: 'lookup', key: 'productTypeId', label: 'Product Type', lookupFieldKey: 'productTypeId' },
    { type: 'lookup', key: 'productStatusId', label: 'Product Status', lookupFieldKey: 'productStatusId' },
    { type: 'lookup', key: 'unitOfMeasureId', label: 'Unit Of Measure', lookupFieldKey: 'unitOfMeasureId' },
    { type: 'boolean', key: 'isActive', label: 'Active', trueLabel: 'Active', falseLabel: 'Inactive' },
  ],
  quotes: [
    { type: 'lookup', key: 'accountId', label: 'Account', lookupFieldKey: 'accountId' },
    { type: 'lookup', key: 'quoteStatusId', label: 'Quote Status', lookupFieldKey: 'quoteStatusId' },
    { type: 'lookup', key: 'approvalStatusId', label: 'Approval Status', lookupFieldKey: 'approvalStatusId' },
    { type: 'dateRange', fromKey: 'validFrom', toKey: 'validTo', fromLabel: 'Valid From', toLabel: 'Valid To' },
  ],
  orders: [
    { type: 'lookup', key: 'accountId', label: 'Account', lookupFieldKey: 'accountId' },
    { type: 'lookup', key: 'orderStatusId', label: 'Order Status', lookupFieldKey: 'orderStatusId' },
    { type: 'lookup', key: 'deliveryStatusId', label: 'Fulfilment Status', lookupFieldKey: 'deliveryStatusId' },
    { type: 'dateRange', fromKey: 'orderDateFrom', toKey: 'orderDateTo', fromLabel: 'Order Date From', toLabel: 'Order Date To' },
  ],
  invoices: [
    { type: 'lookup', key: 'accountId', label: 'Account', lookupFieldKey: 'accountId' },
    { type: 'lookup', key: 'invoiceStatusId', label: 'Invoice Status', lookupFieldKey: 'invoiceStatusId' },
    { type: 'lookup', key: 'paymentStatusId', label: 'Payment Status', lookupFieldKey: 'paymentStatusId' },
    { type: 'dateRange', fromKey: 'dueDateFrom', toKey: 'dueDateTo', fromLabel: 'Due Date From', toLabel: 'Due Date To' },
  ],
  cases: [
    { type: 'lookup', key: 'accountId', label: 'Account', lookupFieldKey: 'accountId' },
    { type: 'lookup', key: 'categoryId', label: 'Case Category', lookupFieldKey: 'categoryId' },
    { type: 'lookup', key: 'caseStatusId', label: 'Case Status', lookupFieldKey: 'caseStatusId' },
    { type: 'lookup', key: 'priorityId', label: 'Priority', lookupFieldKey: 'priorityId' },
    { type: 'lookup', key: 'assignedToUserId', label: 'Assigned User', lookupFieldKey: 'assignedToUserId' },
    { type: 'lookup', key: 'slaId', label: 'SLA', lookupFieldKey: 'slaId' },
    { type: 'boolean', key: 'isActive', label: 'Active', trueLabel: 'Active', falseLabel: 'Inactive' },
  ],
  documents: [
    { type: 'lookup', key: 'documentCategoryId', label: 'Document Category', lookupFieldKey: 'documentCategoryId' },
    { type: 'lookup', key: 'documentStatusId', label: 'Document Status', lookupFieldKey: 'documentStatusId' },
    { type: 'lookup', key: 'accountId', label: 'Account', lookupFieldKey: 'accountId' },
    { type: 'boolean', key: 'isConfidential', label: 'Confidential', trueLabel: 'Yes', falseLabel: 'No' },
    { type: 'boolean', key: 'isActive', label: 'Active', trueLabel: 'Active', falseLabel: 'Inactive' },
    { type: 'dateRange', fromKey: 'createdFrom', toKey: 'createdTo', fromLabel: 'Created From', toLabel: 'Created To' },
  ],
}

const formatDate = (value: unknown) => {
  if (!value) {
    return '-'
  }

  const parsed = new Date(String(value))
  if (Number.isNaN(parsed.getTime())) {
    return String(value)
  }

  return parsed.toLocaleDateString()
}

const formatMoney = (value: unknown) => {
  const numeric = Number(value ?? 0)
  if (Number.isNaN(numeric)) {
    return '-'
  }

  return numeric.toLocaleString(undefined, { style: 'currency', currency: 'USD', minimumFractionDigits: 2 })
}

const definitionToDenseColumn = <T extends { id: string }>(definition: EntityColumnDefinition): DenseColumn<T> => ({
  key: definition.field as keyof T,
  label: definition.label,
  sortable: definition.sortable,
  render:
    definition.type === 'status'
      ? (row) => statusCell(String((row as Record<string, unknown>)[definition.field] ?? 'Not set'))
      : definition.type === 'boolean'
        ? (row) => statusCell((row as Record<string, unknown>)[definition.field] ? 'Yes' : 'No')
        : definition.type === 'date'
          ? (row) => formatDate((row as Record<string, unknown>)[definition.field])
          : definition.type === 'money'
            ? (row) => formatMoney((row as Record<string, unknown>)[definition.field])
            : undefined,
})

export const getEntityFilters = (entityKey?: string): EntityFilterDefinition[] => {
  if (!entityKey) {
    return []
  }

  return entityFilters[entityKey] ?? []
}

export const getEntityColumnConfig = <T extends { id: string }>(
  entityKey: string | undefined,
  fallbackColumns: DenseColumn<T>[],
) => {
  if (!entityKey || !entityColumns[entityKey]) {
    return {
      availableColumns: fallbackColumns,
      defaultVisibleColumnKeys: fallbackColumns.map((column) => String(column.key)),
      requiredColumnKeys: [] as string[],
    }
  }

  const definitions = entityColumns[entityKey]

  return {
    availableColumns: definitions.map((definition) => definitionToDenseColumn<T>(definition)),
    defaultVisibleColumnKeys: definitions
      .filter((definition) => definition.defaultVisible !== false)
      .map((definition) => definition.field),
    requiredColumnKeys: definitions
      .filter((definition) => definition.required)
      .map((definition) => definition.field),
  }
}
