import { useMemo, useState } from 'react'
import {
  Button,
  Checkbox,
  Input,
  Menu,
  MenuItem,
  MenuList,
  MenuPopover,
  MenuTrigger,
  Skeleton,
  SkeletonItem,
} from '@fluentui/react-components'
import {
  ArrowDownloadRegular,
  ArrowSortRegular,
  FilterRegular,
  MoreHorizontalRegular,
  SearchRegular,
  SettingsRegular,
} from '@fluentui/react-icons'
import styles from './DenseDataGrid.module.css'
import { StatusBadge } from '../common/StatusBadge'

export type DenseColumn<T> = {
  key: keyof T
  label: string
  sortable?: boolean
  render?: (row: T) => React.ReactNode
}

export type DenseSort<T> = {
  key: keyof T
  dir: 'asc' | 'desc'
}

export function DenseDataGrid<T extends { id: string }>({
  rows,
  columns,
  loading,
  totalCount,
  page: controlledPage,
  pageSize: controlledPageSize,
  search: controlledSearch,
  sort: controlledSort,
  onPageChange,
  onPageSizeChange,
  onSearchChange,
  onSortChange,
  onEdit,
  onView,
  onDelete,
  emptyMessage,
}: {
  rows: T[]
  columns: DenseColumn<T>[]
  loading?: boolean
  totalCount?: number
  page?: number
  pageSize?: number
  search?: string
  sort?: DenseSort<T> | null
  onPageChange?: (page: number) => void
  onPageSizeChange?: (pageSize: number) => void
  onSearchChange?: (value: string) => void
  onSortChange?: (sort: DenseSort<T> | null) => void
  onEdit?: (row: T) => void
  onView?: (row: T) => void
  onDelete?: (row: T) => void
  emptyMessage?: string
}) {
  const [search, setSearch] = useState('')
  const [sort, setSort] = useState<DenseSort<T> | null>(null)
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [selected, setSelected] = useState<Record<string, boolean>>({})
  const [visibleColumns, setVisibleColumns] = useState<Record<string, boolean>>(
    Object.fromEntries(columns.map((column) => [String(column.key), true])),
  )

  const isControlled =
    controlledPage !== undefined &&
    controlledPageSize !== undefined &&
    controlledSearch !== undefined &&
    onPageChange !== undefined &&
    onPageSizeChange !== undefined &&
    onSearchChange !== undefined

  const effectiveSearch = isControlled ? controlledSearch : search
  const effectiveSort = controlledSort ?? sort
  const effectivePage = isControlled ? controlledPage : page
  const effectivePageSize = isControlled ? controlledPageSize : pageSize

  const filtered = useMemo(() => {
    if (isControlled) {
      return rows
    }

    let result = [...rows]

    if (effectiveSearch.trim()) {
      const q = effectiveSearch.toLowerCase()
      result = result.filter((row) =>
        columns.some((column) => String(row[column.key] ?? '').toLowerCase().includes(q)),
      )
    }

    if (effectiveSort) {
      result.sort((a, b) => {
        const av = String(a[effectiveSort.key] ?? '')
        const bv = String(b[effectiveSort.key] ?? '')
        return effectiveSort.dir === 'asc' ? av.localeCompare(bv) : bv.localeCompare(av)
      })
    }

    return result
  }, [rows, effectiveSearch, effectiveSort, columns, isControlled])

  const paged = useMemo(() => {
    if (isControlled) {
      return filtered
    }

    const start = (effectivePage - 1) * effectivePageSize
    return filtered.slice(start, start + effectivePageSize)
  }, [filtered, effectivePage, effectivePageSize, isControlled])

  const totalRows = isControlled ? totalCount ?? rows.length : filtered.length
  const pages = Math.max(1, Math.ceil(totalRows / effectivePageSize))
  const selectedCount = Object.values(selected).filter(Boolean).length

  const toggleSort = (key: keyof T) => {
    const next = !effectiveSort || effectiveSort.key !== key
      ? { key, dir: 'asc' as const }
      : { key, dir: effectiveSort.dir === 'asc' ? 'desc' as const : 'asc' as const }

    if (onSortChange) {
      onSortChange(next)
    } else {
      setSort(next)
    }
  }

  const exportCsv = () => {
    const visible = columns.filter((column) => visibleColumns[String(column.key)])
    const lines = [visible.map((column) => column.label).join(',')]
    filtered.forEach((row) => {
      lines.push(
        visible
          .map((column) => `"${String(row[column.key] ?? '').replaceAll('"', '""')}"`)
          .join(','),
      )
    })
    const blob = new Blob([lines.join('\n')], { type: 'text/csv;charset=utf-8;' })
    const url = URL.createObjectURL(blob)
    const anchor = document.createElement('a')
    anchor.href = url
    anchor.download = 'grid-export.csv'
    anchor.click()
    URL.revokeObjectURL(url)
  }

  return (
    <div className={styles.wrap}>
      <div className={styles.toolbar}>
        <Input
          size="small"
          contentBefore={<SearchRegular />}
          placeholder="Search rows"
          value={effectiveSearch}
          onChange={(_, data) => {
            if (isControlled) {
              onSearchChange(data.value)
              onPageChange(1)
            } else {
              setSearch(data.value)
              setPage(1)
            }
          }}
        />

        <div>
          <Button size="small" appearance="subtle" icon={<FilterRegular />}>
            Filters
          </Button>
          <Menu>
            <MenuTrigger disableButtonEnhancement>
              <Button size="small" appearance="subtle" icon={<SettingsRegular />}>
                Columns
              </Button>
            </MenuTrigger>
            <MenuPopover>
              <MenuList>
                {columns.map((column) => (
                  <MenuItem key={String(column.key)}>
                    <Checkbox
                      label={column.label}
                      checked={visibleColumns[String(column.key)]}
                      onChange={(_, data) =>
                        setVisibleColumns((current) => ({
                          ...current,
                          [String(column.key)]: Boolean(data.checked),
                        }))
                      }
                    />
                  </MenuItem>
                ))}
              </MenuList>
            </MenuPopover>
          </Menu>
          <Button size="small" appearance="subtle" icon={<ArrowDownloadRegular />} onClick={exportCsv}>
            Export to Excel
          </Button>
        </div>
      </div>

      <div className={styles.tableWrap}>
        <table className={styles.table}>
          <thead>
            <tr>
              <th>
                <Checkbox
                  checked={paged.length > 0 && paged.every((row) => selected[row.id])}
                  onChange={(_, data) => {
                    const checked = Boolean(data.checked)
                    const clone = { ...selected }
                    paged.forEach((row) => {
                      clone[row.id] = checked
                    })
                    setSelected(clone)
                  }}
                />
              </th>
              {columns
                .filter((column) => visibleColumns[String(column.key)])
                .map((column) => (
                  <th key={String(column.key)}>
                    <Button
                      size="small"
                      appearance="subtle"
                      icon={column.sortable ? <ArrowSortRegular /> : undefined}
                      onClick={() => column.sortable && toggleSort(column.key)}
                    >
                      {column.label}
                    </Button>
                  </th>
                ))}
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {loading
              ? Array.from({ length: 6 }).map((_, index) => (
                  <tr key={`s-${index}`}>
                    <td>
                      <Skeleton>
                        <SkeletonItem size={16} />
                      </Skeleton>
                    </td>
                    <td colSpan={columns.length + 1}>
                      <Skeleton>
                        <SkeletonItem size={16} />
                      </Skeleton>
                    </td>
                  </tr>
                ))
              : paged.map((row) => (
                  <tr key={row.id} className={styles.rowHover}>
                    <td>
                      <Checkbox
                        checked={Boolean(selected[row.id])}
                        onChange={(_, data) =>
                          setSelected((current) => ({ ...current, [row.id]: Boolean(data.checked) }))
                        }
                      />
                    </td>
                    {columns
                      .filter((column) => visibleColumns[String(column.key)])
                      .map((column) => (
                        <td key={String(column.key)}>
                          {column.render
                            ? column.render(row)
                            : String(row[column.key] ?? '')}
                        </td>
                      ))}
                    <td>
                      <Menu>
                        <MenuTrigger disableButtonEnhancement>
                          <Button
                            size="small"
                            appearance="subtle"
                            icon={<MoreHorizontalRegular />}
                            disabled={!onEdit && !onView && !onDelete}
                          />
                        </MenuTrigger>
                        <MenuPopover>
                          <MenuList>
                            {onEdit ? <MenuItem onClick={() => onEdit(row)}>Edit</MenuItem> : null}
                            {onView ? <MenuItem onClick={() => onView(row)}>View Details</MenuItem> : null}
                            {onDelete ? <MenuItem onClick={() => onDelete(row)}>Delete</MenuItem> : null}
                          </MenuList>
                        </MenuPopover>
                      </Menu>
                    </td>
                  </tr>
                ))}
          </tbody>
        </table>

        {!loading && paged.length === 0 ? <div className={styles.empty}>{emptyMessage ?? 'No data found for current filters.'}</div> : null}
      </div>

      <div className={styles.footer}>
        <div>
          {selectedCount > 0 ? `${selectedCount} selected` : 'No rows selected'}
          {selectedCount > 0 ? (
            <Button size="small" appearance="subtle" style={{ marginLeft: 8 }}>
              Bulk Update
            </Button>
          ) : null}
        </div>

        <div>
          <span style={{ marginRight: 8 }}>Rows</span>
          <select
            value={effectivePageSize}
            onChange={(event) => {
              const nextSize = Number(event.target.value)
              if (isControlled) {
                onPageSizeChange(nextSize)
                onPageChange(1)
              } else {
                setPageSize(nextSize)
                setPage(1)
              }
            }}
          >
            {[10, 20, 50].map((size) => (
              <option key={size} value={size}>{size}</option>
            ))}
          </select>
          <Button
            size="small"
            appearance="subtle"
            onClick={() => (isControlled ? onPageChange(Math.max(1, effectivePage - 1)) : setPage((p) => Math.max(1, p - 1)))}
          >
            Prev
          </Button>
          <span style={{ margin: '0 6px' }}>{effectivePage} / {pages}</span>
          <Button
            size="small"
            appearance="subtle"
            onClick={() => (isControlled ? onPageChange(Math.min(pages, effectivePage + 1)) : setPage((p) => Math.min(pages, p + 1)))}
          >
            Next
          </Button>
        </div>
      </div>
    </div>
  )
}

export function statusCell(value: string) {
  const lower = value.toLowerCase()
  if (lower.includes('active') || lower.includes('success')) return <StatusBadge label={value} tone="success" />
  if (lower.includes('locked') || lower.includes('failed') || lower.includes('error')) return <StatusBadge label={value} tone="danger" />
  if (lower.includes('pending') || lower.includes('warning')) return <StatusBadge label={value} tone="warning" />
  return <StatusBadge label={value} tone="neutral" />
}
