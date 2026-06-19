import type { RevenueForecast, SalesTarget } from '../types/models'

export type SalesTargetFormState = {
  name: string
  description: string
  targetTypeId: string
  targetPeriodId: string
  startDate: string
  endDate: string
  targetAmount: string
  actualAmount: string
  assignedUserId: string
  assignedTeamId: string
  ownerUserId: string
  ownerTeamId: string
  isActive: boolean
}

export type ForecastFormState = {
  forecastDate: string
  forecastPeriodStart: string
  forecastPeriodEnd: string
  forecastTypeId: string
  forecastRevenue: string
  notes: string
}

export const nullIfBlank = (value: string) => value.trim() || null

export const toDateInput = (value?: string) => {
  if (!value) {
    return ''
  }

  return value.slice(0, 10)
}

export const todayDateInput = () => new Date().toISOString().slice(0, 10)

export const emptySalesTargetForm: SalesTargetFormState = {
  name: '',
  description: '',
  targetTypeId: '',
  targetPeriodId: '',
  startDate: todayDateInput(),
  endDate: todayDateInput(),
  targetAmount: '',
  actualAmount: '',
  assignedUserId: '',
  assignedTeamId: '',
  ownerUserId: '',
  ownerTeamId: '',
  isActive: true,
}

export const emptyForecastForm: ForecastFormState = {
  forecastDate: todayDateInput(),
  forecastPeriodStart: todayDateInput(),
  forecastPeriodEnd: todayDateInput(),
  forecastTypeId: '',
  forecastRevenue: '',
  notes: '',
}

export const salesTargetToForm = (target: SalesTarget): SalesTargetFormState => ({
  name: target.name,
  description: target.description ?? '',
  targetTypeId: target.targetTypeId,
  targetPeriodId: target.targetPeriodId,
  startDate: toDateInput(target.startDate),
  endDate: toDateInput(target.endDate),
  targetAmount: String(target.targetAmount),
  actualAmount: String(target.actualAmount),
  assignedUserId: target.assignedUserId ?? '',
  assignedTeamId: target.assignedTeamId ?? '',
  ownerUserId: target.ownerUserId ?? '',
  ownerTeamId: target.ownerTeamId ?? '',
  isActive: target.isActive,
})

export const salesTargetPayload = (form: SalesTargetFormState) => ({
  name: form.name.trim(),
  description: nullIfBlank(form.description),
  targetTypeId: form.targetTypeId,
  targetPeriodId: form.targetPeriodId,
  startDate: form.startDate,
  endDate: form.endDate,
  targetAmount: Number(form.targetAmount || 0),
  actualAmount: Number(form.actualAmount || 0),
  assignedUserId: nullIfBlank(form.assignedUserId),
  assignedTeamId: nullIfBlank(form.assignedTeamId),
  ownerUserId: nullIfBlank(form.ownerUserId),
  ownerTeamId: nullIfBlank(form.ownerTeamId),
  isActive: form.isActive,
})

export const forecastToForm = (forecast: RevenueForecast): ForecastFormState => ({
  forecastDate: toDateInput(forecast.forecastDate),
  forecastPeriodStart: toDateInput(forecast.forecastPeriodStart),
  forecastPeriodEnd: toDateInput(forecast.forecastPeriodEnd),
  forecastTypeId: forecast.forecastTypeId,
  forecastRevenue: String(forecast.forecastRevenue),
  notes: forecast.notes ?? '',
})

export const forecastPayload = (form: ForecastFormState) => ({
  forecastDate: form.forecastDate,
  forecastPeriodStart: form.forecastPeriodStart,
  forecastPeriodEnd: form.forecastPeriodEnd,
  forecastTypeId: form.forecastTypeId,
  forecastRevenue: form.forecastRevenue.trim() ? Number(form.forecastRevenue) : null,
  notes: nullIfBlank(form.notes),
})

export const formatCurrency = (value?: number) => {
  if (value === undefined || value === null) {
    return ''
  }

  return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(value)
}

export const formatNumber = (value?: number) => new Intl.NumberFormat(undefined, { maximumFractionDigits: 1 }).format(value ?? 0)

export const formatPercent = (value?: number) => `${formatNumber(value)}%`

export const formatDate = (value?: string) => {
  if (!value) {
    return ''
  }

  const parsed = new Date(value)
  return Number.isNaN(parsed.getTime()) ? value.slice(0, 10) : parsed.toLocaleDateString()
}

export const formatDateTime = (value?: string) => {
  if (!value) {
    return ''
  }

  const parsed = new Date(value)
  return Number.isNaN(parsed.getTime()) ? value : parsed.toLocaleString()
}
