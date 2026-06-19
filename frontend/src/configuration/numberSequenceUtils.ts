import { api } from '../api/client'
import type { NumberSequence, NumberSequencePreview } from '../types/models'

export type NumberSequenceFormState = {
  entityName: string
  sequenceCode: string
  description: string
  isActive: boolean
  prefix: string
  separator: string
  minimumDigits: string
  includeYear: boolean
  includeMonth: boolean
  includeDay: boolean
  suffix: string
  formatPreview: string
  resetFrequencyId: string
  lastResetDate: string
  currentNumber: string
  nextNumber: string
}

export const emptyNumberSequenceForm: NumberSequenceFormState = {
  entityName: '',
  sequenceCode: '',
  description: '',
  isActive: true,
  prefix: '',
  separator: '-',
  minimumDigits: '6',
  includeYear: false,
  includeMonth: false,
  includeDay: false,
  suffix: '',
  formatPreview: 'Generated after save',
  resetFrequencyId: '',
  lastResetDate: '',
  currentNumber: '0',
  nextNumber: '1',
}

export const nullIfBlank = (value: string) => value.trim() || null

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

export const fromDateTimeInput = (value: string) => (value ? new Date(value).toISOString() : null)

export const formatDateTime = (value?: string) => {
  if (!value) {
    return ''
  }

  const parsed = new Date(value)
  return Number.isNaN(parsed.getTime()) ? value : parsed.toLocaleString()
}

export const numberSequenceToForm = (sequence: NumberSequence): NumberSequenceFormState => ({
  entityName: sequence.entityName,
  sequenceCode: sequence.sequenceCode,
  description: sequence.description ?? '',
  isActive: sequence.isActive,
  prefix: sequence.prefix,
  separator: sequence.separator || '-',
  minimumDigits: String(sequence.minimumDigits),
  includeYear: sequence.includeYear,
  includeMonth: sequence.includeMonth,
  includeDay: sequence.includeDay,
  suffix: sequence.suffix ?? '',
  formatPreview: sequence.formatPreview ?? '',
  resetFrequencyId: sequence.resetFrequencyId ?? '',
  lastResetDate: toDateTimeInput(sequence.lastResetDate),
  currentNumber: String(sequence.currentNumber),
  nextNumber: String(sequence.nextNumber),
})

export const numberSequencePayload = (form: NumberSequenceFormState) => ({
  entityName: form.entityName.trim(),
  sequenceCode: form.sequenceCode.trim(),
  description: nullIfBlank(form.description),
  isActive: form.isActive,
  prefix: form.prefix.trim(),
  separator: form.separator.trim() || '-',
  minimumDigits: Number(form.minimumDigits || 6),
  includeYear: form.includeYear,
  includeMonth: form.includeMonth,
  includeDay: form.includeDay,
  suffix: nullIfBlank(form.suffix),
  resetFrequencyId: nullIfBlank(form.resetFrequencyId),
  lastResetDate: fromDateTimeInput(form.lastResetDate),
  currentNumber: Number(form.currentNumber || 0),
  nextNumber: Number(form.nextNumber || 1),
})

export const loadNumberSequencePreview = async (sequenceCode: string) => {
  const { data: sequence } = await api.get<NumberSequence>(`api/number-sequences/by-code/${sequenceCode}`)
  const { data } = await api.post<NumberSequencePreview>(`api/number-sequences/${sequence.id}/preview`)
  return data.preview
}
