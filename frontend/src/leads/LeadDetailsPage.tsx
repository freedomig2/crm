import { useCallback, useEffect, useMemo, useState } from 'react'
import { MessageBar, MessageBarBody, Spinner } from '@fluentui/react-components'
import { PeopleRegular } from '@fluentui/react-icons'
import { useNavigate, useParams } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import {
  EntityDetailsGrid,
  EntityHeader,
  EntityPageLayout,
  EntitySummaryCard,
  EntityTabs,
} from '../components/entity-ui/EntityComponents'
import type { Lead, LeadActivity, PagedResult } from '../types/models'
import { AuditHistoryPanel } from '../contacts/ContactRelatedPanels'
import { LeadActivitiesPanel, RelatedConversionPanel } from './LeadRelatedPanels'
import { formatCurrency, formatDate, formatDateTime } from './leadUtils'

const tabs = [
  { key: 'summary', label: 'Summary' },
  { key: 'qualification', label: 'Qualification' },
  { key: 'scoring', label: 'Scoring' },
  { key: 'activities', label: 'Activities' },
  { key: 'conversion', label: 'Conversion' },
  { key: 'audit-history', label: 'Audit History' },
]

export function LeadDetailsPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const { hasPermission } = useAuth()
  const canView = hasPermission('Leads.View')
  const canEdit = hasPermission('Leads.Update')
  const canConvert = hasPermission('Leads.Convert')
  const canScore = hasPermission('Leads.Score')
  const [lead, setLead] = useState<Lead | null>(null)
  const [latestActivity, setLatestActivity] = useState<LeadActivity | null>(null)
  const [activeTab, setActiveTab] = useState('summary')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const load = useCallback(async () => {
    if (!id || !canView) {
      setLoading(false)
      return
    }

    setError('')
    try {
      const { data } = await api.get<Lead>(`api/leads/${id}`)
      setLead(data)
      const activities = await api.get<PagedResult<LeadActivity>>(`api/leads/${id}/activities`, {
        params: { page: 1, pageSize: 1 },
      })
      setLatestActivity(activities.data.items[0] ?? null)
    } catch {
      setError('Failed to load lead details.')
    } finally {
      setLoading(false)
    }
  }, [canView, id])

  useEffect(() => {
    void Promise.resolve().then(load)
  }, [load])

  const calculateScore = async () => {
    if (!id) {
      return
    }

    setError('')
    try {
      await api.post(`api/leads/${id}/calculate-score`)
      await load()
    } catch {
      setError('Failed to calculate lead score.')
    }
  }

  const summaryRows = useMemo(
    () => lead ? [
      { label: 'Status', value: lead.leadStatusName ?? '' },
      { label: 'Rating', value: lead.ratingName ?? '' },
      { label: 'Score', value: `${lead.score} (${lead.scoreGrade ?? 'Cold'})` },
      { label: 'Owner', value: lead.ownerUserName ?? lead.ownerTeamName ?? '' },
      { label: 'Assigned To', value: lead.assignedToUserName ?? lead.assignedToTeamName ?? '' },
      { label: 'Source', value: lead.leadSourceName ?? '' },
      { label: 'Estimated Value', value: formatCurrency(lead.estimatedValue) },
      { label: 'Latest Activity', value: latestActivity ? `${latestActivity.subject} - ${formatDateTime(latestActivity.activityDate)}` : '' },
      { label: 'Conversion Status', value: lead.convertedAt ? `Converted ${formatDateTime(lead.convertedAt)}` : 'Not converted' },
    ] : [],
    [lead, latestActivity],
  )

  const detailsRows = useMemo(
    () => lead ? [
      { label: 'Lead Number', value: lead.leadNumber },
      { label: 'Topic', value: lead.topic },
      { label: 'Full Name', value: lead.fullName ?? '' },
      { label: 'Company Name', value: lead.companyName ?? '' },
      { label: 'Job Title', value: lead.jobTitle ?? '' },
      { label: 'Email', value: lead.email ?? '' },
      { label: 'Mobile Phone', value: lead.mobilePhone ?? '' },
      { label: 'Work Phone', value: lead.workPhone ?? '' },
      { label: 'Website', value: lead.website ?? '' },
      { label: 'Industry', value: lead.industryName ?? '' },
      { label: 'Active', value: lead.isActive ? 'Yes' : 'No' },
      { label: 'Created At', value: formatDateTime(lead.createdAt) },
      { label: 'Description', value: lead.description ?? '' },
      { label: 'Notes', value: lead.notes ?? '' },
    ] : [],
    [lead],
  )

  const qualificationRows = useMemo(
    () => lead ? [
      { label: 'Qualification Status', value: lead.qualificationStatusName ?? '' },
      { label: 'Estimated Value', value: formatCurrency(lead.estimatedValue) },
      { label: 'Estimated Close Date', value: formatDate(lead.estimatedCloseDate) },
      { label: 'Disqualification Reason', value: lead.disqualifiedReasonName ?? '' },
    ] : [],
    [lead],
  )

  const scoringRows = useMemo(
    () => lead ? [
      { label: 'Score', value: String(lead.score) },
      { label: 'Score Grade', value: lead.scoreGrade ?? '' },
      { label: 'Rating', value: lead.ratingName ?? '' },
    ] : [],
    [lead],
  )

  if (!canView) {
    return (
      <EntityPageLayout
        header={<EntityHeader icon={<PeopleRegular />} title="Lead Details" actions={[{ key: 'back', label: 'Back to List', onClick: () => navigate('/leads') }]} />}
      >
        <MessageBar intent="error">
          <MessageBarBody>You do not have permission to view leads.</MessageBarBody>
        </MessageBar>
      </EntityPageLayout>
    )
  }

  const actions = [
    ...(canEdit && id ? [{ key: 'edit', label: 'Edit', onClick: () => navigate(`/leads/${id}/edit`), appearance: 'primary' as const }] : []),
    ...(canConvert && id && lead?.leadStatusName === 'Qualified' && !lead.convertedAt ? [{ key: 'convert', label: 'Convert', onClick: () => navigate(`/leads/${id}/convert`), appearance: 'secondary' as const }] : []),
    ...(canScore && id ? [{ key: 'score', label: 'Calculate Score', onClick: () => void calculateScore(), appearance: 'secondary' as const }] : []),
    ...(id ? [{ key: 'timeline', label: 'Timeline', onClick: () => navigate(`/leads/${id}/timeline`), appearance: 'subtle' as const }] : []),
    { key: 'back', label: 'Back to List', onClick: () => navigate('/leads'), appearance: 'subtle' as const },
  ]

  return (
    <EntityPageLayout
      header={<EntityHeader icon={<PeopleRegular />} title={lead?.topic ?? 'Lead Details'} subtitle={lead?.leadNumber} status={lead?.leadStatusName} actions={actions} />}
      tabs={<EntityTabs tabs={tabs} activeTab={activeTab} onTabChange={setActiveTab} />}
      alerts={error ? [{ intent: 'error', text: error }] : undefined}
    >
      {loading ? <Spinner size="small" label="Loading lead..." /> : null}

      {!loading && lead && activeTab === 'summary' ? (
        <>
          <EntitySummaryCard title="Summary" rows={summaryRows} />
          <EntityDetailsGrid rows={detailsRows} />
        </>
      ) : null}

      {!loading && lead && activeTab === 'qualification' ? <EntityDetailsGrid rows={qualificationRows} /> : null}
      {!loading && lead && activeTab === 'scoring' ? <EntityDetailsGrid rows={scoringRows} /> : null}
      {!loading && lead && activeTab === 'activities' ? <LeadActivitiesPanel leadId={lead.id} editable /> : null}
      {!loading && lead && activeTab === 'conversion' ? <RelatedConversionPanel accountId={lead.convertedAccountId} contactId={lead.convertedContactId} /> : null}
      {!loading && lead && activeTab === 'audit-history' ? <AuditHistoryPanel entityName="Lead" entityId={lead.id} /> : null}
    </EntityPageLayout>
  )
}
