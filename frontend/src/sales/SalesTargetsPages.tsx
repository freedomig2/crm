import { useEffect, useMemo, useState } from 'react'
import { Dropdown, Field, Input, MessageBar, MessageBarBody, Option, Spinner, Switch, Textarea } from '@fluentui/react-components'
import { TargetArrowRegular } from '@fluentui/react-icons'
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
import { FilterField } from '../components/filters/FilterField'
import { LookupFilterField } from '../components/filters/LookupFilterField'
import { DenseDataGrid, statusCell, type DenseColumn, type DenseSort } from '../components/grid/DenseDataGrid'
import { useListQueryState } from '../hooks/useListQueryState'
import { CommandBar } from '../layout/components/CommandBar'
import { PageHeader } from '../layout/components/PageHeader'
import type { PagedResult, SalesTarget } from '../types/models'
import {
  emptySalesTargetForm,
  formatCurrency,
  formatDate,
  formatPercent,
  salesTargetPayload,
  salesTargetToForm,
  type SalesTargetFormState,
} from './salesUtils'
import styles from './Sales.module.css'

type TargetQuery = {
  page: number
  pageSize: number
  search: string
  sortBy: string
  sortDir: 'asc' | 'desc'
  targetTypeId: string
  targetPeriodId: string
  assignedUserId: string
  assignedTeamId: string
  isActive: string
}

type AssignmentMode = 'user' | 'team'

