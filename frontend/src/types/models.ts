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
  createdAt?: string
  updatedAt?: string
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

export type NumberSequence = {
  id: string
  entityName: string
  sequenceCode: string
  prefix: string
  suffix?: string
  separator: string
  currentNumber: number
  nextNumber: number
  minimumDigits: number
  resetFrequencyId?: string
  resetFrequencyName?: string
  lastResetDate?: string
  includeYear: boolean
  includeMonth: boolean
  includeDay: boolean
  formatPreview?: string
  description?: string
  isActive: boolean
  createdAt: string
  updatedAt?: string
}

export type NumberSequencePreview = {
  preview: string
}

export type Workflow = {
  id: string
  name: string
  code: string
  description?: string
  workflowTypeId: string
  workflowTypeName: string
  workflowTypeCode: string
  workflowStatusId: string
  workflowStatusName: string
  workflowStatusCode: string
  triggerEntity: string
  triggerEvent: string
  isDefault: boolean
  isSystem: boolean
  version: number
  sortOrder: number
  isActive: boolean
  createdAt: string
  updatedAt?: string
}

export type ReportingDashboardSummary = {
  totalLeads: number
  openOpportunities: number
  totalAccounts: number
  openCases: number
  openActivities: number
  pipelineValue: number
  weightedPipelineValue: number
  revenueThisMonth: number
  winRate: number
}

export type ReportLibraryItem = {
  key: string
  name: string
  category: string
  description: string
  route: string
  isImplemented: boolean
  lastUpdatedAt: string
}

export type KpiMonitoringItem = {
  key: string
  name: string
  currentValue: number
  targetValue: number
  unit: string
  achievementPercent: number
  trend: string
}

export type NotificationTemplate = {
  id: string
  name: string
  code: string
  subjectTemplate: string
  bodyTemplate: string
  channelId: string
  channelName: string
  channelCode: string
  isSystem: boolean
  isActive: boolean
  createdAt: string
  updatedAt?: string
}

export type Notification = {
  id: string
  recipientUserId: string
  recipientUserEmail?: string
  notificationTemplateId?: string
  notificationTemplateName?: string
  channelId?: string
  channelName?: string
  channelCode?: string
  statusId: string
  statusName: string
  statusCode: string
  priorityId?: string
  priorityName?: string
  subject: string
  message: string
  actionUrl?: string
  relatedEntityType?: string
  relatedEntityId?: string
  sentAt?: string
  readAt?: string
  isDismissed: boolean
  isActive: boolean
  createdAt: string
  updatedAt?: string
}

export type SecurityPolicy = {
  id: string
  entityName: string
  scopeTypeName: string
  scopeTypeCode: string
  maskSensitiveFields: boolean
  sensitiveFieldList?: string
  description?: string
  isActive: boolean
  createdAt: string
  updatedAt?: string
}

export type IntegrationConnection = {
  id: string
  name: string
  code: string
  providerCode: string
  providerName: string
  directionCode: string
  directionName: string
  authTypeCode: string
  authTypeName: string
  endpointUrl?: string
  apiKeyReference?: string
  lastSyncStatusCode?: string
  lastSyncStatusName?: string
  lastSyncAt?: string
  description?: string
  isActive: boolean
  createdAt: string
  updatedAt?: string
}

export type IntegrationSyncRun = {
  id: string
  integrationConnectionId: string
  integrationConnectionName: string
  integrationConnectionCode: string
  triggerTypeCode: string
  triggerTypeName: string
  statusCode: string
  statusName: string
  startedAt: string
  completedAt?: string
  recordsProcessed: number
  errorMessage?: string
  createdAt: string
}

export type CustomFieldDefinition = {
  id: string
  entityName: string
  fieldKey: string
  displayName: string
  dataTypeCode: string
  dataTypeName: string
  isRequired: boolean
  isIndexed: boolean
  defaultValue?: string
  optionsJson?: string
  sortOrder: number
  description?: string
  isActive: boolean
  createdAt: string
  updatedAt?: string
}

export type RecordStatusDefinition = {
  id: string
  entityName: string
  statusCode: string
  statusName: string
  isDefault: boolean
  isClosedState: boolean
  sortOrder: number
  description?: string
  isActive: boolean
  createdAt: string
  updatedAt?: string
}

export type AiPromptTemplate = {
  id: string
  name: string
  useCaseCode: string
  systemPrompt: string
  isSystem: boolean
  version: number
  isActive: boolean
  createdAt: string
  updatedAt?: string
}

