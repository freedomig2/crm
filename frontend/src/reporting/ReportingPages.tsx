import { useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { MessageBar, MessageBarBody, Spinner } from '@fluentui/react-components'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { DenseDataGrid, type DenseColumn } from '../components/grid/DenseDataGrid'
import { CommandBar } from '../layout/components/CommandBar'
import { PageHeader } from '../layout/components/PageHeader'
import { formatCurrency, formatPercent } from '../sales/salesUtils'

type ReportingDashboardSummary = {
  totalLeads: number
  openOpportunities: number
  totalAccounts: number
  openCases: number
  openActivities: number
  pipelineValue: number
  weightedPipelineValue: number
  revenueThisMonth: number
  winRate: number
}

type ReportLibraryItem = {
  id: string
  key: string
  name: string
  category: string
  description: string
  route: string
  isImplemented: boolean
  lastUpdatedAt: string
}

type KpiMonitoringItem = {
  id: string
  key: string
  name: string
  currentValue: number
  targetValue: number
  unit: string
  achievementPercent: number
  trend: string
}

export function ReportingDashboardsPage() {
  const { hasPermission } = useAuth()
  const canView = hasPermission('Reports.View')
  const [summary, setSummary] = useState<ReportingDashboardSummary | null>(null)
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
        const { data } = await api.get<ReportingDashboardSummary>('api/reports/dashboards')
        setSummary(data)
      } catch {
        setError('Failed to load reporting dashboards.')
      } finally {
        setLoading(false)
      }
    })()
  }, [canView])

  if (!canView) {
    return (
      <div>
        <PageHeader title="Reporting Dashboards" subtitle="Executive metrics across CRM modules." />
        <MessageBar intent="error"><MessageBarBody>You do not have permission to view reports.</MessageBarBody></MessageBar>
      </div>
    )
  }

  return (
    <div>
      <PageHeader title="Reporting Dashboards" subtitle="Executive metrics across CRM modules." />
      {loading ? <Spinner size="small" label="Loading dashboards..." style={{ marginBottom: 10 }} /> : null}
      {error ? <MessageBar intent="error" style={{ marginBottom: 10 }}><MessageBarBody>{error}</MessageBarBody></MessageBar> : null}

      <section style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(220px, 1fr))', gap: 12 }}>
        <MetricCard label="Total Leads" value={String(summary?.totalLeads ?? 0)} />
        <MetricCard label="Open Opportunities" value={String(summary?.openOpportunities ?? 0)} />
        <MetricCard label="Total Accounts" value={String(summary?.totalAccounts ?? 0)} />
        <MetricCard label="Open Cases" value={String(summary?.openCases ?? 0)} />
        <MetricCard label="Open Activities" value={String(summary?.openActivities ?? 0)} />
        <MetricCard label="Pipeline Value" value={formatCurrency(summary?.pipelineValue)} />
        <MetricCard label="Weighted Pipeline" value={formatCurrency(summary?.weightedPipelineValue)} />
        <MetricCard label="Revenue This Month" value={formatCurrency(summary?.revenueThisMonth)} />
        <MetricCard label="Win Rate" value={formatPercent(summary?.winRate)} />
      </section>
    </div>
  )
}

