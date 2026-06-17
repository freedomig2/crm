import { useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { MessageBar, MessageBarBody, Spinner } from '@fluentui/react-components'
import { api } from '../../api/client'
import { DenseDataGrid, type DenseColumn, type DenseSort } from '../grid/DenseDataGrid'
import { CommandBar } from '../../layout/components/CommandBar'
import { PageHeader } from '../../layout/components/PageHeader'
import { useAuth } from '../../auth/AuthContext'
import type { PagedResult } from '../../types/models'

export function EntityListPage<TItem extends { id: string }>({
  config,
  title,
  subtitle,
  endpoint,
  columns,
  listPath,
  createPath,
  detailsPath,
  editPath,
  permissions,
  emptyMessage,
}: {
  config?: unknown
  title: string
  subtitle?: string
  endpoint: string
  columns: DenseColumn<TItem>[]
  listPath: string
  createPath: string
  detailsPath: (id: string) => string
  editPath: (id: string) => string
  permissions: { view: string; create?: string; update?: string; delete?: string }
  emptyMessage?: string
}) {
  void config
  void listPath
  const navigate = useNavigate()
  const { hasPermission } = useAuth()
  const canCreate = permissions.create ? hasPermission(permissions.create) : false
  const canEdit = permissions.update ? hasPermission(permissions.update) : false
  const canDelete = permissions.delete ? hasPermission(permissions.delete) : false

  const [rows, setRows] = useState<TItem[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [query, setQuery] = useState({ page: 1, pageSize: 20, search: '', sortBy: '', sortDir: 'asc' as 'asc' | 'desc' })

  useEffect(() => {
    const run = async () => {
      setLoading(true)
      setError('')
      try {
        const { data } = await api.get<PagedResult<TItem>>(endpoint, {
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
        setError('Failed to load records.')
      } finally {
        setLoading(false)
      }
    }

    void run()
  }, [endpoint, query.page, query.pageSize, query.search, query.sortBy, query.sortDir])

  const actions = useMemo(
    () => (canCreate ? [{ key: 'create', label: `Create ${title.replace(/s$/, '')}`, onClick: () => navigate(createPath) }] : []),
    [canCreate, createPath, navigate, title],
  )

  if (!hasPermission(permissions.view)) {
    return (
      <div>
        <PageHeader title={title} subtitle={subtitle} />
        <MessageBar intent="error">
          <MessageBarBody>You do not have permission to view this page.</MessageBarBody>
        </MessageBar>
      </div>
    )
  }

  return (
    <div>
      <PageHeader title={title} subtitle={subtitle} quickAction={canCreate ? `Create ${title.replace(/s$/, '')}` : undefined} onQuickAction={canCreate ? () => navigate(createPath) : undefined} />
      <CommandBar actions={actions} />

      {loading ? <Spinner size="small" label="Loading..." style={{ margin: '10px 0' }} /> : null}
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
        sort={query.sortBy ? ({ key: query.sortBy as keyof TItem, dir: query.sortDir }) : null}
        onPageChange={(page) => setQuery((current) => ({ ...current, page }))}
        onPageSizeChange={(pageSize) => setQuery((current) => ({ ...current, pageSize, page: 1 }))}
        onSearchChange={(search) => setQuery((current) => ({ ...current, search, page: 1 }))}
        onSortChange={(sort: DenseSort<TItem> | null) =>
          setQuery((current) => ({
            ...current,
            sortBy: sort ? String(sort.key) : '',
            sortDir: sort?.dir ?? 'asc',
            page: 1,
          }))
        }
        onView={(row) => navigate(detailsPath(row.id))}
        onEdit={canEdit ? (row) => navigate(editPath(row.id)) : undefined}
        onDelete={canDelete ? (row) => navigate(editPath(row.id)) : undefined}
        emptyMessage={emptyMessage}
      />
    </div>
  )
}
