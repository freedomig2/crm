import { api } from '../../api/client'

type AnyRecord = Record<string, unknown>

export type LookupOption = {
  value: string
  label: string
}

const cache = new Map<string, LookupOption[]>()

const guidLike = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i

const lookupCategoryCodeForFieldKey = (fieldKey: string): string | null => {
  const map: Record<string, string> = {
    salutationid: 'SALUTATION',
    contacttitleid: 'SALUTATION',
    salutationlookupid: 'SALUTATION',
    genderlookupid: 'GENDER',
    contactroleid: 'CONTACT_ROLE',
    preferredcommunicationid: 'CONTACT_METHOD',
    preferredcontactmethodid: 'CONTACT_METHOD',
    preferredcommunicationmethodid: 'CONTACT_METHOD',
    preferredlanguageid: 'LANGUAGE',
    preferredtimezoneid: 'TIME_ZONE',
    communicationtypeid: 'COMMUNICATION_TYPE',
    interactiontypeid: 'INTERACTION_TYPE',
    leadsourceid: 'LEAD_SOURCE',
    leadstatusid: 'LEAD_STATUS',
    qualificationstatusid: 'LEAD_QUALIFICATION_STATUS',
    accounttypeid: 'ACCOUNT_TYPE',
    ownershiptypeid: 'OWNERSHIP_TYPE',
    customerstatusid: 'CUSTOMER_STATUS',
    customersegmentid: 'CUSTOMER_SEGMENT',
    opportunitystageid: 'OPPORTUNITY_STAGE',
    opportunitystatusid: 'OPPORTUNITY_STATUS',
    salesprocessstageid: 'SALES_PROCESS_STAGE',
    opportunityratingid: 'OPPORTUNITY_RATING',
    opportunitysourceid: 'OPPORTUNITY_SOURCE',
    currencyid: 'CURRENCY',
    quotestatusid: 'QUOTE_STATUS',
    approvalstatusid: 'QUOTE_APPROVAL_STATUS',
    orderapprovalstatusid: 'ORDER_APPROVAL_STATUS',
    orderstatusid: 'ORDER_STATUS',
    deliverystatusid: 'ORDER_DELIVERY_STATUS',
    billingstatusid: 'ORDER_BILLING_STATUS',
    invoicestatusid: 'INVOICE_STATUS',
    paymentstatusid: 'INVOICE_PAYMENT_STATUS',
    casestatusid: 'CASE_STATUS',
    casepriorityid: 'CASE_PRIORITY',
    severityid: 'CASE_SEVERITY',
    categoryid: 'CASE_CATEGORY',
    winreasonid: 'WIN_REASON',
    lossreasonid: 'LOSS_REASON',
    threatlevelid: 'COMPETITOR_THREAT_LEVEL',
    industryid: 'INDUSTRY',
    disqualifiedreasonid: 'LEAD_DISQUALIFICATION_REASON',
    activitytypeid: 'ACTIVITY_TYPE',
    genderid: 'GENDER',
    outcomeid: 'ACTIVITY_OUTCOME',
    priorityid: 'PRIORITY',
    ruletypeid: 'LEAD_SCORE_RULE_TYPE',
    resetfrequencyid: 'NUMBER_SEQUENCE_RESET_FREQUENCY',
    targettypeid: 'SALES_TARGET_TYPE',
    targetperiodid: 'SALES_TARGET_PERIOD',
    forecasttypeid: 'FORECAST_TYPE',
    producttypeid: 'PRODUCT_TYPE',
    productstatusid: 'PRODUCT_STATUS',
    discounttypeid: 'DISCOUNT_TYPE',
    documentcategoryid: 'DOCUMENT_CATEGORY',
    documentstatusid: 'DOCUMENT_STATUS',
  }
  const route = typeof window !== 'undefined' ? window.location.pathname.toLowerCase() : ''
  if (fieldKey === 'ratingid') {
    return route.includes('/opportunities') ? 'OPPORTUNITY_RATING' : 'LEAD_RATING'
  }

  if (fieldKey === 'sourceid') {
    return route.includes('/opportunities') ? 'OPPORTUNITY_SOURCE' : 'CASE_SOURCE'
  }

  if (fieldKey === 'statusid') {
    if (route.includes('/activities')) {
      return 'ACTIVITY_STATUS'
    }
  }

  return map[fieldKey.toLowerCase()] ?? null
}

