import { useEffect, useMemo, useState } from 'react'
import { Button, Dropdown, Field, Input, MessageBar, MessageBarBody, Option, Textarea } from '@fluentui/react-components'
import { BranchRequestRegular } from '@fluentui/react-icons'
import { useNavigate, useParams } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { EntityDetailsGrid, EntityHeader, EntityPageLayout, FormSectionCard, LookupCombobox } from '../components/entity-ui/EntityComponents'
import type { Opportunity, OpportunityCompetitor, PagedResult } from '../types/models'
import { formatCurrency, formatDate, nullIfBlank } from './opportunityUtils'
import styles from '../contacts/Contacts.module.css'

export function OpportunityOutcomePage({ mode }: { mode: 'won' | 'lost' }) {
  const { id } = useParams()
  const navigate = useNavigate()
  const { hasPermission } = useAuth()
  const canSubmit = mode === 'won' ? hasPermission('Opportunities.MarkWon') : hasPermission('Opportunities.MarkLost')
  const [opportunity, setOpportunity] = useState<Opportunity | null>(null)
  const [competitors, setCompetitors] = useState<OpportunityCompetitor[]>([])
  const [actualRevenue, setActualRevenue] = useState('')
  const [actualCloseDate, setActualCloseDate] = useState('')
  const [winReasonId, setWinReasonId] = useState('')
  const [lossReasonId, setLossReasonId] = useState('')
  const [lostToCompetitorId, setLostToCompetitorId] = useState('')
  const [notes, setNotes] = useState('')
  const [loading, setLoading] = useState(false)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!id || !canSubmit) return
    void (async () => {
      setLoading(true)
      setError('')
      try {
        const [{ data: opportunityData }, { data: competitorData }] = await Promise.all([
          api.get<Opportunity>(`api/opportunities/${id}`),
          api.get<PagedResult<OpportunityCompetitor>>(`api/opportunities/${id}/competitors`, { params: { page: 1, pageSize: 100 } }),
        ])
        setOpportunity(opportunityData)
        setCompetitors(competitorData.items)
        setActualRevenue(opportunityData.actualRevenue === undefined || opportunityData.actualRevenue === null ? String(opportunityData.estimatedRevenue ?? '') : String(opportunityData.actualRevenue))
        setActualCloseDate(new Date().toISOString().slice(0, 10))
      } catch {
        setError('Failed to load opportunity.')
      } finally {
        setLoading(false)
      }
    })()
  }, [canSubmit, id])

  const reviewRows = useMemo(
    () => opportunity ? [
      { label: 'Opportunity Number', value: opportunity.opportunityNumber },
      { label: 'Topic', value: opportunity.topic },
      { label: 'Account', value: opportunity.accountName ?? '' },
      { label: 'Stage', value: opportunity.opportunityStageName ?? '' },
      { label: 'Status', value: opportunity.opportunityStatusName ?? '' },
      { label: 'Estimated Revenue', value: formatCurrency(opportunity.estimatedRevenue) },
      { label: 'Estimated Close', value: formatDate(opportunity.estimatedCloseDate) },
    ] : [],
    [opportunity],
  )

  const validate = () => {
    if (mode === 'won') {
      if (!actualRevenue.trim() || !actualCloseDate || !winReasonId) return 'Actual revenue, actual close date, and win reason are required.'
    } else if (!actualCloseDate || !lossReasonId) {
      return 'Actual close date and loss reason are required.'
    }
    return ''
  }

  const submit = async () => {
    if (!id) return
    const validation = validate()
    if (validation) {
      setError(validation)
      return
    }

    setSaving(true)
    setError('')
    try {
      if (mode === 'won') {
        await api.post(`api/opportunities/${id}/mark-won`, {
          actualRevenue: Number(actualRevenue),
          actualCloseDate,
          winReasonId,
          notes: nullIfBlank(notes),
        })
      } else {
        await api.post(`api/opportunities/${id}/mark-lost`, {
          actualCloseDate,
          lossReasonId,
          lostToCompetitorId: nullIfBlank(lostToCompetitorId),
          notes: nullIfBlank(notes),
        })
      }
      navigate(`/opportunities/${id}`)
    } catch (err) {
      const maybe = err as { response?: { data?: unknown } }
      setError(typeof maybe.response?.data === 'string' ? maybe.response.data : `Failed to mark opportunity ${mode}.`)
    } finally {
      setSaving(false)
    }
  }

  const title = mode === 'won' ? 'Mark Opportunity Won' : 'Mark Opportunity Lost'

  return (
    <EntityPageLayout
      header={<EntityHeader icon={<BranchRequestRegular />} title={title} subtitle={opportunity?.opportunityNumber} status={opportunity?.opportunityStatusName} actions={[{ key: 'back', label: 'Back to Opportunity', onClick: () => navigate(id ? `/opportunities/${id}` : '/opportunities'), appearance: 'subtle' }]} />}
      alerts={error ? [{ intent: 'error', text: error }] : undefined}
    >
      {!canSubmit ? <MessageBar intent="error"><MessageBarBody>You do not have permission to update this opportunity outcome.</MessageBarBody></MessageBar> : null}
      {loading ? <MessageBar><MessageBarBody>Loading opportunity...</MessageBarBody></MessageBar> : null}
      {!loading && opportunity ? <EntityDetailsGrid rows={reviewRows} /> : null}

      {!loading && opportunity ? (
        <FormSectionCard title={mode === 'won' ? 'Won Details' : 'Lost Details'}>
          {mode === 'won' ? (
            <>
              <Field label="Actual Revenue" required>
                <Input size="small" type="number" value={actualRevenue} onChange={(_, data) => setActualRevenue(data.value)} disabled={!canSubmit} />
              </Field>
              <Field label="Win Reason" required>
                <LookupCombobox fieldKey="winReasonId" value={winReasonId} onChange={setWinReasonId} disabled={!canSubmit} />
              </Field>
            </>
          ) : (
            <>
              <Field label="Loss Reason" required>
                <LookupCombobox fieldKey="lossReasonId" value={lossReasonId} onChange={setLossReasonId} disabled={!canSubmit} />
              </Field>
              <Field label="Lost To Competitor">
                <Dropdown
                  size="small"
                  selectedOptions={lostToCompetitorId ? [lostToCompetitorId] : []}
                  value={competitors.find((item) => item.id === lostToCompetitorId)?.competitorName ?? ''}
                  onOptionSelect={(_, data) => setLostToCompetitorId(data.optionValue ?? '')}
                  disabled={!canSubmit}
                >
                  <Option value="">None</Option>
                  {competitors.map((competitor) => (
                    <Option key={competitor.id} value={competitor.id}>{competitor.competitorName}</Option>
                  ))}
                </Dropdown>
              </Field>
            </>
          )}
          <Field label="Actual Close Date" required>
            <Input size="small" type="date" value={actualCloseDate} onChange={(_, data) => setActualCloseDate(data.value)} disabled={!canSubmit} />
          </Field>
          <Field label="Notes">
            <Textarea value={notes} onChange={(_, data) => setNotes(data.value)} disabled={!canSubmit} />
          </Field>
          <div className={styles.inlineActions}>
            <Button size="small" appearance="primary" onClick={() => void submit()} disabled={saving || !canSubmit}>{mode === 'won' ? 'Mark Won' : 'Mark Lost'}</Button>
            <Button size="small" appearance="subtle" onClick={() => navigate(id ? `/opportunities/${id}` : '/opportunities')}>Cancel</Button>
          </div>
        </FormSectionCard>
      ) : null}
    </EntityPageLayout>
  )
}
