import { useEffect, useMemo, useState } from 'react'
import { Dropdown, Field, Input, MessageBar, MessageBarBody, Option, Spinner, Switch, Textarea } from '@fluentui/react-components'
import { ClipboardTaskRegular } from '@fluentui/react-icons'
import { useNavigate, useParams } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { AuditHistoryPanel } from '../contacts/ContactRelatedPanels'
import { DeleteConfirmDialog } from '../components/crud/DeleteConfirmDialog'
import {
  EntityDetailsGrid,
  EntityHeader,
  EntityPageLayout,
  EntityTabPlaceholder,
  EntityTabs,
  FormSectionCard,
  LookupCombobox,
  StickySaveBar,
} from '../components/entity-ui/EntityComponents'
import { DenseDataGrid, statusCell, type DenseColumn, type DenseSort } from '../components/grid/DenseDataGrid'
import { CommandBar } from '../layout/components/CommandBar'
import { PageHeader } from '../layout/components/PageHeader'
import type { NumberSequence, NumberSequencePreview, PagedResult } from '../types/models'
import {
  emptyNumberSequenceForm,
  formatDateTime,
  numberSequencePayload,
  numberSequenceToForm,
  type NumberSequenceFormState,
} from './numberSequenceUtils'
import styles from '../contacts/Contacts.module.css'

type SequenceQuery = {
  page: number
  pageSize: number
  search: string
  sortBy: string
  sortDir: 'asc' | 'desc'
  resetFrequencyId: string
  isActive: string
}

const formTabs = [
  { key: 'general', label: 'General' },
  { key: 'format', label: 'Format' },
  { key: 'reset-rules', label: 'Reset Rules' },
  { key: 'audit-history', label: 'Audit History' },
]

const detailsTabs = [
  { key: 'summary', label: 'Summary' },
  { key: 'audit-history', label: 'Audit History' },
]

