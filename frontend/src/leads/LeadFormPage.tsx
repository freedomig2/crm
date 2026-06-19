import { useEffect, useMemo, useState } from 'react'
import { Field, Input, MessageBar, MessageBarBody, Switch, Textarea } from '@fluentui/react-components'
import { PeopleRegular } from '@fluentui/react-icons'
import { useNavigate, useParams } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { loadNumberSequencePreview } from '../configuration/numberSequenceUtils'
import { loadLookupOptionsByCategoryCode } from '../components/entity-ui/referenceData'
import {
  EntityHeader,
  EntityPageLayout,
  EntityTabPlaceholder,
  EntityTabs,
  FormSectionCard,
  LookupCombobox,
  StickySaveBar,
} from '../components/entity-ui/EntityComponents'
import type { Lead } from '../types/models'
import { AuditHistoryPanel } from '../contacts/ContactRelatedPanels'
import { LeadActivitiesPanel, RelatedConversionPanel } from './LeadRelatedPanels'
import { emptyLeadForm, leadPayload, leadToForm, type LeadFormState } from './leadUtils'

const tabs = [
  { key: 'general', label: 'General' },
  { key: 'qualification', label: 'Qualification' },
  { key: 'scoring', label: 'Scoring' },
  { key: 'activities', label: 'Activities' },
  { key: 'conversion', label: 'Conversion' },
  { key: 'audit-history', label: 'Audit History' },
]

