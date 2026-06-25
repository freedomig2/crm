import { Badge, Button, Card, Spinner, Switch } from '@fluentui/react-components'
import {
  ArrowCircleUpRegular,
  ArrowCircleDownRegular,
  ArrowTrendingRegular,
  ClipboardTaskRegular,
  DismissCircleRegular,
  EyeOffRegular,
  PinRegular,
  StarRegular,
} from '@fluentui/react-icons'
import { useCallback, useEffect, useMemo, useState } from 'react'
import type { AxiosRequestConfig } from 'axios'
import {
  Bar,
  BarChart,
  CartesianGrid,
  Funnel,
  FunnelChart,
  LabelList,
  Legend,
  Line,
  LineChart,
  Pie,
  PieChart,
  RadialBar,
  RadialBarChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'
import { useNavigate } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import type { CommandAction } from '../layout/components/CommandBar'
import { PageHeader } from '../layout/components/PageHeader'
import { CommandBar } from '../layout/components/CommandBar'
import type {
  DashboardActivityFeed,
  DashboardCustomers,
  DashboardLayoutPreference,
  DashboardManagement,
  DashboardMyActivities,
  DashboardMyOpenTasks,
  DashboardMyWork,
  DashboardPipeline,
  DashboardRevenue,
  DashboardService,
  DashboardSummary,
  DashboardWidgetPreference,
} from '../types/models'
import styles from './DashboardPage.module.css'

const formatMoney = (value?: number) => new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(value ?? 0)
const formatDate = (value?: string) => (value ? new Date(value).toLocaleDateString() : 'Not set')
const formatPercent = (value?: number) => `${Math.round(value ?? 0)}%`
const formatTimelineTime = (value?: string) => (value ? new Date(value).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : '--:--')

type WidgetId =
  | 'kpis'
  | 'sales'
  | 'my-work'
  | 'activity-timeline'
  | 'customers'
  | 'service'
  | 'management'
  | 'quick-actions'

const widgetOrder: WidgetId[] = ['kpis', 'sales', 'my-work', 'activity-timeline', 'customers', 'service', 'management', 'quick-actions']

const emptySummary: DashboardSummary = {
  welcome: {
    userName: 'User',
    dateLabel: '',
    currentRole: 'Contributor',
    businessUnit: 'Unassigned',
    team: 'Unassigned',
    openTasks: 0,
    overdueActivities: 0,
    opportunitiesClosingThisWeek: 0,
    slaBreaches: 0,
    hasManagementAccess: false,
  },
  kpis: [],
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

const emptyPipeline: DashboardPipeline = {
  funnelStages: [],
  opportunityStageDistribution: [],
  forecastAccuracyPercent: 0,
}

const emptyRevenue: DashboardRevenue = {
  revenueThisMonth: 0,
  revenueThisQuarter: 0,
  monthlyTrend: [],
}

const emptyWork: DashboardMyWork = {
  assignedLeads: [],
  assignedOpportunities: [],
  assignedCases: [],
  assignedActivities: [],
  pendingApprovals: [],
  overdueTasks: [],
}

const emptyTasks: DashboardMyOpenTasks = {
  items: [],
  totalCount: 0,
  overdueCount: 0,
  page: 1,
  pageSize: 20,
}

const emptyActivities: DashboardMyActivities = {
  items: [],
  totalCount: 0,
  page: 1,
  pageSize: 20,
  activitiesByStatus: [],
}

const emptyCustomers: DashboardCustomers = {
  topCustomers: [],
  atRiskCustomers: [],
  newCustomers: [],
}

const emptyService: DashboardService = {
  casesByPriority: [],
  casesByStatus: [],
  slaCompliancePercent: 0,
  casesRequiringAttention: [],
}

const emptyManagement: DashboardManagement = {
  isVisible: false,
  topSalespeople: [],
  teamPerformance: [],
  leadConversionTrend: [],
  revenueByTeam: [],
}

const emptyFeed: DashboardActivityFeed = {
  items: [],
}

function createDefaultWidgetLayout(includeManagement: boolean): DashboardWidgetPreference[] {
  return widgetOrder
    .filter((id) => includeManagement || id !== 'management')
    .map((widgetId, index) => ({ widgetId, order: index, isVisible: true, isPinned: false }))
}

function mergeWidgetLayout(preference: DashboardLayoutPreference | null, includeManagement: boolean): DashboardWidgetPreference[] {
  const defaults = createDefaultWidgetLayout(includeManagement)
  if (!preference || !Array.isArray(preference.widgets)) {
    return defaults
  }

  const map = new Map(preference.widgets.map((widget) => [widget.widgetId, widget]))
  return defaults
    .map((widget, index) => {
      const existing = map.get(widget.widgetId)
      return {
        widgetId: widget.widgetId,
        order: existing?.order ?? index,
        isVisible: existing?.isVisible ?? true,
        isPinned: existing?.isPinned ?? false,
      }
    })
    .sort((a, b) => a.order - b.order)
    .map((widget, index) => ({ ...widget, order: index }))
}

export function DashboardPage() {
  const navigate = useNavigate()
  const { hasPermission } = useAuth()
  const [summary, setSummary] = useState<DashboardSummary>(emptySummary)
  const [pipeline, setPipeline] = useState<DashboardPipeline>(emptyPipeline)
  const [revenue, setRevenue] = useState<DashboardRevenue>(emptyRevenue)
  const [myWork, setMyWork] = useState<DashboardMyWork>(emptyWork)
  const [myTasks, setMyTasks] = useState<DashboardMyOpenTasks>(emptyTasks)
  const [myActivities, setMyActivities] = useState<DashboardMyActivities>(emptyActivities)
  const [customers, setCustomers] = useState<DashboardCustomers>(emptyCustomers)
  const [service, setService] = useState<DashboardService>(emptyService)
  const [management, setManagement] = useState<DashboardManagement>(emptyManagement)
  const [activityFeed, setActivityFeed] = useState<DashboardActivityFeed>(emptyFeed)
  const [widgetLayout, setWidgetLayout] = useState<DashboardWidgetPreference[]>(createDefaultWidgetLayout(false))
  const [customizeWidgets, setCustomizeWidgets] = useState(false)
  const [savingLayout, setSavingLayout] = useState(false)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const visibleWidgets = useMemo(
    () => widgetLayout
      .filter((widget) => widget.isVisible)
      .sort((a, b) => {
        if (a.isPinned !== b.isPinned) {
          return a.isPinned ? -1 : 1
        }
        return a.order - b.order
      }),
    [widgetLayout],
  )

  const fetchDashboardData = useCallback(async () => {
    const safeGet = async <T,>(url: string, config?: AxiosRequestConfig): Promise<T | null> => {
      try {
        const response = await api.get<T>(url, config)
        return response.data ?? null
      } catch {
        return null
      }
    }

    const [
      summaryRes,
      myWorkRes,
      myTasksRes,
      myActivitiesRes,
      pipelineRes,
      revenueRes,
      casesRes,
      customersRes,
      managementRes,
      activityFeedRes,
      layoutRes,
    ] = await Promise.all([
      safeGet<DashboardSummary>('api/dashboard/summary'),
      safeGet<DashboardMyWork>('api/dashboard/my-work'),
      safeGet<DashboardMyOpenTasks>('api/dashboard/my-tasks', { params: { page: 1, pageSize: 8 } }),
      safeGet<DashboardMyActivities>('api/dashboard/my-activities', { params: { page: 1, pageSize: 10 } }),
      safeGet<DashboardPipeline>('api/dashboard/pipeline'),
      safeGet<DashboardRevenue>('api/dashboard/revenue'),
      safeGet<DashboardService>('api/dashboard/cases'),
      safeGet<DashboardCustomers>('api/dashboard/customers'),
      safeGet<DashboardManagement>('api/dashboard/management'),
      safeGet<DashboardActivityFeed>('api/dashboard/activity-feed'),
      safeGet<DashboardLayoutPreference>('api/dashboard/layout-preferences'),
    ])

    if (!summaryRes) {
      throw new Error('Summary endpoint unavailable')
    }

    const summaryData = summaryRes
    const managementData = managementRes ?? emptyManagement
    const includeManagement = summaryData.welcome?.hasManagementAccess && managementData.isVisible

    return {
      summary: summaryData,
      myWork: myWorkRes ?? emptyWork,
      myTasks: myTasksRes ?? emptyTasks,
      myActivities: myActivitiesRes ?? emptyActivities,
      pipeline: pipelineRes ?? emptyPipeline,
      revenue: revenueRes ?? emptyRevenue,
      service: casesRes ?? emptyService,
      customers: customersRes ?? emptyCustomers,
      management: managementData,
      activityFeed: activityFeedRes ?? emptyFeed,
      widgetLayout: mergeWidgetLayout(layoutRes, Boolean(includeManagement)),
    }
  }, [])

  const loadDashboard = useCallback(async () => {
    setLoading(true)
    setError(null)

    try {
      const payload = await fetchDashboardData()
      setSummary(payload.summary)
      setMyWork(payload.myWork)
      setMyTasks(payload.myTasks)
      setMyActivities(payload.myActivities)
      setPipeline(payload.pipeline)
      setRevenue(payload.revenue)
      setService(payload.service)
      setCustomers(payload.customers)
      setManagement(payload.management)
      setActivityFeed(payload.activityFeed)
      setWidgetLayout(payload.widgetLayout)
    } catch {
      setError('Dashboard data could not be loaded. Please retry.')
    } finally {
      setLoading(false)
    }
  }, [fetchDashboardData])

  useEffect(() => {
    void Promise.resolve().then(async () => {
      await loadDashboard()
    })
  }, [loadDashboard])

  const persistLayout = async () => {
    setSavingLayout(true)
    try {
      await api.put('api/dashboard/layout-preferences', {
        layoutVersion: 'v1',
        widgets: widgetLayout,
      })
    } finally {
      setSavingLayout(false)
    }
  }

  const updateWidget = (widgetId: string, updater: (widget: DashboardWidgetPreference) => DashboardWidgetPreference) => {
    setWidgetLayout((current) => current.map((widget) => widget.widgetId === widgetId ? updater(widget) : widget))
  }

  const moveWidget = (widgetId: string, direction: -1 | 1) => {
    setWidgetLayout((current) => {
      const sorted = [...current].sort((a, b) => a.order - b.order)
      const index = sorted.findIndex((widget) => widget.widgetId === widgetId)
      const targetIndex = index + direction
      if (index < 0 || targetIndex < 0 || targetIndex >= sorted.length) {
        return current
      }

      const next = [...sorted]
      const [moved] = next.splice(index, 1)
      next.splice(targetIndex, 0, moved)
      return next.map((widget, order) => ({ ...widget, order }))
    })
  }

  const completeTask = async (activityId: string) => {
    await api.post(`api/activities/${activityId}/complete`, {})
    await loadDashboard()
  }

  const actions: CommandAction[] = [{ key: 'refresh', label: 'Refresh', onClick: () => void loadDashboard() }]

  if (hasPermission('Dashboard.View')) {
    actions.push({ key: 'my-work', label: 'My Work', onClick: () => { navigate('/dashboard/my-work') } })
  }

  if (hasPermission('Activities.View')) {
    actions.push({ key: 'my-activities', label: 'My Activities', onClick: () => { navigate('/dashboard/my-activities') } })
    actions.push({ key: 'my-open-tasks', label: 'My Open Tasks', onClick: () => { navigate('/dashboard/my-open-tasks') } })
  }

  actions.push({ key: 'customize', label: customizeWidgets ? 'Close Layout Editor' : 'Customize Widgets', onClick: () => setCustomizeWidgets((current) => !current) })
  actions.push({ key: 'save-layout', label: savingLayout ? 'Saving...' : 'Save Layout', onClick: () => { void persistLayout() } })

  const kpiCards = summary.kpis.length > 0
    ? summary.kpis
    : [
      {
        key: 'total-leads',
        icon: 'PeopleAddRegular',
        title: 'Total Leads',
        currentValue: summary.totalLeads,
        previousValue: Math.max(0, summary.totalLeads - summary.newLeads),
        trendPercent: summary.totalLeads === 0 ? 0 : Math.round((summary.newLeads / summary.totalLeads) * 100),
        comparisonLabel: 'vs previous period',
        actionPath: '/leads',
        positiveTrendIsGood: true,
      },
      {
        key: 'pipeline-value',
        icon: 'ArrowTrendingRegular',
        title: 'Pipeline Value',
        currentValue: summary.pipelineValue,
        previousValue: Math.max(0, summary.pipelineValue * 0.9),
        trendPercent: 10,
        comparisonLabel: 'vs previous period',
        actionPath: '/sales/pipeline',
        positiveTrendIsGood: true,
      },
      {
        key: 'revenue-this-month',
        icon: 'MoneyRegular',
        title: 'Revenue This Month',
        currentValue: revenue.revenueThisMonth,
        previousValue: Math.max(0, revenue.revenueThisMonth * 0.88),
        trendPercent: 12,
        comparisonLabel: 'vs previous period',
        actionPath: '/sales/revenue',
        positiveTrendIsGood: true,
      },
      {
        key: 'open-cases',
        icon: 'HeadsetRegular',
        title: 'Open Cases',
        currentValue: summary.openCases,
        previousValue: Math.max(0, summary.openCases + 2),
        trendPercent: -8,
        comparisonLabel: 'vs previous period',
        actionPath: '/service/cases',
        positiveTrendIsGood: false,
      },
    ]

  const renderSection = (widgetId: WidgetId) => {
    if (widgetId === 'kpis') {
      return (
        <section className={styles.kpiGrid}>
          {kpiCards.map((kpi) => {
            const trendPositive = kpi.trendPercent >= 0
            const trendGood = kpi.positiveTrendIsGood ? trendPositive : !trendPositive
            return (
              <Card className={styles.kpiCard} key={kpi.key}>
                <div className={styles.kpiHeader}>
                  <span className={styles.kpiIcon}>{kpi.icon}</span>
                  <Badge appearance={trendGood ? 'tint' : 'filled'} color={trendGood ? 'success' : 'danger'}>
                    {trendPositive ? '+' : ''}{Math.round(kpi.trendPercent)}%
                  </Badge>
                </div>
                <p className={styles.kpiTitle}>{kpi.title}</p>
                <p className={styles.kpiValue}>{kpi.title.toLowerCase().includes('revenue') || kpi.title.toLowerCase().includes('pipeline') ? formatMoney(kpi.currentValue) : Math.round(kpi.currentValue).toLocaleString()}</p>
                <p className={styles.kpiMeta}>{kpi.comparisonLabel}: {kpi.title.toLowerCase().includes('revenue') || kpi.title.toLowerCase().includes('pipeline') ? formatMoney(kpi.previousValue) : Math.round(kpi.previousValue).toLocaleString()}</p>
                <Button appearance="subtle" icon={<ArrowTrendingRegular />} onClick={() => navigate(kpi.actionPath)}>View {kpi.title}</Button>
              </Card>
            )
          })}
        </section>
      )
    }

    if (widgetId === 'sales') {
      return (
        <section className={styles.section}>
          <div className={styles.sectionHeader}>
            <h2>Sales Performance</h2>
            <Badge appearance="outline">2 x 2 analytics</Badge>
          </div>
          <div className={styles.analyticsGrid}>
            <Card className={styles.panelCard}>
              <h3>Pipeline Funnel</h3>
              <ResponsiveContainer width="100%" height={220}>
                <FunnelChart>
                  <Tooltip formatter={(value) => Number(value).toLocaleString()} />
                  <Funnel dataKey="count" data={pipeline.funnelStages} isAnimationActive>
                    <LabelList position="right" fill="#0f172a" stroke="none" dataKey="name" />
                  </Funnel>
                </FunnelChart>
              </ResponsiveContainer>
            </Card>

            <Card className={styles.panelCard}>
              <h3>Revenue Trend</h3>
              <ResponsiveContainer width="100%" height={220}>
                <LineChart data={revenue.monthlyTrend}>
                  <CartesianGrid strokeDasharray="2 2" />
                  <XAxis dataKey="month" tick={{ fontSize: 11 }} />
                  <YAxis tick={{ fontSize: 11 }} />
                  <Tooltip formatter={(value) => formatMoney(Number(value))} />
                  <Legend />
                  <Line type="monotone" dataKey="actualRevenue" name="Actual Revenue" stroke="#0f6cbd" strokeWidth={2.2} dot={false} />
                  <Line type="monotone" dataKey="forecastRevenue" name="Forecast Revenue" stroke="#c084fc" strokeWidth={2.2} dot={false} />
                </LineChart>
              </ResponsiveContainer>
            </Card>

            <Card className={styles.panelCard}>
              <h3>Opportunity Stage Distribution</h3>
              <ResponsiveContainer width="100%" height={220}>
                <PieChart>
                  <Pie data={pipeline.opportunityStageDistribution} dataKey="count" nameKey="name" innerRadius={44} outerRadius={82} fill="#0f766e" />
                  <Tooltip formatter={(value) => Number(value).toLocaleString()} />
                </PieChart>
              </ResponsiveContainer>
            </Card>

            <Card className={styles.panelCard}>
              <h3>Forecast Accuracy</h3>
              <ResponsiveContainer width="100%" height={220}>
                <RadialBarChart
                  innerRadius="70%"
                  outerRadius="100%"
                  data={[{ name: 'Accuracy', value: pipeline.forecastAccuracyPercent }]}
                  startAngle={180}
                  endAngle={0}
                >
                  <RadialBar dataKey="value" cornerRadius={8} fill="#2563eb" />
                  <Tooltip formatter={(value) => `${Math.round(Number(value))}%`} />
                </RadialBarChart>
              </ResponsiveContainer>
              <p className={styles.gaugeLabel}>{formatPercent(pipeline.forecastAccuracyPercent)}</p>
            </Card>
          </div>
        </section>
      )
    }

    if (widgetId === 'my-work') {
      const groupedActivities = myActivities.items.reduce<Record<string, typeof myActivities.items>>((groups, item) => {
        const key = new Date(item.activityDate).toLocaleDateString()
        if (!groups[key]) {
          groups[key] = []
        }
        groups[key].push(item)
        return groups
      }, {})

      return (
        <section className={styles.section}>
          <div className={styles.sectionHeader}>
            <h2>My Work</h2>
            <Badge appearance="outline">Action required</Badge>
          </div>
          <div className={styles.workGrid}>
            <Card className={styles.panelCard}>
              <h3>My Open Tasks</h3>
              <table className={styles.table}>
                <thead>
                  <tr>
                    <th>Subject</th>
                    <th>Due Date</th>
                    <th>Priority</th>
                    <th>Related</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {myTasks.items.map((task) => (
                    <tr key={task.id}>
                      <td>{task.subject}</td>
                      <td>{formatDate(task.dueDate)}</td>
                      <td>{task.priorityName ?? 'Normal'}</td>
                      <td>{task.relatedRecord}</td>
                      <td className={styles.rowActions}>
                        <Button size="small" appearance="subtle" onClick={() => navigate('/dashboard/my-open-tasks')}>Open</Button>
                        <Button size="small" appearance="subtle" onClick={() => { void completeTask(task.id) }}>Complete</Button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
              {myTasks.items.length === 0 ? <p className={styles.empty}>No open tasks.</p> : null}
            </Card>

            <Card className={styles.panelCard}>
              <h3>My Activities</h3>
              <div className={styles.activityGroups}>
                {Object.entries(groupedActivities).map(([date, items]) => (
                  <div key={date}>
                    <p className={styles.groupTitle}>{date}</p>
                    <ul className={styles.rowList}>
                      {items.map((item) => (
                        <li key={item.id} className={styles.rowItem}>
                          <span>{item.activityTypeName ?? 'Activity'}: {item.subject}</span>
                          <span>{item.relatedRecord}</span>
                        </li>
                      ))}
                    </ul>
                  </div>
                ))}
                {Object.keys(groupedActivities).length === 0 ? <p className={styles.empty}>No activities assigned.</p> : null}
              </div>
            </Card>

            <Card className={styles.panelCard}>
              <h3>Opportunities Closing Soon</h3>
              <table className={styles.table}>
                <thead>
                  <tr>
                    <th>Opportunity</th>
                    <th>Account</th>
                    <th>Revenue</th>
                    <th>Probability</th>
                    <th>Close Date</th>
                  </tr>
                </thead>
                <tbody>
                  {summary.opportunitiesClosingSoon.map((item) => (
                    <tr key={item.id}>
                      <td>{item.topic}</td>
                      <td>{item.accountName ?? 'N/A'}</td>
                      <td>{formatMoney(item.estimatedRevenue)}</td>
                      <td>{Math.round(item.probability)}%</td>
                      <td>{formatDate(item.estimatedCloseDate)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
              {summary.opportunitiesClosingSoon.length === 0 ? <p className={styles.empty}>No opportunities closing soon.</p> : null}
            </Card>

            <Card className={styles.panelCard}>
              <h3>Pending Approvals</h3>
              <ul className={styles.rowList}>
                {myWork.pendingApprovals.map((approval) => (
                  <li key={approval.id} className={styles.rowItem}>
                    <span>{approval.type}: {approval.referenceNumber}</span>
                    <span>{approval.approvalStatusName ?? 'Pending'} • {formatMoney(approval.totalAmount)}</span>
                  </li>
                ))}
              </ul>
              {myWork.pendingApprovals.length === 0 ? <p className={styles.empty}>No pending approvals.</p> : null}
            </Card>
          </div>
        </section>
      )
    }

    if (widgetId === 'activity-timeline') {
      return (
        <section className={styles.section}>
          <div className={styles.sectionHeader}>
            <h2>Recent Activity Timeline</h2>
            <Badge appearance="outline">Live feed</Badge>
          </div>
          <Card className={styles.panelCard}>
            <ul className={styles.timelineList}>
              {activityFeed.items.map((item) => (
                <li key={item.id} className={styles.timelineItem}>
                  <div>
                    <p className={styles.timelineTime}>{formatTimelineTime(item.timestamp)}</p>
                  </div>
                  <div className={styles.timelineContent}>
                    <p>{item.action}</p>
                    <p>{item.userName} • {item.entity}</p>
                  </div>
                  <Button appearance="subtle" onClick={() => navigate(item.route)}>Open</Button>
                </li>
              ))}
            </ul>
            {activityFeed.items.length === 0 ? <p className={styles.empty}>No timeline events yet.</p> : null}
          </Card>
        </section>
      )
    }

    if (widgetId === 'customers') {
      return (
        <section className={styles.section}>
          <div className={styles.sectionHeader}>
            <h2>Customer Insights</h2>
            <Badge appearance="outline">Revenue and risk</Badge>
          </div>
          <div className={styles.tripleGrid}>
            <Card className={styles.panelCard}>
              <h3>Top Customers</h3>
              <table className={styles.table}>
                <thead>
                  <tr>
                    <th>Account</th>
                    <th>Revenue</th>
                    <th>Open Opportunities</th>
                  </tr>
                </thead>
                <tbody>
                  {customers.topCustomers.map((customer) => (
                    <tr key={customer.accountId}>
                      <td>{customer.accountName}</td>
                      <td>{formatMoney(customer.revenue)}</td>
                      <td>{customer.openOpportunities}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </Card>

            <Card className={styles.panelCard}>
              <h3>At Risk Customers</h3>
              <ul className={styles.rowList}>
                {customers.atRiskCustomers.map((customer) => (
                  <li key={customer.accountId} className={styles.rowItem}>
                    <span>{customer.accountName}</span>
                    <span>{customer.reason ?? 'Requires review'}</span>
                  </li>
                ))}
              </ul>
              {customers.atRiskCustomers.length === 0 ? <p className={styles.empty}>No at-risk customers detected.</p> : null}
            </Card>

            <Card className={styles.panelCard}>
              <h3>New Customers</h3>
              <ul className={styles.rowList}>
                {customers.newCustomers.map((customer) => (
                  <li key={customer.accountId} className={styles.rowItem}>
                    <span>{customer.accountName}</span>
                    <span>{formatDate(customer.createdAt)} • {formatMoney(customer.revenue)}</span>
                  </li>
                ))}
              </ul>
            </Card>
          </div>
        </section>
      )
    }

    if (widgetId === 'service') {
      return (
        <section className={styles.section}>
          <div className={styles.sectionHeader}>
            <h2>Service Performance</h2>
            <Badge appearance="outline">SLA and case pressure</Badge>
          </div>
          <div className={styles.analyticsGrid}>
            <Card className={styles.panelCard}>
              <h3>Cases By Priority</h3>
              <ResponsiveContainer width="100%" height={220}>
                <BarChart data={service.casesByPriority}>
                  <CartesianGrid strokeDasharray="2 2" />
                  <XAxis dataKey="name" tick={{ fontSize: 11 }} />
                  <YAxis tick={{ fontSize: 11 }} />
                  <Tooltip />
                  <Bar dataKey="count" fill="#b45309" />
                </BarChart>
              </ResponsiveContainer>
            </Card>

            <Card className={styles.panelCard}>
              <h3>Cases By Status</h3>
              <ResponsiveContainer width="100%" height={220}>
                <PieChart>
                  <Pie data={service.casesByStatus} dataKey="count" nameKey="name" innerRadius={40} outerRadius={82} fill="#047857" />
                  <Tooltip />
                </PieChart>
              </ResponsiveContainer>
            </Card>

            <Card className={styles.panelCard}>
              <h3>SLA Compliance</h3>
              <ResponsiveContainer width="100%" height={220}>
                <RadialBarChart
                  innerRadius="70%"
                  outerRadius="100%"
                  data={[{ name: 'SLA', value: service.slaCompliancePercent }]}
                  startAngle={180}
                  endAngle={0}
                >
                  <RadialBar dataKey="value" cornerRadius={8} fill="#059669" />
                  <Tooltip formatter={(value) => `${Math.round(Number(value))}%`} />
                </RadialBarChart>
              </ResponsiveContainer>
              <p className={styles.gaugeLabel}>{formatPercent(service.slaCompliancePercent)}</p>
            </Card>

            <Card className={styles.panelCard}>
              <h3>Cases Requiring Attention</h3>
              <table className={styles.table}>
                <thead>
                  <tr>
                    <th>Case Number</th>
                    <th>Priority</th>
                    <th>Due Date</th>
                    <th>Assigned User</th>
                  </tr>
                </thead>
                <tbody>
                  {service.casesRequiringAttention.map((item) => (
                    <tr key={item.id}>
                      <td>{item.caseNumber}</td>
                      <td>{item.priorityName ?? 'N/A'}</td>
                      <td>{formatDate(item.dueAt)}</td>
                      <td>{item.assignedToName ?? 'Unassigned'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </Card>
          </div>
        </section>
      )
    }

    if (widgetId === 'management') {
      if (!summary.welcome.hasManagementAccess || !management.isVisible) {
        return null
      }

      return (
        <section className={styles.section}>
          <div className={styles.sectionHeader}>
            <h2>Management Insights</h2>
            <Badge appearance="filled" color="informative">Managers and Administrators</Badge>
          </div>
          <div className={styles.analyticsGrid}>
            <Card className={styles.panelCard}>
              <h3>Top Salespeople</h3>
              <table className={styles.table}>
                <thead>
                  <tr>
                    <th>User</th>
                    <th>Revenue</th>
                    <th>Won</th>
                    <th>Win Rate</th>
                  </tr>
                </thead>
                <tbody>
                  {management.topSalespeople.map((item) => (
                    <tr key={item.userId}>
                      <td>{item.userName}</td>
                      <td>{formatMoney(item.revenue)}</td>
                      <td>{item.opportunitiesWon}</td>
                      <td>{formatPercent(item.winRate)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </Card>

            <Card className={styles.panelCard}>
              <h3>Team Performance</h3>
              <table className={styles.table}>
                <thead>
                  <tr>
                    <th>Team</th>
                    <th>Target</th>
                    <th>Actual</th>
                    <th>Achievement %</th>
                  </tr>
                </thead>
                <tbody>
                  {management.teamPerformance.map((team) => (
                    <tr key={team.teamId}>
                      <td>{team.teamName}</td>
                      <td>{formatMoney(team.target)}</td>
                      <td>{formatMoney(team.actual)}</td>
                      <td>{formatPercent(team.achievementPercent)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </Card>

            <Card className={styles.panelCard}>
              <h3>Lead Conversion Trend</h3>
              <ResponsiveContainer width="100%" height={220}>
                <LineChart data={management.leadConversionTrend}>
                  <CartesianGrid strokeDasharray="2 2" />
                  <XAxis dataKey="name" tick={{ fontSize: 11 }} />
                  <YAxis tick={{ fontSize: 11 }} />
                  <Tooltip />
                  <Line type="monotone" dataKey="count" stroke="#2563eb" strokeWidth={2.2} dot={false} />
                </LineChart>
              </ResponsiveContainer>
            </Card>

            <Card className={styles.panelCard}>
              <h3>Revenue By Team</h3>
              <ResponsiveContainer width="100%" height={220}>
                <BarChart data={management.revenueByTeam}>
                  <CartesianGrid strokeDasharray="2 2" />
                  <XAxis dataKey="name" tick={{ fontSize: 11 }} />
                  <YAxis tick={{ fontSize: 11 }} />
                  <Tooltip formatter={(value) => formatMoney(Number(value))} />
                  <Bar dataKey="value" fill="#1d4ed8" />
                </BarChart>
              </ResponsiveContainer>
            </Card>
          </div>
        </section>
      )
    }

    if (widgetId === 'quick-actions') {
      const quickActions = [
        { key: 'new-lead', label: 'New Lead', to: '/leads/create' },
        { key: 'new-opportunity', label: 'New Opportunity', to: '/opportunities/create' },
        { key: 'new-quote', label: 'New Quote', to: '/sales/quotes/create' },
        { key: 'new-case', label: 'New Case', to: '/service/cases/create' },
        { key: 'new-contact', label: 'New Contact', to: '/contacts/create' },
        { key: 'upload-document', label: 'Upload Document', to: '/documents/create' },
      ]

      return (
        <section className={styles.section}>
          <div className={styles.sectionHeader}>
            <h2>Quick Actions</h2>
          </div>
          <div className={styles.quickActionGrid}>
            {quickActions.map((action) => (
              <button className={styles.quickActionTile} key={action.key} onClick={() => navigate(action.to)}>
                <span>+</span>
                <span>{action.label}</span>
              </button>
            ))}
          </div>
        </section>
      )
    }

    return null
  }

  return (
    <div className={styles.layout}>
      <PageHeader
        title="Dashboard"
        subtitle="Command center for attention, performance, and next-best actions"
      />

      <CommandBar actions={actions} />

      <section className={styles.welcomeHeader}>
        <div>
          <h2>Welcome back, {summary.welcome.userName}</h2>
          <p>{summary.welcome.dateLabel}</p>
          <ul className={styles.attentionList}>
            <li>{summary.welcome.openTasks} open tasks</li>
            <li>{summary.welcome.overdueActivities} overdue activities</li>
            <li>{summary.welcome.opportunitiesClosingThisWeek} opportunities closing this week</li>
            <li>{summary.welcome.slaBreaches} SLA breaches</li>
          </ul>
        </div>
        <div className={styles.rolePanel}>
          <p><strong>Current Role:</strong> {summary.welcome.currentRole}</p>
          <p><strong>Business Unit:</strong> {summary.welcome.businessUnit}</p>
          <p><strong>Team:</strong> {summary.welcome.team}</p>
        </div>
      </section>

      {customizeWidgets ? (
        <section className={styles.customizePanel}>
          <h3>Widget Personalization</h3>
          <p>Rearrange, hide, pin, and save your dashboard layout.</p>
          <ul className={styles.customizeList}>
            {[...widgetLayout].sort((a, b) => a.order - b.order).map((widget) => (
              <li key={widget.widgetId}>
                <span>{widget.widgetId}</span>
                <div className={styles.customizeActions}>
                  <Button size="small" icon={<ArrowCircleUpRegular />} onClick={() => moveWidget(widget.widgetId, -1)} />
                  <Button size="small" icon={<ArrowCircleDownRegular />} onClick={() => moveWidget(widget.widgetId, 1)} />
                  <Button
                    size="small"
                    icon={widget.isPinned ? <DismissCircleRegular /> : <PinRegular />}
                    onClick={() => updateWidget(widget.widgetId, (current) => ({ ...current, isPinned: !current.isPinned }))}
                  >
                    {widget.isPinned ? 'Unpin' : 'Pin'}
                  </Button>
                  <Switch
                    checked={widget.isVisible}
                    label={widget.isVisible ? 'Visible' : 'Hidden'}
                    onChange={(_, data) => updateWidget(widget.widgetId, (current) => ({ ...current, isVisible: data.checked }))}
                  />
                </div>
              </li>
            ))}
          </ul>
        </section>
      ) : null}

      {error ? <div className={styles.error}>{error}</div> : null}

      {loading ? <Spinner label="Loading dashboard" /> : null}

      <div className={styles.widgetStack}>
        {visibleWidgets.map((widget) => (
          <div key={widget.widgetId} className={styles.widgetContainer}>
            <div className={styles.widgetMeta}>
              {widget.isPinned ? <Badge appearance="filled" icon={<StarRegular />}>Pinned</Badge> : null}
              {!widget.isVisible ? <Badge appearance="outline" icon={<EyeOffRegular />}>Hidden</Badge> : null}
              {customizeWidgets ? <Badge appearance="outline" icon={<ClipboardTaskRegular />}>{widget.widgetId}</Badge> : null}
            </div>
            {renderSection(widget.widgetId as WidgetId)}
          </div>
        ))}
      </div>
    </div>
  )
}
