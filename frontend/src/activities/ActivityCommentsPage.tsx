import { Button, Dropdown, Field, Input, MessageBar, MessageBarBody, Option, Spinner, Switch } from '@fluentui/react-components'
import { useEffect, useMemo, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { DeleteConfirmDialog } from '../components/crud/DeleteConfirmDialog'
import { FormSectionCard } from '../components/entity-ui/EntityComponents'
import { FilterField } from '../components/filters/FilterField'
import { DenseDataGrid, type DenseColumn, type DenseSort } from '../components/grid/DenseDataGrid'
import { useListQueryState } from '../hooks/useListQueryState'
import { CommandBar } from '../layout/components/CommandBar'
import { PageHeader } from '../layout/components/PageHeader'
import type { Activity, ActivityComment, PagedResult } from '../types/models'
import styles from '../sales/Sales.module.css'

type ActivityCommentQuery = {
  page: number
  pageSize: number
  search: string
  sortBy: string
  sortDir: 'asc' | 'desc'
  isInternal: string
}

type ActivityCommentForm = {
  commentText: string
  isInternal: boolean
}

const emptyForm: ActivityCommentForm = {
  commentText: '',
  isInternal: false,
}

export function ActivityCommentsPage() {
  const navigate = useNavigate()
  const { id: activityId } = useParams()
  const { hasPermission } = useAuth()
  const canViewActivities = hasPermission('Activities.View')
  const canView = hasPermission('ActivityComments.View')
  const canCreate = hasPermission('ActivityComments.Create')
  const canDelete = hasPermission('ActivityComments.Delete')
  const canComplete = hasPermission('Activities.Complete')

  const [activity, setActivity] = useState<Activity | null>(null)
  const [rows, setRows] = useState<ActivityComment[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [deleteTarget, setDeleteTarget] = useState<ActivityComment | null>(null)
  const [form, setForm] = useState<ActivityCommentForm>(emptyForm)

  const defaultQuery: ActivityCommentQuery = {
    page: 1,
    pageSize: 20,
    search: '',
    sortBy: '',
    sortDir: 'desc',
    isInternal: '',
  }

  const { query, setQuery } = useListQueryState<ActivityCommentQuery>({ defaults: defaultQuery, numberKeys: ['page', 'pageSize'] })
  const [draftFilters, setDraftFilters] = useState<Pick<ActivityCommentQuery, 'isInternal'>>({
    isInternal: query.isInternal,
  })

  const loadActivity = async () => {
    if (!activityId || !canViewActivities) {
      return
    }

    try {
      const { data } = await api.get<Activity>(`api/activities/${activityId}`)
      setActivity(data)
    } catch {
      setActivity(null)
    }
  }

  const loadComments = async () => {
    if (!activityId || !canView) {
      return
    }

    setLoading(true)
    setError('')
    try {
      const { data } = await api.get<PagedResult<ActivityComment>>(`api/activities/${activityId}/comments`, {
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
      setError('Failed to load activity comments.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void Promise.resolve().then(async () => {
      await Promise.all([loadActivity(), loadComments()])
    })
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [activityId, canView, canViewActivities, query])

  const saveComment = async () => {
    if (!activityId || !canCreate) {
      return
    }

    if (!form.commentText.trim()) {
      setError('Comment text is required.')
      return
    }

    setLoading(true)
    setError('')
    try {
      await api.post(`api/activities/${activityId}/comments`, {
        commentText: form.commentText.trim(),
        isInternal: form.isInternal,
      })

      setForm(emptyForm)
      await loadComments()
    } catch {
      setError('Failed to add activity comment.')
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
    try {
      await api.delete(`api/activities/comments/${deleteTarget.id}`)
      setDeleteTarget(null)
      await loadComments()
    } catch {
      setError('Failed to delete activity comment.')
    } finally {
      setLoading(false)
    }
  }

  const completeActivity = async () => {
    if (!activityId || !canComplete) {
      return
    }

    setLoading(true)
    setError('')
    try {
      await api.post(`api/activities/${activityId}/complete`, {})
      await loadActivity()
    } catch {
      setError('Failed to complete activity.')
    } finally {
      setLoading(false)
    }
  }

  const activeFilterCount = [query.isInternal].filter(Boolean).length

  const columns = useMemo<DenseColumn<ActivityComment>[]>(
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
        <PageHeader title="Activity Comments" subtitle="Manage comments for a selected activity task." />
        <MessageBar intent="error"><MessageBarBody>You do not have permission to view activity comments.</MessageBarBody></MessageBar>
      </div>
    )
  }

  return (
    <div>
      <PageHeader
        title="Activity Comments"
        subtitle={activity ? `${activity.activityNumber} - ${activity.subject}` : 'Manage comments for a selected activity task.'}
        actions={[
          { key: 'back', label: 'Back to Tasks', onClick: () => navigate('/activities/tasks'), appearance: 'subtle' },
          { key: 'edit-activity', label: 'Edit Task', onClick: () => activityId && navigate(`/activities/tasks/${activityId}/edit`), appearance: 'secondary' },
          ...(canComplete ? [{ key: 'complete-activity', label: 'Complete Task', onClick: () => void completeActivity(), appearance: 'secondary' as const }] : []),
        ]}
      />
      <CommandBar actions={[
        { key: 'back', label: 'Back to Tasks', onClick: () => navigate('/activities/tasks') },
        ...(canComplete ? [{ key: 'complete-activity', label: 'Complete Task', onClick: () => void completeActivity() }] : []),
      ]}
      />

      {activity ? (
        <div className={styles.metricGrid} style={{ marginBottom: 10 }}>
          <div className={styles.metricCard}><p className={styles.metricLabel}>Status</p><p className={styles.metricValue}>{activity.statusName ?? ''}</p></div>
          <div className={styles.metricCard}><p className={styles.metricLabel}>Type</p><p className={styles.metricValue}>{activity.activityTypeName ?? ''}</p></div>
          <div className={styles.metricCard}><p className={styles.metricLabel}>Activity Date</p><p className={styles.metricValue}>{activity.activityDate}</p></div>
          <div className={styles.metricCard}><p className={styles.metricLabel}>Due Date</p><p className={styles.metricValue}>{activity.dueDate ?? ''}</p></div>
        </div>
      ) : null}

      {canCreate ? (
        <FormSectionCard title="Add Activity Comment">
          <Field label="Comment" required>
            <Input size="small" value={form.commentText} onChange={(_, data) => setForm((current) => ({ ...current, commentText: data.value }))} />
          </Field>
          <Field label="Internal Note">
            <Switch checked={form.isInternal} onChange={(_, data) => setForm((current) => ({ ...current, isInternal: Boolean(data.checked) }))} />
          </Field>
          <div className={styles.inlineActions}>
            <Button size="small" appearance="primary" disabled={loading} onClick={() => void saveComment()}>Add Comment</Button>
            <Button size="small" appearance="subtle" disabled={loading} onClick={() => setForm(emptyForm)}>Reset</Button>
          </div>
        </FormSectionCard>
      ) : null}

      {loading ? <Spinner size="small" label="Loading activity comments..." style={{ margin: '8px 0' }} /> : null}
      {error ? <MessageBar intent="error" style={{ marginBottom: 10 }}><MessageBarBody>{error}</MessageBarBody></MessageBar> : null}

      <DenseDataGrid
        rows={rows}
        columns={columns}
        loading={loading}
        totalCount={totalCount}
        page={query.page}
        pageSize={query.pageSize}
        search={query.search}
        sort={query.sortBy ? ({ key: query.sortBy as keyof ActivityComment, dir: query.sortDir }) : null}
        onPageChange={(page) => setQuery((current) => ({ ...current, page }))}
        onPageSizeChange={(pageSize) => setQuery((current) => ({ ...current, pageSize, page: 1 }))}
        onSearchChange={(search) => setQuery((current) => ({ ...current, search, page: 1 }))}
        onSortChange={(sort: DenseSort<ActivityComment> | null) => setQuery((current) => ({ ...current, sortBy: sort ? String(sort.key) : '', sortDir: sort?.dir ?? 'desc', page: 1 }))}
        onDelete={canDelete ? (row) => setDeleteTarget(row) : undefined}
        emptyMessage="No activity comments match the current filters."
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

      <DeleteConfirmDialog
        open={Boolean(deleteTarget)}
        title="Delete Activity Comment"
        message="Delete this activity comment?"
        onConfirm={() => void removeComment()}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  )
}