export function SalesTargetsListPage() {
  const navigate = useNavigate()
  const { hasPermission } = useAuth()
  const canView = hasPermission('SalesTargets.View')
  const canCreate = hasPermission('SalesTargets.Create')
  const canEdit = hasPermission('SalesTargets.Update')
  const canDelete = hasPermission('SalesTargets.Delete')
  const [rows, setRows] = useState<SalesTarget[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [deleteTarget, setDeleteTarget] = useState<SalesTarget | null>(null)
  const defaultQuery: TargetQuery = {
    page: 1,
    pageSize: 20,
    search: '',
    sortBy: '',
    sortDir: 'asc',
    targetTypeId: '',
    targetPeriodId: '',
    assignedUserId: '',
    assignedTeamId: '',
    isActive: '',
  }
  const { query, setQuery } = useListQueryState<TargetQuery>({ defaults: defaultQuery, numberKeys: ['page', 'pageSize'] })
  const [draftFilters, setDraftFilters] = useState<Pick<TargetQuery, 'targetTypeId' | 'targetPeriodId' | 'assignedUserId' | 'assignedTeamId' | 'isActive'>>({
    targetTypeId: query.targetTypeId,
    targetPeriodId: query.targetPeriodId,
    assignedUserId: query.assignedUserId,
    assignedTeamId: query.assignedTeamId,
    isActive: query.isActive,
  })

  const load = async () => {
    if (!canView) {
      return
    }

    setLoading(true)
    setError('')
    try {
      const { data } = await api.get<PagedResult<SalesTarget>>('api/sales-targets', {
        params: {
          page: query.page,
          pageSize: query.pageSize,
          search: query.search || undefined,
          sortBy: query.sortBy || undefined,
          sortDir: query.sortDir,
          targetTypeId: query.targetTypeId || undefined,
          targetPeriodId: query.targetPeriodId || undefined,
          assignedUserId: query.assignedUserId || undefined,
          assignedTeamId: query.assignedTeamId || undefined,
          isActive: query.isActive || undefined,
        },
      })
      setRows(data.items)
      setTotalCount(data.totalCount)
    } catch {
      setError('Failed to load sales targets.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void Promise.resolve().then(load)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [canView, query])

  const activeFilterCount = [query.targetTypeId, query.targetPeriodId, query.assignedUserId, query.assignedTeamId, query.isActive].filter(Boolean).length

  const refresh = () => setQuery((current) => ({ ...current }))

  const remove = async () => {
    if (!deleteTarget) {
      return
    }

    setLoading(true)
    setError('')
    try {
      await api.delete(`api/sales-targets/${deleteTarget.id}`)
      setDeleteTarget(null)
      refresh()
    } catch {
      setError('Failed to delete sales target.')
    } finally {
      setLoading(false)
    }
  }

  const columns = useMemo<DenseColumn<SalesTarget>[]>(
    () => [
      { key: 'name', label: 'Name', sortable: true },
      { key: 'targetTypeName', label: 'Type', sortable: true, render: (row) => row.targetTypeName ?? 'Not set' },
      { key: 'targetPeriodName', label: 'Period', sortable: true, render: (row) => row.targetPeriodName ?? 'Not set' },
      { key: 'startDate', label: 'Start', sortable: true, render: (row) => formatDate(row.startDate) },
      { key: 'endDate', label: 'End', sortable: true, render: (row) => formatDate(row.endDate) },
      { key: 'targetAmount', label: 'Target', sortable: true, render: (row) => formatCurrency(row.targetAmount) },
      { key: 'actualAmount', label: 'Actual', sortable: true, render: (row) => formatCurrency(row.actualAmount) },
      { key: 'achievementPercentage', label: 'Achievement', sortable: true, render: (row) => <Achievement value={row.achievementPercentage} /> },
      { key: 'assignedUserName', label: 'Assigned To', sortable: true, render: (row) => row.assignedUserName ?? row.assignedTeamName ?? 'Not set' },
      { key: 'isActive', label: 'Active', sortable: true, render: (row) => statusCell(row.isActive ? 'Active' : 'Inactive') },
    ],
    [],
  )

  if (!canView) {
    return (
      <div>
        <PageHeader title="Sales Targets" subtitle="Manage revenue and activity targets." />
        <MessageBar intent="error"><MessageBarBody>You do not have permission to view sales targets.</MessageBarBody></MessageBar>
      </div>
    )
  }

  return (
    <div>
      <PageHeader
        title="Sales Targets"
        subtitle="Manage revenue and activity targets."
        quickAction={canCreate ? 'New Target' : undefined}
        onQuickAction={canCreate ? () => navigate('/sales/targets/create') : undefined}
      />
      <CommandBar
        actions={[
          ...(canCreate ? [{ key: 'create', label: 'New Target', onClick: () => navigate('/sales/targets/create') }] : []),
          { key: 'refresh', label: 'Refresh', onClick: refresh },
        ]}
      />

      {loading ? <Spinner size="small" label="Loading sales targets..." style={{ margin: '8px 0' }} /> : null}
      {error ? <MessageBar intent="error" style={{ marginBottom: 10 }}><MessageBarBody>{error}</MessageBarBody></MessageBar> : null}

      <DenseDataGrid
        rows={rows}
        columns={columns}
        loading={loading}
        totalCount={totalCount}
        page={query.page}
        pageSize={query.pageSize}
        search={query.search}
        sort={query.sortBy ? ({ key: query.sortBy as keyof SalesTarget, dir: query.sortDir }) : null}
        onPageChange={(page) => setQuery((current) => ({ ...current, page }))}
        onPageSizeChange={(pageSize) => setQuery((current) => ({ ...current, pageSize, page: 1 }))}
        onSearchChange={(search) => setQuery((current) => ({ ...current, search, page: 1 }))}
        onSortChange={(sort: DenseSort<SalesTarget> | null) =>
          setQuery((current) => ({ ...current, sortBy: sort ? String(sort.key) : '', sortDir: sort?.dir ?? 'asc', page: 1 }))
        }
        onView={(row) => navigate(`/sales/targets/${row.id}`)}
        onEdit={canEdit ? (row) => navigate(`/sales/targets/${row.id}/edit`) : undefined}
        onDelete={canDelete ? (row) => setDeleteTarget(row) : undefined}
        emptyMessage="No sales targets match the current filters."
        activeFilterCount={activeFilterCount}
        filterPanel={
          <>
            <LookupFilterField label="Type" fieldKey="targetTypeId" value={draftFilters.targetTypeId} onChange={(value) => setDraftFilters((current) => ({ ...current, targetTypeId: value }))} />
            <LookupFilterField label="Period" fieldKey="targetPeriodId" value={draftFilters.targetPeriodId} onChange={(value) => setDraftFilters((current) => ({ ...current, targetPeriodId: value }))} />
            <LookupFilterField
              label="Assigned User"
              fieldKey="assignedToUserId"
              value={draftFilters.assignedUserId}
              onChange={(value) =>
                setDraftFilters((current) => ({
                  ...current,
                  assignedUserId: value,
                  assignedTeamId: '',
                }))
              }
            />
            <LookupFilterField
              label="Assigned Team"
              fieldKey="ownerTeamId"
              value={draftFilters.assignedTeamId}
              onChange={(value) =>
                setDraftFilters((current) => ({
                  ...current,
                  assignedTeamId: value,
                  assignedUserId: '',
                }))
              }
            />
            <FilterField label="Active">
              <Dropdown
                size="small"
                selectedOptions={draftFilters.isActive ? [draftFilters.isActive] : []}
                value={draftFilters.isActive === 'true' ? 'Active' : draftFilters.isActive === 'false' ? 'Inactive' : ''}
                onOptionSelect={(_, data) => setDraftFilters((current) => ({ ...current, isActive: data.optionValue ?? '' }))}
              >
                <Option value="">All</Option>
                <Option value="true">Active</Option>
                <Option value="false">Inactive</Option>
              </Dropdown>
            </FilterField>
          </>
        }
        onApplyFilters={() => setQuery((current) => ({ ...current, ...draftFilters, page: 1 }))}
        onCancelFilters={() =>
          setDraftFilters({
            targetTypeId: query.targetTypeId,
            targetPeriodId: query.targetPeriodId,
            assignedUserId: query.assignedUserId,
            assignedTeamId: query.assignedTeamId,
            isActive: query.isActive,
          })
        }
        onClearFilters={() =>
          setDraftFilters({
            targetTypeId: '',
            targetPeriodId: '',
            assignedUserId: '',
            assignedTeamId: '',
            isActive: '',
          })
        }
      />

      <DeleteConfirmDialog
        open={Boolean(deleteTarget)}
        title="Delete Sales Target"
        message={`Delete ${deleteTarget?.name ?? 'this target'}?`}
        onConfirm={() => void remove()}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  )
}

export function SalesTargetFormPage({ mode }: { mode: 'create' | 'edit' }) {
  const navigate = useNavigate()
  const { id } = useParams()
  const { hasPermission } = useAuth()
  const isEdit = mode === 'edit'
  const canSave = isEdit ? hasPermission('SalesTargets.Update') : hasPermission('SalesTargets.Create')
  const [form, setForm] = useState<SalesTargetFormState>(emptySalesTargetForm)
  const [assignmentMode, setAssignmentMode] = useState<AssignmentMode>('user')
  const [target, setTarget] = useState<SalesTarget | null>(null)
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
        const { data } = await api.get<SalesTarget>(`api/sales-targets/${id}`)
        setTarget(data)
        setForm(salesTargetToForm(data))
        setAssignmentMode(data.assignedTeamId ? 'team' : 'user')
      } catch {
        setError('Failed to load sales target.')
      } finally {
        setLoading(false)
      }
    })()
  }, [id, isEdit])

  const setValue = <K extends keyof SalesTargetFormState>(key: K, value: SalesTargetFormState[K]) => {
    setForm((current) => ({ ...current, [key]: value }))
  }

  const validate = () => {
    const next: Record<string, string> = {}
    if (!form.name.trim()) next.name = 'Name is required.'
    if (!form.targetTypeId) next.targetTypeId = 'Target type is required.'
    if (!form.targetPeriodId) next.targetPeriodId = 'Target period is required.'
    if (!form.startDate) next.startDate = 'Start date is required.'
    if (!form.endDate) next.endDate = 'End date is required.'
    if (form.startDate && form.endDate && form.endDate < form.startDate) next.endDate = 'End date must be on or after start date.'
    if (Number(form.targetAmount || 0) < 0) next.targetAmount = 'Target amount cannot be negative.'
    if (Number(form.actualAmount || 0) < 0) next.actualAmount = 'Actual amount cannot be negative.'
    if (!form.assignedUserId && !form.assignedTeamId) next.assignedTo = 'Assign the target to a user or team.'
    if (form.assignedUserId && form.assignedTeamId) next.assignedTo = 'Choose one assignment owner.'
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
        await api.put(`api/sales-targets/${id}`, salesTargetPayload(form))
        if (closeAfterSave) {
          navigate('/sales/targets')
        } else {
          const { data } = await api.get<SalesTarget>(`api/sales-targets/${id}`)
          setTarget(data)
          setForm(salesTargetToForm(data))
        }
      } else {
        const { data } = await api.post<SalesTarget>('api/sales-targets', salesTargetPayload(form))
        navigate(closeAfterSave ? '/sales/targets' : `/sales/targets/${data.id}/edit`)
      }
    } catch {
      setError('Save failed. Please review target values.')
    } finally {
      setSaving(false)
    }
  }

  const alerts = [
    ...(!canSave ? [{ intent: 'error' as const, text: `You do not have permission to ${isEdit ? 'update' : 'create'} sales targets.` }] : []),
    ...(error ? [{ intent: 'error' as const, text: error }] : []),
    ...Object.values(fieldErrors).map((message) => ({ intent: 'warning' as const, text: message })),
  ]

  const updateAssignmentMode = (modeValue: AssignmentMode) => {
    setAssignmentMode(modeValue)
    setForm((current) => ({ ...current, assignedUserId: '', assignedTeamId: '' }))
  }

  return (
    <EntityPageLayout
      header={<EntityHeader icon={<TargetArrowRegular />} title={isEdit ? 'Edit Sales Target' : 'Create Sales Target'} subtitle={target?.targetTypeName} status={target?.isActive ? 'Active' : target ? 'Inactive' : undefined} actions={[
        { key: 'save', label: 'Save', onClick: () => void save(false), appearance: 'primary', disabled: loading || saving || !canSave },
        { key: 'save-close', label: 'Save & Close', onClick: () => void save(true), appearance: 'secondary', disabled: loading || saving || !canSave },
        { key: 'cancel', label: 'Cancel', onClick: () => navigate('/sales/targets'), appearance: 'subtle' },
      ]} />}
      alerts={alerts}
      stickyBar={<StickySaveBar onSave={() => void save(false)} onSaveAndClose={() => void save(true)} onCancel={() => navigate('/sales/targets')} disableActions={loading || saving || !canSave} />}
    >
      {loading ? <Spinner size="small" label="Loading sales target..." /> : null}
      <FormSectionCard title="Target">
        <Field label="Name" required validationMessage={fieldErrors.name}>
          <Input size="small" value={form.name} readOnly={!canSave} onChange={(_, data) => setValue('name', data.value)} />
        </Field>
        <Field label="Target Type" required validationMessage={fieldErrors.targetTypeId}>
          <LookupCombobox fieldKey="targetTypeId" value={form.targetTypeId} disabled={!canSave} onChange={(value) => setValue('targetTypeId', value)} />
        </Field>
        <Field label="Target Period" required validationMessage={fieldErrors.targetPeriodId}>
          <LookupCombobox fieldKey="targetPeriodId" value={form.targetPeriodId} disabled={!canSave} onChange={(value) => setValue('targetPeriodId', value)} />
        </Field>
        <Field label="Start Date" required validationMessage={fieldErrors.startDate}>
          <Input size="small" type="date" value={form.startDate} readOnly={!canSave} onChange={(_, data) => setValue('startDate', data.value)} />
        </Field>
        <Field label="End Date" required validationMessage={fieldErrors.endDate}>
          <Input size="small" type="date" value={form.endDate} readOnly={!canSave} onChange={(_, data) => setValue('endDate', data.value)} />
        </Field>
        <Field label="Target Amount" validationMessage={fieldErrors.targetAmount}>
          <Input size="small" type="number" value={form.targetAmount} readOnly={!canSave} onChange={(_, data) => setValue('targetAmount', data.value)} />
        </Field>
        <Field label="Actual Amount" validationMessage={fieldErrors.actualAmount}>
          <Input size="small" type="number" value={form.actualAmount} readOnly={!canSave} onChange={(_, data) => setValue('actualAmount', data.value)} />
        </Field>
        <Field label="Assignment" validationMessage={fieldErrors.assignedTo}>
          <Dropdown
            size="small"
            selectedOptions={[assignmentMode]}
            value={assignmentMode === 'team' ? 'Team' : 'User'}
            disabled={!canSave}
            onOptionSelect={(_, data) => updateAssignmentMode((data.optionValue as AssignmentMode) || 'user')}
          >
            <Option value="user">User</Option>
            <Option value="team">Team</Option>
          </Dropdown>
        </Field>
        {assignmentMode === 'user' ? (
          <Field label="Assigned User" required validationMessage={fieldErrors.assignedTo}>
            <LookupCombobox fieldKey="assignedToUserId" value={form.assignedUserId} disabled={!canSave} onChange={(value) => setForm((current) => ({ ...current, assignedUserId: value, assignedTeamId: '' }))} />
          </Field>
        ) : (
          <Field label="Assigned Team" required validationMessage={fieldErrors.assignedTo}>
            <LookupCombobox fieldKey="ownerTeamId" value={form.assignedTeamId} disabled={!canSave} onChange={(value) => setForm((current) => ({ ...current, assignedTeamId: value, assignedUserId: '' }))} />
          </Field>
        )}
        <Field label="Owner User">
          <LookupCombobox fieldKey="ownerUserId" value={form.ownerUserId} disabled={!canSave} onChange={(value) => setForm((current) => ({ ...current, ownerUserId: value, ownerTeamId: '' }))} />
        </Field>
        <Field label="Owner Team">
          <LookupCombobox fieldKey="ownerTeamId" value={form.ownerTeamId} disabled={!canSave} onChange={(value) => setForm((current) => ({ ...current, ownerTeamId: value, ownerUserId: '' }))} />
        </Field>
        <Field label="Active">
          <Switch checked={form.isActive} disabled={!canSave} onChange={(_, data) => setValue('isActive', Boolean(data.checked))} />
        </Field>
        <Field label="Description">
          <Textarea value={form.description} readOnly={!canSave} onChange={(_, data) => setValue('description', data.value)} />
        </Field>
      </FormSectionCard>
    </EntityPageLayout>
  )
}

