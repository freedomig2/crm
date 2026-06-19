import { useCallback, useEffect, useMemo, useState } from 'react'
import { Button, MessageBar, MessageBarBody, Spinner } from '@fluentui/react-components'
import { DataPieRegular } from '@fluentui/react-icons'
import { useNavigate, useParams } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import {
  EntityDetailsGrid,
  EntityHeader,
  EntityPageLayout,
  EntitySummaryCard,
  EntityTabs,
  FormSectionCard,
} from '../components/entity-ui/EntityComponents'
import type { OpportunitySummary } from '../types/models'
import { AuditHistoryPanel } from '../contacts/ContactRelatedPanels'
import { OpportunityActivitiesPanel, OpportunityCompetitorsPanel, OpportunityProductsPanel } from './OpportunityRelatedPanels'
import { formatCurrency, formatDate, formatDateTime } from './opportunityUtils'
import { OpportunityPipelineAnalyticsPanel } from '../sales/SalesAnalyticsPages'
import styles from '../contacts/Contacts.module.css'

const tabs = [
  { key: 'summary', label: 'Summary' },
  { key: 'products', label: 'Products' },
  { key: 'competitors', label: 'Competitors' },
  { key: 'activities', label: 'Activities' },
  { key: 'pipeline', label: 'Pipeline' },
  { key: 'pipeline-analytics', label: 'Pipeline Analytics' },
  { key: 'win-loss', label: 'Win/Loss' },
  { key: 'audit-history', label: 'Audit History' },
]

