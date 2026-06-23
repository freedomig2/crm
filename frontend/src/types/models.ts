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
  ownerTeamId?: string
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
