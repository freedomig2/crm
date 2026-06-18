import {
  BookDatabaseRegular,
  BuildingBankRegular,
  BuildingRegular,
  CalendarAgendaRegular,
  ClipboardTaskRegular,
  ContactCardRegular,
  DataPieRegular,
  KeyRegular,
  PeopleRegular,
  PersonCallRegular,
  PersonRegular,
  SettingsRegular,
  ShieldRegular,
} from '@fluentui/react-icons'
import type { EntityField } from '../crud/adminConfig'
import { isForeignKeyField } from './referenceData'

export type SectionDef = {
  key: string
  title: string
  icon?: React.ReactNode
  fields: string[]
}

const words = (source: string) =>
  source
    .replace(/([a-z])([A-Z])/g, '$1 $2')
    .replace(/Id$/i, '')
    .replace(/Lookup/gi, '')
    .replace(/\s+/g, ' ')
    .trim()

const friendlyOverrides: Record<string, string> = {
  ownerUserId: 'Owner',
  ownerTeamId: 'Owner Team',
  lookupCategoryId: 'Lookup Category',
  parentDepartmentId: 'Parent Department',
  parentAccountId: 'Parent Account',
  primaryContactId: 'Primary Contact',
  accountTypeId: 'Account Type',
  industryId: 'Industry',
  ownershipTypeId: 'Ownership Type',
  customerStatusId: 'Customer Status',
  customerSegmentId: 'Customer Segment',
  contactRoleId: 'Contact Role',
  salutationId: 'Salutation',
  preferredCommunicationId: 'Preferred Communication',
  addressTypeId: 'Address Type',
  countryId: 'Country',
  paymentTermsId: 'Payment Terms',
  preferredCurrencyId: 'Preferred Currency',
  preferredLanguageId: 'Preferred Language',
  timeZoneId: 'Time Zone',
  riskRatingId: 'Risk Rating',
  lifecycleStageId: 'Lifecycle Stage',
  relationshipTypeId: 'Relationship Type',
  strengthId: 'Relationship Strength',
  sourceAccountId: 'Source Account',
  targetAccountId: 'Target Account',
  activityTypeId: 'Activity Type',
  priorityId: 'Priority',
  statusId: 'Status',
  outcomeId: 'Outcome',
  assignedToUserId: 'Assigned To',
  relatedEntityId: 'Related Record',
  relatedEntityType: 'Related Entity Type',
  isEnabled: 'Enabled',
  isActive: 'Active',
  isDefault: 'Default',
}

export const friendlyLabel = (field: Pick<EntityField, 'key' | 'label'>): string => {
  if (friendlyOverrides[field.key]) {
    return friendlyOverrides[field.key]
  }

  if (isForeignKeyField(field.key)) {
    return words(field.key)
  }

  if (/lookup id/i.test(field.label)) {
    return words(field.key)
  }

  return field.label
    .replace(/Lookup Id/gi, '')
    .replace(/\(ISO\)/gi, '')
    .replace(/\s+/g, ' ')
    .trim()
}

const fallbackSections = (fields: EntityField[]): SectionDef[] => {
  const security = fields.filter((field) => ['isEnabled', 'isActive', 'isDefault', 'dataType'].includes(field.key)).map((field) => field.key)
  const notes = fields.filter((field) => ['description', 'notes'].includes(field.key)).map((field) => field.key)
  const main = fields.filter((field) => !security.includes(field.key) && !notes.includes(field.key)).map((field) => field.key)

  return [
    { key: 'general', title: 'General Information', icon: <BookDatabaseRegular />, fields: main },
    { key: 'notes', title: 'Description', icon: <ClipboardTaskRegular />, fields: notes },
    { key: 'security', title: 'Security', icon: <ShieldRegular />, fields: security },
  ].filter((section) => section.fields.length > 0)
}

