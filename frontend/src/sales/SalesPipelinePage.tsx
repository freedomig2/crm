import { useCallback, useEffect, useMemo, useState } from 'react'
import { Badge, Input, MessageBar, MessageBarBody, Spinner } from '@fluentui/react-components'
import { BranchRequestRegular } from '@fluentui/react-icons'
import { useNavigate } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { EntityHeader, EntityPageLayout } from '../components/entity-ui/EntityComponents'
import { DateRangeFilterField } from '../components/filters/DateRangeFilterField'
import { FilterField } from '../components/filters/FilterField'
import { LookupFilterField } from '../components/filters/LookupFilterField'
import { DenseDataGrid } from '../components/grid/DenseDataGrid'
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
  const [draftFilters, setDraftFilters] = useState<PipelineFilters>(() => loadStoredFilters())
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

  const setDraftFilter = <K extends keyof PipelineFilters>(key: K, value: PipelineFilters[K]) => {
    setDraftFilters((current) => ({ ...current, [key]: value }))
  }

  const activeFilterCount = Object.values(filters).filter(Boolean).length

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
      <DenseDataGrid
        rows={[]}
        columns={[]}
        page={1}
        pageSize={10}
        search=""
        onPageChange={() => undefined}
        onPageSizeChange={() => undefined}
        onSearchChange={() => undefined}
        emptyMessage=""
        activeFilterCount={activeFilterCount}
        filterPanel={
          <>
            <LookupFilterField label="Account" fieldKey="accountId" value={draftFilters.accountId} onChange={(value) => setDraftFilter('accountId', value)} />
            <LookupFilterField label="Stage" fieldKey="opportunityStageId" value={draftFilters.stageId} onChange={(value) => setDraftFilter('stageId', value)} />
            <LookupFilterField label="Rating" fieldKey="opportunityRatingId" value={draftFilters.ratingId} onChange={(value) => setDraftFilter('ratingId', value)} />
            <LookupFilterField label="Industry" fieldKey="industryId" value={draftFilters.industryId} onChange={(value) => setDraftFilter('industryId', value)} />
            <LookupFilterField label="Owner" fieldKey="ownerUserId" value={draftFilters.ownerUserId} onChange={(value) => setDraftFilter('ownerUserId', value)} />
            <LookupFilterField label="Team" fieldKey="ownerTeamId" value={draftFilters.ownerTeamId} onChange={(value) => setDraftFilter('ownerTeamId', value)} />
            <FilterField label="Min Revenue">
              <Input size="small" type="number" value={draftFilters.minRevenue} onChange={(_, data) => setDraftFilter('minRevenue', data.value)} />
            </FilterField>
            <FilterField label="Max Revenue">
              <Input size="small" type="number" value={draftFilters.maxRevenue} onChange={(_, data) => setDraftFilter('maxRevenue', data.value)} />
            </FilterField>
            <FilterField label="Min Probability">
              <Input size="small" type="number" value={draftFilters.minProbability} onChange={(_, data) => setDraftFilter('minProbability', data.value)} />
            </FilterField>
            <FilterField label="Max Probability">
              <Input size="small" type="number" value={draftFilters.maxProbability} onChange={(_, data) => setDraftFilter('maxProbability', data.value)} />
            </FilterField>
            <DateRangeFilterField
              fromLabel="Close From"
              toLabel="Close To"
              fromValue={draftFilters.closeDateFrom}
              toValue={draftFilters.closeDateTo}
              onFromChange={(value) => setDraftFilter('closeDateFrom', value)}
              onToChange={(value) => setDraftFilter('closeDateTo', value)}
            />
          </>
        }
        onApplyFilters={() => setFilters(draftFilters)}
        onCancelFilters={() => setDraftFilters(filters)}
        onClearFilters={() => setDraftFilters(emptyFilters)}
      />

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