export function NumberSequencesListPage() {
  const navigate = useNavigate()
  const { hasPermission } = useAuth()
  const canView = hasPermission('NumberSequences.View')
  const canCreate = hasPermission('NumberSequences.Create')
  const canEdit = hasPermission('NumberSequences.Update')
  const canDelete = hasPermission('NumberSequences.Delete')
  const canPreview = hasPermission('NumberSequences.Preview')
  const canReset = hasPermission('NumberSequences.Reset')
  const [rows, setRows] = useState<NumberSequence[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [message, setMessage] = useState('')
  const [deleteTarget, setDeleteTarget] = useState<NumberSequence | null>(null)
  const [query, setQuery] = useState<SequenceQuery>({
    page: 1,
    pageSize: 20,
    search: '',
    sortBy: '',
    sortDir: 'asc',
    resetFrequencyId: '',
    isActive: '',
  })

  const load = async () => {
    if (!canView) {
      return
    }

    setLoading(true)
    setError('')
    try {
      const { data } = await api.get<PagedResult<NumberSequence>>('api/number-sequences', {
        params: {
          page: query.page,
          pageSize: query.pageSize,
          search: query.search || undefined,
          sortBy: query.sortBy || undefined,
          sortDir: query.sortDir,
          resetFrequencyId: query.resetFrequencyId || undefined,
          isActive: query.isActive || undefined,
        },
      })
      setRows(data.items)
      setTotalCount(data.totalCount)
    } catch {
      setError('Failed to load number sequences.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void Promise.resolve().then(load)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [canView, query])

  const refresh = () => setQuery((current) => ({ ...current }))

  const preview = async (row: NumberSequence) => {
    setLoading(true)
    setError('')
    setMessage('')
    try {
      const { data } = await api.post<NumberSequencePreview>(`api/number-sequences/${row.id}/preview`)
      setMessage(`${row.sequenceCode} preview: ${data.preview}`)
    } catch {
      setError('Failed to preview number sequence.')
    } finally {
      setLoading(false)
    }
  }

  const reset = async (row: NumberSequence) => {
    setLoading(true)
    setError('')
    setMessage('')
    try {
      await api.post(`api/number-sequences/${row.id}/reset`)
      setMessage(`${row.sequenceCode} was reset.`)
      refresh()
    } catch {
      setError('Failed to reset number sequence.')
    } finally {
      setLoading(false)
    }
  }

  const remove = async () => {
    if (!deleteTarget) {
      return
    }

    setLoading(true)
    setError('')
    try {
      await api.delete(`api/number-sequences/${deleteTarget.id}`)
      setDeleteTarget(null)
      refresh()
    } catch {
      setError('Failed to delete number sequence.')
    } finally {
      setLoading(false)
    }
  }

  const columns = useMemo<DenseColumn<NumberSequence>[]>(
    () => [
      { key: 'entityName', label: 'Entity Name', sortable: true },
      { key: 'sequenceCode', label: 'Sequence Code', sortable: true },
      { key: 'prefix', label: 'Prefix', sortable: true },
      { key: 'formatPreview', label: 'Format Preview', sortable: true, render: (row) => row.formatPreview || 'Not set' },
      { key: 'currentNumber', label: 'Current Number', sortable: true },
      { key: 'nextNumber', label: 'Next Number', sortable: true },
      { key: 'resetFrequencyName', label: 'Reset Frequency', sortable: true, render: (row) => row.resetFrequencyName || 'Not set' },
      { key: 'isActive', label: 'Active', sortable: true, render: (row) => statusCell(row.isActive ? 'Active' : 'Inactive') },
      { key: 'updatedAt', label: 'Updated At', sortable: true, render: (row) => formatDateTime(row.updatedAt) || 'Not updated' },
    ],
    [],
  )

  if (!canView) {
    return (
      <div>
        <PageHeader title="Number Sequences" subtitle="Configure record numbering formats." />
        <MessageBar intent="error">
          <MessageBarBody>You do not have permission to view number sequences.</MessageBarBody>
        </MessageBar>
      </div>
    )
  }

  return (
    <div>
      <PageHeader
        title="Number Sequences"
        subtitle="Configure readable, unique numbers for CRM records."
        quickAction={canCreate ? 'New Sequence' : undefined}
        onQuickAction={canCreate ? () => navigate('/configuration/number-sequences/create') : undefined}
      />
      <CommandBar actions={[
        ...(canCreate ? [{ key: 'create', label: 'New Sequence', onClick: () => navigate('/configuration/number-sequences/create') }] : []),
      ]} />

      <section className={styles.filterBar}>
        <Field label="Reset Frequency">
          <LookupCombobox fieldKey="resetFrequencyId" value={query.resetFrequencyId} onChange={(value) => setQuery((current) => ({ ...current, resetFrequencyId: value, page: 1 }))} />
        </Field>
        <Field label="Active">
          <Dropdown
            size="small"
            selectedOptions={query.isActive ? [query.isActive] : []}
            value={query.isActive === 'true' ? 'Active' : query.isActive === 'false' ? 'Inactive' : ''}
            onOptionSelect={(_, data) => setQuery((current) => ({ ...current, isActive: data.optionValue ?? '', page: 1 }))}
          >
            <Option value="">All</Option>
            <Option value="true">Active</Option>
            <Option value="false">Inactive</Option>
          </Dropdown>
        </Field>
      </section>

      {loading ? <Spinner size="small" label="Loading number sequences..." style={{ margin: '8px 0' }} /> : null}
      {error ? (
        <MessageBar intent="error" style={{ marginBottom: 10 }}>
          <MessageBarBody>{error}</MessageBarBody>
        </MessageBar>
      ) : null}
      {message ? (
        <MessageBar intent="success" style={{ marginBottom: 10 }}>
          <MessageBarBody>{message}</MessageBarBody>
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
        sort={query.sortBy ? ({ key: query.sortBy as keyof NumberSequence, dir: query.sortDir }) : null}
        onPageChange={(page) => setQuery((current) => ({ ...current, page }))}
        onPageSizeChange={(pageSize) => setQuery((current) => ({ ...current, pageSize, page: 1 }))}
        onSearchChange={(search) => setQuery((current) => ({ ...current, search, page: 1 }))}
        onSortChange={(sort: DenseSort<NumberSequence> | null) =>
          setQuery((current) => ({
            ...current,
            sortBy: sort ? String(sort.key) : '',
            sortDir: sort?.dir ?? 'asc',
            page: 1,
          }))
        }
        onView={(row) => navigate(`/configuration/number-sequences/${row.id}`)}
        onEdit={canEdit ? (row) => navigate(`/configuration/number-sequences/${row.id}/edit`) : undefined}
        onDelete={canDelete ? (row) => setDeleteTarget(row) : undefined}
        customActions={[
          ...(canPreview ? [{ key: 'preview', label: 'Preview', onClick: (row: NumberSequence) => void preview(row) }] : []),
          ...(canReset ? [{ key: 'reset', label: 'Reset', onClick: (row: NumberSequence) => void reset(row) }] : []),
        ]}
        emptyMessage="No number sequences match the current filters."
      />

      <DeleteConfirmDialog
        open={Boolean(deleteTarget)}
        title="Delete Number Sequence"
        message={`Delete ${deleteTarget?.sequenceCode ?? 'this sequence'}?`}
        onConfirm={() => void remove()}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  )
}

export function NumberSequenceFormPage({ mode }: { mode: 'create' | 'edit' }) {
  const navigate = useNavigate()
  const { id } = useParams()
  const { hasPermission } = useAuth()
  const isEdit = mode === 'edit'
  const canSave = isEdit ? hasPermission('NumberSequences.Update') : hasPermission('NumberSequences.Create')
  const [form, setForm] = useState<NumberSequenceFormState>(emptyNumberSequenceForm)
  const [sequence, setSequence] = useState<NumberSequence | null>(null)
  const [activeTab, setActiveTab] = useState('general')
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
        const { data } = await api.get<NumberSequence>(`api/number-sequences/${id}`)
        setSequence(data)
        setForm(numberSequenceToForm(data))
      } catch {
        setError('Failed to load number sequence.')
      } finally {
        setLoading(false)
      }
    })()
  }, [id, isEdit])

  const setValue = <K extends keyof NumberSequenceFormState>(key: K, value: NumberSequenceFormState[K]) => {
    setForm((current) => ({ ...current, [key]: value }))
  }

  const validate = () => {
    const next: Record<string, string> = {}
    const minDigits = Number(form.minimumDigits)
    const currentNumber = Number(form.currentNumber)
    const nextNumber = Number(form.nextNumber)

    if (!form.entityName.trim()) next.entityName = 'Entity name is required.'
    if (!form.sequenceCode.trim()) next.sequenceCode = 'Sequence code is required.'
    if (form.sequenceCode.trim() !== form.sequenceCode.trim().toUpperCase() || /\s/.test(form.sequenceCode)) next.sequenceCode = 'Sequence code must be uppercase with no spaces.'
    if (!form.prefix.trim()) next.prefix = 'Prefix is required.'
    if (minDigits < 3 || minDigits > 12) next.minimumDigits = 'Minimum digits must be between 3 and 12.'
    if (currentNumber < 0) next.currentNumber = 'Current number cannot be negative.'
    if (nextNumber <= currentNumber) next.nextNumber = 'Next number must be greater than current number.'
    if ((form.separator.trim() || '-').length > 3) next.separator = 'Separator cannot exceed 3 characters.'

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
        await api.put(`api/number-sequences/${id}`, numberSequencePayload(form))
        if (closeAfterSave) {
          navigate('/configuration/number-sequences')
        } else {
          const { data } = await api.get<NumberSequence>(`api/number-sequences/${id}`)
          setSequence(data)
          setForm(numberSequenceToForm(data))
        }
      } else {
        const { data } = await api.post<NumberSequence>('api/number-sequences', numberSequencePayload(form))
        navigate(closeAfterSave ? '/configuration/number-sequences' : `/configuration/number-sequences/${data.id}/edit`)
      }
    } catch {
      setError('Save failed. Please review sequence values.')
    } finally {
      setSaving(false)
    }
  }

  const renderText = (key: keyof NumberSequenceFormState, label: string, required?: boolean, type: 'text' | 'number' | 'datetime-local' = 'text', readOnly?: boolean) => (
    <Field label={label} required={required} validationMessage={fieldErrors[String(key)]}>
      <Input
        size="small"
        type={type}
        value={String(form[key] ?? '')}
        readOnly={readOnly || !canSave}
        onChange={(_, data) => setValue(key, data.value as NumberSequenceFormState[typeof key])}
      />
    </Field>
  )

  const renderSwitch = (key: keyof NumberSequenceFormState, label: string) => (
    <Field label={label} validationMessage={fieldErrors[String(key)]}>
      <Switch checked={Boolean(form[key])} disabled={!canSave} onChange={(_, data) => setValue(key, Boolean(data.checked) as NumberSequenceFormState[typeof key])} />
    </Field>
  )

  const alerts = [
    ...(!canSave ? [{ intent: 'error' as const, text: `You do not have permission to ${isEdit ? 'update' : 'create'} number sequences.` }] : []),
    ...(error ? [{ intent: 'error' as const, text: error }] : []),
    ...Object.values(fieldErrors).map((message) => ({ intent: 'warning' as const, text: message })),
  ]

  return (
    <EntityPageLayout
      header={<EntityHeader icon={<ClipboardTaskRegular />} title={isEdit ? 'Edit Number Sequence' : 'Create Number Sequence'} subtitle={sequence?.sequenceCode ?? 'Configure numbering for CRM records.'} status={sequence?.isActive ? 'Active' : sequence ? 'Inactive' : undefined} actions={[
        { key: 'save', label: 'Save', onClick: () => void save(false), appearance: 'primary', disabled: loading || saving || !canSave },
        { key: 'save-close', label: 'Save & Close', onClick: () => void save(true), appearance: 'secondary', disabled: loading || saving || !canSave },
        { key: 'cancel', label: 'Cancel', onClick: () => navigate('/configuration/number-sequences'), appearance: 'subtle' },
      ]} />}
      tabs={<EntityTabs tabs={formTabs} activeTab={activeTab} onTabChange={setActiveTab} />}
      alerts={alerts}
      stickyBar={<StickySaveBar onSave={() => void save(false)} onSaveAndClose={() => void save(true)} onCancel={() => navigate('/configuration/number-sequences')} disableActions={loading || saving || !canSave} />}
    >
      {loading ? (
        <MessageBar>
          <MessageBarBody>Loading number sequence...</MessageBarBody>
        </MessageBar>
      ) : null}

      {!loading && activeTab === 'general' ? (
        <FormSectionCard title="General">
          {renderText('entityName', 'Entity Name', true)}
          {renderText('sequenceCode', 'Sequence Code', true)}
          <Field label="Description">
            <Textarea value={form.description} readOnly={!canSave} onChange={(_, data) => setValue('description', data.value)} />
          </Field>
          {renderSwitch('isActive', 'Active')}
        </FormSectionCard>
      ) : null}

      {!loading && activeTab === 'format' ? (
        <FormSectionCard title="Format">
          {renderText('prefix', 'Prefix', true)}
          {renderText('separator', 'Separator')}
          {renderText('minimumDigits', 'Minimum Digits', true, 'number')}
          {renderSwitch('includeYear', 'Include Year')}
          {renderSwitch('includeMonth', 'Include Month')}
          {renderSwitch('includeDay', 'Include Day')}
          {renderText('suffix', 'Suffix')}
          {renderText('formatPreview', 'Format Preview', false, 'text', true)}
        </FormSectionCard>
      ) : null}

      {!loading && activeTab === 'reset-rules' ? (
        <FormSectionCard title="Reset Rules">
          <Field label="Reset Frequency">
            <LookupCombobox fieldKey="resetFrequencyId" value={form.resetFrequencyId} disabled={!canSave} onChange={(value) => setValue('resetFrequencyId', value)} />
          </Field>
          {renderText('lastResetDate', 'Last Reset Date', false, 'datetime-local')}
          {renderText('currentNumber', 'Current Number', true, 'number')}
          {renderText('nextNumber', 'Next Number', true, 'number')}
        </FormSectionCard>
      ) : null}

      {!loading && activeTab === 'audit-history' ? (
        isEdit && id ? <AuditHistoryPanel entityName="NumberSequence" entityId={id} /> : <EntityTabPlaceholder text="Audit history appears after the sequence is saved." />
      ) : null}
    </EntityPageLayout>
  )
}