export type AiDashboardSummary = {
  openLeads: number
  openOpportunities: number
  openCases: number
  insights: string[]
}

export type AiRecommendation = {
  scenarioCode: string
  actions: string[]
}

export type Account = {
  id: string
  accountNumber: string
  name: string
  legalName?: string
  tradingName?: string
  accountTypeId?: string
  accountTypeName?: string
  industryId?: string
  industryName?: string
  ownershipTypeId?: string
  ownershipTypeName?: string
  customerStatusId?: string
  customerStatusName?: string
  customerSegmentId?: string
  customerSegmentName?: string
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
  ownerUserName?: string
  ownerTeamId?: string
  ownerTeamName?: string
  createdAt?: string
  updatedAt?: string
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

export type Lead = {
  id: string
  leadNumber: string
  topic: string
  firstName?: string
  middleName?: string
  lastName?: string
  fullName?: string
  companyName?: string
  jobTitle?: string
  email?: string
  mobilePhone?: string
  workPhone?: string
  website?: string
  leadSourceId?: string
  leadSourceName?: string
  leadStatusId: string
  leadStatusName?: string
  qualificationStatusId?: string
  qualificationStatusName?: string
  ratingId?: string
  ratingName?: string
  industryId?: string
  industryName?: string
  estimatedValue?: number
  estimatedCloseDate?: string
  score: number
  scoreGrade?: string
  assignedToUserId?: string
  assignedToUserName?: string
  assignedToTeamId?: string
  assignedToTeamName?: string
  convertedAccountId?: string
  convertedAccountName?: string
  convertedContactId?: string
  convertedContactName?: string
  convertedOpportunityId?: string
  convertedAt?: string
  convertedById?: string
  convertedByName?: string
  disqualifiedReasonId?: string
  disqualifiedReasonName?: string
  description?: string
  notes?: string
  isActive: boolean
  ownerUserId?: string
  ownerUserName?: string
  ownerTeamId?: string
  ownerTeamName?: string
  createdAt: string
}

export type LeadActivity = {
  id: string
  leadId: string
  leadTopic?: string
  activityTypeId: string
  activityTypeName?: string
  subject: string
  description?: string
  activityDate: string
  dueDate?: string
  completedDate?: string
  statusId: string
  statusName?: string
  priorityId?: string
  priorityName?: string
  assignedToUserId?: string
  assignedToUserName?: string
}

export type LeadScoreRule = {
  id: string
  name: string
  code: string
  description?: string
  ruleTypeId: string
  ruleTypeName?: string
  fieldName?: string
  operator?: string
  compareValue?: string
  scoreValue: number
  sortOrder: number
  isActive: boolean
}

export type LeadTimelineItem = {
  id: string
  itemType: string
  title: string
  description?: string
  occurredAt: string
  status?: string
  priority?: string
  assignedToName?: string
}

export type LeadConversionResult = {
  leadId: string
  convertedAccountId: string
  convertedAccountName?: string
  convertedContactId: string
  convertedContactName?: string
  convertedOpportunityId?: string
  opportunityMessage?: string
}

export type LeadDashboardGroup = {
  name: string
  count: number
}

export type LeadDashboardItem = {
  id: string
  leadNumber: string
  topic: string
  statusName?: string
  score: number
  createdAt: string
  convertedAt?: string
}

export type LeadDashboardSummary = {
  totalLeads: number
  newLeads: number
  qualifiedLeads: number
  convertedLeads: number
  disqualifiedLeads: number
  averageLeadScore: number
  hotLeads: number
  leadsBySource: LeadDashboardGroup[]
  leadsByStatus: LeadDashboardGroup[]
  recentLeads: LeadDashboardItem[]
  recentlyConvertedLeads: LeadDashboardItem[]
}

export type DashboardChartPoint = {
  name: string
  count: number
  value: number
}

export type DashboardLeadListItem = {
  id: string
  leadNumber: string
  topic: string
  companyName?: string
  statusName?: string
  score: number
  ownerName?: string
}

export type DashboardOpportunityListItem = {
  id: string
  opportunityNumber: string
  topic: string
  accountName?: string
  estimatedRevenue?: number
  probability: number
  estimatedCloseDate?: string
  stageName?: string
}

export type DashboardCaseListItem = {
  id: string
  caseNumber: string
  subject: string
  priorityName?: string
  statusName?: string
  dueAt?: string
  assignedToName?: string
}

export type DashboardActivityItem = {
  id: string
  activityNumber: string
  subject: string
  activityTypeName?: string
  statusName?: string
  priorityName?: string
  dueDate?: string
  activityDate: string
  relatedRecord: string
}

export type DashboardTaskItem = {
  id: string
  subject: string
  priorityName?: string
  dueDate?: string
  relatedRecord: string
  statusName?: string
  isOverdue: boolean
}

export type DashboardApprovalItem = {
  id: string
  referenceNumber: string
  type: string
  accountName?: string
  totalAmount: number
  approvalStatusName?: string
}

export type DashboardSlaAlertItem = {
  id: string
  caseNumber: string
  title: string
  priorityName?: string
  dueAt?: string
  assignedToName?: string
}

export type DashboardSummary = {
  welcome: DashboardWelcome
  kpis: DashboardKpi[]
  totalLeads: number
  newLeads: number
  qualifiedLeads: number
  convertedLeads: number
  openOpportunities: number
  pipelineValue: number
  weightedPipeline: number
  winRate: number
  openCases: number
  overdueTasks: number
  revenueThisMonth: number
  slaBreaches: number
  recentLeads: DashboardLeadListItem[]
  opportunitiesClosingSoon: DashboardOpportunityListItem[]
  upcomingFollowUps: DashboardTaskItem[]
  slaAlerts: DashboardSlaAlertItem[]
}

export type DashboardWelcome = {
  userName: string
  dateLabel: string
  currentRole: string
  businessUnit: string
  team: string
  openTasks: number
  overdueActivities: number
  opportunitiesClosingThisWeek: number
  slaBreaches: number
  hasManagementAccess: boolean
}

export type DashboardKpi = {
  key: string
  icon: string
  title: string
  currentValue: number
  previousValue: number
  trendPercent: number
  comparisonLabel: string
  actionPath: string
  positiveTrendIsGood: boolean
}

export type DashboardPipeline = {
  funnelStages: DashboardChartPoint[]
  opportunityStageDistribution: DashboardChartPoint[]
  forecastAccuracyPercent: number
}

export type DashboardRevenueTrendPoint = {
  month: string
  actualRevenue: number
  forecastRevenue: number
}

export type DashboardRevenue = {
  revenueThisMonth: number
  revenueThisQuarter: number
  monthlyTrend: DashboardRevenueTrendPoint[]
}

export type DashboardCustomerInsightItem = {
  accountId: string
  accountName: string
  revenue: number
  openOpportunities: number
  reason?: string
  createdAt?: string
}

export type DashboardCustomers = {
  topCustomers: DashboardCustomerInsightItem[]
  atRiskCustomers: DashboardCustomerInsightItem[]
  newCustomers: DashboardCustomerInsightItem[]
}

export type DashboardService = {
  casesByPriority: DashboardChartPoint[]
  casesByStatus: DashboardChartPoint[]
  slaCompliancePercent: number
  casesRequiringAttention: DashboardCaseListItem[]
}

export type DashboardSalespersonPerformance = {
  userId: string
  userName: string
  revenue: number
  opportunitiesWon: number
  winRate: number
}

export type DashboardTeamPerformance = {
  teamId: string
  teamName: string
  target: number
  actual: number
  achievementPercent: number
}

export type DashboardManagement = {
  isVisible: boolean
  topSalespeople: DashboardSalespersonPerformance[]
  teamPerformance: DashboardTeamPerformance[]
  leadConversionTrend: DashboardChartPoint[]
  revenueByTeam: DashboardChartPoint[]
}

export type DashboardActivityFeedItem = {
  id: string
  userName: string
  action: string
  entity: string
  timestamp: string
  route: string
}

export type DashboardActivityFeed = {
  items: DashboardActivityFeedItem[]
}

export type DashboardWidgetPreference = {
  widgetId: string
  order: number
  isVisible: boolean
  isPinned: boolean
}

export type DashboardLayoutPreference = {
  layoutVersion: string
  widgets: DashboardWidgetPreference[]
}

export type DashboardMyWork = {
  assignedLeads: DashboardLeadListItem[]
  assignedOpportunities: DashboardOpportunityListItem[]
  assignedCases: DashboardCaseListItem[]
  assignedActivities: DashboardActivityItem[]
  pendingApprovals: DashboardApprovalItem[]
  overdueTasks: DashboardTaskItem[]
}

export type DashboardMyActivities = {
  items: DashboardActivityItem[]
  totalCount: number
  page: number
  pageSize: number
  activitiesByStatus: DashboardChartPoint[]
}

export type DashboardMyOpenTasks = {
  items: DashboardTaskItem[]
  totalCount: number
  overdueCount: number
  page: number
  pageSize: number
}

export type Opportunity = {
  id: string
  opportunityNumber: string
  topic: string
  accountId: string
  accountName?: string
  contactId?: string
  contactName?: string
  leadId?: string
  leadTopic?: string
  opportunityStageId: string
  opportunityStageName?: string
  opportunityStageCode?: string
  opportunityStatusId: string
  opportunityStatusName?: string
  opportunityStatusCode?: string
  salesProcessStageId?: string
  salesProcessStageName?: string
  ratingId?: string
  ratingName?: string
  priorityId?: string
  priorityName?: string
  estimatedRevenue?: number
  estimatedCloseDate?: string
  probability: number
  weightedRevenue?: number
  actualRevenue?: number
  actualCloseDate?: string
  currencyId?: string
  currencyName?: string
  sourceId?: string
  sourceName?: string
  winReasonId?: string
  winReasonName?: string
  lossReasonId?: string
  lossReasonName?: string
  lostToCompetitorId?: string
  lostToCompetitorName?: string
  description?: string
  notes?: string
  isActive: boolean
  ownerUserId?: string
  ownerUserName?: string
  ownerTeamId?: string
  ownerTeamName?: string
  createdAt: string
  updatedAt?: string
}

export type OpportunityProduct = {
  id: string
  opportunityId: string
  productId?: string
  productName: string
  description?: string
  quantity: number
  unitPrice: number
  discountPercent?: number
  discountAmount?: number
  taxAmount?: number
  lineTotal: number
  sortOrder: number
}

export type Product = {
  id: string
  productCode: string
  name: string
  description?: string
  productCategoryId: string
  productCategoryName?: string
  productTypeId: string
  productTypeName?: string
  unitOfMeasureId: string
  unitOfMeasureName?: string
  productStatusId: string
  productStatusName?: string
  sku?: string
  barcode?: string
  manufacturer?: string
  brand?: string
  costPrice?: number
  standardPrice?: number
  taxRate?: number
  weight?: number
  volume?: number
  isStockItem: boolean
  allowDiscount: boolean
  effectiveFrom?: string
  effectiveTo?: string
  isActive: boolean
  ownerUserId?: string
  ownerUserName?: string
  ownerTeamId?: string
  ownerTeamName?: string
  createdAt: string
  updatedAt?: string
}

export type ProductCategory = {
  id: string
  name: string
  code: string
  description?: string
  parentCategoryId?: string
  parentCategoryName?: string
  sortOrder: number
  productCount: number
  isActive: boolean
  createdAt: string
  updatedAt?: string
}

export type PriceList = {
  id: string
  priceListNumber: string
  name: string
  description?: string
  currencyId: string
  currencyName?: string
  effectiveFrom?: string
  effectiveTo?: string
  isDefault: boolean
  isActive: boolean
  createdAt: string
  updatedAt?: string
}

export type PriceListItem = {
  id: string
  priceListId: string
  productId: string
  productCode: string
  productName: string
  unitPrice: number
  minimumQuantity?: number
  maximumQuantity?: number
  discountPercent?: number
  effectiveFrom?: string
  effectiveTo?: string
  createdAt: string
  updatedAt?: string
}

export type ProductBundle = {
  id: string
  bundleCode: string
  name: string
  description?: string
  bundlePrice?: number
  allowComponentOverride: boolean
  isActive: boolean
  createdAt: string
  updatedAt?: string
}

export type ProductBundleItem = {
  id: string
  productBundleId: string
  productId: string
  productCode: string
  productName: string
  quantity: number
  sortOrder: number
  unitPrice: number
  lineTotal: number
  createdAt: string
  updatedAt?: string
}

export type UnitOfMeasure = {
  id: string
  name: string
  code: string
  description?: string
  sortOrder: number
  isDefault: boolean
  isActive: boolean
  createdAt: string
  updatedAt?: string
}

export type Discount = {
  id: string
  name: string
  code: string
  discountTypeId: string
  discountTypeName?: string
  value: number
  maximumAmount?: number
  isStackable: boolean
  isActive: boolean
  effectiveFrom?: string
  effectiveTo?: string
  description?: string
  createdAt: string
  updatedAt?: string
}

export type Quote = {
  id: string
  quoteNumber: string
  accountId: string
  accountName?: string
  contactId?: string
  contactName?: string
  opportunityId?: string
  opportunityTopic?: string
  priceListId: string
  priceListName?: string
  currencyId: string
  currencyName?: string
  quoteStatusId: string
  quoteStatusName?: string
  approvalStatusId: string
  approvalStatusName?: string
  validFrom?: string
  validTo?: string
  subtotalAmount: number
  discountAmount: number
  taxAmount: number
  totalAmount: number
  notes?: string
  termsAndConditions?: string
  approvedById?: string
  approvedAt?: string
  convertedOrderId?: string
  convertedAt?: string
  isActive: boolean
  ownerUserId?: string
  ownerUserName?: string
  ownerTeamId?: string
  ownerTeamName?: string
  createdAt: string
  updatedAt?: string
}

export type QuoteLine = {
  id: string
  quoteId: string
  productId?: string
  productBundleId?: string
  productName: string
  description?: string
  unitOfMeasureId?: string
  unitOfMeasureName?: string
  quantity: number
  unitPrice: number
  discountPercent: number
  discountAmount: number
  taxRate: number
  taxAmount: number
  lineTotal: number
  sortOrder: number
  createdAt: string
  updatedAt?: string
}

export type Order = {
  id: string
  orderNumber: string
  quoteId?: string
  quoteNumber?: string
  accountId: string
  accountName?: string
  contactId?: string
  contactName?: string
  opportunityId?: string
  opportunityTopic?: string
  currencyId: string
  currencyName?: string
  orderStatusId: string
  orderStatusName?: string
  approvalStatusId: string
  approvalStatusName?: string
  deliveryStatusId: string
  deliveryStatusName?: string
  billingStatusId: string
  billingStatusName?: string
  orderDate?: string
  expectedDeliveryDate?: string
  deliveryDate?: string
  billingDate?: string
  subtotalAmount: number
  discountAmount: number
  taxAmount: number
  totalAmount: number
  notes?: string
  approvedById?: string
  approvedAt?: string
  convertedInvoiceId?: string
  convertedInvoiceAt?: string
  isActive: boolean
  ownerUserId?: string
  ownerUserName?: string
  ownerTeamId?: string
  ownerTeamName?: string
  createdAt: string
  updatedAt?: string
}

export type OrderLine = {
  id: string
  orderId: string
  productId?: string
  productBundleId?: string
  productName: string
  description?: string
  unitOfMeasureId?: string
  unitOfMeasureName?: string
  quantity: number
  unitPrice: number
  discountPercent: number
  discountAmount: number
  taxRate: number
  taxAmount: number
  lineTotal: number
  sortOrder: number
  createdAt: string
  updatedAt?: string
}

export type Invoice = {
  id: string
  invoiceNumber: string
  orderId?: string
  orderNumber?: string
  quoteId?: string
  quoteNumber?: string
  accountId: string
  accountName?: string
  contactId?: string
  contactName?: string
  opportunityId?: string
  opportunityTopic?: string
  currencyId: string
  currencyName?: string
  invoiceStatusId: string
  invoiceStatusName?: string
  paymentStatusId: string
  paymentStatusName?: string
  dueDate?: string
  invoiceDate?: string
  paidDate?: string
  subtotalAmount: number
  discountAmount: number
  taxAmount: number
  totalAmount: number
  paidAmount: number
  notes?: string
  isActive: boolean
  ownerUserId?: string
  ownerUserName?: string
  ownerTeamId?: string
  ownerTeamName?: string
  createdAt: string
  updatedAt?: string
}

export type InvoiceLine = {
  id: string
  invoiceId: string
  productId?: string
  productBundleId?: string
  productName: string
  description?: string
  unitOfMeasureId?: string
  unitOfMeasureName?: string
  quantity: number
  unitPrice: number
  discountPercent: number
  discountAmount: number
  taxRate: number
  taxAmount: number
  lineTotal: number
  sortOrder: number
  createdAt: string
  updatedAt?: string
}

export type Case = {
  id: string
  caseNumber: string
  accountId: string
  accountName?: string
  contactId?: string
  contactName?: string
  opportunityId?: string
  opportunityTopic?: string
  subject: string
  description?: string
  caseStatusId: string
  caseStatusName?: string
  caseStatusCode?: string
  priorityId: string
  priorityName?: string
  severityId?: string
  severityName?: string
  categoryId?: string
  categoryName?: string
  sourceId?: string
  sourceName?: string
  assignedToUserId?: string
  assignedToUserName?: string
  escalatedToUserId?: string
  escalatedToUserName?: string
  openedAt: string
  dueAt?: string
  resolvedAt?: string
  closedAt?: string
  resolutionSummary?: string
  isActive: boolean
  ownerUserId?: string
  ownerUserName?: string
  ownerTeamId?: string
  ownerTeamName?: string
  createdAt: string
  updatedAt?: string
}

export type CaseComment = {
  id: string
  caseId: string
  commentText: string
  isInternal: boolean
  createdAt: string
  createdById?: string
  createdByName?: string
}

export type Activity = {
  id: string
  activityNumber: string
  activityTypeId: string
  activityTypeName?: string
  activityTypeCode?: string
  statusId: string
  statusName?: string
  statusCode?: string
  priorityId?: string
  priorityName?: string
  subject: string
  description?: string
  activityDate: string
  dueDate?: string
  completedDate?: string
  assignedToUserId?: string
  assignedToUserName?: string
  accountId?: string
  accountName?: string
  contactId?: string
  contactName?: string
  leadId?: string
  leadTopic?: string
  opportunityId?: string
  opportunityTopic?: string
  caseId?: string
  caseNumber?: string
  isPrivate: boolean
  outcomeId?: string
  outcomeName?: string
  reminderAt?: string
  isActive: boolean
  ownerUserId?: string
  ownerTeamId?: string
  createdAt: string
  updatedAt?: string
}

export type ActivityComment = {
  id: string
  activityId: string
  commentText: string
  isInternal: boolean
  createdAt: string
  createdById?: string
  createdByName?: string
}

export type Document = {
  id: string
  documentNumber: string
  title: string
  description?: string
  fileName: string
  contentType: string
  fileSizeBytes: number
  storagePath: string
  documentCategoryId: string
  documentCategoryName?: string
  documentCategoryCode?: string
  documentStatusId: string
  documentStatusName?: string
  documentStatusCode?: string
  accountId?: string
  accountName?: string
  contactId?: string
  contactName?: string
  leadId?: string
  leadTopic?: string
  opportunityId?: string
  opportunityTopic?: string
  caseId?: string
  caseNumber?: string
  effectiveDate?: string
  expiryDate?: string
  isConfidential: boolean
  currentVersion: number
  isActive: boolean
  ownerUserId?: string
  ownerUserName?: string
  ownerTeamId?: string
  ownerTeamName?: string
  createdAt: string
  updatedAt?: string
}

export type DocumentVersion = {
  id: string
  documentId: string
  versionNumber: number
  fileName: string
  contentType: string
  fileSizeBytes: number
  storagePath: string
  changeSummary?: string
  createdAt: string
  updatedAt?: string
}

export type OpportunityCompetitor = {
  id: string
  opportunityId: string
  competitorName: string
  strengths?: string
  weaknesses?: string
  threatLevelId?: string
  threatLevelName?: string
  isPrimaryCompetitor: boolean
  notes?: string
}

export type OpportunityActivity = {
  id: string
  opportunityId: string
  contactId?: string
  contactName?: string
  activityTypeId: string
  activityTypeName?: string
  subject: string
  description?: string
  activityDate: string
  dueDate?: string
  completedDate?: string
  statusId: string
  statusName?: string
  priorityId?: string
  priorityName?: string
  assignedToUserId?: string
  assignedToUserName?: string
}

export type OpportunityTimelineItem = {
  id: string
  itemType: string
  title: string
  description?: string
  occurredAt: string
  status?: string
  priority?: string
  assignedToName?: string
}

export type OpportunitySummary = {
  opportunity: Opportunity
  latestActivity?: OpportunityActivity
  primaryCompetitor?: OpportunityCompetitor
  productRevenue: number
  productCount: number
  competitorCount: number
  activityCount: number
}

export type OpportunityPipelineStage = {
  stageId: string
  stageName: string
  stageCode?: string
  count: number
  estimatedRevenue: number
  weightedRevenue: number
  opportunities: Opportunity[]
}

export type OpportunityDashboardSummary = {
  totalOpportunities: number
  openOpportunities: number
  wonOpportunities: number
  lostOpportunities: number
  pipelineValue: number
  weightedPipelineValue: number
  averageProbability: number
  closingThisMonth: number
  opportunitiesByStage: LeadDashboardGroup[]
  opportunitiesByOwner: LeadDashboardGroup[]
  recentOpportunities: Opportunity[]
}

export type TrendPoint = {
  name: string
  value: number
  count: number
}

export type LeaderboardItem = {
  name: string
  revenue: number
  winRate: number
  opportunitiesClosed: number
  targetAchievement: number
}

export type SalesPipelineSummary = {
  totalOpportunities: number
  pipelineRevenue: number
  weightedPipelineRevenue: number
  averageProbability: number
  averageDealSize: number
}

export type SalesPipelineCard = {
  id: string
  opportunityNumber: string
  topic: string
  accountName?: string
  estimatedRevenue?: number
  probability: number
  weightedRevenue?: number
  estimatedCloseDate?: string
  opportunityStageId: string
  opportunityStageName?: string
  salesProcessStageId?: string
  ownerName?: string
  ratingName?: string
  ratingCode?: string
  ageInDays: number
}

export type SalesPipelineStage = {
  stageId: string
  stageName: string
  stageCode?: string
  count: number
  pipelineRevenue: number
  weightedRevenue: number
  opportunities: SalesPipelineCard[]
}

export type SalesPipelineBoard = {
  summary: SalesPipelineSummary
  stages: SalesPipelineStage[]
}

export type OpportunityStageHistory = {
  id: string
  opportunityId: string
  previousStageId?: string
  previousStageName?: string
  newStageId: string
  newStageName: string
  changedByUserId?: string
  changedByUserName?: string
  changedAt: string
  notes?: string
}

export type OpportunityPipelineAnalytics = {
  opportunityId: string
  currentStageName?: string
  daysInStage: number
  stageHistory: OpportunityStageHistory[]
  probabilityTrend: TrendPoint[]
  revenueTrend: TrendPoint[]
}

export type SalesTarget = {
  id: string
  name: string
  description?: string
  targetTypeId: string
  targetTypeName?: string
  targetPeriodId: string
  targetPeriodName?: string
  startDate: string
  endDate: string
  targetAmount: number
  actualAmount: number
  achievementPercentage: number
  assignedUserId?: string
  assignedUserName?: string
  assignedTeamId?: string
  assignedTeamName?: string
  isActive: boolean
  ownerUserId?: string
  ownerUserName?: string
  ownerTeamId?: string
  ownerTeamName?: string
  createdAt: string
}

export type RevenueForecast = {
  id: string
  forecastDate: string
  forecastPeriodStart: string
  forecastPeriodEnd: string
  forecastTypeId: string
  forecastTypeName?: string
  totalPipelineRevenue: number
  weightedPipelineRevenue: number
  forecastRevenue: number
  closedRevenue: number
  openRevenue: number
  forecastAccuracy: number
  notes?: string
  createdAt: string
}

export type ForecastDashboard = {
  totalPipeline: number
  weightedPipeline: number
  closedRevenue: number
  forecastRevenue: number
  forecastAccuracy: number
  forecastTrend: TrendPoint[]
  revenueByMonth: TrendPoint[]
  revenueByQuarter: TrendPoint[]
  revenueByOwner: TrendPoint[]
  revenueByTeam: TrendPoint[]
}

export type RevenueTracking = {
  wonRevenue: number
  lostRevenue: number
  pipelineRevenue: number
  weightedRevenue: number
  revenueTrend: TrendPoint[]
  revenueByAccount: TrendPoint[]
  revenueByIndustry: TrendPoint[]
  revenueBySalesperson: TrendPoint[]
}

export type SalesPerformanceDashboard = {
  openOpportunities: number
  wonOpportunities: number
  lostOpportunities: number
  winRate: number
  averageDealSize: number
  averageSalesCycleDays: number
  revenueThisMonth: number
  revenueThisQuarter: number
  revenueThisYear: number
  forecastRevenue: number
  forecastAccuracy: number
  topSalesperson?: string
  topTeam?: string
  pipelineByStage: TrendPoint[]
  revenueTrend: TrendPoint[]
  opportunitiesByOwner: TrendPoint[]
  opportunitiesByIndustry: TrendPoint[]
  winRateTrend: TrendPoint[]
  forecastAccuracyTrend: TrendPoint[]
  topSalespeople: LeaderboardItem[]
  topTeams: LeaderboardItem[]
}