export function ReportsLibraryPage() {
  const navigate = useNavigate()
  const { hasPermission } = useAuth()
  const canView = hasPermission('Reports.View')
  const [rows, setRows] = useState<ReportLibraryItem[]>([])
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
        const { data } = await api.get<Array<Omit<ReportLibraryItem, 'id'>>>('api/reports/library')
        setRows(data.map((item) => ({ ...item, id: item.key })))
      } catch {
        setError('Failed to load report library.')
      } finally {
        setLoading(false)
      }
    })()
  }, [canView])

  const columns = useMemo<DenseColumn<ReportLibraryItem>[]>(() => [
    { key: 'name', label: 'Name', sortable: true },
    { key: 'category', label: 'Category', sortable: true },
    { key: 'description', label: 'Description', sortable: true },
    { key: 'isImplemented', label: 'Implemented', sortable: true, render: (row) => (row.isImplemented ? 'Yes' : 'Planned') },
    { key: 'lastUpdatedAt', label: 'Updated', sortable: true },
  ], [])

  if (!canView) {
    return (
      <div>
        <PageHeader title="Reports Library" subtitle="Available operational and analytical reports." />
        <MessageBar intent="error"><MessageBarBody>You do not have permission to view reports.</MessageBarBody></MessageBar>
      </div>
    )
  }

  return (
    <div>
      <PageHeader title="Reports Library" subtitle="Available operational and analytical reports." />
      <CommandBar actions={[{ key: 'refresh', label: 'Refresh', onClick: () => navigate(0 as unknown as string) }]} />
      {loading ? <Spinner size="small" label="Loading reports..." style={{ marginBottom: 10 }} /> : null}
      {error ? <MessageBar intent="error" style={{ marginBottom: 10 }}><MessageBarBody>{error}</MessageBarBody></MessageBar> : null}

      <DenseDataGrid
        rows={rows}
        columns={columns}
        loading={loading}
        totalCount={rows.length}
        page={1}
        pageSize={20}
        search=""
        sort={null}
        onPageChange={() => {}}
        onPageSizeChange={() => {}}
        onSearchChange={() => {}}
        onSortChange={() => {}}
        onView={(row) => {
          if (row.isImplemented) {
            navigate(row.route)
          }
        }}
        emptyMessage="No reports available."
      />
    </div>
  )
}

export function KpiMonitoringPage() {
  const { hasPermission } = useAuth()
  const canView = hasPermission('Reports.View')
  const [rows, setRows] = useState<KpiMonitoringItem[]>([])
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
        const { data } = await api.get<Array<Omit<KpiMonitoringItem, 'id'>>>('api/reports/kpis')
        setRows(data.map((item) => ({ ...item, id: item.key })))
      } catch {
        setError('Failed to load KPI monitoring metrics.')
      } finally {
        setLoading(false)
      }
    })()
  }, [canView])

  const columns = useMemo<DenseColumn<KpiMonitoringItem>[]>(() => [
    { key: 'name', label: 'KPI', sortable: true },
    {
      key: 'currentValue',
      label: 'Current',
      sortable: true,
      render: (row) => (row.unit === '$' ? formatCurrency(row.currentValue) : row.unit === '%' ? formatPercent(row.currentValue) : String(row.currentValue)),
    },
    {
      key: 'targetValue',
      label: 'Target',
      sortable: true,
      render: (row) => (row.unit === '$' ? formatCurrency(row.targetValue) : row.unit === '%' ? formatPercent(row.targetValue) : String(row.targetValue)),
    },
    { key: 'achievementPercent', label: 'Achievement %', sortable: true, render: (row) => formatPercent(row.achievementPercent) },
    { key: 'trend', label: 'Trend', sortable: true },
  ], [])

  if (!canView) {
    return (
      <div>
        <PageHeader title="KPI Monitoring" subtitle="Target and trend monitoring for critical KPIs." />
        <MessageBar intent="error"><MessageBarBody>You do not have permission to view reports.</MessageBarBody></MessageBar>
      </div>
    )
  }

  return (
    <div>
      <PageHeader title="KPI Monitoring" subtitle="Target and trend monitoring for critical KPIs." />
      {loading ? <Spinner size="small" label="Loading KPIs..." style={{ marginBottom: 10 }} /> : null}
      {error ? <MessageBar intent="error" style={{ marginBottom: 10 }}><MessageBarBody>{error}</MessageBarBody></MessageBar> : null}

      <DenseDataGrid
        rows={rows}
        columns={columns}
        loading={loading}
        totalCount={rows.length}
        page={1}
        pageSize={20}
        search=""
        sort={null}
        onPageChange={() => {}}
        onPageSizeChange={() => {}}
        onSearchChange={() => {}}
        onSortChange={() => {}}
        emptyMessage="No KPI records available."
      />
    </div>
  )
}

function MetricCard({ label, value }: { label: string; value: string }) {
  return (
    <article style={{ border: '1px solid var(--colorNeutralStroke2)', borderRadius: 8, padding: 12, background: 'var(--colorNeutralBackground1)' }}>
      <p style={{ margin: 0, fontSize: 12, color: 'var(--colorNeutralForeground3)' }}>{label}</p>
      <p style={{ margin: '8px 0 0', fontSize: 22, fontWeight: 600 }}>{value}</p>
    </article>
  )
}