export function NumberSequenceDetailsPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const { hasPermission } = useAuth()
  const canView = hasPermission('NumberSequences.View')
  const canEdit = hasPermission('NumberSequences.Update')
  const canPreview = hasPermission('NumberSequences.Preview')
  const canReset = hasPermission('NumberSequences.Reset')
  const [sequence, setSequence] = useState<NumberSequence | null>(null)
  const [activeTab, setActiveTab] = useState('summary')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [message, setMessage] = useState('')

  const load = async () => {
    if (!id || !canView) {
      return
    }

    setLoading(true)
    setError('')
    try {
      const { data } = await api.get<NumberSequence>(`api/number-sequences/${id}`)
      setSequence(data)
    } catch {
      setError('Failed to load number sequence.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void Promise.resolve().then(load)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [canView, id])

  const preview = async () => {
    if (!id) {
      return
    }

    setLoading(true)
    setMessage('')
    setError('')
    try {
      const { data } = await api.post<NumberSequencePreview>(`api/number-sequences/${id}/preview`)
      setMessage(`Preview: ${data.preview}`)
    } catch {
      setError('Failed to preview number sequence.')
    } finally {
      setLoading(false)
    }
  }

  const reset = async () => {
    if (!id) {
      return
    }

    setLoading(true)
    setMessage('')
    setError('')
    try {
      await api.post(`api/number-sequences/${id}/reset`)
      setMessage('Sequence was reset.')
      await load()
    } catch {
      setError('Failed to reset number sequence.')
    } finally {
      setLoading(false)
    }
  }

  const rows = useMemo(
    () => sequence ? [
      { label: 'Entity Name', value: sequence.entityName },
      { label: 'Sequence Code', value: sequence.sequenceCode },
      { label: 'Prefix', value: sequence.prefix },
      { label: 'Separator', value: sequence.separator },
      { label: 'Minimum Digits', value: String(sequence.minimumDigits) },
      { label: 'Date Parts', value: [sequence.includeYear ? 'Year' : '', sequence.includeMonth ? 'Month' : '', sequence.includeDay ? 'Day' : ''].filter(Boolean).join(', ') || 'None' },
      { label: 'Suffix', value: sequence.suffix ?? '' },
      { label: 'Format Preview', value: sequence.formatPreview ?? '' },
      { label: 'Current Number', value: String(sequence.currentNumber) },
      { label: 'Next Number', value: String(sequence.nextNumber) },
      { label: 'Reset Frequency', value: sequence.resetFrequencyName ?? '' },
      { label: 'Last Reset Date', value: formatDateTime(sequence.lastResetDate) },
      { label: 'Active', value: sequence.isActive ? 'Yes' : 'No' },
      { label: 'Updated At', value: formatDateTime(sequence.updatedAt) },
      { label: 'Description', value: sequence.description ?? '' },
    ] : [],
    [sequence],
  )

  return (
    <EntityPageLayout
      header={<EntityHeader icon={<ClipboardTaskRegular />} title={sequence?.entityName ?? 'Number Sequence'} subtitle={sequence?.sequenceCode} status={sequence?.isActive ? 'Active' : sequence ? 'Inactive' : undefined} actions={[
        ...(canEdit && id ? [{ key: 'edit', label: 'Edit', onClick: () => navigate(`/configuration/number-sequences/${id}/edit`), appearance: 'primary' as const }] : []),
        ...(canPreview ? [{ key: 'preview', label: 'Preview', onClick: () => void preview(), appearance: 'secondary' as const, disabled: loading }] : []),
        ...(canReset ? [{ key: 'reset', label: 'Reset', onClick: () => void reset(), appearance: 'subtle' as const, disabled: loading }] : []),
        { key: 'back', label: 'Back to List', onClick: () => navigate('/configuration/number-sequences'), appearance: 'subtle' as const },
      ]} />}
      tabs={<EntityTabs tabs={detailsTabs} activeTab={activeTab} onTabChange={setActiveTab} />}
      alerts={[
        ...(!canView ? [{ intent: 'error' as const, text: 'You do not have permission to view number sequences.' }] : []),
        ...(error ? [{ intent: 'error' as const, text: error }] : []),
        ...(message ? [{ intent: 'warning' as const, text: message }] : []),
      ]}
    >
      {loading ? <Spinner size="small" label="Loading number sequence..." /> : null}
      {!loading && activeTab === 'summary' && sequence ? <EntityDetailsGrid rows={rows} /> : null}
      {!loading && activeTab === 'audit-history' && id ? <AuditHistoryPanel entityName="NumberSequence" entityId={id} /> : null}
    </EntityPageLayout>
  )
}