export function LeadFormPage({ mode }: { mode: 'create' | 'edit' }) {
  const navigate = useNavigate()
  const { id } = useParams()
  const { hasPermission } = useAuth()
  const isEdit = mode === 'edit'
  const canCreate = hasPermission('Leads.Create')
  const canUpdate = hasPermission('Leads.Update')
  const canSave = isEdit ? canUpdate : canCreate
  const [form, setForm] = useState<LeadFormState>(emptyLeadForm)
  const [lead, setLead] = useState<Lead | null>(null)
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
    void loadNumberSequencePreview('LEAD')
      .then((preview) => {
        if (active) {
          setForm((current) => ({ ...current, leadNumber: preview || 'Generated on save' }))
        }
      })
      .catch(() => {
        if (active) {
          setForm((current) => ({ ...current, leadNumber: 'Generated on save' }))
        }
      })

    return () => {
      active = false
    }
  }, [isEdit])

  useEffect(() => {
    if (isEdit || form.leadStatusId) {
      return
    }

    void (async () => {
      const statuses = await loadLookupOptionsByCategoryCode('LEAD_STATUS')
      const status = statuses.find((item) => item.label.toLowerCase() === 'new') ?? statuses[0]
      if (status) {
        setForm((current) => ({ ...current, leadStatusId: status.value }))
      }
    })()
  }, [form.leadStatusId, isEdit])

  useEffect(() => {
    if (!isEdit || !id) {
      return
    }

    void (async () => {
      setLoading(true)
      setError('')
      try {
        const { data } = await api.get<Lead>(`api/leads/${id}`)
        setLead(data)
        setForm(leadToForm(data))
      } catch {
        setError('Failed to load lead.')
      } finally {
        setLoading(false)
      }
    })()
  }, [id, isEdit])

  const title = useMemo(() => (isEdit ? 'Edit Lead' : 'Create Lead'), [isEdit])
  const validationSummary = Object.values(fieldErrors)

  const setValue = <K extends keyof LeadFormState>(key: K, value: LeadFormState[K]) => {
    setForm((current) => ({ ...current, [key]: value }))
  }

  const validate = () => {
    const next: Record<string, string> = {}
    if (!form.topic.trim()) next.topic = 'Topic is required.'
    if (!form.leadStatusId.trim()) next.leadStatusId = 'Status is required.'
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
        await api.put(`api/leads/${id}`, leadPayload(form))
        if (closeAfterSave) {
          navigate('/leads')
        } else {
          const { data } = await api.get<Lead>(`api/leads/${id}`)
          setLead(data)
          setForm(leadToForm(data))
        }
      } else {
        const { data } = await api.post<Lead>('api/leads', leadPayload(form))
        navigate(closeAfterSave ? '/leads' : `/leads/${data.id}/edit`)
      }
    } catch {
      setError('Save failed. Please review lead values.')
    } finally {
      setSaving(false)
    }
  }

  const renderText = (key: keyof LeadFormState, label: string, required?: boolean, type: 'text' | 'date' | 'email' | 'number' | 'url' = 'text', readOnly?: boolean) => (
    <Field label={label} required={required} validationMessage={fieldErrors[String(key)]}>
      <Input
        size="small"
        type={type}
        value={String(form[key] ?? '')}
        onChange={(_, data) => setValue(key, data.value as LeadFormState[typeof key])}
        readOnly={readOnly || !canSave}
      />
    </Field>
  )

  const renderLookup = (key: keyof LeadFormState, label: string, required?: boolean, disabled?: boolean) => (
    <Field label={label} required={required} validationMessage={fieldErrors[String(key)]}>
      <LookupCombobox
        fieldKey={String(key)}
        value={String(form[key] ?? '')}
        disabled={disabled || !canSave}
        onChange={(value) => setValue(key, value as LeadFormState[typeof key])}
      />
    </Field>
  )

  const renderSwitch = (key: keyof LeadFormState, label: string) => (
    <Field label={label} validationMessage={fieldErrors[String(key)]}>
      <Switch
        checked={Boolean(form[key])}
        disabled={!canSave}
        onChange={(_, data) => setValue(key, Boolean(data.checked) as LeadFormState[typeof key])}
      />
    </Field>
  )

  const alerts = [
    ...(!canSave ? [{ intent: 'error' as const, text: `You do not have permission to ${isEdit ? 'update' : 'create'} leads.` }] : []),
    ...(error ? [{ intent: 'error' as const, text: error }] : []),
    ...validationSummary.map((message) => ({ intent: 'warning' as const, text: message })),
  ]

  const headerActions = [
    { key: 'save', label: 'Save', onClick: () => void save(false), appearance: 'primary' as const, disabled: loading || saving || !canSave },
    { key: 'save-close', label: 'Save & Close', onClick: () => void save(true), appearance: 'secondary' as const, disabled: loading || saving || !canSave },
    { key: 'cancel', label: 'Cancel', onClick: () => navigate('/leads'), appearance: 'subtle' as const },
  ]

  return (
    <EntityPageLayout
      header={<EntityHeader icon={<PeopleRegular />} title={title} subtitle="Lead capture, qualification, scoring, and conversion." status={lead?.leadStatusName ?? (isEdit ? undefined : 'New')} actions={headerActions} />}
      tabs={<EntityTabs tabs={tabs} activeTab={activeTab} onTabChange={setActiveTab} />}
      alerts={alerts}
      stickyBar={
        <StickySaveBar
          onSave={() => void save(false)}
          onSaveAndClose={() => void save(true)}
          onCancel={() => navigate('/leads')}
          disableActions={loading || saving || !canSave}
        />
      }
    >
      {loading ? (
        <MessageBar>
          <MessageBarBody>Loading lead...</MessageBarBody>
        </MessageBar>
      ) : null}

      {!loading && activeTab === 'general' ? (
        <>
          <FormSectionCard title="General Information">
            {renderText('leadNumber', 'Lead Number', false, 'text', true)}
            {renderText('topic', 'Topic', true)}
            {renderLookup('leadSourceId', 'Lead Source')}
            {renderLookup('leadStatusId', 'Status', true)}
            {renderLookup('ratingId', 'Rating')}
            {renderLookup('industryId', 'Industry')}
          </FormSectionCard>
          <FormSectionCard title="Contact Information">
            {renderText('firstName', 'First Name')}
            {renderText('middleName', 'Middle Name')}
            {renderText('lastName', 'Last Name')}
            {renderText('companyName', 'Company Name')}
            {renderText('jobTitle', 'Job Title')}
            {renderText('email', 'Email', false, 'email')}
            {renderText('mobilePhone', 'Mobile Phone')}
            {renderText('workPhone', 'Work Phone')}
            {renderText('website', 'Website', false, 'url')}
          </FormSectionCard>
          <FormSectionCard title="Notes">
            <Field label="Description">
              <Textarea value={form.description} readOnly={!canSave} onChange={(_, data) => setValue('description', data.value)} />
            </Field>
            <Field label="Notes">
              <Textarea value={form.notes} readOnly={!canSave} onChange={(_, data) => setValue('notes', data.value)} />
            </Field>
            {renderSwitch('isActive', 'Active')}
          </FormSectionCard>
        </>
      ) : null}

      {!loading && activeTab === 'qualification' ? (
        <>
          <FormSectionCard title="Qualification">
            {renderLookup('qualificationStatusId', 'Qualification Status')}
            {renderText('estimatedValue', 'Estimated Value', false, 'number')}
            {renderText('estimatedCloseDate', 'Estimated Close Date', false, 'date')}
            {renderLookup('disqualifiedReasonId', 'Disqualification Reason')}
          </FormSectionCard>
          <FormSectionCard title="Assignment">
            {renderLookup('assignedToUserId', 'Assigned To User')}
            {renderLookup('assignedToTeamId', 'Assigned To Team')}
            {renderLookup('ownerUserId', 'Owner User')}
            {renderLookup('ownerTeamId', 'Owner Team')}
          </FormSectionCard>
        </>
      ) : null}

      {!loading && activeTab === 'scoring' ? (
        <FormSectionCard title="Scoring">
          <Field label="Score">
            <Input size="small" value={String(lead?.score ?? 0)} readOnly />
          </Field>
          <Field label="Score Grade">
            <Input size="small" value={lead?.scoreGrade ?? 'Cold'} readOnly />
          </Field>
        </FormSectionCard>
      ) : null}

      {!loading && activeTab === 'activities' ? (
        isEdit && id ? <LeadActivitiesPanel leadId={id} editable /> : <EntityTabPlaceholder text="Save the lead before adding activities." />
      ) : null}

      {!loading && activeTab === 'conversion' ? (
        isEdit && lead ? <RelatedConversionPanel accountId={lead.convertedAccountId} contactId={lead.convertedContactId} /> : <EntityTabPlaceholder text="Conversion details appear after the lead is saved." />
      ) : null}

      {!loading && activeTab === 'audit-history' ? (
        isEdit && id ? <AuditHistoryPanel entityName="Lead" entityId={id} /> : <EntityTabPlaceholder text="Audit history appears after the lead is saved." />
      ) : null}
    </EntityPageLayout>
  )
}
