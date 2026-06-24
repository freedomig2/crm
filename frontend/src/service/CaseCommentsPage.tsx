import { Dropdown, Field, Input, MessageBar, MessageBarBody, Option, Switch } from '@fluentui/react-components'
import { useEffect, useMemo, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { FormSectionCard } from '../components/entity-ui/EntityComponents'
import { FilterField } from '../components/filters/FilterField'
import { DenseDataGrid, type DenseColumn, type DenseSort } from '../components/grid/DenseDataGrid'
import { RelatedRecordsSubgrid } from '../components/subgrid/RelatedRecordsSubgrid'
import { SubgridDeleteConfirmDialog } from '../components/subgrid/SubgridDeleteConfirmDialog'
import { SubgridModalForm } from '../components/subgrid/SubgridModalForm'
import { useListQueryState } from '../hooks/useListQueryState'
import { CommandBar } from '../layout/components/CommandBar'
import { PageHeader } from '../layout/components/PageHeader'
import type { Case, CaseComment, PagedResult } from '../types/models'
import styles from '../sales/Sales.module.css'

type CaseCommentQuery = {
  page: number
  pageSize: number
  search: string
  sortBy: string
  sortDir: 'asc' | 'desc'
  isInternal: string
}

type CaseCommentForm = {
  commentText: string
  isInternal: boolean
}

const emptyForm: CaseCommentForm = {
  commentText: '',
  isInternal: false,
}

export function CaseCommentsPage() {
  const navigate = useNavigate()
  const { id: caseId } = useParams()
  const { hasPermission } = useAuth()
  const canViewCases = hasPermission('Cases.View')
  const canView = hasPermission('CaseComments.View')
  const canCreate = hasPermission('CaseComments.Create')
  const canDelete = hasPermission('CaseComments.Delete')
  const canResolve = hasPermission('Cases.Resolve')
  const canClose = hasPermission('Cases.Close')
  const canReopen = hasPermission('Cases.Reopen')

  const [serviceCase, setServiceCase] = useState<Case | null>(null)
  const [rows, setRows] = useState<CaseComment[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [formOpen, setFormOpen] = useState(false)
  const [deleteTarget, setDeleteTarget] = useState<CaseComment | null>(null)
  const [form, setForm] = useState<CaseCommentForm>(emptyForm)

  const defaultQuery: CaseCommentQuery = {
    page: 1,
    pageSize: 20,
    search: '',
    sortBy: '',
    sortDir: 'desc',
    isInternal: '',
  }

  const { query, setQuery } = useListQueryState<CaseCommentQuery>({ defaults: defaultQuery, numberKeys: ['page', 'pageSize'] })
  const [draftFilters, setDraftFilters] = useState<Pick<CaseCommentQuery, 'isInternal'>>({
    isInternal: query.isInternal,
  })

  const loadCase = async () => {
    if (!caseId || !canViewCases) {
      return
    }

    try {
      const { data } = await api.get<Case>(`api/cases/${caseId}`)
      setServiceCase(data)
    } catch {
      setServiceCase(null)
    }
  }

  const loadComments = async () => {
    if (!caseId || !canView) {
      return
    }

    setLoading(true)
    setError('')
    try {
      const { data } = await api.get<PagedResult<CaseComment>>(`api/cases/${caseId}/comments`, {
        params: {
          page: query.page,
          pageSize: query.pageSize,
          search: query.search || undefined,
          sortBy: query.sortBy || undefined,
          sortDir: query.sortDir,
        },
      })

      let items = data.items
      if (query.isInternal === 'true') {
        items = items.filter((item) => item.isInternal)
      }
      if (query.isInternal === 'false') {
        items = items.filter((item) => !item.isInternal)
      }

      setRows(items)
      setTotalCount(items.length === data.items.length ? data.totalCount : items.length)
    } catch {
      setError('Failed to load case comments.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void Promise.resolve().then(async () => {
      await Promise.all([loadCase(), loadComments()])
    })
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [caseId, canView, canViewCases, query])

  const saveComment = async () => {
    if (!caseId || !canCreate) {
      return
    }

    if (!form.commentText.trim()) {
      setError('Comment text is required.')
      return
    }

    setLoading(true)
    setError('')
    setSuccess('')
    try {
      await api.post(`api/cases/${caseId}/comments`, {
        commentText: form.commentText.trim(),
        isInternal: form.isInternal,
      })

      setForm(emptyForm)
      setFormOpen(false)
      setSuccess('Case comment added successfully.')
      await loadComments()
    } catch {
      setError('Failed to add case comment.')
    } finally {
      setLoading(false)
    }
  }

  const removeComment = async () => {
    if (!deleteTarget || !canDelete) {
      return
    }

    setLoading(true)
    setError('')
    setSuccess('')
    try {
      await api.delete(`api/cases/comments/${deleteTarget.id}`)
      setDeleteTarget(null)
      setSuccess('Case comment deleted successfully.')
      await loadComments()
    } catch {
      setError('Failed to delete case comment.')
    } finally {
      setLoading(false)
    }
  }

  const performTransition = async (action: 'resolve' | 'close' | 'reopen') => {
    if (!caseId) {
      return
    }

    setLoading(true)
    setError('')
    try {
      if (action === 'resolve') {
        await api.post(`api/cases/${caseId}/resolve`, { resolutionSummary: serviceCase?.resolutionSummary ?? null })
      }

      if (action === 'close') {
        await api.post(`api/cases/${caseId}/close`, {})
      }

      if (action === 'reopen') {
        await api.post(`api/cases/${caseId}/reopen`, {})
      }

      await loadCase()
    } catch {
      setError('Failed to update case status.')
    } finally {
      setLoading(false)
    }
  }

  const activeFilterCount = [query.isInternal].filter(Boolean).length

  const columns = useMemo<DenseColumn<CaseComment>[]>(
    () => [
      { key: 'commentText', label: 'Comment', sortable: true },
      { key: 'isInternal', label: 'Internal', sortable: true, render: (row) => (row.isInternal ? 'Yes' : 'No') },
      { key: 'createdByName', label: 'Created By', sortable: true },
      { key: 'createdAt', label: 'Created At', sortable: true },
    ],
    [],
  )

  if (!canView) {
    return (
      <div>
        <PageHeader title="Case Comments" subtitle="Manage comments for a selected service case." />
        <MessageBar intent="error"><MessageBarBody>You do not have permission to view case comments.</MessageBarBody></MessageBar>
      </div>
    )
  }

  const transitionActions = [
    ...(canResolve ? [{ key: 'resolve', label: 'Resolve Case', onClick: () => void performTransition('resolve') }] : []),
    ...(canClose ? [{ key: 'close', label: 'Close Case', onClick: () => void performTransition('close') }] : []),
    ...(canReopen ? [{ key: 'reopen', label: 'Reopen Case', onClick: () => void performTransition('reopen') }] : []),
  ]

  return (
    <div>
      <PageHeader
        title="Case Comments"
        subtitle={serviceCase ? `${serviceCase.caseNumber} - ${serviceCase.subject}` : 'Manage comments for a selected service case.'}
        actions={[
          { key: 'back', label: 'Back to Cases', onClick: () => navigate('/service/cases'), appearance: 'subtle' },
          { key: 'edit-case', label: 'Edit Case', onClick: () => caseId && navigate(`/service/cases/${caseId}/edit`), appearance: 'secondary' },
          ...transitionActions.map((action) => ({ ...action, appearance: 'secondary' as const })),
        ]}
      />
      <CommandBar actions={[{ key: 'back', label: 'Back to Cases', onClick: () => navigate('/service/cases') }, ...transitionActions]} />

      {serviceCase ? (
        <div className={styles.metricGrid} style={{ marginBottom: 10 }}>
          <div className={styles.metricCard}><p className={styles.metricLabel}>Status</p><p className={styles.metricValue}>{serviceCase.caseStatusName ?? ''}</p></div>
          <div className={styles.metricCard}><p className={styles.metricLabel}>Priority</p><p className={styles.metricValue}>{serviceCase.priorityName ?? ''}</p></div>
          <div className={styles.metricCard}><p className={styles.metricLabel}>Opened At</p><p className={styles.metricValue}>{serviceCase.openedAt}</p></div>
          <div className={styles.metricCard}><p className={styles.metricLabel}>Due At</p><p className={styles.metricValue}>{serviceCase.dueAt ?? ''}</p></div>
        </div>
      ) : null}

      {success ? <MessageBar intent="success" style={{ marginBottom: 10 }}><MessageBarBody>{success}</MessageBarBody></MessageBar> : null}
      {error ? <MessageBar intent="error" style={{ marginBottom: 10 }}><MessageBarBody>{error}</MessageBarBody></MessageBar> : null}

      <RelatedRecordsSubgrid
        title="Case Comments"
        addLabel={canCreate ? 'Add Comment' : undefined}
        onAdd={canCreate ? () => setFormOpen(true) : undefined}
        onRefresh={() => void loadComments()}
        loading={loading}
        error={error}
        hasRows={rows.length > 0}
        emptyMessage="No case comments match the current filters."
        emptyActionLabel={canCreate ? 'Add Comment' : undefined}
        onEmptyAction={canCreate ? () => setFormOpen(true) : undefined}
      >
        <DenseDataGrid
          rows={rows}
          columns={columns}
          loading={loading}
          totalCount={totalCount}
          page={query.page}
          pageSize={query.pageSize}
          search={query.search}
          sort={query.sortBy ? ({ key: query.sortBy as keyof CaseComment, dir: query.sortDir }) : null}
          onPageChange={(page) => setQuery((current) => ({ ...current, page }))}
          onPageSizeChange={(pageSize) => setQuery((current) => ({ ...current, pageSize, page: 1 }))}
          onSearchChange={(search) => setQuery((current) => ({ ...current, search, page: 1 }))}
          onSortChange={(sort: DenseSort<CaseComment> | null) => setQuery((current) => ({ ...current, sortBy: sort ? String(sort.key) : '', sortDir: sort?.dir ?? 'desc', page: 1 }))}
          onDelete={canDelete ? (row) => setDeleteTarget(row) : undefined}
          emptyMessage="No case comments match the current filters."
          activeFilterCount={activeFilterCount}
          filterPanel={
            <>
              <FilterField label="Internal">
                <Dropdown
                  size="small"
                  selectedOptions={[draftFilters.isInternal]}
                  value={draftFilters.isInternal === '' ? 'All' : draftFilters.isInternal === 'true' ? 'Internal' : 'External'}
                  onOptionSelect={(_, data) => setDraftFilters({ isInternal: data.optionValue ?? '' })}
                >
                  <Option value="">All</Option>
                  <Option value="true">Internal</Option>
                  <Option value="false">External</Option>
                </Dropdown>
              </FilterField>
            </>
          }
          onApplyFilters={() => setQuery((current) => ({ ...current, ...draftFilters, page: 1 }))}
          onCancelFilters={() => setDraftFilters({ isInternal: query.isInternal })}
          onClearFilters={() => setDraftFilters({ isInternal: '' })}
        />
      </RelatedRecordsSubgrid>

      <SubgridModalForm
        open={formOpen}
        title="Add Case Comment"
        submitLabel="Add Comment"
        loading={loading}
        onOpenChange={(open) => {
          setFormOpen(open)
          if (!open) {
            setForm(emptyForm)
          }
        }}
        onSubmit={() => void saveComment()}
      >
        <FormSectionCard title="Comment Details">
          <Field label="Comment" required>
            <Input size="small" value={form.commentText} onChange={(_, data) => setForm((current) => ({ ...current, commentText: data.value }))} />
          </Field>
          <Field label="Internal Note">
            <Switch checked={form.isInternal} onChange={(_, data) => setForm((current) => ({ ...current, isInternal: Boolean(data.checked) }))} />
          </Field>
        </FormSectionCard>
      </SubgridModalForm>

      <SubgridDeleteConfirmDialog
        open={Boolean(deleteTarget)}
        title="Delete Case Comment"
        message="Delete this case comment?"
        onConfirm={() => void removeComment()}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  )
}
