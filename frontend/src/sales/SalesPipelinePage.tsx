import { useCallback, useEffect, useMemo, useState } from 'react'
import { Badge, Button, Field, Input, MessageBar, MessageBarBody, Spinner } from '@fluentui/react-components'
import { BranchRequestRegular } from '@fluentui/react-icons'
import { useNavigate } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { EntityHeader, EntityPageLayout, LookupCombobox } from '../components/entity-ui/EntityComponents'
import type { SalesPipelineBoard, SalesPipelineCard } from '../types/models'
import { formatCurrency, formatDate, formatPercent } from './salesUtils'
import styles from './Sales.module.css'

type PipelineFilters = {
  ownerUserId: string
  ownerTeamId: string
  stageId: string
  minRevenue: string
  maxRevenue: string
  minProbability: string
  maxProbability: string
  closeDateFrom: string
  closeDateTo: string
  accountId: string
  ratingId: string
  industryId: string
}

const storageKey = 'crm.sales.pipeline.filters'

const emptyFilters: PipelineFilters = {
  ownerUserId: '',
  ownerTeamId: '',
  stageId: '',
  minRevenue: '',
  maxRevenue: '',
  minProbability: '',
  maxProbability: '',
  closeDateFrom: '',
  closeDateTo: '',
  accountId: '',
  ratingId: '',
  industryId: '',
}

const loadStoredFilters = (): PipelineFilters => {
  if (typeof window === 'undefined') {
    return emptyFilters
  }

  try {
    const parsed = JSON.parse(localStorage.getItem(storageKey) ?? '{}') as Partial<PipelineFilters>
    return { ...emptyFilters, ...parsed }
  } catch {
    return emptyFilters
  }
}

const ratingTone = (card: SalesPipelineCard): 'success' | 'warning' | 'danger' | 'informative' => {
  const rating = `${card.ratingCode ?? ''} ${card.ratingName ?? ''}`.toLowerCase()
  if (rating.includes('hot') || card.probability >= 75) return 'danger'
  if (rating.includes('warm') || card.probability >= 40) return 'warning'
  if (rating.includes('cold') || card.probability < 20) return 'informative'
  return 'success'
}