export const sectionMap = (configKey: string, fields: EntityField[]): SectionDef[] => {
  if (configKey === 'accounts') {
    return [
      { key: 'general', title: 'General Information', icon: <BuildingBankRegular />, fields: ['accountNumber', 'name', 'legalName', 'tradingName'] },
      { key: 'classification', title: 'Classification', icon: <DataPieRegular />, fields: ['accountTypeId', 'industryId', 'customerSegmentId', 'customerStatusId', 'ownershipTypeId'] },
      { key: 'contact', title: 'Contact Information', icon: <ContactCardRegular />, fields: ['email', 'website', 'mainPhone', 'alternatePhone', 'fax'] },
      { key: 'registration', title: 'Registration', icon: <BookDatabaseRegular />, fields: ['registrationNumber', 'taxNumber'] },
      { key: 'ownership', title: 'Ownership', icon: <PersonRegular />, fields: ['ownerUserId', 'ownerTeamId', 'parentAccountId', 'primaryContactId'] },
      { key: 'financial', title: 'Financial Information', icon: <DataPieRegular />, fields: ['annualRevenue', 'numberOfEmployees'] },
      { key: 'description', title: 'Description', icon: <ClipboardTaskRegular />, fields: ['description'] },
    ].map((section) => ({ ...section, fields: section.fields.filter((fieldKey) => fields.some((field) => field.key === fieldKey)) }))
      .filter((section) => section.fields.length > 0)
  }

  if (configKey === 'contacts') {
    return [
      { key: 'general', title: 'General Information', icon: <PersonRegular />, fields: ['accountId', 'firstName', 'middleName', 'lastName', 'salutationId', 'contactRoleId', 'isPrimaryContact'] },
      { key: 'professional', title: 'Professional', icon: <BookDatabaseRegular />, fields: ['jobTitle', 'departmentName'] },
      { key: 'contact', title: 'Contact Information', icon: <ContactCardRegular />, fields: ['email', 'mobilePhone', 'workPhone', 'extension', 'preferredCommunicationId'] },
      { key: 'ownership', title: 'Ownership', icon: <PersonRegular />, fields: ['ownerUserId', 'ownerTeamId'] },
      { key: 'notes', title: 'Description', icon: <ClipboardTaskRegular />, fields: ['dateOfBirth', 'notes', 'isActive'] },
    ].map((section) => ({ ...section, fields: section.fields.filter((fieldKey) => fields.some((field) => field.key === fieldKey)) }))
      .filter((section) => section.fields.length > 0)
  }

  if (configKey === 'users') {
    return [
      { key: 'general', title: 'General Information', icon: <PersonRegular />, fields: ['email', 'password', 'firstName', 'lastName'] },
      { key: 'security', title: 'Security', icon: <ShieldRegular />, fields: ['isEnabled'] },
    ].map((section) => ({ ...section, fields: section.fields.filter((fieldKey) => fields.some((field) => field.key === fieldKey)) }))
      .filter((section) => section.fields.length > 0)
  }

  if (configKey === 'account-addresses') {
    return [
      { key: 'location', title: 'Address Information', icon: <BookDatabaseRegular />, fields: ['accountId', 'addressTypeId', 'attentionTo', 'line1', 'line2', 'landmark', 'city', 'stateProvince', 'postalCode', 'countryId'] },
      { key: 'geo', title: 'Geo Location', icon: <DataPieRegular />, fields: ['latitude', 'longitude'] },
      { key: 'flags', title: 'Flags', icon: <ShieldRegular />, fields: ['isPrimary', 'isBilling', 'isShipping', 'isActive'] },
    ].map((section) => ({ ...section, fields: section.fields.filter((fieldKey) => fields.some((field) => field.key === fieldKey)) }))
      .filter((section) => section.fields.length > 0)
  }

  if (configKey === 'customer-profiles') {
    return [
      { key: 'general', title: 'General Information', icon: <BookDatabaseRegular />, fields: ['accountId', 'customerSince', 'lastReviewDate', 'nextReviewDate'] },
      { key: 'financial', title: 'Financial Information', icon: <DataPieRegular />, fields: ['creditLimit', 'paymentTermsId', 'preferredCurrencyId'] },
      { key: 'preferences', title: 'Preferences', icon: <ClipboardTaskRegular />, fields: ['preferredLanguageId', 'timeZoneId'] },
      { key: 'risk', title: 'Risk and Lifecycle', icon: <ShieldRegular />, fields: ['riskRatingId', 'lifecycleStageId', 'churnRiskScore', 'satisfactionScore'] },
      { key: 'notes', title: 'Description', icon: <ClipboardTaskRegular />, fields: ['notes'] },
    ].map((section) => ({ ...section, fields: section.fields.filter((fieldKey) => fields.some((field) => field.key === fieldKey)) }))
      .filter((section) => section.fields.length > 0)
  }

  if (configKey === 'account-relationships') {
    return [
      { key: 'general', title: 'General Information', icon: <BookDatabaseRegular />, fields: ['sourceAccountId', 'targetAccountId', 'relationshipTypeId', 'strengthId'] },
      { key: 'timeline', title: 'Timeline', icon: <CalendarAgendaRegular />, fields: ['startDate', 'endDate'] },
      { key: 'description', title: 'Description', icon: <ClipboardTaskRegular />, fields: ['notes', 'isActive'] },
    ].map((section) => ({ ...section, fields: section.fields.filter((fieldKey) => fields.some((field) => field.key === fieldKey)) }))
      .filter((section) => section.fields.length > 0)
  }

  if (configKey === 'account-activities') {
    return [
      { key: 'general', title: 'General Information', icon: <CalendarAgendaRegular />, fields: ['accountId', 'contactId', 'activityTypeId', 'subject', 'description'] },
      { key: 'timeline', title: 'Schedule', icon: <CalendarAgendaRegular />, fields: ['activityDate', 'dueDate', 'followUpRequired', 'followUpDate'] },
      { key: 'classification', title: 'Classification', icon: <DataPieRegular />, fields: ['priorityId', 'statusId', 'outcomeId'] },
      { key: 'assignment', title: 'Assignment and Related', icon: <PersonRegular />, fields: ['assignedToUserId', 'relatedEntityType', 'relatedEntityId', 'isPrivate'] },
    ].map((section) => ({ ...section, fields: section.fields.filter((fieldKey) => fields.some((field) => field.key === fieldKey)) }))
      .filter((section) => section.fields.length > 0)
  }

  return fallbackSections(fields)
}

