import { useEffect, useMemo, useState } from 'react'
import { Bar, BarChart, CartesianGrid, Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { Field, Input, MessageBar, MessageBarBody, Spinner, Textarea } from '@fluentui/react-components'
import { ChartMultipleRegular } from '@fluentui/react-icons'
import { useNavigate, useParams } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { DeleteConfirmDialog } from '../components/crud/DeleteConfirmDialog'
import {
  EntityDetailsGrid,
  EntityHeader,
  EntityPageLayout,
  FormSectionCard,
  LookupCombobox,
  StickySaveBar,
} from '../components/entity-ui/EntityComponents'
import { DateRangeFilterField } from '../components/filters/DateRangeFilterField'
import { LookupFilterField } from '../components/filters/LookupFilterField'
import { DenseDataGrid, type DenseColumn, type DenseSort } from '../components/grid/DenseDataGrid'
import { useListQueryState } from '../hooks/useListQueryState'
import { CommandBar } from '../layout/components/CommandBar'
import { PageHeader } from '../layout/components/PageHeader'
import type { ForecastDashboard, PagedResult, RevenueForecast } from '../types/models'
import {
  emptyForecastForm,
  forecastPayload,
  forecastToForm,
  formatCurrency,
  formatDate,
  formatPercent,
  type ForecastFormState,
} from './salesUtils'
import styles from './Sales.module.css'

type ForecastQuery = {
  page: number
  pageSize: number
  search: string
  sortBy: string
  sortDir: 'asc' | 'desc'
  forecastTypeId: string
  periodFrom: string
  periodTo: string
}

export function ForecastsPage() {
  const navigate = useNavigate()
  const { hasPermission } = useAuth()
  const canView = hasPermission('Forecasts.View')
  const canCreate = hasPermission('Forecasts.Create')
  const canEdit = hasPermission('Forecasts.Update')
  const canDelete = hasPermission('Forecasts.Delete')
  const [rows, setRows] = useState<RevenueForecast[]>([])
  const [dashboard, setDashboard] = useState<ForecastDashboard | null>(null)
  const [totalCount, setTotalCount] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [deleteTarget, setDeleteTarget] = useState<RevenueForecast | null>(null)
  const defaultQuery: ForecastQuery = {
    page: 1,
    pageSize: 20,
    search: '',
    sortBy: '',
    sortDir: 'asc',
    forecastTypeId: '',
    periodFrom: '',
    periodTo: '',
  }
  const { query, setQuery } = useListQueryState<ForecastQuery>({ defaults: defaultQuery, numberKeys: ['page', 'pageSize'] })
  const [draftFilters, setDraftFilters] = useState<Pick<ForecastQuery, 'forecastTypeId' | 'periodFrom' | 'periodTo'>>({
    forecastTypeId: query.forecastTypeId,
    periodFrom: query.periodFrom,
    periodTo: query.periodTo,
  })

  const load = async () => {
    if (!canView) {
      return
    }

    setLoading(true)
    setError('')
    try {
      const [{ data: list }, { data: metrics }] = await Promise.all([
        api.get<PagedResult<RevenueForecast>>('api/forecasts', {
          params: {
            page: query.page,
            pageSize: query.pageSize,
            search: query.search || undefined,
            sortBy: query.sortBy || undefined,
            sortDir: query.sortDir,
            forecastTypeId: query.forecastTypeId || undefined,
            periodFrom: query.periodFrom || undefined,
            periodTo: query.periodTo || undefined,
          },
        }),
        api.get<ForecastDashboard>('api/forecasts/dashboard'),
      ])
      setRows(list.items)
      setTotalCount(list.totalCount)
      setDashboard(metrics)
    } catch {
      setError('Failed to load forecasts.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void Promise.resolve().then(load)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [canView, query])

  const activeFilterCount = [query.forecastTypeId, query.periodFrom, query.periodTo].filter(Boolean).length

  const refresh = () => setQuery((current) => ({ ...current }))

  const remove = async () => {
    if (!deleteTarget) {
      return
    }

    setLoading(true)
    setError('')
    try {
      await api.delete(`api/forecasts/${deleteTarget.id}`)
      setDeleteTarget(null)
      refresh()
    } catch {
      setError('Failed to delete forecast.')
    } finally {
      setLoading(false)
    }
  }

  const columns = useMemo<DenseColumn<RevenueForecast>[]>(
    () => [
      { key: 'forecastDate', label: 'Forecast Date', sortable: true, render: (row) => formatDate(row.forecastDate) },
      { key: 'forecastTypeName', label: 'Type', sortable: true, render: (row) => row.forecastTypeName ?? 'Not set' },
      { key: 'forecastPeriodStart', label: 'Period Start', sortable: true, render: (row) => formatDate(row.forecastPeriodStart) },
      { key: 'forecastPeriodEnd', label: 'Period End', sortable: true, render: (row) => formatDate(row.forecastPeriodEnd) },
      { key: 'totalPipelineRevenue', label: 'Pipeline', sortable: true, render: (row) => formatCurrency(row.totalPipelineRevenue) },
      { key: 'weightedPipelineRevenue', label: 'Weighted', sortable: true, render: (row) => formatCurrency(row.weightedPipelineRevenue) },
      { key: 'forecastRevenue', label: 'Forecast', sortable: true, render: (row) => formatCurrency(row.forecastRevenue) },
      { key: 'closedRevenue', label: 'Closed', sortable: true, render: (row) => formatCurrency(row.closedRevenue) },
      { key: 'forecastAccuracy', label: 'Accuracy', sortable: true, render: (row) => formatPercent(row.forecastAccuracy) },
    ],
    [],
  )

  if (!canView) {
    return (
      <div>
        <PageHeader title="Forecasts" subtitle="Revenue forecast snapshots." />
        <MessageBar intent="error"><MessageBarBody>You do not have permission to view forecasts.</MessageBarBody></MessageBar>
      </div>
    )
  }

  return (
    <div>
      <PageHeader
        title="Forecasts"
        subtitle="Revenue forecast snapshots."
        quickAction={canCreate ? 'New Forecast' : undefined}
        onQuickAction={canCreate ? () => navigate('/sales/forecasts/create') : undefined}
      />
      <CommandBar
        actions={[
          ...(canCreate ? [{ key: 'create', label: 'New Forecast', onClick: () => navigate('/sales/forecasts/create') }] : []),
          { key: 'refresh', label: 'Refresh', onClick: refresh },
        ]}
      />

      <section className={styles.metricGrid}>
        <div className={styles.metricCard}><p className={styles.metricLabel}>Pipeline</p><p className={styles.metricValue}>{formatCurrency(dashboard?.totalPipeline)}</p></div>
        <div className={styles.metricCard}><p className={styles.metricLabel}>Weighted Pipeline</p><p className={styles.metricValue}>{formatCurrency(dashboard?.weightedPipeline)}</p></div>
        <div className={styles.metricCard}><p className={styles.metricLabel}>Closed Revenue</p><p className={styles.metricValue}>{formatCurrency(dashboard?.closedRevenue)}</p></div>
        <div className={styles.metricCard}><p className={styles.metricLabel}>Forecast Revenue</p><p className={styles.metricValue}>{formatCurrency(dashboard?.forecastRevenue)}</p></div>
        <div className={styles.metricCard}><p className={styles.metricLabel}>Forecast Accuracy</p><p className={styles.metricValue}>{formatPercent(dashboard?.forecastAccuracy)}</p></div>
      </section>

      {loading ? <Spinner size="small" label="Loading forecasts..." style={{ margin: '8px 0' }} /> : null}
      {error ? <MessageBar intent="error" style={{ marginBottom: 10 }}><MessageBarBody>{error}</MessageBarBody></MessageBar> : null}

      <DenseDataGrid
        rows={rows}
        columns={columns}
        loading={loading}
        totalCount={totalCount}
        page={query.page}
        pageSize={query.pageSize}
        search={query.search}
        sort={query.sortBy ? ({ key: query.sortBy as keyof RevenueForecast, dir: query.sortDir }) : null}
        onPageChange={(page) => setQuery((current) => ({ ...current, page }))}
        onPageSizeChange={(pageSize) => setQuery((current) => ({ ...current, pageSize, page: 1 }))}
        onSearchChange={(search) => setQuery((current) => ({ ...current, search, page: 1 }))}
        onSortChange={(sort: DenseSort<RevenueForecast> | null) =>
          setQuery((current) => ({ ...current, sortBy: sort ? String(sort.key) : '', sortDir: sort?.dir ?? 'asc', page: 1 }))
        }
        onView={(row) => navigate(`/sales/forecasts/${row.id}`)}
        onEdit={canEdit ? (row) => navigate(`/sales/forecasts/${row.id}/edit`) : undefined}
        onDelete={canDelete ? (row) => setDeleteTarget(row) : undefined}
        emptyMessage="No forecasts match the current filters."
        activeFilterCount={activeFilterCount}
        filterPanel={
          <>
            <LookupFilterField
              label="Forecast Type"
              fieldKey="forecastTypeId"
              value={draftFilters.forecastTypeId}
              onChange={(value) => setDraftFilters((current) => ({ ...current, forecastTypeId: value }))}
            />
            <DateRangeFilterField
              fromLabel="Period From"
              toLabel="Period To"
              fromValue={draftFilters.periodFrom}
              toValue={draftFilters.periodTo}
              onFromChange={(value) => setDraftFilters((current) => ({ ...current, periodFrom: value }))}
              onToChange={(value) => setDraftFilters((current) => ({ ...current, periodTo: value }))}
            />
          </>
        }
        onApplyFilters={() => setQuery((current) => ({ ...current, ...draftFilters, page: 1 }))}
        onCancelFilters={() =>
          setDraftFilters({
            forecastTypeId: query.forecastTypeId,
            periodFrom: query.periodFrom,
            periodTo: query.periodTo,
          })
        }
        onClearFilters={() =>
          setDraftFilters({
            forecastTypeId: '',
            periodFrom: '',
            periodTo: '',
          })
        }
      />

      <section className={styles.chartGrid}>
        <article className={styles.chartCard}>
          <p className={styles.chartTitle}>Forecast Trend</p>
          <ResponsiveContainer width="100%" height={220}>
            <LineChart data={dashboard?.forecastTrend ?? []}>
              <CartesianGrid strokeDasharray="2 2" />
              <XAxis dataKey="name" tick={{ fontSize: 11 }} />
              <YAxis tick={{ fontSize: 11 }} />
              <Tooltip formatter={(value) => formatCurrency(Number(value))} />
              <Line type="monotone" dataKey="value" stroke="#0f6cbd" strokeWidth={2} />
            </LineChart>
          </ResponsiveContainer>
        </article>
        <article className={styles.chartCard}>
          <p className={styles.chartTitle}>Closed Revenue by Month</p>
          <ResponsiveContainer width="100%" height={220}>
            <BarChart data={dashboard?.revenueByMonth ?? []}>
              <CartesianGrid strokeDasharray="2 2" />
              <XAxis dataKey="name" tick={{ fontSize: 11 }} />
              <YAxis tick={{ fontSize: 11 }} />
              <Tooltip formatter={(value) => formatCurrency(Number(value))} />
              <Bar dataKey="value" fill="#0d9488" />
            </BarChart>
          </ResponsiveContainer>
        </article>
        <article className={styles.chartCard}>
          <p className={styles.chartTitle}>Revenue by Owner</p>
          <ResponsiveContainer width="100%" height={220}>
            <BarChart data={dashboard?.revenueByOwner ?? []}>
              <CartesianGrid strokeDasharray="2 2" />
              <XAxis dataKey="name" tick={{ fontSize: 11 }} />
              <YAxis tick={{ fontSize: 11 }} />
              <Tooltip formatter={(value) => formatCurrency(Number(value))} />
              <Bar dataKey="value" fill="#c2410c" />
            </BarChart>
          </ResponsiveContainer>
        </article>
        <article className={styles.chartCard}>
          <p className={styles.chartTitle}>Revenue by Team</p>
          <ResponsiveContainer width="100%" height={220}>
            <BarChart data={dashboard?.revenueByTeam ?? []}>
              <CartesianGrid strokeDasharray="2 2" />
              <XAxis dataKey="name" tick={{ fontSize: 11 }} />
              <YAxis tick={{ fontSize: 11 }} />
              <Tooltip formatter={(value) => formatCurrency(Number(value))} />
              <Bar dataKey="value" fill="#7c3aed" />
            </BarChart>
          </ResponsiveContainer>
        </article>
      </section>

      <DeleteConfirmDialog
        open={Boolean(deleteTarget)}
        title="Delete Forecast"
        message={`Delete ${deleteTarget?.forecastTypeName ?? 'this forecast'}?`}
        onConfirm={() => void remove()}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  )
}

export function ForecastFormPage({ mode }: { mode: 'create' | 'edit' }) {
  const navigate = useNavigate()
  const { id } = useParams()
  const { hasPermission } = useAuth()
  const isEdit = mode === 'edit'
  const canSave = isEdit ? hasPermission('Forecasts.Update') : hasPermission('Forecasts.Create')
  const [form, setForm] = useState<ForecastFormState>(emptyForecastForm)
  const [forecast, setForecast] = useState<RevenueForecast | null>(null)
  const [loading, setLoading] = useState(isEdit)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({})

  useEffect(() => {
    if (!isEdit || !id) {
      return
    }

    void (async () => {
      setLoading(true)
      setError('')
      try {
        const { data } = await api.get<RevenueForecast>(`api/forecasts/${id}`)
        setForecast(data)
        setForm(forecastToForm(data))
      } catch {
        setError('Failed to load forecast.')
      } finally {
        setLoading(false)
      }
    })()
  }, [id, isEdit])

  const setValue = <K extends keyof ForecastFormState>(key: K, value: ForecastFormState[K]) => {
    setForm((current) => ({ ...current, [key]: value }))
  }

  const validate = () => {
    const next: Record<string, string> = {}
    if (!form.forecastTypeId) next.forecastTypeId = 'Forecast type is required.'
    if (!form.forecastPeriodStart) next.forecastPeriodStart = 'Period start is required.'
    if (!form.forecastPeriodEnd) next.forecastPeriodEnd = 'Period end is required.'
    if (form.forecastPeriodStart && form.forecastPeriodEnd && form.forecastPeriodEnd < form.forecastPeriodStart) next.forecastPeriodEnd = 'Period end must be on or after period start.'
    if (form.forecastRevenue.trim() && Number(form.forecastRevenue) < 0) next.forecastRevenue = 'Forecast revenue cannot be negative.'
    setFieldErrors(next)
    return Object.keys(next).length === 0
  }

  const save = async (closeAfterSave: boolean) => {
    if (!canSave || !validate()) {
      return
    }

    setSaving(true)
    setError('')
    try {
      if (isEdit && id) {
        await api.put(`api/forecasts/${id}`, forecastPayload(form))
        if (closeAfterSave) {
          navigate('/sales/forecasts')
        } else {
          const { data } = await api.get<RevenueForecast>(`api/forecasts/${id}`)
          setForecast(data)
          setForm(forecastToForm(data))
        }
      } else {
        const { data } = await api.post<RevenueForecast>('api/forecasts', forecastPayload(form))
        navigate(closeAfterSave ? '/sales/forecasts' : `/sales/forecasts/${data.id}/edit`)
      }
    } catch {
      setError('Save failed. Please review forecast values.')
    } finally {
      setSaving(false)
    }
  }

  const alerts = [
    ...(!canSave ? [{ intent: 'error' as const, text: `You do not have permission to ${isEdit ? 'update' : 'create'} forecasts.` }] : []),
    ...(error ? [{ intent: 'error' as const, text: error }] : []),
    ...Object.values(fieldErrors).map((message) => ({ intent: 'warning' as const, text: message })),
  ]

  return (
    <EntityPageLayout
      header={<EntityHeader icon={<ChartMultipleRegular />} title={isEdit ? 'Edit Forecast' : 'Create Forecast'} subtitle={forecast?.forecastTypeName} actions={[
        { key: 'save', label: 'Save', onClick: () => void save(false), appearance: 'primary', disabled: loading || saving || !canSave },
        { key: 'save-close', label: 'Save & Close', onClick: () => void save(true), appearance: 'secondary', disabled: loading || saving || !canSave },
        { key: 'cancel', label: 'Cancel', onClick: () => navigate('/sales/forecasts'), appearance: 'subtle' },
      ]} />}
      alerts={alerts}
      stickyBar={<StickySaveBar onSave={() => void save(false)} onSaveAndClose={() => void save(true)} onCancel={() => navigate('/sales/forecasts')} disableActions={loading || saving || !canSave} />}
    >
      {loading ? <Spinner size="small" label="Loading forecast..." /> : null}
      <FormSectionCard title="Forecast">
        <Field label="Forecast Date">
          <Input size="small" type="date" value={form.forecastDate} readOnly={!canSave} onChange={(_, data) => setValue('forecastDate', data.value)} />
        </Field>
        <Field label="Forecast Type" required validationMessage={fieldErrors.forecastTypeId}>
          <LookupCombobox fieldKey="forecastTypeId" value={form.forecastTypeId} disabled={!canSave} onChange={(value) => setValue('forecastTypeId', value)} />
        </Field>
        <Field label="Period Start" required validationMessage={fieldErrors.forecastPeriodStart}>
          <Input size="small" type="date" value={form.forecastPeriodStart} readOnly={!canSave} onChange={(_, data) => setValue('forecastPeriodStart', data.value)} />
        </Field>
        <Field label="Period End" required validationMessage={fieldErrors.forecastPeriodEnd}>
          <Input size="small" type="date" value={form.forecastPeriodEnd} readOnly={!canSave} onChange={(_, data) => setValue('forecastPeriodEnd', data.value)} />
        </Field>
        <Field label="Forecast Revenue" validationMessage={fieldErrors.forecastRevenue}>
          <Input size="small" type="number" value={form.forecastRevenue} readOnly={!canSave} onChange={(_, data) => setValue('forecastRevenue', data.value)} />
        </Field>
        <Field label="Notes">
          <Textarea value={form.notes} readOnly={!canSave} onChange={(_, data) => setValue('notes', data.value)} />
        </Field>
      </FormSectionCard>
    </EntityPageLayout>
  )
}

export function ForecastDetailsPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const { hasPermission } = useAuth()
  const canView = hasPermission('Forecasts.View')
  const canEdit = hasPermission('Forecasts.Update')
  const [forecast, setForecast] = useState<RevenueForecast | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!id || !canView) {
      return
    }

    void (async () => {
      setLoading(true)
      setError('')
      try {
        const { data } = await api.get<RevenueForecast>(`api/forecasts/${id}`)
        setForecast(data)
      } catch {
        setError('Failed to load forecast.')
      } finally {
        setLoading(false)
      }
    })()
  }, [canView, id])

  const rows = useMemo(
    () => forecast ? [
      { label: 'Forecast Date', value: formatDate(forecast.forecastDate) },
      { label: 'Type', value: forecast.forecastTypeName ?? '' },
      { label: 'Period Start', value: formatDate(forecast.forecastPeriodStart) },
      { label: 'Period End', value: formatDate(forecast.forecastPeriodEnd) },
      { label: 'Total Pipeline', value: formatCurrency(forecast.totalPipelineRevenue) },
      { label: 'Weighted Pipeline', value: formatCurrency(forecast.weightedPipelineRevenue) },
      { label: 'Forecast Revenue', value: formatCurrency(forecast.forecastRevenue) },
      { label: 'Closed Revenue', value: formatCurrency(forecast.closedRevenue) },
      { label: 'Open Revenue', value: formatCurrency(forecast.openRevenue) },
      { label: 'Forecast Accuracy', value: formatPercent(forecast.forecastAccuracy) },
      { label: 'Notes', value: forecast.notes ?? '' },
    ] : [],
    [forecast],
  )

  return (
    <EntityPageLayout
      header={<EntityHeader icon={<ChartMultipleRegular />} title="Forecast" subtitle={forecast?.forecastTypeName} actions={[
        ...(canEdit && id ? [{ key: 'edit', label: 'Edit', onClick: () => navigate(`/sales/forecasts/${id}/edit`), appearance: 'primary' as const }] : []),
        { key: 'back', label: 'Back to List', onClick: () => navigate('/sales/forecasts'), appearance: 'subtle' as const },
      ]} />}
      alerts={error ? [{ intent: 'error', text: error }] : undefined}
    >
      {!canView ? <MessageBar intent="error"><MessageBarBody>You do not have permission to view forecasts.</MessageBarBody></MessageBar> : null}
      {loading ? <Spinner size="small" label="Loading forecast..." /> : null}
      {!loading && forecast ? <EntityDetailsGrid rows={rows} /> : null}
    </EntityPageLayout>
  )
}