export function OpportunityDetailsPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const { hasPermission } = useAuth()
  const canView = hasPermission('Opportunities.View')
  const canEdit = hasPermission('Opportunities.Update')
  const canMarkWon = hasPermission('Opportunities.MarkWon')
  const canMarkLost = hasPermission('Opportunities.MarkLost')
  const canViewTimeline = hasPermission('Opportunities.ViewTimeline')
  const canViewPipeline = hasPermission('Pipeline.View')
  const [summary, setSummary] = useState<OpportunitySummary | null>(null)
  const [activeTab, setActiveTab] = useState('summary')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const opportunity = summary?.opportunity ?? null

  const load = useCallback(async () => {
    if (!id || !canView) {
      setLoading(false)
      return
    }

    setError('')
    try {
      const { data } = await api.get<OpportunitySummary>(`api/opportunities/${id}/summary`)
      setSummary(data)
    } catch {
      setError('Failed to load opportunity details.')
    } finally {
      setLoading(false)
    }
  }, [canView, id])

  useEffect(() => {
    void Promise.resolve().then(load)
  }, [load])

  const summaryRows = useMemo(
    () => opportunity ? [
      { label: 'Stage', value: opportunity.opportunityStageName ?? '' },
      { label: 'Status', value: opportunity.opportunityStatusName ?? '' },
      { label: 'Account', value: opportunity.accountName ?? '' },
      { label: 'Contact', value: opportunity.contactName ?? '' },
      { label: 'Estimated Revenue', value: formatCurrency(opportunity.estimatedRevenue) },
      { label: 'Weighted Revenue', value: formatCurrency(opportunity.weightedRevenue) },
      { label: 'Probability', value: `${opportunity.probability}%` },
      { label: 'Estimated Close', value: formatDate(opportunity.estimatedCloseDate) },
      { label: 'Primary Competitor', value: summary?.primaryCompetitor?.competitorName ?? '' },
      { label: 'Latest Activity', value: summary?.latestActivity ? `${summary.latestActivity.subject} - ${formatDateTime(summary.latestActivity.activityDate)}` : '' },
      { label: 'Products', value: `${summary?.productCount ?? 0} (${formatCurrency(summary?.productRevenue)})` },
      { label: 'Activities', value: String(summary?.activityCount ?? 0) },
    ] : [],
    [opportunity, summary],
  )

  const detailsRows = useMemo(
    () => opportunity ? [
      { label: 'Opportunity Number', value: opportunity.opportunityNumber },
      { label: 'Topic', value: opportunity.topic },
      { label: 'Lead', value: opportunity.leadTopic ?? '' },
      { label: 'Source', value: opportunity.sourceName ?? '' },
      { label: 'Sales Process Stage', value: opportunity.salesProcessStageName ?? '' },
      { label: 'Rating', value: opportunity.ratingName ?? '' },
      { label: 'Priority', value: opportunity.priorityName ?? '' },
      { label: 'Currency', value: opportunity.currencyName ?? '' },
      { label: 'Owner', value: opportunity.ownerUserName ?? opportunity.ownerTeamName ?? '' },
      { label: 'Active', value: opportunity.isActive ? 'Yes' : 'No' },
      { label: 'Created At', value: formatDateTime(opportunity.createdAt) },
      { label: 'Description', value: opportunity.description ?? '' },
      { label: 'Notes', value: opportunity.notes ?? '' },
    ] : [],
    [opportunity],
  )

  const winLossRows = useMemo(
    () => opportunity ? [
      { label: 'Actual Revenue', value: formatCurrency(opportunity.actualRevenue) },
      { label: 'Actual Close Date', value: formatDate(opportunity.actualCloseDate) },
      { label: 'Win Reason', value: opportunity.winReasonName ?? '' },
      { label: 'Loss Reason', value: opportunity.lossReasonName ?? '' },
      { label: 'Lost To Competitor', value: opportunity.lostToCompetitorName ?? '' },
    ] : [],
    [opportunity],
  )

  if (!canView) {
    return (
      <EntityPageLayout header={<EntityHeader icon={<DataPieRegular />} title="Opportunity Details" actions={[{ key: 'back', label: 'Back to List', onClick: () => navigate('/opportunities') }]} />}>
        <MessageBar intent="error"><MessageBarBody>You do not have permission to view opportunities.</MessageBarBody></MessageBar>
      </EntityPageLayout>
    )
  }

  const actions = [
    ...(canEdit && id ? [{ key: 'edit', label: 'Edit', onClick: () => navigate(`/opportunities/${id}/edit`), appearance: 'primary' as const }] : []),
    ...(canMarkWon && id && opportunity?.opportunityStatusCode !== 'WON' ? [{ key: 'mark-won', label: 'Mark Won', onClick: () => navigate(`/opportunities/${id}/mark-won`), appearance: 'secondary' as const }] : []),
    ...(canMarkLost && id && opportunity?.opportunityStatusCode !== 'LOST' ? [{ key: 'mark-lost', label: 'Mark Lost', onClick: () => navigate(`/opportunities/${id}/mark-lost`), appearance: 'secondary' as const }] : []),
    ...(canViewTimeline && id ? [{ key: 'timeline', label: 'Timeline', onClick: () => navigate(`/opportunities/${id}/timeline`), appearance: 'subtle' as const }] : []),
    { key: 'back', label: 'Back to List', onClick: () => navigate('/opportunities'), appearance: 'subtle' as const },
  ]

  return (
    <EntityPageLayout
      header={<EntityHeader icon={<DataPieRegular />} title={opportunity?.topic ?? 'Opportunity Details'} subtitle={opportunity?.opportunityNumber} status={opportunity?.opportunityStatusName} actions={actions} />}
      tabs={<EntityTabs tabs={tabs} activeTab={activeTab} onTabChange={setActiveTab} />}
      alerts={error ? [{ intent: 'error', text: error }] : undefined}
    >
      {loading ? <Spinner size="small" label="Loading opportunity..." /> : null}

      {!loading && opportunity && activeTab === 'summary' ? (
        <>
          <EntitySummaryCard title="Summary" rows={summaryRows} />
          <EntityDetailsGrid rows={detailsRows} />
        </>
      ) : null}

      {!loading && opportunity && activeTab === 'products' ? <OpportunityProductsPanel opportunityId={opportunity.id} editable /> : null}
      {!loading && opportunity && activeTab === 'competitors' ? <OpportunityCompetitorsPanel opportunityId={opportunity.id} editable /> : null}
      {!loading && opportunity && activeTab === 'activities' ? <OpportunityActivitiesPanel opportunityId={opportunity.id} editable /> : null}
      {!loading && opportunity && activeTab === 'pipeline' ? (
        <FormSectionCard title="Pipeline Position">
          <table className={styles.recordTable}>
            <tbody>
              <tr><th>Stage</th><td>{opportunity.opportunityStageName ?? 'Not set'}</td></tr>
              <tr><th>Sales Process Stage</th><td>{opportunity.salesProcessStageName ?? 'Not set'}</td></tr>
              <tr><th>Status</th><td>{opportunity.opportunityStatusName ?? 'Not set'}</td></tr>
              <tr><th>Weighted Revenue</th><td>{formatCurrency(opportunity.weightedRevenue)}</td></tr>
            </tbody>
          </table>
          <div className={styles.inlineActions}>
            <Button size="small" appearance="secondary" onClick={() => navigate('/sales/pipeline')} disabled={!canViewPipeline}>Open Pipeline</Button>
          </div>
        </FormSectionCard>
      ) : null}
      {!loading && opportunity && activeTab === 'pipeline-analytics' ? <OpportunityPipelineAnalyticsPanel opportunityId={opportunity.id} /> : null}
      {!loading && opportunity && activeTab === 'win-loss' ? <EntityDetailsGrid rows={winLossRows} /> : null}
      {!loading && opportunity && activeTab === 'audit-history' ? <AuditHistoryPanel entityName="Opportunity" entityId={opportunity.id} /> : null}
    </EntityPageLayout>
  )
}