export const tabsForEntity = (configKey: string): Array<{ key: string; label: string }> => {
  if (configKey === 'accounts') {
    return [
      { key: 'general', label: 'General' },
      { key: 'contacts', label: 'Contacts' },
      { key: 'addresses', label: 'Addresses' },
      { key: 'customer-profile', label: 'Customer Profile' },
      { key: 'activities', label: 'Activities' },
      { key: 'relationships', label: 'Relationships' },
      { key: 'hierarchy', label: 'Hierarchy' },
      { key: 'audit-history', label: 'Audit History' },
    ]
  }

  if (configKey === 'contacts') {
    return [
      { key: 'general', label: 'General' },
      { key: 'activities', label: 'Activities' },
      { key: 'notes', label: 'Notes' },
      { key: 'audit-history', label: 'Audit History' },
    ]
  }

  if (configKey === 'users') {
    return [
      { key: 'general', label: 'General' },
      { key: 'security', label: 'Security' },
      { key: 'roles', label: 'Roles' },
      { key: 'teams', label: 'Teams' },
      { key: 'audit-history', label: 'Audit History' },
    ]
  }

  return [
    { key: 'general', label: 'General' },
    { key: 'audit-history', label: 'Audit History' },
  ]
}

export const getPageTitle = (baseTitle: string, mode: 'create' | 'edit' | 'details') => {
  const singular = baseTitle.endsWith('s') ? baseTitle.slice(0, -1) : baseTitle
  if (mode === 'create') {
    return `Create ${singular}`
  }
  if (mode === 'edit') {
    return `Edit ${singular}`
  }
  return `${singular} Details`
}

export const getEntityIcon = (configKey: string) => {
  if (configKey === 'users') return <PeopleRegular />
  if (configKey === 'roles') return <ShieldRegular />
  if (configKey === 'permissions') return <KeyRegular />
  if (configKey === 'teams' || configKey === 'departments') return <BuildingRegular />
  if (configKey === 'accounts' || configKey === 'account-addresses' || configKey === 'customer-profiles' || configKey === 'account-relationships' || configKey === 'account-activities') return <BuildingBankRegular />
  if (configKey === 'contacts') return <PersonCallRegular />
  if (configKey === 'lookup-categories' || configKey === 'lookup-values') return <BookDatabaseRegular />
  if (configKey === 'system-settings') return <SettingsRegular />
  return <BookDatabaseRegular />
}

export const isLookupLikeField = (fieldKey: string) => isForeignKeyField(fieldKey)
export const isDateLikeField = (fieldKey: string) => fieldKey.toLowerCase().includes('date')
export const isMultilineField = (fieldKey: string) => ['description', 'notes', 'oldValues', 'newValues', 'value'].includes(fieldKey)

export const wideField = (fieldKey: string): boolean => ['description', 'notes', 'oldValues', 'newValues', 'value'].includes(fieldKey)

export const statusFromItem = (item: Record<string, unknown> | null): string | undefined => {
  if (!item) {
    return undefined
  }

  if (typeof item.isActive === 'boolean') {
    return item.isActive ? 'Active' : 'Inactive'
  }

  if (typeof item.isEnabled === 'boolean') {
    return item.isEnabled ? 'Active' : 'Inactive'
  }

  return undefined
}
