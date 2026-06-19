import { useEffect, useMemo, useState } from 'react'
import { Bar, BarChart, CartesianGrid, Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { MessageBar, MessageBarBody, Spinner } from '@fluentui/react-components'
import { ChartMultipleRegular, DataPieRegular } from '@fluentui/react-icons'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { EntityHeader, EntityPageLayout, FormSectionCard } from '../components/entity-ui/EntityComponents'
import { PageHeader } from '../layout/components/PageHeader'
import type { OpportunityPipelineAnalytics, RevenueTracking, SalesPerformanceDashboard } from '../types/models'
import { formatCurrency, formatDateTime, formatPercent } from './salesUtils'
import styles from './Sales.module.css'

export function RevenueTrackingPage() {
  const { hasPermission } = useAuth()
  const canView = hasPermission('SalesPerformance.View')
  const [metrics, setMetrics] = useState<RevenueTracking | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!canView) {
      return
    }

    void (async () => {
      setLoading(true)
      setError('')
      try {
        const { data } = await api.get<RevenueTracking>('api/sales/revenue')
        setMetrics(data)
      } catch {
        setError('Failed to load revenue tracking.')
      } finally {
        setLoading(false)
      }
    })()
  }, [canView])

  if (!canView) {
    return (
      <div>
        <PageHeader title="Revenue Tracking" subtitle="Sales revenue breakdown." />
        <MessageBar intent="error"><MessageBarBody>You do not have permission to view sales performance.</MessageBarBody></MessageBar>
      </div>
    )
  }

  return (
    <div>
      <PageHeader title="Revenue Tracking" subtitle="Sales revenue breakdown." />
      {loading ? <Spinner size="small" label="Loading revenue tracking..." /> : null}
      {error ? <MessageBar intent="error" style={{ marginBottom: 10 }}><MessageBarBody>{error}</MessageBarBody></MessageBar> : null}

      <section className={styles.metricGrid}>
        <div className={styles.metricCard}><p className={styles.metricLabel}>Won Revenue</p><p className={styles.metricValue}>{formatCurrency(metrics?.wonRevenue)}</p></div>
        <div className={styles.metricCard}><p className={styles.metricLabel}>Lost Revenue</p><p className={styles.metricValue}>{formatCurrency(metrics?.lostRevenue)}</p></div>
        <div className={styles.metricCard}><p className={styles.metricLabel}>Pipeline Revenue</p><p className={styles.metricValue}>{formatCurrency(metrics?.pipelineRevenue)}</p></div>
        <div className={styles.metricCard}><p className={styles.metricLabel}>Weighted Revenue</p><p className={styles.metricValue}>{formatCurrency(metrics?.weightedRevenue)}</p></div>
      </section>

      <section className={styles.chartGrid}>
        <SalesLineChart title="Revenue Trend" data={metrics?.revenueTrend ?? []} />
        <SalesBarChart title="Revenue by Account" data={metrics?.revenueByAccount ?? []} fill="#0d9488" />
        <SalesBarChart title="Revenue by Industry" data={metrics?.revenueByIndustry ?? []} fill="#c2410c" />
        <SalesBarChart title="Revenue by Salesperson" data={metrics?.revenueBySalesperson ?? []} fill="#7c3aed" />
      </section>
    </div>
  )
}

