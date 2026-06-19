import { useEffect, useMemo, useState } from 'react'
import { Button, Field, Input, MessageBar, MessageBarBody, Switch, Textarea } from '@fluentui/react-components'
import { BranchRequestRegular } from '@fluentui/react-icons'
import { useNavigate, useParams } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { EntityDetailsGrid, EntityHeader, EntityPageLayout, FormSectionCard, LookupCombobox } from '../components/entity-ui/EntityComponents'
import type { Lead, LeadConversionResult } from '../types/models'
import { formatCurrency, formatDate, formatDateTime, nullIfBlank } from './leadUtils'
import styles from '../contacts/Contacts.module.css'

const steps = ['Review Lead', 'Account', 'Contact', 'Opportunity', 'Confirm']

export function LeadConversionPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const { hasPermission } = useAuth()
  const canConvert = hasPermission('Leads.Convert')
  const [lead, setLead] = useState<Lead | null>(null)
  const [activeStep, setActiveStep] = useState(0)
  const [createAccount, setCreateAccount] = useState(true)
  const [existingAccountId, setExistingAccountId] = useState('')
  const [createContact, setCreateContact] = useState(true)
  const [existingContactId, setExistingContactId] = useState('')
  const [createOpportunity, setCreateOpportunity] = useState(false)
  const [opportunityTopic, setOpportunityTopic] = useState('')
  const [estimatedValue, setEstimatedValue] = useState('')
  const [estimatedCloseDate, setEstimatedCloseDate] = useState('')
  const [result, setResult] = useState<LeadConversionResult | null>(null)
  const [loading, setLoading] = useState(false)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!id || !canConvert) {
      return
    }

    void (async () => {
      setLoading(true)
      setError('')
      try {
        const { data } = await api.get<Lead>(`api/leads/${id}`)
        setLead(data)
        setOpportunityTopic(data.topic)
        setEstimatedValue(data.estimatedValue === undefined || data.estimatedValue === null ? '' : String(data.estimatedValue))
        setEstimatedCloseDate(data.estimatedCloseDate?.slice(0, 10) ?? '')
      } catch {
        setError('Failed to load lead for conversion.')
      } finally {
        setLoading(false)
      }
    })()
  }, [canConvert, id])

  const reviewRows = useMemo(
    () => lead ? [
      { label: 'Lead Number', value: lead.leadNumber },
      { label: 'Topic', value: lead.topic },
      { label: 'Status', value: lead.leadStatusName ?? '' },
      { label: 'Qualification', value: lead.qualificationStatusName ?? '' },
      { label: 'Company', value: lead.companyName ?? '' },
      { label: 'Contact', value: lead.fullName ?? '' },
      { label: 'Estimated Value', value: formatCurrency(lead.estimatedValue) },
      { label: 'Estimated Close', value: formatDate(lead.estimatedCloseDate) },
    ] : [],
    [lead],
  )

  const validationMessage = useMemo(() => {
    if (!lead) return ''
    if (lead.convertedAt) return `This lead was converted ${formatDateTime(lead.convertedAt)}.`
    if (lead.leadStatusName === 'Disqualified' || lead.qualificationStatusName === 'Disqualified') return 'Disqualified leads cannot be converted.'
    if (lead.leadStatusName !== 'Qualified' && lead.qualificationStatusName !== 'Qualified') return 'Lead must be qualified before conversion.'
    if (!createAccount && !existingAccountId) return 'Select an existing account or choose create account.'
    if (!createContact && !existingContactId) return 'Select an existing contact or choose create contact.'
    return ''
  }, [createAccount, createContact, existingAccountId, existingContactId, lead])

  const submit = async () => {
    if (!id || validationMessage) {
      setError(validationMessage)
      return
    }

    setSaving(true)
    setError('')
    try {
      const { data } = await api.post<LeadConversionResult>(`api/leads/${id}/convert`, {
        createAccount,
        existingAccountId: createAccount ? null : existingAccountId,
        createContact,
        existingContactId: createContact ? null : existingContactId,
        createOpportunity,
        opportunityTopic: nullIfBlank(opportunityTopic),
        estimatedValue: estimatedValue.trim() ? Number(estimatedValue) : null,
        estimatedCloseDate: nullIfBlank(estimatedCloseDate),
      })
      setResult(data)
    } catch (err) {
      const maybe = err as { response?: { data?: unknown } }
      setError(typeof maybe.response?.data === 'string' ? maybe.response.data : 'Lead conversion failed.')
    } finally {
      setSaving(false)
    }
  }

  const headerActions = [
    { key: 'back', label: 'Back to Lead', onClick: () => navigate(id ? `/leads/${id}` : '/leads'), appearance: 'subtle' as const },
  ]

  return (
    <EntityPageLayout
      header={<EntityHeader icon={<BranchRequestRegular />} title="Convert Lead" subtitle={lead?.leadNumber} status={lead?.leadStatusName} actions={headerActions} />}
      alerts={[...(error ? [{ intent: 'error' as const, text: error }] : []), ...(validationMessage ? [{ intent: 'warning' as const, text: validationMessage }] : [])]}
    >
      {!canConvert ? (
        <MessageBar intent="error">
          <MessageBarBody>You do not have permission to convert leads.</MessageBarBody>
        </MessageBar>
      ) : null}

      <div className={styles.inlineActions}>
        {steps.map((step, index) => (
          <Button key={step} size="small" appearance={activeStep === index ? 'primary' : 'subtle'} onClick={() => setActiveStep(index)}>
            {step}
          </Button>
        ))}
      </div>

      {!loading && lead && activeStep === 0 ? <EntityDetailsGrid rows={reviewRows} /> : null}

      {!loading && activeStep === 1 ? (
        <FormSectionCard title="Account">
          <Field label="Create New Account">
            <Switch checked={createAccount} onChange={(_, data) => setCreateAccount(Boolean(data.checked))} />
          </Field>
          {!createAccount ? (
            <Field label="Existing Account" required>
              <LookupCombobox fieldKey="accountId" value={existingAccountId} onChange={setExistingAccountId} />
            </Field>
          ) : null}
          <Field label="New Account Name">
            <Input size="small" value={lead?.companyName || lead?.topic || ''} readOnly />
          </Field>
        </FormSectionCard>
      ) : null}

      {!loading && activeStep === 2 ? (
        <FormSectionCard title="Contact">
          <Field label="Create New Contact">
            <Switch checked={createContact} onChange={(_, data) => setCreateContact(Boolean(data.checked))} />
          </Field>
          {!createContact ? (
            <Field label="Existing Contact" required>
              <LookupCombobox fieldKey="contactId" value={existingContactId} onChange={setExistingContactId} />
            </Field>
          ) : null}
          <Field label="New Contact">
            <Input size="small" value={lead?.fullName || lead?.email || 'Converted Lead'} readOnly />
          </Field>
        </FormSectionCard>
      ) : null}

      {!loading && activeStep === 3 ? (
        <FormSectionCard title="Opportunity">
          <Field label="Create Opportunity">
            <Switch checked={createOpportunity} onChange={(_, data) => setCreateOpportunity(Boolean(data.checked))} />
          </Field>
          <Field label="Opportunity Topic">
            <Input size="small" value={opportunityTopic} disabled={!createOpportunity} onChange={(_, data) => setOpportunityTopic(data.value)} />
          </Field>
          <Field label="Estimated Value">
            <Input size="small" type="number" value={estimatedValue} disabled={!createOpportunity} onChange={(_, data) => setEstimatedValue(data.value)} />
          </Field>
          <Field label="Estimated Close Date">
            <Input size="small" type="date" value={estimatedCloseDate} disabled={!createOpportunity} onChange={(_, data) => setEstimatedCloseDate(data.value)} />
          </Field>
          <Field label="Opportunity Module">
            <Textarea value="When enabled, conversion creates an open opportunity linked to this lead, account, and contact." readOnly />
          </Field>
        </FormSectionCard>
      ) : null}

      {!loading && activeStep === 4 ? (
        <FormSectionCard title="Confirm Conversion">
          <Field label="Account Action">
            <Input size="small" value={createAccount ? 'Create new account' : 'Link existing account'} readOnly />
          </Field>
          <Field label="Contact Action">
            <Input size="small" value={createContact ? 'Create new contact' : 'Link existing contact'} readOnly />
          </Field>
          <Field label="Opportunity Action">
            <Input size="small" value={createOpportunity ? 'Create linked opportunity' : 'Skip opportunity'} readOnly />
          </Field>
          <div className={styles.inlineActions}>
            <Button appearance="primary" size="small" onClick={() => void submit()} disabled={saving || Boolean(validationMessage)}>
              Convert Lead
            </Button>
          </div>
        </FormSectionCard>
      ) : null}

      {result ? (
        <FormSectionCard title="Conversion Result">
          <Field label="Account">
            <Input size="small" value={result.convertedAccountName ?? 'Converted account'} readOnly />
          </Field>
          <Field label="Contact">
            <Input size="small" value={result.convertedContactName ?? 'Converted contact'} readOnly />
          </Field>
          <Field label="Opportunity">
            <Input size="small" value={result.convertedOpportunityId ? 'Opportunity created' : result.opportunityMessage ?? 'No opportunity created'} readOnly />
          </Field>
          <div className={styles.inlineActions}>
            <Button size="small" appearance="secondary" onClick={() => navigate(`/leads/${result.leadId}`)}>Open Lead</Button>
            {result.convertedOpportunityId ? <Button size="small" appearance="secondary" onClick={() => navigate(`/opportunities/${result.convertedOpportunityId}`)}>Open Opportunity</Button> : null}
          </div>
        </FormSectionCard>
      ) : null}
    </EntityPageLayout>
  )
}
