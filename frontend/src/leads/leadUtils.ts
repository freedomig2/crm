import type { Lead, LeadScoreRule } from '../types/models'

export type LeadFormState = {
  leadNumber: string
  topic: string
  firstName: string
  middleName: string
  lastName: string
  companyName: string
  jobTitle: string
  email: string
  mobilePhone: string
  workPhone: string
  website: string
  leadSourceId: string
  leadStatusId: string
  qualificationStatusId: string
  ratingId: string
  industryId: string
  estimatedValue: string
  estimatedCloseDate: string
  assignedToUserId: string
  assignedToTeamId: string
  ownerUserId: string
  ownerTeamId: string
  disqualifiedReasonId: string
  description: string
  notes: string
  isActive: boolean
}

export type LeadScoreRuleFormState = {
  name: string
  code: string
  description: string
  ruleTypeId: string
  fieldName: string
  operator: string
  compareValue: string
  scoreValue: string
  sortOrder: string
  isActive: boolean
}

export const emptyLeadForm: LeadFormState = {
  leadNumber: '',
  topic: '',
  firstName: '',
  middleName: '',
  lastName: '',
  companyName: '',
  jobTitle: '',
  email: '',
  mobilePhone: '',
  workPhone: '',
  website: '',
  leadSourceId: '',
  leadStatusId: '',
  qualificationStatusId: '',
  ratingId: '',
  industryId: '',
  estimatedValue: '',
  estimatedCloseDate: '',
  assignedToUserId: '',
  assignedToTeamId: '',
  ownerUserId: '',
  ownerTeamId: '',
  disqualifiedReasonId: '',
  description: '',
  notes: '',
  isActive: true,
}

export const emptyLeadScoreRuleForm: LeadScoreRuleFormState = {
  name: '',
  code: '',
  description: '',
  ruleTypeId: '',
  fieldName: '',
  operator: '',
  compareValue: '',
  scoreValue: '10',
  sortOrder: '0',
  isActive: true,
}

export const nullIfBlank = (value: string) => value.trim() || null
export const toDateInput = (value?: string) => (value ? value.slice(0, 10) : '')

export const toDateTimeInput = (value?: string) => {
  if (!value) {
    return ''
  }

  const parsed = new Date(value)
  if (Number.isNaN(parsed.getTime())) {
    return value.slice(0, 16)
  }

  const local = new Date(parsed.getTime() - parsed.getTimezoneOffset() * 60000)
  return local.toISOString().slice(0, 16)
}

export const leadToForm = (lead: Lead): LeadFormState => ({
  leadNumber: lead.leadNumber,
  topic: lead.topic,
  firstName: lead.firstName ?? '',
  middleName: lead.middleName ?? '',
  lastName: lead.lastName ?? '',
  companyName: lead.companyName ?? '',
  jobTitle: lead.jobTitle ?? '',
  email: lead.email ?? '',
  mobilePhone: lead.mobilePhone ?? '',
  workPhone: lead.workPhone ?? '',
  website: lead.website ?? '',
  leadSourceId: lead.leadSourceId ?? '',
  leadStatusId: lead.leadStatusId ?? '',
  qualificationStatusId: lead.qualificationStatusId ?? '',
  ratingId: lead.ratingId ?? '',
  industryId: lead.industryId ?? '',
  estimatedValue: lead.estimatedValue === undefined || lead.estimatedValue === null ? '' : String(lead.estimatedValue),
  estimatedCloseDate: toDateInput(lead.estimatedCloseDate),
  assignedToUserId: lead.assignedToUserId ?? '',
  assignedToTeamId: lead.assignedToTeamId ?? '',
  ownerUserId: lead.ownerUserId ?? '',
  ownerTeamId: lead.ownerTeamId ?? '',
  disqualifiedReasonId: lead.disqualifiedReasonId ?? '',
  description: lead.description ?? '',
  notes: lead.notes ?? '',
  isActive: lead.isActive,
})

export const leadPayload = (form: LeadFormState) => ({
  leadNumber: form.leadNumber.trim(),
  topic: form.topic.trim(),
  firstName: nullIfBlank(form.firstName),
  middleName: nullIfBlank(form.middleName),
  lastName: nullIfBlank(form.lastName),
  companyName: nullIfBlank(form.companyName),
  jobTitle: nullIfBlank(form.jobTitle),
  email: nullIfBlank(form.email),
  mobilePhone: nullIfBlank(form.mobilePhone),
  workPhone: nullIfBlank(form.workPhone),
  website: nullIfBlank(form.website),
  leadSourceId: nullIfBlank(form.leadSourceId),
  leadStatusId: form.leadStatusId,
  qualificationStatusId: nullIfBlank(form.qualificationStatusId),
  ratingId: nullIfBlank(form.ratingId),
  industryId: nullIfBlank(form.industryId),
  estimatedValue: form.estimatedValue.trim() ? Number(form.estimatedValue) : null,
  estimatedCloseDate: nullIfBlank(form.estimatedCloseDate),
  assignedToUserId: nullIfBlank(form.assignedToUserId),
  assignedToTeamId: nullIfBlank(form.assignedToTeamId),
  ownerUserId: nullIfBlank(form.ownerUserId),
  ownerTeamId: nullIfBlank(form.ownerTeamId),
  disqualifiedReasonId: nullIfBlank(form.disqualifiedReasonId),
  description: nullIfBlank(form.description),
  notes: nullIfBlank(form.notes),
  isActive: form.isActive,
})

export const leadScoreRuleToForm = (rule: LeadScoreRule): LeadScoreRuleFormState => ({
  name: rule.name,
  code: rule.code,
  description: rule.description ?? '',
  ruleTypeId: rule.ruleTypeId,
  fieldName: rule.fieldName ?? '',
  operator: rule.operator ?? '',
  compareValue: rule.compareValue ?? '',
  scoreValue: String(rule.scoreValue),
  sortOrder: String(rule.sortOrder),
  isActive: rule.isActive,
})

export const leadScoreRulePayload = (form: LeadScoreRuleFormState) => ({
  name: form.name.trim(),
  code: form.code.trim(),
  description: nullIfBlank(form.description),
  ruleTypeId: form.ruleTypeId,
  fieldName: nullIfBlank(form.fieldName),
  operator: nullIfBlank(form.operator),
  compareValue: nullIfBlank(form.compareValue),
  scoreValue: Number(form.scoreValue || 0),
  sortOrder: Number(form.sortOrder || 0),
  isActive: form.isActive,
})

export const formatDateTime = (value?: string) => {
  if (!value) {
    return ''
  }

  const parsed = new Date(value)
  return Number.isNaN(parsed.getTime()) ? value : parsed.toLocaleString()
}

export const formatDate = (value?: string) => {
  if (!value) {
    return ''
  }

  const parsed = new Date(value)
  return Number.isNaN(parsed.getTime()) ? value.slice(0, 10) : parsed.toLocaleDateString()
}

export const formatCurrency = (value?: number) => {
  if (value === undefined || value === null) {
    return ''
  }

  return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(value)
}