const endpointForFieldKey = (fieldKey: string): string => {
  if (fieldKey.includes('owneruser') || fieldKey.includes('assignedtouser') || fieldKey.includes('createdby') || fieldKey.includes('updatedby')) {
    return 'api/users'
  }

  if (fieldKey.includes('ownerteam') || fieldKey.includes('team')) {
    return 'api/teams'
  }

  if (fieldKey.includes('department')) {
    return 'api/departments'
  }

  if (fieldKey.includes('account')) {
    return 'api/accounts'
  }

  if (fieldKey.includes('contact')) {
    return 'api/contacts'
  }

  if (fieldKey.includes('lead')) {
    return 'api/leads'
  }

  if (fieldKey.includes('opportunity')) {
    return 'api/opportunities'
  }

  if (fieldKey.includes('case')) {
    return 'api/cases'
  }

  if (fieldKey.includes('competitor')) {
    return 'api/opportunity-competitors'
  }

  if (fieldKey.includes('productcategory')) {
    return 'api/product-categories'
  }

  if (fieldKey.includes('productbundle')) {
    return 'api/product-bundles'
  }

  if (fieldKey.includes('product')) {
    return 'api/products'
  }

  if (fieldKey.includes('unitofmeasure')) {
    return 'api/unit-of-measures'
  }

  if (fieldKey.includes('pricelist')) {
    return 'api/price-lists'
  }

  if (fieldKey.includes('discount')) {
    return 'api/discounts'
  }

  if (fieldKey.includes('role')) {
    return 'api/roles'
  }

  if (fieldKey.includes('lookupcategory')) {
    return 'api/lookup-categories'
  }

  return 'api/lookup-values'
}

const asText = (value: unknown): string => {
  if (value === null || value === undefined) {
    return ''
  }

  return String(value)
}

const labelFromRecord = (record: AnyRecord): string => {
  const name = asText(record.name)
  const title = asText(record.title)
  const code = asText(record.code)
  const email = asText(record.email)
  const accountNumber = asText(record.accountNumber)
  const contactNumber = asText(record.contactNumber)
  const fullName = asText(record.fullName)
  const subject = asText(record.subject)
  const competitorName = asText(record.competitorName)
  const topic = asText(record.topic)
  const leadNumber = asText(record.leadNumber)
  const opportunityNumber = asText(record.opportunityNumber)

  if (name && accountNumber) {
    return `${name} (${accountNumber})`
  }

  if (topic && leadNumber) {
    return `${topic} (${leadNumber})`
  }

  if (topic && opportunityNumber) {
    return `${topic} (${opportunityNumber})`
  }

  if (fullName && contactNumber) {
    return `${fullName} (${contactNumber})`
  }

  if (fullName) {
    return fullName
  }

  if (competitorName) {
    return competitorName
  }

  if (name && code) {
    return name
  }

  if (name) {
    return name
  }

  if (title) {
    return title
  }

  if (email) {
    return email
  }

  if (subject) {
    return subject
  }

  if (topic) {
    return topic
  }

  const firstName = asText(record.firstName)
  const lastName = asText(record.lastName)
  if (firstName || lastName) {
    return `${firstName} ${lastName}`.trim()
  }

  return asText(record.id)
}

const normalizeItems = (payload: unknown): AnyRecord[] => {
  if (Array.isArray(payload)) {
    return payload as AnyRecord[]
  }

  const paged = payload as { items?: AnyRecord[] }
  return Array.isArray(paged?.items) ? paged.items : []
}

export const isForeignKeyField = (fieldKey: string): boolean => {
  const lower = fieldKey.toLowerCase()
  return lower !== 'id' && lower.endsWith('id')
}

export const loadLookupOptionsByCategoryCode = async (categoryCode: string): Promise<LookupOption[]> => {
  const cacheKey = `category:${categoryCode.toUpperCase()}`
  if (cache.has(cacheKey)) {
    return cache.get(cacheKey) ?? []
  }

  try {
    const { data } = await api.get(`api/lookups/${encodeURIComponent(categoryCode.toUpperCase())}/values`, {
      params: { limit: 500 },
    })
    const items = normalizeItems(data)
      .map((record) => ({
        value: asText(record.id),
        label: labelFromRecord(record),
      }))
      .filter((item) => item.value.length > 0)

    cache.set(cacheKey, items)
    return items
  } catch {
    cache.set(cacheKey, [])
    return []
  }
}

export const loadLookupOptions = async (fieldKey: string): Promise<LookupOption[]> => {
  const key = fieldKey.toLowerCase()
  const categoryCode = lookupCategoryCodeForFieldKey(key)
  if (categoryCode) {
    return loadLookupOptionsByCategoryCode(categoryCode)
  }

  if (cache.has(key)) {
    return cache.get(key) ?? []
  }

  const endpoint = endpointForFieldKey(key)

  try {
    const { data } = await api.get(endpoint, { params: { page: 1, pageSize: 200 } })
    const items = normalizeItems(data)
      .map((record) => ({
        value: asText(record.id),
        label: labelFromRecord(record),
      }))
      .filter((item) => item.value.length > 0)

    cache.set(key, items)
    return items
  } catch {
    cache.set(key, [])
    return []
  }
}

export const resolveDisplayValue = async (fieldKey: string, value: unknown): Promise<string> => {
  const raw = asText(value)
  if (!raw) {
    return ''
  }

  const isGuid = guidLike.test(raw)
  if (!isGuid || !isForeignKeyField(fieldKey)) {
    return raw
  }

  const options = await loadLookupOptions(fieldKey)
  return options.find((option) => option.value === raw)?.label ?? 'Linked record'
}
