import { useEffect, useMemo, useState } from 'react'
import { Dropdown, Field, Input, MessageBar, MessageBarBody, Option, Spinner, Switch, Textarea } from '@fluentui/react-components'
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
import { FilterField } from '../components/filters/FilterField'
import { LookupFilterField } from '../components/filters/LookupFilterField'
import { DenseDataGrid, statusCell, type DenseColumn, type DenseSort } from '../components/grid/DenseDataGrid'
import { useListQueryState } from '../hooks/useListQueryState'
import { CommandBar } from '../layout/components/CommandBar'
import { PageHeader } from '../layout/components/PageHeader'
import type { LeadScoreRule, PagedResult } from '../types/models'
import {
  emptyLeadScoreRuleForm,
  leadScoreRulePayload,
  leadScoreRuleToForm,
  type LeadScoreRuleFormState,
} from './leadUtils'

type RuleQuery = {
  page: number
  pageSize: number
  search: string
  sortBy: string
  sortDir: 'asc' | 'desc'
  ruleTypeId: string
  isActive: string
}

export function LeadScoreRulesListPage() {
  const navigate = useNavigate()
  const { hasPermission } = useAuth()
  const canView = hasPermission('LeadScoreRules.View')
  const canCreate = hasPermission('LeadScoreRules.Create')
  const canEdit = hasPermission('LeadScoreRules.Update')
  const canDelete = hasPermission('LeadScoreRules.Delete')
  const canRun = hasPermission('LeadScoreRules.Run')
  const [rows, setRows] = useState<LeadScoreRule[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [deleteTarget, setDeleteTarget] = useState<LeadScoreRule | null>(null)
  const defaultQuery: RuleQuery = {
    page: 1,
    pageSize: 20,
    search: '',
    sortBy: '',
    sortDir: 'asc',
    ruleTypeId: '',
    isActive: '',
  }
  const { query, setQuery } = useListQueryState<RuleQuery>({ defaults: defaultQuery, numberKeys: ['page', 'pageSize'] })
  const [draftFilters, setDraftFilters] = useState<Pick<RuleQuery, 'ruleTypeId' | 'isActive'>>({
    ruleTypeId: query.ruleTypeId,
    isActive: query.isActive,
  })

  const load = async () => {
    if (!canView) {
      return
    }

    setLoading(true)
    setError('')
    try {
      const { data } = await api.get<PagedResult<LeadScoreRule>>('api/lead-score-rules', {
        params: {
          page: query.page,
          pageSize: query.pageSize,
          search: query.search || undefined,
          sortBy: query.sortBy || undefined,
          sortDir: query.sortDir,
          ruleTypeId: query.ruleTypeId || undefined,
          isActive: query.isActive || undefined,
        },
      })
      setRows(data.items)
      setTotalCount(data.totalCount)
    } catch {
      setError('Failed to load lead score rules.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void Promise.resolve().then(load)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [canView, query])

  const activeFilterCount = [query.ruleTypeId, query.isActive].filter(Boolean).length

  const refresh = () => setQuery((current) => ({ ...current }))

  const remove = async () => {
    if (!deleteTarget) {
      return
    }

    setLoading(true)
    setError('')
    try {
      await api.delete(`api/lead-score-rules/${deleteTarget.id}`)
      setDeleteTarget(null)
      refresh()
    } catch {
      setError('Failed to delete lead score rule.')
    } finally {
      setLoading(false)
    }
  }

  const runRules = async () => {
    setLoading(true)
    setError('')
    try {
      await api.post('api/lead-score-rules/run')
      refresh()
    } catch {
      setError('Failed to run lead score rules.')
    } finally {
      setLoading(false)
    }
  }

  const columns = useMemo<DenseColumn<LeadScoreRule>[]>(
    () => [
      { key: 'name', label: 'Name', sortable: true },
      { key: 'code', label: 'Code', sortable: true },
      { key: 'ruleTypeName', label: 'Rule Type', sortable: true, render: (row) => row.ruleTypeName || 'Not set' },
      { key: 'fieldName', label: 'Field', sortable: true, render: (row) => row.fieldName || 'Not set' },
      { key: 'operator', label: 'Operator', sortable: true, render: (row) => row.operator || 'Not set' },
      { key: 'compareValue', label: 'Compare Value', sortable: true, render: (row) => row.compareValue || 'Not set' },
      { key: 'scoreValue', label: 'Score Value', sortable: true },
      { key: 'sortOrder', label: 'Sort Order', sortable: true },
      { key: 'isActive', label: 'Active', sortable: true, render: (row) => statusCell(row.isActive ? 'Active' : 'Inactive') },
    ],
    [],
  )

  if (!canView) {
    return (
      <div>
        <PageHeader title="Lead Score Rules" subtitle="Manage scoring rules for lead qualification." />
        <MessageBar intent="error">
          <MessageBarBody>You do not have permission to view lead score rules.</MessageBarBody>
        </MessageBar>
      </div>
    )
  }

  return (
    <div>
      <PageHeader
        title="Lead Score Rules"
        subtitle="Manage scoring rules for lead qualification."
        quickAction={canCreate ? 'Create Rule' : undefined}
        onQuickAction={canCreate ? () => navigate('/lead-score-rules/create') : undefined}
      />
      <CommandBar
        actions={[
          ...(canCreate ? [{ key: 'create', label: 'Create Rule', onClick: () => navigate('/lead-score-rules/create') }] : []),
          ...(canRun ? [{ key: 'run', label: 'Run Rules', onClick: () => void runRules() }] : []),
        ]}
      />

      {loading ? <Spinner size="small" label="Loading lead score rules..." style={{ margin: '8px 0' }} /> : null}
      {error ? (
        <MessageBar intent="error" style={{ marginBottom: 10 }}>
          <MessageBarBody>{error}</MessageBarBody>
        </MessageBar>
      ) : null}

      <DenseDataGrid
        rows={rows}
        columns={columns}
        loading={loading}
        totalCount={totalCount}
        page={query.page}
        pageSize={query.pageSize}
        search={query.search}
        sort={query.sortBy ? ({ key: query.sortBy as keyof LeadScoreRule, dir: query.sortDir }) : null}
        onPageChange={(page) => setQuery((current) => ({ ...current, page }))}
        onPageSizeChange={(pageSize) => setQuery((current) => ({ ...current, pageSize, page: 1 }))}
        onSearchChange={(search) => setQuery((current) => ({ ...current, search, page: 1 }))}
        onSortChange={(sort: DenseSort<LeadScoreRule> | null) =>
          setQuery((current) => ({
            ...current,
            sortBy: sort ? String(sort.key) : '',
            sortDir: sort?.dir ?? 'asc',
            page: 1,
          }))
        }
        onView={(row) => navigate(`/lead-score-rules/${row.id}`)}
        onEdit={canEdit ? (row) => navigate(`/lead-score-rules/${row.id}/edit`) : undefined}
        onDelete={canDelete ? (row) => setDeleteTarget(row) : undefined}
        emptyMessage="No lead score rules match the current filters."
        activeFilterCount={activeFilterCount}
        filterPanel={
          <>
            <LookupFilterField label="Rule Type" fieldKey="ruleTypeId" value={draftFilters.ruleTypeId} onChange={(value) => setDraftFilters((current) => ({ ...current, ruleTypeId: value }))} />
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
            ruleTypeId: query.ruleTypeId,
            isActive: query.isActive,
          })
        }
        onClearFilters={() =>
          setDraftFilters({
            ruleTypeId: '',
            isActive: '',
          })
        }
      />

      <DeleteConfirmDialog
        open={Boolean(deleteTarget)}
        title="Delete Lead Score Rule"
        message={`Delete ${deleteTarget?.name ?? 'this rule'}?`}
        onConfirm={() => void remove()}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  )
}

export function LeadScoreRuleFormPage({ mode }: { mode: 'create' | 'edit' }) {
  const navigate = useNavigate()
  const { id } = useParams()
  const { hasPermission } = useAuth()
  const isEdit = mode === 'edit'
  const canSave = isEdit ? hasPermission('LeadScoreRules.Update') : hasPermission('LeadScoreRules.Create')
  const [form, setForm] = useState<LeadScoreRuleFormState>(emptyLeadScoreRuleForm)
  const [rule, setRule] = useState<LeadScoreRule | null>(null)
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
        const { data } = await api.get<LeadScoreRule>(`api/lead-score-rules/${id}`)
        setRule(data)
        setForm(leadScoreRuleToForm(data))
      } catch {
        setError('Failed to load lead score rule.')
      } finally {
        setLoading(false)
      }
    })()
  }, [id, isEdit])

  const setValue = <K extends keyof LeadScoreRuleFormState>(key: K, value: LeadScoreRuleFormState[K]) => {
    setForm((current) => ({ ...current, [key]: value }))
  }

  const validate = () => {
    const next: Record<string, string> = {}
    if (!form.name.trim()) next.name = 'Name is required.'
    if (!form.code.trim()) next.code = 'Code is required.'
    if (!form.ruleTypeId.trim()) next.ruleTypeId = 'Rule type is required.'
    if (!form.scoreValue.trim() || Number(form.scoreValue) === 0) next.scoreValue = 'Score value must not be zero.'
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
        await api.put(`api/lead-score-rules/${id}`, leadScoreRulePayload(form))
        if (closeAfterSave) {
          navigate('/lead-score-rules')
        } else {
          const { data } = await api.get<LeadScoreRule>(`api/lead-score-rules/${id}`)
          setRule(data)
          setForm(leadScoreRuleToForm(data))
        }
      } else {
        const { data } = await api.post<LeadScoreRule>('api/lead-score-rules', leadScoreRulePayload(form))
        navigate(closeAfterSave ? '/lead-score-rules' : `/lead-score-rules/${data.id}/edit`)
      }
    } catch {
      setError('Save failed. Please review rule values.')
    } finally {
      setSaving(false)
    }
  }

  const alerts = [
    ...(!canSave ? [{ intent: 'error' as const, text: `You do not have permission to ${isEdit ? 'update' : 'create'} lead score rules.` }] : []),
    ...(error ? [{ intent: 'error' as const, text: error }] : []),
    ...Object.values(fieldErrors).map((message) => ({ intent: 'warning' as const, text: message })),
  ]

  return (
    <EntityPageLayout
      header={<EntityHeader icon={<ChartMultipleRegular />} title={isEdit ? 'Edit Lead Score Rule' : 'Create Lead Score Rule'} subtitle={rule?.code} status={rule?.isActive ? 'Active' : rule ? 'Inactive' : undefined} actions={[
        { key: 'save', label: 'Save', onClick: () => void save(false), appearance: 'primary', disabled: loading || saving || !canSave },
        { key: 'save-close', label: 'Save & Close', onClick: () => void save(true), appearance: 'secondary', disabled: loading || saving || !canSave },
        { key: 'cancel', label: 'Cancel', onClick: () => navigate('/lead-score-rules'), appearance: 'subtle' },
      ]} />}
      alerts={alerts}
      stickyBar={<StickySaveBar onSave={() => void save(false)} onSaveAndClose={() => void save(true)} onCancel={() => navigate('/lead-score-rules')} disableActions={loading || saving || !canSave} />}
    >
      <FormSectionCard title="Rule">
        <Field label="Name" required validationMessage={fieldErrors.name}>
          <Input size="small" value={form.name} readOnly={!canSave} onChange={(_, data) => setValue('name', data.value)} />
        </Field>
        <Field label="Code" required validationMessage={fieldErrors.code}>
          <Input size="small" value={form.code} readOnly={!canSave} onChange={(_, data) => setValue('code', data.value)} />
        </Field>
        <Field label="Rule Type" required validationMessage={fieldErrors.ruleTypeId}>
          <LookupCombobox fieldKey="ruleTypeId" value={form.ruleTypeId} disabled={!canSave} onChange={(value) => setValue('ruleTypeId', value)} />
        </Field>
        <Field label="Field Name">
          <Input size="small" value={form.fieldName} readOnly={!canSave} onChange={(_, data) => setValue('fieldName', data.value)} />
        </Field>
        <Field label="Operator">
          <Input size="small" value={form.operator} readOnly={!canSave} onChange={(_, data) => setValue('operator', data.value)} />
        </Field>
        <Field label="Compare Value">
          <Input size="small" value={form.compareValue} readOnly={!canSave} onChange={(_, data) => setValue('compareValue', data.value)} />
        </Field>
        <Field label="Score Value" required validationMessage={fieldErrors.scoreValue}>
          <Input size="small" type="number" value={form.scoreValue} readOnly={!canSave} onChange={(_, data) => setValue('scoreValue', data.value)} />
        </Field>
        <Field label="Sort Order">
          <Input size="small" type="number" value={form.sortOrder} readOnly={!canSave} onChange={(_, data) => setValue('sortOrder', data.value)} />
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

export function LeadScoreRuleDetailsPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const { hasPermission } = useAuth()
  const canView = hasPermission('LeadScoreRules.View')
  const canEdit = hasPermission('LeadScoreRules.Update')
  const [rule, setRule] = useState<LeadScoreRule | null>(null)
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
        const { data } = await api.get<LeadScoreRule>(`api/lead-score-rules/${id}`)
        setRule(data)
      } catch {
        setError('Failed to load lead score rule.')
      } finally {
        setLoading(false)
      }
    })()
  }, [canView, id])

  const rows = useMemo(
    () => rule ? [
      { label: 'Name', value: rule.name },
      { label: 'Code', value: rule.code },
      { label: 'Rule Type', value: rule.ruleTypeName ?? '' },
      { label: 'Field Name', value: rule.fieldName ?? '' },
      { label: 'Operator', value: rule.operator ?? '' },
      { label: 'Compare Value', value: rule.compareValue ?? '' },
      { label: 'Score Value', value: String(rule.scoreValue) },
      { label: 'Sort Order', value: String(rule.sortOrder) },
      { label: 'Active', value: rule.isActive ? 'Yes' : 'No' },
      { label: 'Description', value: rule.description ?? '' },
    ] : [],
    [rule],
  )

  return (
    <EntityPageLayout
      header={<EntityHeader icon={<ChartMultipleRegular />} title={rule?.name ?? 'Lead Score Rule'} subtitle={rule?.code} status={rule?.isActive ? 'Active' : rule ? 'Inactive' : undefined} actions={[
        ...(canEdit && id ? [{ key: 'edit', label: 'Edit', onClick: () => navigate(`/lead-score-rules/${id}/edit`), appearance: 'primary' as const }] : []),
        { key: 'back', label: 'Back to List', onClick: () => navigate('/lead-score-rules'), appearance: 'subtle' as const },
      ]} />}
      alerts={error ? [{ intent: 'error', text: error }] : undefined}
    >
      {!canView ? (
        <MessageBar intent="error">
          <MessageBarBody>You do not have permission to view lead score rules.</MessageBarBody>
        </MessageBar>
      ) : null}
      {loading ? <Spinner size="small" label="Loading lead score rule..." /> : null}
      {!loading && rule ? <EntityDetailsGrid rows={rows} /> : null}
    </EntityPageLayout>
  )
}
