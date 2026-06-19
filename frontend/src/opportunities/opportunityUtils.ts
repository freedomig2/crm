import type { Opportunity } from '../types/models'
import { formatCurrency, formatDate, formatDateTime, nullIfBlank, toDateInput } from '../leads/leadUtils'

export type OpportunityFormState = {
  opportunityNumber: string
  topic: string
  accountId: string
  contactId: string
  leadId: string
  opportunityStageId: string
  opportunityStatusId: string
  salesProcessStageId: string
  ratingId: string
  priorityId: string
  estimatedRevenue: string
  estimatedCloseDate: string
  probability: string
  actualRevenue: string
  actualCloseDate: string
  currencyId: string
  sourceId: string
  winReasonId: string
  lossReasonId: string
  lostToCompetitorId: string
  description: string
  notes: string
  ownerUserId: string
  ownerTeamId: string
  isActive: boolean
}

export const emptyOpportunityForm: OpportunityFormState = {
  opportunityNumber: '',
  topic: '',
  accountId: '',
  contactId: '',
  leadId: '',
  opportunityStageId: '',
  opportunityStatusId: '',
  salesProcessStageId: '',
  ratingId: '',
  priorityId: '',
  estimatedRevenue: '',
  estimatedCloseDate: '',
  probability: '0',
  actualRevenue: '',
  actualCloseDate: '',
  currencyId: '',
  sourceId: '',
  winReasonId: '',
  lossReasonId: '',
  lostToCompetitorId: '',
  description: '',
  notes: '',
  ownerUserId: '',
  ownerTeamId: '',
  isActive: true,
}

export const opportunityToForm = (opportunity: Opportunity): OpportunityFormState => ({
  opportunityNumber: opportunity.opportunityNumber,
  topic: opportunity.topic,
  accountId: opportunity.accountId,
  contactId: opportunity.contactId ?? '',
  leadId: opportunity.leadId ?? '',
  opportunityStageId: opportunity.opportunityStageId,
  opportunityStatusId: opportunity.opportunityStatusId,
  salesProcessStageId: opportunity.salesProcessStageId ?? '',
  ratingId: opportunity.ratingId ?? '',
  priorityId: opportunity.priorityId ?? '',
  estimatedRevenue: opportunity.estimatedRevenue === undefined || opportunity.estimatedRevenue === null ? '' : String(opportunity.estimatedRevenue),
  estimatedCloseDate: toDateInput(opportunity.estimatedCloseDate),
  probability: String(opportunity.probability ?? 0),
  actualRevenue: opportunity.actualRevenue === undefined || opportunity.actualRevenue === null ? '' : String(opportunity.actualRevenue),
  actualCloseDate: toDateInput(opportunity.actualCloseDate),
  currencyId: opportunity.currencyId ?? '',
  sourceId: opportunity.sourceId ?? '',
  winReasonId: opportunity.winReasonId ?? '',
  lossReasonId: opportunity.lossReasonId ?? '',
  lostToCompetitorId: opportunity.lostToCompetitorId ?? '',
  description: opportunity.description ?? '',
  notes: opportunity.notes ?? '',
  ownerUserId: opportunity.ownerUserId ?? '',
  ownerTeamId: opportunity.ownerTeamId ?? '',
  isActive: opportunity.isActive,
})

export const opportunityPayload = (form: OpportunityFormState) => ({
  opportunityNumber: form.opportunityNumber.trim(),
  topic: form.topic.trim(),
  accountId: form.accountId,
  contactId: nullIfBlank(form.contactId),
  leadId: nullIfBlank(form.leadId),
  opportunityStageId: form.opportunityStageId,
  opportunityStatusId: form.opportunityStatusId,
  salesProcessStageId: nullIfBlank(form.salesProcessStageId),
  ratingId: nullIfBlank(form.ratingId),
  priorityId: nullIfBlank(form.priorityId),
  estimatedRevenue: form.estimatedRevenue.trim() ? Number(form.estimatedRevenue) : null,
  estimatedCloseDate: nullIfBlank(form.estimatedCloseDate),
  probability: Number(form.probability || 0),
  actualRevenue: form.actualRevenue.trim() ? Number(form.actualRevenue) : null,
  actualCloseDate: nullIfBlank(form.actualCloseDate),
  currencyId: nullIfBlank(form.currencyId),
  sourceId: nullIfBlank(form.sourceId),
  winReasonId: nullIfBlank(form.winReasonId),
  lossReasonId: nullIfBlank(form.lossReasonId),
  lostToCompetitorId: nullIfBlank(form.lostToCompetitorId),
  description: nullIfBlank(form.description),
  notes: nullIfBlank(form.notes),
  ownerUserId: nullIfBlank(form.ownerUserId),
  ownerTeamId: nullIfBlank(form.ownerTeamId),
  isActive: form.isActive,
})

export { formatCurrency, formatDate, formatDateTime, nullIfBlank }
