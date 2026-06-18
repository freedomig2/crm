import { api } from '../../api/client'

type AnyRecord = Record<string, unknown>

export type LookupOption = {
  value: string
  label: string
}

const cache = new Map<string, LookupOption[]>()

const guidLike = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i

const endpointForFieldKey = (fieldKey: string): string => {
  if (fieldKey.includes('ownerUser') || fieldKey.includes('assignedToUser') || fieldKey.includes('createdBy') || fieldKey.includes('updatedBy')) {
    return 'api/users'
  }

  if (fieldKey.includes('ownerTeam') || fieldKey.includes('team')) {
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

  if (fieldKey.includes('role')) {
    return 'api/roles'
  }

  if (fieldKey.includes('lookupCategory')) {
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
  const subject = asText(record.subject)

  if (name && accountNumber) {
    return `${name} (${accountNumber})`
  }

  if (name && code) {
    return `${name} (${code})`
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

export const loadLookupOptions = async (fieldKey: string): Promise<LookupOption[]> => {
  const key = fieldKey.toLowerCase()
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
