import { Badge, Button, Spinner } from '@fluentui/react-components'
import { useEffect, useState } from 'react'
import { Bar, BarChart, CartesianGrid, Pie, PieChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { useNavigate } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import type { CommandAction } from '../layout/components/CommandBar'
import { PageHeader } from '../layout/components/PageHeader'
import { CommandBar } from '../layout/components/CommandBar'
import type { DashboardChartPoint, DashboardSummary } from '../types/models'
import styles from './DashboardWorkspace.module.css'

const formatMoney = (value?: number) => new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(value ?? 0)
const formatDate = (value?: string) => (value ? new Date(value).toLocaleDateString() : 'Not set')
const asChartPoints = (value: unknown): DashboardChartPoint[] => (Array.isArray(value) ? (value as DashboardChartPoint[]) : [])

const emptySummary: DashboardSummary = {
  totalLeads: 0,
  newLeads: 0,
  qualifiedLeads: 0,
  convertedLeads: 0,
  openOpportunities: 0,
  pipelineValue: 0,
  weightedPipeline: 0,
  winRate: 0,
  openCases: 0,
  overdueTasks: 0,
  revenueThisMonth: 0,
  slaBreaches: 0,
  recentLeads: [],
  opportunitiesClosingSoon: [],
  upcomingFollowUps: [],
  slaAlerts: [],
}

export function DashboardPage() {
  const navigate = useNavigate()
  const { hasPermission } = useAuth()
  const [summary, setSummary] = useState<DashboardSummary | null>(null)
  const [leadsByStatus, setLeadsByStatus] = useState<DashboardChartPoint[]>([])
  const [opportunitiesByStage, setOpportunitiesByStage] = useState<DashboardChartPoint[]>([])
  const [revenueForecast, setRevenueForecast] = useState<DashboardChartPoint[]>([])
  const [casesByPriority, setCasesByPriority] = useState<DashboardChartPoint[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const fetchDashboardData = async () => {
    const [summaryRes, leadsRes, opportunitiesRes, revenueRes, casesRes] = await Promise.all([
      api.get<DashboardSummary>('api/dashboard/summary'),
      api.get<DashboardChartPoint[]>('api/dashboard/charts/leads-by-status'),
      api.get<DashboardChartPoint[]>('api/dashboard/charts/opportunities-by-stage'),
      api.get<DashboardChartPoint[]>('api/dashboard/charts/revenue-forecast'),
      api.get<DashboardChartPoint[]>('api/dashboard/charts/cases-by-priority'),
    ])

    return {
      summary: (summaryRes.data && typeof summaryRes.data === 'object') ? summaryRes.data : emptySummary,
      leadsByStatus: asChartPoints(leadsRes.data),
      opportunitiesByStage: asChartPoints(opportunitiesRes.data),
      revenueForecast: asChartPoints(revenueRes.data),
      casesByPriority: asChartPoints(casesRes.data),
    }
  }

  const loadDashboard = async () => {
    setLoading(true)
    setError(null)

    try {
      const payload = await fetchDashboardData()
      setSummary(payload.summary)
      setLeadsByStatus(payload.leadsByStatus)
      setOpportunitiesByStage(payload.opportunitiesByStage)
      setRevenueForecast(payload.revenueForecast)
      setCasesByPriority(payload.casesByPriority)
    } catch {
      setError('Dashboard data could not be loaded. Please retry.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void (async () => {
      setLoading(true)
      setError(null)
      try {
        const payload = await fetchDashboardData()
        setSummary(payload.summary)
        setLeadsByStatus(payload.leadsByStatus)
        setOpportunitiesByStage(payload.opportunitiesByStage)
        setRevenueForecast(payload.revenueForecast)
        setCasesByPriority(payload.casesByPriority)
      } catch {
        setError('Dashboard data could not be loaded. Please retry.')
      } finally {
        setLoading(false)
      }
    })()
  }, [])

  const actions: CommandAction[] = [{ key: 'refresh', label: 'Refresh', onClick: () => void loadDashboard() }]

  if (hasPermission('Dashboard.View')) {
    actions.push({ key: 'my-work', label: 'My Work', onClick: () => { navigate('/dashboard/my-work') } })
  }

  if (hasPermission('Activities.View')) {
    actions.push({ key: 'my-activities', label: 'My Activities', onClick: () => { navigate('/dashboard/my-activities') } })
    actions.push({ key: 'my-open-tasks', label: 'My Open Tasks', onClick: () => { navigate('/dashboard/my-open-tasks') } })
  }

  return (
    <div className={styles.layout}>
      <PageHeader
        title="Dashboard"
        subtitle="Enterprise command center with live CRM operational metrics"
      />

      <CommandBar actions={actions} />

      <section className={styles.hero}>
        <h2 className={styles.heroTitle}>Operations pulse for Sales and Service</h2>
        <p className={styles.heroSubtitle}>Monitor conversion momentum, pipeline pressure, customer service exposure, and team follow-up velocity in one place.</p>
      </section>

      {error ? <div className={styles.error}>{error}</div> : null}

      {loading ? <Spinner label="Loading dashboard" /> : null}

      <section className={styles.statsGrid}>
        <article className={styles.statCard}>
          <p className={styles.statLabel}>Total Leads</p>
          <p className={styles.statValue}>{summary?.totalLeads ?? 0}</p>
          <p className={styles.statSubtle}>New: {summary?.newLeads ?? 0} • Qualified: {summary?.qualifiedLeads ?? 0}</p>
        </article>
        <article className={styles.statCard}>
          <p className={styles.statLabel}>Open Opportunities</p>
          <p className={styles.statValue}>{summary?.openOpportunities ?? 0}</p>
          <p className={styles.statSubtle}>Win rate <span className={styles.kpiAccent}>{summary?.winRate ?? 0}%</span></p>
        </article>
        <article className={styles.statCard}>
          <p className={styles.statLabel}>Pipeline Value</p>
          <p className={styles.statValue}>{formatMoney(summary?.pipelineValue)}</p>
          <p className={styles.statSubtle}>Weighted {formatMoney(summary?.weightedPipeline)}</p>
        </article>
        <article className={styles.statCard}>
          <p className={styles.statLabel}>Revenue This Month</p>
          <p className={styles.statValue}>{formatMoney(summary?.revenueThisMonth)}</p>
          <p className={styles.statSubtle}>Converted leads {summary?.convertedLeads ?? 0}</p>
        </article>
        <article className={styles.statCard}>
          <p className={styles.statLabel}>Open Cases</p>
          <p className={styles.statValue}>{summary?.openCases ?? 0}</p>
          <p className={styles.statSubtle}>SLA breaches <span className={styles.warning}>{summary?.slaBreaches ?? 0}</span></p>
        </article>
        <article className={styles.statCard}>
          <p className={styles.statLabel}>Overdue Tasks</p>
          <p className={styles.statValue}>{summary?.overdueTasks ?? 0}</p>
          <p className={styles.statSubtle}>Action required today</p>
        </article>
      </section>

      <section className={styles.contentGrid}>
        <div className={styles.leftColumn}>
          <article className={styles.panel}>
            <div className={styles.panelHeader}>
              <h3 className={styles.panelTitle}>Leads by Status</h3>
              <Badge appearance="outline">Live</Badge>
            </div>
            <ResponsiveContainer width="100%" height={220}>
              <BarChart data={leadsByStatus}>
              <CartesianGrid strokeDasharray="2 2" />
              <XAxis dataKey="name" tick={{ fontSize: 11 }} />
              <YAxis tick={{ fontSize: 11 }} />
              <Tooltip />
                <Bar dataKey="count" fill="#0b5cab" />
              </BarChart>
            </ResponsiveContainer>
          </article>

          <article className={styles.panel}>
            <div className={styles.panelHeader}>
              <h3 className={styles.panelTitle}>Opportunities by Stage</h3>
              <Badge appearance="tint">Revenue weighted</Badge>
            </div>
            <ResponsiveContainer width="100%" height={220}>
              <BarChart data={opportunitiesByStage}>
              <CartesianGrid strokeDasharray="2 2" />
              <XAxis dataKey="name" tick={{ fontSize: 11 }} />
              <YAxis tick={{ fontSize: 11 }} />
                <Tooltip formatter={(value) => formatMoney(Number(value))} />
                <Bar dataKey="value" fill="#0f766e" />
              </BarChart>
            </ResponsiveContainer>
          </article>

          <article className={styles.panel}>
            <div className={styles.panelHeader}>
              <h3 className={styles.panelTitle}>Forecast Trend</h3>
              <Badge appearance="ghost">12 periods</Badge>
            </div>
            <ResponsiveContainer width="100%" height={220}>
              <BarChart data={revenueForecast}>
                <CartesianGrid strokeDasharray="2 2" />
                <XAxis dataKey="name" tick={{ fontSize: 11 }} />
                <YAxis tick={{ fontSize: 11 }} />
                <Tooltip formatter={(value) => formatMoney(Number(value))} />
                <Bar dataKey="value" fill="#7c3aed" />
              </BarChart>
            </ResponsiveContainer>
          </article>

          <article className={styles.panel}>
            <div className={styles.panelHeader}>
              <h3 className={styles.panelTitle}>Cases by Priority</h3>
              <Badge appearance="outline">Service load</Badge>
            </div>
            <ResponsiveContainer width="100%" height={220}>
              <PieChart>
                <Pie data={casesByPriority} dataKey="count" nameKey="name" outerRadius={82} fill="#d97706" />
                <Tooltip />
              </PieChart>
            </ResponsiveContainer>
          </article>
        </div>

        <div className={styles.rightColumn}>
          <article className={styles.panel}>
            <div className={styles.panelHeader}>
              <h3 className={styles.panelTitle}>Recent Leads</h3>
              <Button size="small" appearance="subtle" onClick={() => navigate('/leads')}>Open Leads</Button>
            </div>
            <ul className={styles.rowList}>
              {(summary?.recentLeads ?? []).map((item) => (
                <li className={styles.rowItem} key={item.id}>
                  <p className={styles.rowTitle}>{item.leadNumber} - {item.topic}</p>
                  <p className={styles.rowMeta}>
                    <span>{item.statusName ?? 'Status not set'}</span>
                    <span>Score {item.score}</span>
                    <span>{item.ownerName ?? 'Unassigned'}</span>
                  </p>
                </li>
              ))}
            </ul>
            {(summary?.recentLeads ?? []).length === 0 ? <p className={styles.empty}>No recent leads</p> : null}
          </article>

          <article className={styles.panel}>
            <div className={styles.panelHeader}>
              <h3 className={styles.panelTitle}>Opportunities Closing Soon</h3>
              <Button size="small" appearance="subtle" onClick={() => navigate('/opportunities')}>Open Opportunities</Button>
            </div>
            <ul className={styles.rowList}>
              {(summary?.opportunitiesClosingSoon ?? []).map((item) => (
                <li className={styles.rowItem} key={item.id}>
                  <p className={styles.rowTitle}>{item.opportunityNumber} - {item.topic}</p>
                  <p className={styles.rowMeta}>
                    <span>{item.stageName ?? 'Stage not set'}</span>
                    <span>{formatMoney(item.estimatedRevenue)}</span>
                    <span>{formatDate(item.estimatedCloseDate)}</span>
                  </p>
                </li>
              ))}
            </ul>
            {(summary?.opportunitiesClosingSoon ?? []).length === 0 ? <p className={styles.empty}>No opportunities closing in the next 14 days</p> : null}
          </article>

          <article className={styles.panel}>
            <div className={styles.panelHeader}>
              <h3 className={styles.panelTitle}>Upcoming Follow Ups</h3>
              <Button size="small" appearance="subtle" onClick={() => navigate('/dashboard/my-open-tasks')}>View Task Board</Button>
            </div>
            <ul className={styles.rowList}>
              {(summary?.upcomingFollowUps ?? []).map((item) => (
                <li className={styles.rowItem} key={item.id}>
                  <p className={styles.rowTitle}>{item.subject}</p>
                  <p className={styles.rowMeta}>
                    <span>{item.priorityName ?? 'Normal'}</span>
                    <span>{item.relatedRecord}</span>
                    <span>{formatDate(item.dueDate)}</span>
                  </p>
                </li>
              ))}
            </ul>
            {(summary?.upcomingFollowUps ?? []).length === 0 ? <p className={styles.empty}>No follow-up tasks in the next 7 days</p> : null}
          </article>

          <article className={styles.panel}>
            <div className={styles.panelHeader}>
              <h3 className={styles.panelTitle}>SLA Alerts</h3>
              <Button size="small" appearance="subtle" onClick={() => navigate('/service/cases')}>Open Cases</Button>
            </div>
            <ul className={styles.rowList}>
              {(summary?.slaAlerts ?? []).map((item) => (
                <li className={styles.rowItem} key={item.id}>
                  <p className={styles.rowTitle}>{item.caseNumber} - {item.title}</p>
                  <p className={styles.rowMeta}>
                    <span>{item.priorityName ?? 'Not set'}</span>
                    <span>{item.assignedToName ?? 'Unassigned'}</span>
                    <span className={styles.warning}>{formatDate(item.dueAt)}</span>
                  </p>
                </li>
              ))}
            </ul>
            {(summary?.slaAlerts ?? []).length === 0 ? <p className={styles.empty}>No active SLA breaches</p> : null}
          </article>
        </div>
      </section>

      <div>
        <Button appearance="secondary" onClick={() => navigate('/dashboard/my-work')}>Go To My Work</Button>
      </div>
    </div>
  )
}