export function SalesPerformancePage() {
  const { hasPermission } = useAuth()
  const canView = hasPermission('SalesPerformance.View')
  const [metrics, setMetrics] = useState<SalesPerformanceDashboard | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!canView) {
      return
    }

    void (async () => {
      setLoading(true)
      setError('')
      try {
        const { data } = await api.get<SalesPerformanceDashboard>('api/sales/performance')
        setMetrics(data)
      } catch {
        setError('Failed to load sales performance.')
      } finally {
        setLoading(false)
      }
    })()
  }, [canView])

  if (!canView) {
    return (
      <div>
        <PageHeader title="Sales Performance" subtitle="Pipeline, revenue, win rate, and sales cycle metrics." />
        <MessageBar intent="error"><MessageBarBody>You do not have permission to view sales performance.</MessageBarBody></MessageBar>
      </div>
    )
  }

  return (
    <div>
      <PageHeader title="Sales Performance" subtitle="Pipeline, revenue, win rate, and sales cycle metrics." />
      {loading ? <Spinner size="small" label="Loading sales performance..." /> : null}
      {error ? <MessageBar intent="error" style={{ marginBottom: 10 }}><MessageBarBody>{error}</MessageBarBody></MessageBar> : null}

      <section className={styles.metricGrid}>
        <div className={styles.metricCard}><p className={styles.metricLabel}>Open Opportunities</p><p className={styles.metricValue}>{metrics?.openOpportunities ?? 0}</p></div>
        <div className={styles.metricCard}><p className={styles.metricLabel}>Won Opportunities</p><p className={styles.metricValue}>{metrics?.wonOpportunities ?? 0}</p></div>
        <div className={styles.metricCard}><p className={styles.metricLabel}>Lost Opportunities</p><p className={styles.metricValue}>{metrics?.lostOpportunities ?? 0}</p></div>
        <div className={styles.metricCard}><p className={styles.metricLabel}>Win Rate</p><p className={styles.metricValue}>{formatPercent(metrics?.winRate)}</p></div>
        <div className={styles.metricCard}><p className={styles.metricLabel}>Average Deal Size</p><p className={styles.metricValue}>{formatCurrency(metrics?.averageDealSize)}</p></div>
        <div className={styles.metricCard}><p className={styles.metricLabel}>Sales Cycle</p><p className={styles.metricValue}>{metrics?.averageSalesCycleDays ?? 0} days</p></div>
        <div className={styles.metricCard}><p className={styles.metricLabel}>Revenue This Month</p><p className={styles.metricValue}>{formatCurrency(metrics?.revenueThisMonth)}</p></div>
        <div className={styles.metricCard}><p className={styles.metricLabel}>Revenue This Quarter</p><p className={styles.metricValue}>{formatCurrency(metrics?.revenueThisQuarter)}</p></div>
        <div className={styles.metricCard}><p className={styles.metricLabel}>Forecast Revenue</p><p className={styles.metricValue}>{formatCurrency(metrics?.forecastRevenue)}</p></div>
        <div className={styles.metricCard}><p className={styles.metricLabel}>Forecast Accuracy</p><p className={styles.metricValue}>{formatPercent(metrics?.forecastAccuracy)}</p></div>
      </section>

      <section className={styles.chartGrid}>
        <SalesBarChart title="Pipeline by Stage" data={metrics?.pipelineByStage ?? []} fill="#0f6cbd" />
        <SalesLineChart title="Revenue Trend" data={metrics?.revenueTrend ?? []} />
        <SalesBarChart title="Opportunities by Owner" data={metrics?.opportunitiesByOwner ?? []} fill="#0d9488" />
        <SalesBarChart title="Opportunities by Industry" data={metrics?.opportunitiesByIndustry ?? []} fill="#c2410c" />
        <SalesLineChart title="Win Rate Trend" data={metrics?.winRateTrend ?? []} percent />
        <SalesLineChart title="Forecast Accuracy Trend" data={metrics?.forecastAccuracyTrend ?? []} percent />
      </section>

      <section className={styles.chartGrid}>
        <LeaderboardTable title="Top Salespeople" rows={metrics?.topSalespeople ?? []} />
        <LeaderboardTable title="Top Teams" rows={metrics?.topTeams ?? []} />
      </section>
    </div>
  )
}