export function SalesTargetDetailsPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const { hasPermission } = useAuth()
  const canView = hasPermission('SalesTargets.View')
  const canEdit = hasPermission('SalesTargets.Update')
  const [target, setTarget] = useState<SalesTarget | null>(null)
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
        const { data } = await api.get<SalesTarget>(`api/sales-targets/${id}`)
        setTarget(data)
      } catch {
        setError('Failed to load sales target.')
      } finally {
        setLoading(false)
      }
    })()
  }, [canView, id])

  const rows = useMemo(
    () => target ? [
      { label: 'Name', value: target.name },
      { label: 'Type', value: target.targetTypeName ?? '' },
      { label: 'Period', value: target.targetPeriodName ?? '' },
      { label: 'Start Date', value: formatDate(target.startDate) },
      { label: 'End Date', value: formatDate(target.endDate) },
      { label: 'Target Amount', value: formatCurrency(target.targetAmount) },
      { label: 'Actual Amount', value: formatCurrency(target.actualAmount) },
      { label: 'Achievement', value: formatPercent(target.achievementPercentage) },
      { label: 'Assigned To', value: target.assignedUserName ?? target.assignedTeamName ?? '' },
      { label: 'Owner', value: target.ownerUserName ?? target.ownerTeamName ?? '' },
      { label: 'Active', value: target.isActive ? 'Yes' : 'No' },
      { label: 'Description', value: target.description ?? '' },
    ] : [],
    [target],
  )

  return (
    <EntityPageLayout
      header={<EntityHeader icon={<TargetArrowRegular />} title={target?.name ?? 'Sales Target'} subtitle={target?.targetTypeName} status={target?.isActive ? 'Active' : target ? 'Inactive' : undefined} actions={[
        ...(canEdit && id ? [{ key: 'edit', label: 'Edit', onClick: () => navigate(`/sales/targets/${id}/edit`), appearance: 'primary' as const }] : []),
        { key: 'back', label: 'Back to List', onClick: () => navigate('/sales/targets'), appearance: 'subtle' as const },
      ]} />}
      alerts={error ? [{ intent: 'error', text: error }] : undefined}
    >
      {!canView ? <MessageBar intent="error"><MessageBarBody>You do not have permission to view sales targets.</MessageBarBody></MessageBar> : null}
      {loading ? <Spinner size="small" label="Loading sales target..." /> : null}
      {!loading && target ? (
        <>
          <section className={styles.metricGrid}>
            <div className={styles.metricCard}><p className={styles.metricLabel}>Target</p><p className={styles.metricValue}>{formatCurrency(target.targetAmount)}</p></div>
            <div className={styles.metricCard}><p className={styles.metricLabel}>Actual</p><p className={styles.metricValue}>{formatCurrency(target.actualAmount)}</p></div>
            <div className={styles.metricCard}><p className={styles.metricLabel}>Achievement</p><p className={styles.metricValue}>{formatPercent(target.achievementPercentage)}</p></div>
          </section>
          <EntityDetailsGrid rows={rows} />
        </>
      ) : null}
    </EntityPageLayout>
  )
}

function Achievement({ value }: { value: number }) {
  const width = Math.max(0, Math.min(100, value))
  return (
    <div>
      <div className={styles.progressTrack}>
        <div className={styles.progressFill} style={{ width: `${width}%` }} />
      </div>
      <span>{formatPercent(value)}</span>
    </div>
  )
}
