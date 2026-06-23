import { useEffect, useMemo, useState } from 'react'
import { MessageBar, MessageBarBody, Spinner } from '@fluentui/react-components'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { DenseDataGrid, statusCell, type DenseColumn, type DenseSort } from '../components/grid/DenseDataGrid'
import { useListQueryState } from '../hooks/useListQueryState'
import { CommandBar } from '../layout/components/CommandBar'
import { PageHeader } from '../layout/components/PageHeader'
import type { Notification, PagedResult } from '../types/models'

type NotificationQuery = {
  page: number
  pageSize: number
  search: string
  sortBy: string
  sortDir: 'asc' | 'desc'
}

export function MyNotificationsPage() {
  const { hasPermission } = useAuth()
  const canView = hasPermission('Notifications.View')
  const canMarkRead = hasPermission('Notifications.MarkRead')
  const [rows, setRows] = useState<Notification[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  const { query, setQuery } = useListQueryState<NotificationQuery>({
    defaults: { page: 1, pageSize: 20, search: '', sortBy: '', sortDir: 'desc' },
    numberKeys: ['page', 'pageSize'],
  })

  const load = async () => {
    if (!canView) {
      return
    }

    setLoading(true)
    setError('')
    try {
      const { data } = await api.get<PagedResult<Notification>>('api/notifications/mine', {
        params: {
          page: query.page,
          pageSize: query.pageSize,
          search: query.search || undefined,
          sortBy: query.sortBy || undefined,
          sortDir: query.sortDir,
        },
      })
      setRows(data.items)
      setTotalCount(data.totalCount)
    } catch {
      setError('Failed to load notifications.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void Promise.resolve().then(load)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [canView, query])

  const markRead = async (row: Notification) => {
    if (!canMarkRead) {
      return
    }

    setLoading(true)
    setError('')
    try {
      await api.post(`api/notifications/${row.id}/mark-read`)
      await load()
    } catch {
      setError('Failed to mark notification as read.')
      setLoading(false)
    }
  }

  const markAllRead = async () => {
    if (!canMarkRead) {
      return
    }

    setLoading(true)
    setError('')
    try {
      await api.post('api/notifications/mine/mark-all-read')
      await load()
    } catch {
      setError('Failed to mark all notifications as read.')
      setLoading(false)
    }
  }

  const columns = useMemo<DenseColumn<Notification>[]>(
    () => [
      { key: 'subject', label: 'Subject', sortable: true },
      { key: 'message', label: 'Message', sortable: true },
      { key: 'statusName', label: 'Status', sortable: true, render: (row) => statusCell(row.statusName) },
      { key: 'priorityName', label: 'Priority', sortable: true, render: (row) => row.priorityName ?? 'Not set' },
      { key: 'channelName', label: 'Channel', sortable: true, render: (row) => row.channelName ?? 'Not set' },
      { key: 'createdAt', label: 'Created At', sortable: true },
      { key: 'readAt', label: 'Read At', sortable: true, render: (row) => row.readAt ?? 'Unread' },
    ],
    [],
  )

  if (!canView) {
    return (
      <div>
        <PageHeader title="My Notifications" subtitle="View and manage your personal notifications." />
        <MessageBar intent="error">
          <MessageBarBody>You do not have permission to view notifications.</MessageBarBody>
        </MessageBar>
      </div>
    )
  }

  return (
    <div>
      <PageHeader title="My Notifications" subtitle="View and manage your personal notifications." />
      <CommandBar actions={canMarkRead ? [{ key: 'mark-all-read', label: 'Mark All As Read', onClick: () => void markAllRead() }] : []} />

      {loading ? <Spinner size="small" label="Loading notifications..." style={{ marginBottom: 10 }} /> : null}
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
        sort={query.sortBy ? ({ key: query.sortBy as keyof Notification, dir: query.sortDir }) : null}
        onPageChange={(page) => setQuery((current) => ({ ...current, page }))}
        onPageSizeChange={(pageSize) => setQuery((current) => ({ ...current, pageSize, page: 1 }))}
        onSearchChange={(search) => setQuery((current) => ({ ...current, search, page: 1 }))}
        onSortChange={(sort: DenseSort<Notification> | null) =>
          setQuery((current) => ({
            ...current,
            sortBy: sort ? String(sort.key) : '',
            sortDir: sort?.dir ?? 'desc',
            page: 1,
          }))
        }
        customActions={canMarkRead ? [{ key: 'mark-read', label: 'Mark Read', onClick: (row: Notification) => void markRead(row) }] : undefined}
        emptyMessage="You have no notifications."
      />
    </div>
  )
}
