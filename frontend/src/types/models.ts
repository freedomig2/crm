export type PagedResult<T> = {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}

export type User = {
  id: string
  email: string
  firstName?: string
  lastName?: string
  isEnabled: boolean
  isLocked: boolean
  roles: string[]
  permissions: string[]
}

export type AuthResponse = {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: User
}

export type Role = { id: string; name: string; description?: string }
export type Permission = { id: string; name: string; module: string; action: string }
export type Team = { id: string; name: string; description?: string; ownerUserId?: string; isActive: boolean }
export type Department = { id: string; name: string; description?: string; parentDepartmentId?: string; isActive: boolean }
export type SystemSetting = { id: string; category: string; key: string; value: string; dataType: number; description?: string }
export type AuditLog = { id: string; entityName: string; entityId: string; action: string; oldValues?: string; newValues?: string; userId?: string; ipAddress?: string; userAgent?: string; createdAt: string }
export type LookupCategory = { id: string; name: string; code: string; description?: string; isActive: boolean }
export type LookupValue = { id: string; lookupCategoryId: string; name: string; code: string; sortOrder: number; isDefault: boolean; isActive: boolean }

export type Account = {
  id: string
  accountNumber: string
  name: string
  legalName?: string
  tradingName?: string
  accountTypeId?: string
  industryId?: string
  ownershipTypeId?: string
  customerStatusId?: string
  customerSegmentId?: string
  website?: string
  mainPhone?: string
  alternatePhone?: string
  email?: string
  fax?: string
  taxNumber?: string
  registrationNumber?: string
  annualRevenue?: number
  numberOfEmployees?: number
  description?: string
  parentAccountId?: string
  primaryContactId?: string
  isActive: boolean
  ownerUserId?: string
  ownerTeamId?: string
}

export type Contact = {
  id: string
  contactNumber: string
  accountId: string
  accountName?: string
  contactRoleId?: string
  contactRoleName?: string
  salutationLookupId?: string
  salutationName?: string
  genderLookupId?: string
  genderName?: string
  firstName: string
  middleName?: string
  lastName: string
  preferredName?: string
  fullName: string
  jobTitle?: string
  department?: string
  email?: string
  alternateEmail?: string
  workPhone?: string
  mobilePhone?: string
  homePhone?: string
  fax?: string
  preferredContactMethodId?: string
  preferredContactMethodName?: string
  preferredLanguageId?: string
  preferredLanguageName?: string
  preferredTimeZoneId?: string
  preferredTimeZoneName?: string
  marketingConsent: boolean
  emailOptIn: boolean
  smsOptIn: boolean
  phoneOptIn: boolean
  isPrimaryContact: boolean
  dateOfBirth?: string
  notes?: string
  isActive: boolean
  ownerUserId?: string
  ownerTeamId?: string
}

export type ContactCommunication = {
  id: string
  contactId: string
  contactName?: string
  communicationTypeId?: string
  communicationTypeName?: string
  value: string
  isPrimary: boolean
  isVerified: boolean
  verificationDate?: string
  notes?: string
}

export type ContactInteraction = {
  id: string
  contactId: string
  contactName?: string
  accountId: string
  accountName?: string
  interactionTypeId?: string
  interactionTypeName?: string
  subject: string
  description?: string
  interactionDate: string
  outcome?: string
  followUpDate?: string
  assignedToUserId?: string
  assignedToUserName?: string
}

export type AccountAddress = {
  id: string
  accountId: string
  addressTypeId?: string
  attentionTo?: string
  line1: string
  line2?: string
  landmark?: string
  city?: string
  stateProvince?: string
  postalCode?: string
  countryId?: string
  latitude?: number
  longitude?: number
  isPrimary: boolean
  isBilling: boolean
  isShipping: boolean
  isActive: boolean
}

export type CustomerProfile = {
  id: string
  accountId: string
  creditLimit?: number
  paymentTermsId?: string
  preferredCurrencyId?: string
  preferredLanguageId?: string
  timeZoneId?: string
  riskRatingId?: string
  lifecycleStageId?: string
  customerSince?: string
  lastReviewDate?: string
  nextReviewDate?: string
  churnRiskScore?: number
  satisfactionScore?: number
  notes?: string
}

export type AccountRelationship = {
  id: string
  sourceAccountId: string
  targetAccountId: string
  relationshipTypeId?: string
  startDate?: string
  endDate?: string
  strengthId?: string
  notes?: string
  isActive: boolean
}

export type AccountActivity = {
  id: string
  accountId: string
  contactId?: string
  activityTypeId?: string
  subject: string
  description?: string
  activityDate: string
  dueDate?: string
  priorityId?: string
  statusId?: string
  outcomeId?: string
  assignedToUserId?: string
  relatedEntityType?: string
  relatedEntityId?: string
  isPrivate: boolean
  followUpRequired: boolean
  followUpDate?: string
}
