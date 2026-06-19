import { useCallback, useEffect, useMemo, useState } from 'react'
import { Button, MessageBar, MessageBarBody, Spinner } from '@fluentui/react-components'
import { BranchRequestRegular } from '@fluentui/react-icons'
import { useNavigate } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { EntityHeader, EntityPageLayout } from '../components/entity-ui/EntityComponents'
import type { OpportunityPipelineStage } from '../types/models'
import { formatCurrency, formatDate } from './opportunityUtils'
import styles from './Opportunities.module.css'

export function OpportunityPipelinePage() {
  const navigate = useNavigate()
  const { hasPermission } = useAuth()
  const canView = hasPermission('Opportunities.ViewPipeline')
  const canChangeStage = hasPermission('Opportunities.ChangeStage')
  const [stages, setStages] = useState<OpportunityPipelineStage[]>([])
  const [draggedOpportunityId, setDraggedOpportunityId] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  const load = useCallback(async () => {
    if (!canView) return
    setLoading(true)
    setError('')
    try {
      const { data } = await api.get<OpportunityPipelineStage[]>('api/opportunities/pipeline')
      setStages(data)
    } catch {
      setError('Failed to load opportunity pipeline.')
    } finally {
      setLoading(false)
    }
  }, [canView])

  useEffect(() => {
    void Promise.resolve().then(load)
  }, [load])

  const totals = useMemo(
    () => ({
      count: stages.reduce((sum, stage) => sum + stage.count, 0),
      estimated: stages.reduce((sum, stage) => sum + stage.estimatedRevenue, 0),
      weighted: stages.reduce((sum, stage) => sum + stage.weightedRevenue, 0),
      stages: stages.length,
    }),
    [stages],
  )

  const changeStage = async (stageId: string) => {
    if (!draggedOpportunityId || !canChangeStage) return
    setError('')
    try {
      await api.post(`api/opportunities/${draggedOpportunityId}/change-stage`, { opportunityStageId: stageId })
      setDraggedOpportunityId(null)
      await load()
    } catch {
      setError('Failed to change opportunity stage.')
    }
  }

  return (
    <EntityPageLayout
      header={<EntityHeader icon={<BranchRequestRegular />} title="Opportunity Pipeline" subtitle="Open opportunities grouped by stage." actions={[{ key: 'list', label: 'Opportunity List', onClick: () => navigate('/opportunities'), appearance: 'subtle' }]} />}
      alerts={error ? [{ intent: 'error', text: error }] : undefined}
    >
      {!canView ? <MessageBar intent="error"><MessageBarBody>You do not have permission to view the opportunity pipeline.</MessageBarBody></MessageBar> : null}
      {loading ? <Spinner size="small" label="Loading pipeline..." /> : null}

      {canView ? (
        <>
          <section className={styles.summaryStrip}>
            <div className={styles.summaryItem}><p className={styles.summaryLabel}>Open Opportunities</p><p className={styles.summaryValue}>{totals.count}</p></div>
            <div className={styles.summaryItem}><p className={styles.summaryLabel}>Pipeline Value</p><p className={styles.summaryValue}>{formatCurrency(totals.estimated)}</p></div>
            <div className={styles.summaryItem}><p className={styles.summaryLabel}>Weighted Value</p><p className={styles.summaryValue}>{formatCurrency(totals.weighted)}</p></div>
            <div className={styles.summaryItem}><p className={styles.summaryLabel}>Stages</p><p className={styles.summaryValue}>{totals.stages}</p></div>
          </section>

          <div className={styles.pipelineGrid}>
            {stages.map((stage) => (
              <section
                key={stage.stageId}
                className={styles.stageColumn}
                onDragOver={(event) => {
                  if (canChangeStage) event.preventDefault()
                }}
                onDrop={(event) => {
                  event.preventDefault()
                  void changeStage(stage.stageId)
                }}
              >
                <div className={styles.stageHeader}>
                  <p className={styles.stageTitle}>{stage.stageName}</p>
                  <p className={styles.stageMeta}>{stage.count} deals | {formatCurrency(stage.estimatedRevenue)} | weighted {formatCurrency(stage.weightedRevenue)}</p>
                </div>
                <div className={styles.cardList}>
                  {stage.opportunities.length === 0 ? <p className={styles.stageMeta}>No opportunities</p> : null}
                  {stage.opportunities.map((opportunity) => (
                    <button
                      key={opportunity.id}
                      type="button"
                      className={styles.pipelineCard}
                      draggable={canChangeStage}
                      onDragStart={() => setDraggedOpportunityId(opportunity.id)}
                      onClick={() => navigate(`/opportunities/${opportunity.id}`)}
                    >
                      <p className={styles.cardTitle}>{opportunity.topic}</p>
                      <p className={styles.cardMeta}>{opportunity.opportunityNumber} | {opportunity.accountName ?? 'No account'}</p>
                      <p className={styles.cardMeta}>{formatCurrency(opportunity.estimatedRevenue)} | {opportunity.probability}% | close {formatDate(opportunity.estimatedCloseDate) || 'not set'}</p>
                    </button>
                  ))}
                </div>
              </section>
            ))}
          </div>

          <Button size="small" appearance="secondary" onClick={() => void load()}>Refresh Pipeline</Button>
        </>
      ) : null}
    </EntityPageLayout>
  )
}
