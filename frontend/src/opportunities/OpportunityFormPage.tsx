import { useEffect, useMemo, useState } from 'react'
import { Field, Input, MessageBar, MessageBarBody, Switch, Textarea } from '@fluentui/react-components'
import { DataPieRegular } from '@fluentui/react-icons'
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
import type { Opportunity } from '../types/models'
import { AuditHistoryPanel } from '../contacts/ContactRelatedPanels'
import { OpportunityActivitiesPanel, OpportunityCompetitorsPanel, OpportunityProductsPanel } from './OpportunityRelatedPanels'
import { emptyOpportunityForm, opportunityPayload, opportunityToForm, type OpportunityFormState } from './opportunityUtils'

const tabs = [
  { key: 'general', label: 'General' },
  { key: 'products', label: 'Products' },
  { key: 'competitors', label: 'Competitors' },
  { key: 'activities', label: 'Activities' },
  { key: 'win-loss', label: 'Win/Loss' },
  { key: 'audit-history', label: 'Audit History' },
]

export function OpportunityFormPage({ mode }: { mode: 'create' | 'edit' }) {
  const navigate = useNavigate()
  const { id } = useParams()
  const { hasPermission } = useAuth()
  const isEdit = mode === 'edit'
  const canCreate = hasPermission('Opportunities.Create')
  const canUpdate = hasPermission('Opportunities.Update')
  const canSave = isEdit ? canUpdate : canCreate
  const [form, setForm] = useState<OpportunityFormState>(emptyOpportunityForm)
  const [opportunity, setOpportunity] = useState<Opportunity | null>(null)
  const [activeTab, setActiveTab] = useState('general')
  const [loading, setLoading] = useState(isEdit)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({})

  useEffect(() => {
    if (isEdit) return
    let active = true
    void loadNumberSequencePreview('OPPORTUNITY')
      .then((preview) => {
        if (active) setForm((current) => ({ ...current, opportunityNumber: preview || 'Generated on save' }))
      })
      .catch(() => {
        if (active) setForm((current) => ({ ...current, opportunityNumber: 'Generated on save' }))
      })
    return () => {
      active = false
    }
  }, [isEdit])

  useEffect(() => {
    if (isEdit || (form.opportunityStageId && form.opportunityStatusId)) return
    void (async () => {
      const [stages, statuses] = await Promise.all([
        loadLookupOptionsByCategoryCode('OPPORTUNITY_STAGE'),
        loadLookupOptionsByCategoryCode('OPPORTUNITY_STATUS'),
      ])
      const qualify = stages.find((item) => item.label.toLowerCase() === 'qualify') ?? stages[0]
      const open = statuses.find((item) => item.label.toLowerCase() === 'open') ?? statuses[0]
      setForm((current) => ({
        ...current,
        opportunityStageId: current.opportunityStageId || qualify?.value || '',
        opportunityStatusId: current.opportunityStatusId || open?.value || '',
      }))
    })()
  }, [form.opportunityStageId, form.opportunityStatusId, isEdit])

  useEffect(() => {
    if (!isEdit || !id) return
    void (async () => {
      setLoading(true)
      setError('')
      try {
        const { data } = await api.get<Opportunity>(`api/opportunities/${id}`)
        setOpportunity(data)
        setForm(opportunityToForm(data))
      } catch {
        setError('Failed to load opportunity.')
      } finally {
        setLoading(false)
      }
    })()
  }, [id, isEdit])

  const title = useMemo(() => (isEdit ? 'Edit Opportunity' : 'Create Opportunity'), [isEdit])
  const validationSummary = Object.values(fieldErrors)

  const setValue = <K extends keyof OpportunityFormState>(key: K, value: OpportunityFormState[K]) => {
    setForm((current) => ({ ...current, [key]: value }))
  }

  const validate = () => {
    const next: Record<string, string> = {}
    if (!form.topic.trim()) next.topic = 'Topic is required.'
    if (!form.accountId.trim()) next.accountId = 'Account is required.'
    if (!form.opportunityStageId.trim()) next.opportunityStageId = 'Stage is required.'
    if (!form.opportunityStatusId.trim()) next.opportunityStatusId = 'Status is required.'
    const probability = Number(form.probability || 0)
    if (Number.isNaN(probability) || probability < 0 || probability > 100) next.probability = 'Probability must be between 0 and 100.'
    setFieldErrors(next)
    return Object.keys(next).length === 0
  }

  const save = async (closeAfterSave: boolean) => {
    if (!canSave || !validate()) return
    setSaving(true)
    setError('')
    try {
      if (isEdit && id) {
        await api.put(`api/opportunities/${id}`, opportunityPayload(form))
        if (closeAfterSave) {
          navigate('/opportunities')
        } else {
          const { data } = await api.get<Opportunity>(`api/opportunities/${id}`)
          setOpportunity(data)
          setForm(opportunityToForm(data))
        }
      } else {
        const { data } = await api.post<Opportunity>('api/opportunities', opportunityPayload(form))
        navigate(closeAfterSave ? '/opportunities' : `/opportunities/${data.id}/edit`)
      }
    } catch (err) {
      const maybe = err as { response?: { data?: unknown } }
      setError(typeof maybe.response?.data === 'string' ? maybe.response.data : 'Save failed. Please review opportunity values.')
    } finally {
      setSaving(false)
    }
  }

  const renderText = (key: keyof OpportunityFormState, label: string, required?: boolean, type: 'text' | 'date' | 'number' = 'text', readOnly?: boolean) => (
    <Field label={label} required={required} validationMessage={fieldErrors[String(key)]}>
      <Input
        size="small"
        type={type}
        value={String(form[key] ?? '')}
        onChange={(_, data) => setValue(key, data.value as OpportunityFormState[typeof key])}
        readOnly={readOnly || !canSave}
      />
    </Field>
  )

  const renderLookup = (key: keyof OpportunityFormState, label: string, required?: boolean, fieldKey?: string) => (
    <Field label={label} required={required} validationMessage={fieldErrors[String(key)]}>
      <LookupCombobox
        fieldKey={fieldKey ?? String(key)}
        value={String(form[key] ?? '')}
        disabled={!canSave}
        onChange={(value) => setValue(key, value as OpportunityFormState[typeof key])}
      />
    </Field>
  )

  const renderSwitch = (key: keyof OpportunityFormState, label: string) => (
    <Field label={label} validationMessage={fieldErrors[String(key)]}>
      <Switch checked={Boolean(form[key])} disabled={!canSave} onChange={(_, data) => setValue(key, Boolean(data.checked) as OpportunityFormState[typeof key])} />
    </Field>
  )

  const alerts = [
    ...(!canSave ? [{ intent: 'error' as const, text: `You do not have permission to ${isEdit ? 'update' : 'create'} opportunities.` }] : []),
    ...(error ? [{ intent: 'error' as const, text: error }] : []),
    ...validationSummary.map((message) => ({ intent: 'warning' as const, text: message })),
  ]

  const headerActions = [
    { key: 'save', label: 'Save', onClick: () => void save(false), appearance: 'primary' as const, disabled: loading || saving || !canSave },
    { key: 'save-close', label: 'Save & Close', onClick: () => void save(true), appearance: 'secondary' as const, disabled: loading || saving || !canSave },
    { key: 'cancel', label: 'Cancel', onClick: () => navigate('/opportunities'), appearance: 'subtle' as const },
  ]

  return (
    <EntityPageLayout
      header={<EntityHeader icon={<DataPieRegular />} title={title} subtitle="Opportunity pipeline, revenue, products, competitors, and activities." status={opportunity?.opportunityStatusName ?? (isEdit ? undefined : 'Open')} actions={headerActions} />}
      tabs={<EntityTabs tabs={tabs} activeTab={activeTab} onTabChange={setActiveTab} />}
      alerts={alerts}
      stickyBar={<StickySaveBar onSave={() => void save(false)} onSaveAndClose={() => void save(true)} onCancel={() => navigate('/opportunities')} disableActions={loading || saving || !canSave} />}
    >
      {loading ? <MessageBar><MessageBarBody>Loading opportunity...</MessageBarBody></MessageBar> : null}

      {!loading && activeTab === 'general' ? (
        <>
          <FormSectionCard title="General Information">
            {renderText('opportunityNumber', 'Opportunity Number', false, 'text', true)}
            {renderText('topic', 'Topic', true)}
            {renderLookup('accountId', 'Account', true)}
            {renderLookup('contactId', 'Contact')}
            {renderLookup('leadId', 'Source Lead')}
            {renderLookup('sourceId', 'Source', false, 'opportunitySourceId')}
          </FormSectionCard>
          <FormSectionCard title="Pipeline">
            {renderLookup('opportunityStageId', 'Stage', true)}
            {renderLookup('opportunityStatusId', 'Status', true)}
            {renderLookup('salesProcessStageId', 'Sales Process Stage')}
            {renderLookup('ratingId', 'Rating', false, 'opportunityRatingId')}
            {renderLookup('priorityId', 'Priority')}
            {renderText('probability', 'Probability', false, 'number')}
          </FormSectionCard>
          <FormSectionCard title="Revenue">
            {renderText('estimatedRevenue', 'Estimated Revenue', false, 'number')}
            {renderText('estimatedCloseDate', 'Estimated Close Date', false, 'date')}
            {renderLookup('currencyId', 'Currency')}
            {renderLookup('ownerUserId', 'Owner User')}
            {renderLookup('ownerTeamId', 'Owner Team')}
            {renderSwitch('isActive', 'Active')}
          </FormSectionCard>
          <FormSectionCard title="Notes">
            <Field label="Description">
              <Textarea value={form.description} readOnly={!canSave} onChange={(_, data) => setValue('description', data.value)} />
            </Field>
            <Field label="Notes">
              <Textarea value={form.notes} readOnly={!canSave} onChange={(_, data) => setValue('notes', data.value)} />
            </Field>
          </FormSectionCard>
        </>
      ) : null}

      {!loading && activeTab === 'products' ? (
        isEdit && id ? <OpportunityProductsPanel opportunityId={id} editable /> : <EntityTabPlaceholder text="Save the opportunity before adding products." />
      ) : null}

      {!loading && activeTab === 'competitors' ? (
        isEdit && id ? <OpportunityCompetitorsPanel opportunityId={id} editable /> : <EntityTabPlaceholder text="Save the opportunity before adding competitors." />
      ) : null}

      {!loading && activeTab === 'activities' ? (
        isEdit && id ? <OpportunityActivitiesPanel opportunityId={id} editable /> : <EntityTabPlaceholder text="Save the opportunity before adding activities." />
      ) : null}

      {!loading && activeTab === 'win-loss' ? (
        <FormSectionCard title="Win/Loss">
          {renderText('actualRevenue', 'Actual Revenue', false, 'number')}
          {renderText('actualCloseDate', 'Actual Close Date', false, 'date')}
          {renderLookup('winReasonId', 'Win Reason')}
          {renderLookup('lossReasonId', 'Loss Reason')}
          {renderLookup('lostToCompetitorId', 'Lost To Competitor')}
        </FormSectionCard>
      ) : null}

      {!loading && activeTab === 'audit-history' ? (
        isEdit && id ? <AuditHistoryPanel entityName="Opportunity" entityId={id} /> : <EntityTabPlaceholder text="Audit history appears after the opportunity is saved." />
      ) : null}
    </EntityPageLayout>
  )
}