export function OpportunityPipelineAnalyticsPanel({ opportunityId }: { opportunityId: string }) {
  const { hasPermission } = useAuth()
  const canView = hasPermission('Pipeline.View')
  const [analytics, setAnalytics] = useState<OpportunityPipelineAnalytics | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!canView) {
      return
    }

    void (async () => {
      setLoading(true)
      setError('')
      try {
        const { data } = await api.get<OpportunityPipelineAnalytics>(`api/opportunities/${opportunityId}/pipeline-analytics`)
        setAnalytics(data)
      } catch {
        setError('Failed to load pipeline analytics.')
      } finally {
        setLoading(false)
      }
    })()
  }, [canView, opportunityId])

  const history = useMemo(() => analytics?.stageHistory ?? [], [analytics])

  if (!canView) {
    return (
      <MessageBar intent="error">
        <MessageBarBody>You do not have permission to view pipeline analytics.</MessageBarBody>
      </MessageBar>
    )
  }

  return (
    <div>
      {loading ? <Spinner size="small" label="Loading pipeline analytics..." /> : null}
      {error ? <MessageBar intent="error" style={{ marginBottom: 10 }}><MessageBarBody>{error}</MessageBarBody></MessageBar> : null}

      <section className={styles.metricGrid}>
        <div className={styles.metricCard}><p className={styles.metricLabel}>Current Stage</p><p className={styles.metricValue}>{analytics?.currentStageName ?? 'Not set'}</p></div>
        <div className={styles.metricCard}><p className={styles.metricLabel}>Days in Stage</p><p className={styles.metricValue}>{analytics?.daysInStage ?? 0}</p></div>
        <div className={styles.metricCard}><p className={styles.metricLabel}>Stage Changes</p><p className={styles.metricValue}>{history.length}</p></div>
      </section>

      <section className={styles.chartGrid}>
        <SalesLineChart title="Probability Trend" data={analytics?.probabilityTrend ?? []} percent />
        <SalesLineChart title="Revenue Trend" data={analytics?.revenueTrend ?? []} />
      </section>

      <FormSectionCard title="Stage History" icon={<DataPieRegular />}>
        <div className={styles.timeline}>
          {history.length === 0 ? <div className={styles.emptyState}>No stage changes recorded.</div> : null}
          {history.map((item) => (
            <article key={item.id} className={styles.timelineItem}>
              <div className={styles.timelineTop}>
                <div>
                  <p className={styles.timelineTitle}>{item.previousStageName ?? 'Not set'} to {item.newStageName}</p>
                  <p className={styles.timelineMeta}>{item.changedByUserName ?? 'System'} | {formatDateTime(item.changedAt)}</p>
                </div>
              </div>
              {item.notes ? <p className={styles.timelineBody}>{item.notes}</p> : null}
            </article>
          ))}
        </div>
      </FormSectionCard>
    </div>
  )
}

function SalesBarChart({ title, data, fill }: { title: string; data: Array<{ name: string; value: number }>; fill: string }) {
  return (
    <article className={styles.chartCard}>
      <p className={styles.chartTitle}>{title}</p>
      <ResponsiveContainer width="100%" height={220}>
        <BarChart data={data}>
          <CartesianGrid strokeDasharray="2 2" />
          <XAxis dataKey="name" tick={{ fontSize: 11 }} />
          <YAxis tick={{ fontSize: 11 }} />
          <Tooltip formatter={(value) => formatCurrency(Number(value))} />
          <Bar dataKey="value" fill={fill} />
        </BarChart>
      </ResponsiveContainer>
    </article>
  )
}

function SalesLineChart({ title, data, percent }: { title: string; data: Array<{ name: string; value: number }>; percent?: boolean }) {
  return (
    <article className={styles.chartCard}>
      <p className={styles.chartTitle}>{title}</p>
      <ResponsiveContainer width="100%" height={220}>
        <LineChart data={data}>
          <CartesianGrid strokeDasharray="2 2" />
          <XAxis dataKey="name" tick={{ fontSize: 11 }} />
          <YAxis tick={{ fontSize: 11 }} />
          <Tooltip formatter={(value) => (percent ? formatPercent(Number(value)) : formatCurrency(Number(value)))} />
          <Line type="monotone" dataKey="value" stroke="#0f6cbd" strokeWidth={2} />
        </LineChart>
      </ResponsiveContainer>
    </article>
  )
}

function LeaderboardTable({
  title,
  rows,
}: {
  title: string
  rows: Array<{ name: string; revenue: number; winRate: number; opportunitiesClosed: number; targetAchievement: number }>
}) {
  return (
    <article className={styles.tableCard}>
      <p className={styles.chartTitle}>{title}</p>
      <table className={styles.recordTable}>
        <thead>
          <tr>
            <th>Name</th>
            <th>Revenue</th>
            <th>Win Rate</th>
            <th>Closed</th>
            <th>Target</th>
          </tr>
        </thead>
        <tbody>
          {rows.length === 0 ? (
            <tr><td colSpan={5}>No records</td></tr>
          ) : rows.map((row) => (
            <tr key={row.name}>
              <td>{row.name}</td>
              <td>{formatCurrency(row.revenue)}</td>
              <td>{formatPercent(row.winRate)}</td>
              <td>{row.opportunitiesClosed}</td>
              <td>{formatPercent(row.targetAchievement)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </article>
  )
}

export function SalesAnalyticsShell() {
  return (
    <EntityPageLayout header={<EntityHeader icon={<ChartMultipleRegular />} title="Sales Analytics" />}>
      <div />
    </EntityPageLayout>
  )
}