export function SalesPipelinePage() {
  const navigate = useNavigate()
  const { hasPermission } = useAuth()
  const canView = hasPermission('Pipeline.View')
  const canMove = hasPermission('Pipeline.MoveStage')
  const [board, setBoard] = useState<SalesPipelineBoard | null>(null)
  const [filters, setFilters] = useState<PipelineFilters>(() => loadStoredFilters())
  const [draggedOpportunityId, setDraggedOpportunityId] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    localStorage.setItem(storageKey, JSON.stringify(filters))
  }, [filters])

  const load = useCallback(async () => {
    if (!canView) {
      return
    }

    setLoading(true)
    setError('')
    try {
      const { data } = await api.get<SalesPipelineBoard>('api/sales-pipeline/board', {
        params: {
          ownerUserId: filters.ownerUserId || undefined,
          ownerTeamId: filters.ownerTeamId || undefined,
          stageId: filters.stageId || undefined,
          minRevenue: filters.minRevenue || undefined,
          maxRevenue: filters.maxRevenue || undefined,
          minProbability: filters.minProbability || undefined,
          maxProbability: filters.maxProbability || undefined,
          closeDateFrom: filters.closeDateFrom || undefined,
          closeDateTo: filters.closeDateTo || undefined,
          accountId: filters.accountId || undefined,
          ratingId: filters.ratingId || undefined,
          industryId: filters.industryId || undefined,
        },
      })
      setBoard(data)
    } catch {
      setError('Failed to load sales pipeline.')
    } finally {
      setLoading(false)
    }
  }, [canView, filters])

  useEffect(() => {
    void Promise.resolve().then(load)
  }, [load])

  const summary = useMemo(() => board?.summary ?? {
    totalOpportunities: 0,
    pipelineRevenue: 0,
    weightedPipelineRevenue: 0,
    averageProbability: 0,
    averageDealSize: 0,
  }, [board])

  const setFilter = <K extends keyof PipelineFilters>(key: K, value: PipelineFilters[K]) => {
    setFilters((current) => ({ ...current, [key]: value }))
  }

  const moveStage = async (newStageId: string) => {
    if (!draggedOpportunityId || !canMove) {
      return
    }

    setError('')
    try {
      await api.post('api/sales-pipeline/move-stage', {
        opportunityId: draggedOpportunityId,
        newStageId,
      })
      setDraggedOpportunityId(null)
      await load()
    } catch {
      setError('Failed to move opportunity stage.')
    }
  }

  if (!canView) {
    return (
      <EntityPageLayout header={<EntityHeader icon={<BranchRequestRegular />} title="Sales Pipeline" actions={[{ key: 'list', label: 'Opportunities', onClick: () => navigate('/opportunities') }]} />}>
        <MessageBar intent="error">
          <MessageBarBody>You do not have permission to view the sales pipeline.</MessageBarBody>
        </MessageBar>
      </EntityPageLayout>
    )
  }

  return (
    <EntityPageLayout
      header={<EntityHeader icon={<BranchRequestRegular />} title="Sales Pipeline" subtitle="Open opportunities by stage." actions={[
        { key: 'opportunities', label: 'Opportunities', onClick: () => navigate('/opportunities'), appearance: 'subtle' },
        { key: 'refresh', label: 'Refresh', onClick: () => void load(), appearance: 'secondary' },
      ]} />}
      alerts={error ? [{ intent: 'error', text: error }] : undefined}
    >
      <section className={styles.filterBar}>
        <Field label="Account">
          <LookupCombobox fieldKey="accountId" value={filters.accountId} onChange={(value) => setFilter('accountId', value)} />
        </Field>
        <Field label="Stage">
          <LookupCombobox fieldKey="opportunityStageId" value={filters.stageId} onChange={(value) => setFilter('stageId', value)} />
        </Field>
        <Field label="Rating">
          <LookupCombobox fieldKey="opportunityRatingId" value={filters.ratingId} onChange={(value) => setFilter('ratingId', value)} />
        </Field>
        <Field label="Industry">
          <LookupCombobox fieldKey="industryId" value={filters.industryId} onChange={(value) => setFilter('industryId', value)} />
        </Field>
        <Field label="Owner">
          <LookupCombobox fieldKey="ownerUserId" value={filters.ownerUserId} onChange={(value) => setFilter('ownerUserId', value)} />
        </Field>
        <Field label="Team">
          <LookupCombobox fieldKey="ownerTeamId" value={filters.ownerTeamId} onChange={(value) => setFilter('ownerTeamId', value)} />
        </Field>
        <Field label="Min Revenue">
          <Input size="small" type="number" value={filters.minRevenue} onChange={(_, data) => setFilter('minRevenue', data.value)} />
        </Field>
        <Field label="Max Revenue">
          <Input size="small" type="number" value={filters.maxRevenue} onChange={(_, data) => setFilter('maxRevenue', data.value)} />
        </Field>
        <Field label="Min Probability">
          <Input size="small" type="number" value={filters.minProbability} onChange={(_, data) => setFilter('minProbability', data.value)} />
        </Field>
        <Field label="Max Probability">
          <Input size="small" type="number" value={filters.maxProbability} onChange={(_, data) => setFilter('maxProbability', data.value)} />
        </Field>
        <Field label="Close From">
          <Input size="small" type="date" value={filters.closeDateFrom} onChange={(_, data) => setFilter('closeDateFrom', data.value)} />
        </Field>
        <Field label="Close To">
          <Input size="small" type="date" value={filters.closeDateTo} onChange={(_, data) => setFilter('closeDateTo', data.value)} />
        </Field>
        <div className={styles.inlineActions}>
          <Button size="small" appearance="secondary" onClick={() => setFilters(emptyFilters)}>Clear Filters</Button>
        </div>
      </section>

      {loading ? <Spinner size="small" label="Loading pipeline..." /> : null}

      <section className={styles.summaryStrip}>
        <div className={styles.metricCard}><p className={styles.metricLabel}>Open Opportunities</p><p className={styles.metricValue}>{summary.totalOpportunities}</p></div>
        <div className={styles.metricCard}><p className={styles.metricLabel}>Pipeline Revenue</p><p className={styles.metricValue}>{formatCurrency(summary.pipelineRevenue)}</p></div>
        <div className={styles.metricCard}><p className={styles.metricLabel}>Weighted Revenue</p><p className={styles.metricValue}>{formatCurrency(summary.weightedPipelineRevenue)}</p></div>
        <div className={styles.metricCard}><p className={styles.metricLabel}>Average Probability</p><p className={styles.metricValue}>{formatPercent(summary.averageProbability)}</p></div>
        <div className={styles.metricCard}><p className={styles.metricLabel}>Average Deal Size</p><p className={styles.metricValue}>{formatCurrency(summary.averageDealSize)}</p></div>
      </section>

      <div className={styles.pipelineGrid}>
        {(board?.stages ?? []).map((stage) => (
          <section
            key={stage.stageId}
            className={styles.stageColumn}
            onDragOver={(event) => {
              if (canMove) event.preventDefault()
            }}
            onDrop={(event) => {
              event.preventDefault()
              void moveStage(stage.stageId)
            }}
          >
            <div className={styles.stageHeader}>
              <p className={styles.stageTitle}>{stage.stageName}</p>
              <p className={styles.stageMeta}>{stage.count} deals | {formatCurrency(stage.pipelineRevenue)} | weighted {formatCurrency(stage.weightedRevenue)}</p>
            </div>
            <div className={styles.cardList}>
              {stage.opportunities.length === 0 ? <p className={styles.stageMeta}>No opportunities</p> : null}
              {stage.opportunities.map((opportunity) => (
                <button
                  key={opportunity.id}
                  type="button"
                  className={styles.pipelineCard}
                  draggable={canMove}
                  onDragStart={() => setDraggedOpportunityId(opportunity.id)}
                  onClick={() => navigate(`/opportunities/${opportunity.id}`)}
                >
                  <p className={styles.cardTitle}>{opportunity.topic}</p>
                  <p className={styles.cardMeta}>{opportunity.opportunityNumber} | {opportunity.accountName ?? 'No account'}</p>
                  <p className={styles.cardMeta}>{formatCurrency(opportunity.estimatedRevenue)} | {formatPercent(opportunity.probability)} | weighted {formatCurrency(opportunity.weightedRevenue)}</p>
                  <p className={styles.cardMeta}>Close {formatDate(opportunity.estimatedCloseDate) || 'not set'} | {opportunity.ownerName ?? 'Unassigned'}</p>
                  <div className={styles.cardBadges}>
                    <Badge size="small" appearance="tint" color={ratingTone(opportunity)}>{opportunity.ratingName ?? 'Unrated'}</Badge>
                    <Badge size="small" appearance="tint" color="informative">{opportunity.ageInDays} days</Badge>
                  </div>
                </button>
              ))}
            </div>
          </section>
        ))}
      </div>
    </EntityPageLayout>
  )
}
