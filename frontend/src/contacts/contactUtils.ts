import type { Contact } from '../types/models'

export type ContactFormState = {
  contactNumber: string
  accountId: string
  contactRoleId: string
  salutationLookupId: string
  genderLookupId: string
  firstName: string
  middleName: string
  lastName: string
  preferredName: string
  jobTitle: string
  department: string
  email: string
  alternateEmail: string
  workPhone: string
  mobilePhone: string
  homePhone: string
  fax: string
  preferredContactMethodId: string
  preferredLanguageId: string
  preferredTimeZoneId: string
  marketingConsent: boolean
  emailOptIn: boolean
  smsOptIn: boolean
  phoneOptIn: boolean
  isPrimaryContact: boolean
  dateOfBirth: string
  notes: string
  ownerUserId: string
  ownerTeamId: string
  isActive: boolean
}

export const emptyContactForm: ContactFormState = {
  contactNumber: '',
  accountId: '',
  contactRoleId: '',
  salutationLookupId: '',
  genderLookupId: '',
  firstName: '',
  middleName: '',
  lastName: '',
  preferredName: '',
  jobTitle: '',
  department: '',
  email: '',
  alternateEmail: '',
  workPhone: '',
  mobilePhone: '',
  homePhone: '',
  fax: '',
  preferredContactMethodId: '',
  preferredLanguageId: '',
  preferredTimeZoneId: '',
  marketingConsent: false,
  emailOptIn: false,
  smsOptIn: false,
  phoneOptIn: false,
  isPrimaryContact: false,
  dateOfBirth: '',
  notes: '',
  ownerUserId: '',
  ownerTeamId: '',
  isActive: true,
}

export const toDateInput = (value?: string) => (value ? value.slice(0, 10) : '')
export const nullIfBlank = (value: string) => value.trim() || null

export const contactToForm = (contact: Contact): ContactFormState => ({
  contactNumber: contact.contactNumber,
  accountId: contact.accountId,
  contactRoleId: contact.contactRoleId ?? '',
  salutationLookupId: contact.salutationLookupId ?? '',
  genderLookupId: contact.genderLookupId ?? '',
  firstName: contact.firstName,
  middleName: contact.middleName ?? '',
  lastName: contact.lastName,
  preferredName: contact.preferredName ?? '',
  jobTitle: contact.jobTitle ?? '',
  department: contact.department ?? '',
  email: contact.email ?? '',
  alternateEmail: contact.alternateEmail ?? '',
  workPhone: contact.workPhone ?? '',
  mobilePhone: contact.mobilePhone ?? '',
  homePhone: contact.homePhone ?? '',
  fax: contact.fax ?? '',
  preferredContactMethodId: contact.preferredContactMethodId ?? '',
  preferredLanguageId: contact.preferredLanguageId ?? '',
  preferredTimeZoneId: contact.preferredTimeZoneId ?? '',
  marketingConsent: contact.marketingConsent,
  emailOptIn: contact.emailOptIn,
  smsOptIn: contact.smsOptIn,
  phoneOptIn: contact.phoneOptIn,
  isPrimaryContact: contact.isPrimaryContact,
  dateOfBirth: toDateInput(contact.dateOfBirth),
  notes: contact.notes ?? '',
  ownerUserId: contact.ownerUserId ?? '',
  ownerTeamId: contact.ownerTeamId ?? '',
  isActive: contact.isActive,
})

export const contactPayload = (form: ContactFormState) => ({
  contactNumber: form.contactNumber.trim(),
  accountId: form.accountId,
  contactRoleId: nullIfBlank(form.contactRoleId),
  salutationLookupId: nullIfBlank(form.salutationLookupId),
  genderLookupId: nullIfBlank(form.genderLookupId),
  firstName: form.firstName.trim(),
  middleName: nullIfBlank(form.middleName),
  lastName: form.lastName.trim(),
  preferredName: nullIfBlank(form.preferredName),
  jobTitle: nullIfBlank(form.jobTitle),
  department: nullIfBlank(form.department),
  email: nullIfBlank(form.email),
  alternateEmail: nullIfBlank(form.alternateEmail),
  workPhone: nullIfBlank(form.workPhone),
  mobilePhone: nullIfBlank(form.mobilePhone),
  homePhone: nullIfBlank(form.homePhone),
  fax: nullIfBlank(form.fax),
  preferredContactMethodId: nullIfBlank(form.preferredContactMethodId),
  preferredLanguageId: nullIfBlank(form.preferredLanguageId),
  preferredTimeZoneId: nullIfBlank(form.preferredTimeZoneId),
  marketingConsent: form.marketingConsent,
  emailOptIn: form.emailOptIn,
  smsOptIn: form.smsOptIn,
  phoneOptIn: form.phoneOptIn,
  isPrimaryContact: form.isPrimaryContact,
  dateOfBirth: nullIfBlank(form.dateOfBirth),
  notes: nullIfBlank(form.notes),
  ownerUserId: nullIfBlank(form.ownerUserId),
  ownerTeamId: nullIfBlank(form.ownerTeamId),
  isActive: form.isActive,
})

export const formatDateTime = (value?: string) => {
  if (!value) {
    return ''
  }

  const parsed = new Date(value)
  return Number.isNaN(parsed.getTime()) ? value : parsed.toLocaleString()
}
