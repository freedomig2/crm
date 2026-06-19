import { useEffect, useMemo, useState } from 'react'
import { Field, Input, MessageBar, MessageBarBody, Switch, Textarea } from '@fluentui/react-components'
import { PersonCallRegular } from '@fluentui/react-icons'
import { useNavigate, useParams } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { loadNumberSequencePreview } from '../configuration/numberSequenceUtils'
import {
  EntityHeader,
  EntityPageLayout,
  EntityTabPlaceholder,
  EntityTabs,
  FormSectionCard,
  LookupCombobox,
  StickySaveBar,
} from '../components/entity-ui/EntityComponents'
import type { Contact } from '../types/models'
import { AuditHistoryPanel, ContactCommunicationsPanel, ContactInteractionsPanel } from './ContactRelatedPanels'
import { contactPayload, contactToForm, emptyContactForm, type ContactFormState } from './contactUtils'

const tabs = [
  { key: 'general', label: 'General' },
  { key: 'communication', label: 'Communication' },
  { key: 'preferences', label: 'Preferences' },
  { key: 'relationships', label: 'Relationships' },
  { key: 'interaction-history', label: 'Interaction History' },
  { key: 'audit-history', label: 'Audit History' },
]

export function ContactFormPage({ mode }: { mode: 'create' | 'edit' }) {
  const navigate = useNavigate()
  const { id } = useParams()
  const { hasPermission } = useAuth()
  const isEdit = mode === 'edit'
  const canCreate = hasPermission('Contacts.Create')
  const canUpdate = hasPermission('Contacts.Update')
  const canSetPrimary = hasPermission('Contacts.SetPrimary')
  const canSave = isEdit ? canUpdate : canCreate
  const [form, setForm] = useState<ContactFormState>(emptyContactForm)
  const [contact, setContact] = useState<Contact | null>(null)
  const [activeTab, setActiveTab] = useState('general')
  const [loading, setLoading] = useState(isEdit)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({})

  useEffect(() => {
    if (isEdit) {
      return
    }

    let active = true
    void loadNumberSequencePreview('CONTACT')
      .then((preview) => {
        if (active) {
          setForm((current) => ({ ...current, contactNumber: preview || 'Generated on save' }))
        }
      })
      .catch(() => {
        if (active) {
          setForm((current) => ({ ...current, contactNumber: 'Generated on save' }))
        }
      })

    return () => {
      active = false
    }
  }, [isEdit])

  useEffect(() => {
    if (!isEdit || !id) {
      return
    }

    void (async () => {
      setLoading(true)
      setError('')
      try {
        const { data } = await api.get<Contact>(`api/contacts/${id}`)
        setContact(data)
        setForm(contactToForm(data))
      } catch {
        setError('Failed to load contact.')
      } finally {
        setLoading(false)
      }
    })()
  }, [id, isEdit])

  const title = useMemo(() => (isEdit ? 'Edit Contact' : 'Create Contact'), [isEdit])
  const validationSummary = Object.values(fieldErrors)

  const setValue = <K extends keyof ContactFormState>(key: K, value: ContactFormState[K]) => {
    setForm((current) => ({ ...current, [key]: value }))
  }

  const validate = () => {
    const next: Record<string, string> = {}
    if (!form.accountId.trim()) next.accountId = 'Account is required.'
    if (!form.firstName.trim()) next.firstName = 'First name is required.'
    if (!form.lastName.trim()) next.lastName = 'Last name is required.'
    if (form.isPrimaryContact && !canSetPrimary && !contact?.isPrimaryContact) next.isPrimaryContact = 'Set primary permission is required.'
    setFieldErrors(next)
    return Object.keys(next).length === 0
  }

  const save = async (closeAfterSave: boolean) => {
    if (!canSave || !validate()) {
      return
    }

    setSaving(true)
    setError('')
    try {
      if (isEdit && id) {
        await api.put(`api/contacts/${id}`, contactPayload(form))
        if (closeAfterSave) {
          navigate('/contacts')
        } else {
          const { data } = await api.get<Contact>(`api/contacts/${id}`)
          setContact(data)
          setForm(contactToForm(data))
        }
      } else {
        const { data } = await api.post<Contact>('api/contacts', contactPayload(form))
        navigate(closeAfterSave ? '/contacts' : `/contacts/${data.id}/edit`)
      }
    } catch {
      setError('Save failed. Please review contact values.')
    } finally {
      setSaving(false)
    }
  }

  const renderText = (key: keyof ContactFormState, label: string, required?: boolean, type: 'text' | 'date' | 'email' = 'text', readOnly?: boolean) => (
    <Field label={label} required={required} validationMessage={fieldErrors[String(key)]}>
      <Input
        size="small"
        type={type}
        value={String(form[key] ?? '')}
        onChange={(_, data) => setValue(key, data.value as ContactFormState[typeof key])}
        readOnly={readOnly || !canSave}
      />
    </Field>
  )

  const renderLookup = (key: keyof ContactFormState, label: string, required?: boolean, disabled?: boolean) => (
    <Field label={label} required={required} validationMessage={fieldErrors[String(key)]}>
      <LookupCombobox
        fieldKey={String(key)}
        value={String(form[key] ?? '')}
        disabled={disabled || !canSave}
        onChange={(value) => setValue(key, value as ContactFormState[typeof key])}
      />
    </Field>
  )

  const renderSwitch = (key: keyof ContactFormState, label: string, disabled?: boolean) => (
    <Field label={label} validationMessage={fieldErrors[String(key)]}>
      <Switch
        checked={Boolean(form[key])}
        disabled={disabled || !canSave}
        onChange={(_, data) => setValue(key, Boolean(data.checked) as ContactFormState[typeof key])}
      />
    </Field>
  )

  const alerts = [
    ...(!canSave ? [{ intent: 'error' as const, text: `You do not have permission to ${isEdit ? 'update' : 'create'} contacts.` }] : []),
    ...(error ? [{ intent: 'error' as const, text: error }] : []),
    ...validationSummary.map((message) => ({ intent: 'warning' as const, text: message })),
  ]

  const headerActions = [
    { key: 'save', label: 'Save', onClick: () => void save(false), appearance: 'primary' as const, disabled: loading || saving || !canSave },
    { key: 'save-close', label: 'Save & Close', onClick: () => void save(true), appearance: 'secondary' as const, disabled: loading || saving || !canSave },
    { key: 'cancel', label: 'Cancel', onClick: () => navigate('/contacts'), appearance: 'subtle' as const },
  ]

  return (
    <EntityPageLayout
      header={<EntityHeader icon={<PersonCallRegular />} title={title} subtitle="Contact management for individual people." status={contact?.isActive ? 'Active' : isEdit ? 'Inactive' : undefined} actions={headerActions} />}
      tabs={<EntityTabs tabs={tabs} activeTab={activeTab} onTabChange={setActiveTab} />}
      alerts={alerts}
      stickyBar={
        <StickySaveBar
          onSave={() => void save(false)}
          onSaveAndClose={() => void save(true)}
          onCancel={() => navigate('/contacts')}
          disableActions={loading || saving || !canSave}
        />
      }
    >
      {loading ? (
        <MessageBar>
          <MessageBarBody>Loading contact...</MessageBarBody>
        </MessageBar>
      ) : null}

      {!loading && activeTab === 'general' ? (
        <>
          <FormSectionCard title="General">
            {renderText('contactNumber', 'Contact Number', false, 'text', true)}
            {renderLookup('salutationLookupId', 'Salutation')}
            {renderText('firstName', 'First Name', true)}
            {renderText('middleName', 'Middle Name')}
            {renderText('lastName', 'Last Name', true)}
            {renderText('preferredName', 'Preferred Name')}
            {renderLookup('genderLookupId', 'Gender')}
            {renderText('dateOfBirth', 'Date Of Birth', false, 'date')}
          </FormSectionCard>
          <FormSectionCard title="Professional">
            {renderText('jobTitle', 'Job Title')}
            {renderText('department', 'Department')}
          </FormSectionCard>
          <FormSectionCard title="Notes">
            <Field label="Notes">
              <Textarea value={form.notes} readOnly={!canSave} onChange={(_, data) => setValue('notes', data.value)} />
            </Field>
            {renderSwitch('isActive', 'Active')}
          </FormSectionCard>
        </>
      ) : null}

      {!loading && activeTab === 'communication' ? (
        <>
          <FormSectionCard title="Communication">
            {renderText('email', 'Email', false, 'email')}
            {renderText('alternateEmail', 'Alternate Email', false, 'email')}
            {renderText('mobilePhone', 'Mobile')}
            {renderText('workPhone', 'Work Phone')}
            {renderText('homePhone', 'Home Phone')}
            {renderText('fax', 'Fax')}
          </FormSectionCard>
          {isEdit && id ? <ContactCommunicationsPanel contactId={id} editable /> : <EntityTabPlaceholder text="Save the contact before adding communication channels." />}
        </>
      ) : null}

      {!loading && activeTab === 'preferences' ? (
        <FormSectionCard title="Preferences">
          {renderLookup('preferredContactMethodId', 'Preferred Contact Method')}
          {renderLookup('preferredLanguageId', 'Preferred Language')}
          {renderLookup('preferredTimeZoneId', 'Preferred Time Zone')}
          {renderSwitch('emailOptIn', 'Email Opt-In')}
          {renderSwitch('smsOptIn', 'SMS Opt-In')}
          {renderSwitch('phoneOptIn', 'Phone Opt-In')}
          {renderSwitch('marketingConsent', 'Marketing Consent')}
        </FormSectionCard>
      ) : null}

      {!loading && activeTab === 'relationships' ? (
        <FormSectionCard title="Relationships">
          {renderLookup('accountId', 'Account', true)}
          {renderLookup('contactRoleId', 'Contact Role')}
          {renderSwitch('isPrimaryContact', 'Primary Contact', !canSetPrimary && !contact?.isPrimaryContact)}
        </FormSectionCard>
      ) : null}

      {!loading && activeTab === 'interaction-history' ? (
        isEdit && id ? <ContactInteractionsPanel contactId={id} accountId={form.accountId} editable /> : <EntityTabPlaceholder text="Save the contact before adding interaction history." />
      ) : null}

      {!loading && activeTab === 'audit-history' ? (
        isEdit && id ? <AuditHistoryPanel entityName="Contact" entityId={id} /> : <EntityTabPlaceholder text="Audit history appears after the contact is saved." />
      ) : null}
    </EntityPageLayout>
  )
}
