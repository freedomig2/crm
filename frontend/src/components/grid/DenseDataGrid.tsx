import { useCallback, useEffect, useMemo, useState } from 'react'
import {
  Button,
  Checkbox,
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
  MoreHorizontalRegular,
} from '@fluentui/react-icons'
import styles from './DenseDataGrid.module.css'
import { StatusBadge } from '../common/StatusBadge'
import { FilterDrawer } from '../filters/FilterDrawer'
import { FilterPopover } from '../filters/FilterPopover'
import { ColumnChooserPopover } from './ColumnChooserPopover'
import { ListCommandBar } from './ListCommandBar'

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
  customActions,
  emptyMessage,
  filterPanel,
  activeFilterCount,
  onApplyFilters,
  onCancelFilters,
  onClearFilters,
  entityKey,
  defaultVisibleColumnKeys,
  requiredColumnKeys = [],
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
  customActions?: Array<{ key: string; label: string; onClick: (row: T) => void; disabled?: (row: T) => boolean }>
  emptyMessage?: string
  filterPanel?: React.ReactNode
  activeFilterCount?: number
  onApplyFilters?: () => void
  onCancelFilters?: () => void
  onClearFilters?: () => void
  entityKey?: string
  defaultVisibleColumnKeys?: string[]
  requiredColumnKeys?: string[]
}) {
  const [search, setSearch] = useState('')
  const [sort, setSort] = useState<DenseSort<T> | null>(null)
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [selected, setSelected] = useState<Record<string, boolean>>({})
  const [searchDraft, setSearchDraft] = useState(controlledSearch ?? '')
  const [filterOpen, setFilterOpen] = useState(false)
  const storageKey = useMemo(() => (entityKey ? `crm.grid.columns.${entityKey}` : ''), [entityKey])

  const resolveDefaultVisibility = useCallback(() => {
    const columnKeys = columns.map((column) => String(column.key))

    const defaults = defaultVisibleColumnKeys && defaultVisibleColumnKeys.length > 0
      ? Object.fromEntries(columnKeys.map((key) => [key, defaultVisibleColumnKeys.includes(key)]))
      : Object.fromEntries(columnKeys.map((key) => [key, true]))

    if (!storageKey) {
      return defaults
    }

    const raw = localStorage.getItem(storageKey)
    if (!raw) {
      return defaults
    }

    try {
      const parsed = JSON.parse(raw) as Record<string, boolean>
      const next = Object.fromEntries(
        columnKeys.map((key) => [
          key,
          requiredColumnKeys.includes(key)
            ? true
            : key in parsed
              ? Boolean(parsed[key])
              : Boolean(defaults[key]),
        ]),
      )

      const hasAny = Object.values(next).some(Boolean)
      return hasAny ? next : defaults
    } catch {
      return defaults
    }
  }, [columns, defaultVisibleColumnKeys, requiredColumnKeys, storageKey])

  const [visibleColumns, setVisibleColumns] = useState<Record<string, boolean>>(resolveDefaultVisibility)

  const effectiveVisibleColumns = useMemo(() => {
    const next = Object.fromEntries(
      columns.map((column) => {
        const key = String(column.key)
        return [key, key in visibleColumns ? visibleColumns[key] : true]
      }),
    ) as Record<string, boolean>

    const hasVisible = columns.some((column) => next[String(column.key)])
    return hasVisible ? next : Object.fromEntries(columns.map((column) => [String(column.key), true]))
  }, [columns, visibleColumns])

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
  const visibleDataColumns = columns.filter((column) => effectiveVisibleColumns[String(column.key)])
  const defaultColumnVisibility = useMemo(
    () =>
      Object.fromEntries(
        columns.map((column) => {
          const key = String(column.key)
          const isDefault = defaultVisibleColumnKeys && defaultVisibleColumnKeys.length > 0
            ? defaultVisibleColumnKeys.includes(key)
            : true
          return [key, requiredColumnKeys.includes(key) ? true : isDefault]
        }),
      ) as Record<string, boolean>,
    [columns, defaultVisibleColumnKeys, requiredColumnKeys],
  )
  const [isMobile, setIsMobile] = useState(() => window.innerWidth <= 900)
  const appliedFilterCount = activeFilterCount ?? 0

  useEffect(() => {
    const onResize = () => setIsMobile(window.innerWidth <= 900)
    window.addEventListener('resize', onResize)
    return () => window.removeEventListener('resize', onResize)
  }, [])

  useEffect(() => {
    const timeout = window.setTimeout(() => {
      if (searchDraft === effectiveSearch) {
        return
      }

      if (isControlled) {
        onSearchChange(searchDraft)
        onPageChange(1)
      } else {
        setSearch(searchDraft)
        setPage(1)
      }
    }, 300)

    return () => window.clearTimeout(timeout)
  }, [effectiveSearch, isControlled, onPageChange, onSearchChange, searchDraft])

  useEffect(() => {
    if (!storageKey) {
      return
    }

    localStorage.setItem(storageKey, JSON.stringify(visibleColumns))
  }, [storageKey, visibleColumns])

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

  const applyFilters = () => {
    onApplyFilters?.()
    setFilterOpen(false)
  }

  const clearFilters = () => {
    onClearFilters?.()
  }

  const cancelFilters = () => {
    onCancelFilters?.()
    setFilterOpen(false)
  }

  const filterActions = (
    <div className={styles.filterActions}>
      <Button size="small" appearance="subtle" onClick={clearFilters}>
        Clear
      </Button>
      <Button size="small" appearance="subtle" onClick={cancelFilters}>
        Cancel
      </Button>
      <Button size="small" appearance="primary" onClick={applyFilters}>
        Apply
      </Button>
    </div>
  )

  const filterContent = filterPanel ? (
    <>
      <div className={styles.filterBody}>{filterPanel}</div>
      {filterActions}
    </>
  ) : (
    <>
      <div className={styles.filterBody}>No additional filters are configured for this list.</div>
      {filterActions}
    </>
  )

  return (
    <div className={styles.wrap}>
      <ListCommandBar
        searchValue={searchDraft}
        onSearchChange={setSearchDraft}
        rightActions={
          <>
            {isMobile ? (
              <FilterDrawer
                open={filterOpen}
                onOpenChange={setFilterOpen}
                onClose={cancelFilters}
                activeCount={appliedFilterCount}
              >
                {filterContent}
              </FilterDrawer>
            ) : (
              <FilterPopover open={filterOpen} onOpenChange={setFilterOpen} activeCount={appliedFilterCount}>
                {filterContent}
              </FilterPopover>
            )}
          <ColumnChooserPopover
            options={columns.map((column) => {
              const key = String(column.key)
              return {
                key,
                label: column.label,
                checked: effectiveVisibleColumns[key],
                disabled: requiredColumnKeys.includes(key),
              }
            })}
            onToggle={(key, nextChecked) => {
              if (requiredColumnKeys.includes(key)) {
                return
              }

              setVisibleColumns((current) => ({
                ...current,
                [key]: nextChecked,
              }))
            }}
            onReset={() => {
              setVisibleColumns(defaultColumnVisibility)

              if (storageKey) {
                localStorage.removeItem(storageKey)
              }
            }}
          />
          <Button size="small" appearance="subtle" icon={<ArrowDownloadRegular />} onClick={exportCsv}>
            Export to Excel
          </Button>
          </>
        }
      />

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
              {visibleDataColumns.map((column) => (
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
                    {visibleDataColumns.map((column, columnIndex) => (
                        <td key={String(column.key)}>
                          {onView && columnIndex === 0 ? (
                            <button
                              type="button"
                              className={styles.cellLink}
                              onClick={() => onView(row)}
                            >
                              {column.render ? column.render(row) : String(row[column.key] ?? '-')}
                            </button>
                          ) : (
                            column.render
                              ? column.render(row)
                              : String(row[column.key] ?? '-')
                          )}
                        </td>
                      ))}
                    <td>
                      <Menu>
                        <MenuTrigger disableButtonEnhancement>
                          <Button
                            size="small"
                            appearance="subtle"
                            icon={<MoreHorizontalRegular />}
                            disabled={!onEdit && !onView && !onDelete && !customActions?.length}
                          />
                        </MenuTrigger>
                        <MenuPopover>
                          <MenuList>
                            {onEdit ? <MenuItem onClick={() => onEdit(row)}>Edit</MenuItem> : null}
                            {onView ? <MenuItem onClick={() => onView(row)}>View Details</MenuItem> : null}
                            {customActions?.map((action) => (
                              <MenuItem key={action.key} onClick={() => action.onClick(row)} disabled={action.disabled?.(row)}>
                                {action.label}
                              </MenuItem>
                            ))}
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

// eslint-disable-next-line react-refresh/only-export-components
export function statusCell(value: string) {
  const lower = value.toLowerCase()
  if (lower.includes('active') || lower.includes('success')) return <StatusBadge label={value} tone="success" />
  if (lower.includes('locked') || lower.includes('failed') || lower.includes('error')) return <StatusBadge label={value} tone="danger" />
  if (lower.includes('pending') || lower.includes('warning')) return <StatusBadge label={value} tone="warning" />
  return <StatusBadge label={value} tone="neutral" />
}
