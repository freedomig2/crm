import { useCallback, useEffect, useMemo, useRef, useState, type MouseEvent as ReactMouseEvent } from 'react'
import {
  Button,
  Checkbox,
  Link,
  Skeleton,
  SkeletonItem,
} from '@fluentui/react-components'
import {
  ArrowDownloadRegular,
  ArrowSortRegular,
} from '@fluentui/react-icons'
import styles from './DenseDataGrid.module.css'
import { StatusBadge } from '../common/StatusBadge'
import { FilterDrawer } from '../filters/FilterDrawer'
import { FilterPopover } from '../filters/FilterPopover'
import { ColumnChooserPopover } from './ColumnChooserPopover'
import { ListCommandBar } from './ListCommandBar'
import { EntityBulkActionsBar, type SelectionAction } from './EntityBulkActionsBar'

export type DenseColumn<T> = {
  key: keyof T
  label: string
  sortable?: boolean
  render?: (row: T) => React.ReactNode
  textMaxLength?: number
}

export type DenseSort<T> = {
  key: keyof T
  dir: 'asc' | 'desc'
}

export type DenseGridSelectionState<T> = {
  selectedRowIds: string[]
  selectedRows: T[]
  selectedCount: number
  activeRow: T | null
}

export type DenseCommandAction<T> = {
  key: string
  label: string
  onClick: (rows: T[]) => void | Promise<void>
  disabled?: (rows: T[]) => boolean
  requiresSelection?: 'none' | 'single' | 'multiple' | 'any'
  allowBulk?: boolean
  appearance?: 'primary' | 'secondary' | 'subtle'
}

type GridCellTextProps = {
  value: unknown
  maxLength?: number
}

function shouldTruncateByKey(key: string): boolean {
  const lowerKey = key.toLowerCase()

  if (/(date|time|amount|total|rate|percent|percentage|count|qty|quantity|number|id|code|status|priority)/.test(lowerKey)) {
    return false
  }

  return true
}

function resolveCellTextLength(key: string, value: unknown, explicitMaxLength?: number): number | null {
  if (explicitMaxLength && explicitMaxLength > 0) {
    return explicitMaxLength
  }

  if (value == null) {
    return 40
  }

  if (typeof value === 'number' || typeof value === 'boolean' || value instanceof Date) {
    return null
  }

  if (!shouldTruncateByKey(key)) {
    return null
  }

  const lowerKey = key.toLowerCase()
  if (/(description|notes|note|comment|summary|details|body)/.test(lowerKey)) {
    return 60
  }

  if (/(short|abbr|initial|code|type)/.test(lowerKey)) {
    return 25
  }

  return 40
}

function isActionsColumn<T>(column: DenseColumn<T>): boolean {
  const lower = String(column.label).toLowerCase()
  return /\bactions?\b/.test(lower)
}

export function GridCellText({ value, maxLength = 40 }: GridCellTextProps) {
  const normalized = value == null ? '-' : String(value)
  const display = normalized.length > maxLength ? `${normalized.slice(0, maxLength)}...` : normalized

  return (
    <span className={styles.cellText} title={normalized}>
      {display}
    </span>
  )
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
  commandActions,
  createAction,
  onSelectionChange,
  emptyMessage,
  filterPanel,
  activeFilterCount,
  onApplyFilters,
  onCancelFilters,
  onClearFilters,
  entityKey,
  primaryColumnKey,
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
  customActions?: Array<{ key: string; label: string; onClick: (row: T) => void; disabled?: (row: T) => boolean; allowBulk?: boolean }>
  commandActions?: DenseCommandAction<T>[]
  createAction?: { label: string; onClick: () => void }
  onSelectionChange?: (selection: DenseGridSelectionState<T>) => void
  emptyMessage?: string
  filterPanel?: React.ReactNode
  activeFilterCount?: number
  onApplyFilters?: () => void
  onCancelFilters?: () => void
  onClearFilters?: () => void
  entityKey?: string
  primaryColumnKey?: keyof T
  defaultVisibleColumnKeys?: string[]
  requiredColumnKeys?: string[]
}) {
  const [search, setSearch] = useState('')
  const [sort, setSort] = useState<DenseSort<T> | null>(null)
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [selected, setSelected] = useState<Record<string, boolean>>({})
  const [activeRowId, setActiveRowId] = useState<string | null>(null)
  const [searchDraft, setSearchDraft] = useState(controlledSearch ?? '')
  const [filterOpen, setFilterOpen] = useState(false)
  const [lastClickedRowId, setLastClickedRowId] = useState<string | null>(null)
  const tableRef = useRef<HTMLTableElement | null>(null)
  const rafRef = useRef<number | null>(null)
  const liveWidthsRef = useRef<Record<string, number>>({})
  const resizeRef = useRef<{
    key: string
    startX: number
    startWidth: number
    nextWidth: number
  } | null>(null)
  const listIdentityKey = useMemo(() => {
    if (entityKey) {
      return entityKey
    }

    if (typeof window === 'undefined') {
      return ''
    }

    return window.location.pathname
      .toLowerCase()
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/^-+|-+$/g, '')
  }, [entityKey])
  const storageKey = useMemo(() => (listIdentityKey ? `crm.grid.columns.${listIdentityKey}` : ''), [listIdentityKey])
  const widthStorageKey = useMemo(() => (listIdentityKey ? `crm.grid.widths.${listIdentityKey}` : ''), [listIdentityKey])
  const layoutStorageKey = useMemo(() => (listIdentityKey ? `crm-grid-layout-${listIdentityKey}` : ''), [listIdentityKey])

  const gridColumns = useMemo(() => columns.filter((column) => !isActionsColumn(column)), [columns])

  const resolveDefaultVisibility = useCallback(() => {
    const columnKeys = gridColumns.map((column) => String(column.key))

    const defaults = defaultVisibleColumnKeys && defaultVisibleColumnKeys.length > 0
      ? Object.fromEntries(columnKeys.map((key) => [key, defaultVisibleColumnKeys.includes(key)]))
      : Object.fromEntries(columnKeys.map((key) => [key, true]))

    if (!storageKey) {
      return defaults
    }

    const raw = localStorage.getItem(storageKey)
    const layoutRaw = layoutStorageKey ? localStorage.getItem(layoutStorageKey) : null
    if (layoutRaw) {
      try {
        const parsedLayout = JSON.parse(layoutRaw) as { visibleColumns?: Record<string, boolean> }
        const layoutVisible = parsedLayout.visibleColumns
        if (layoutVisible) {
          const next = Object.fromEntries(
            columnKeys.map((key) => [
              key,
              requiredColumnKeys.includes(key)
                ? true
                : key in layoutVisible
                  ? Boolean(layoutVisible[key])
                  : Boolean(defaults[key]),
            ]),
          )

          const hasAny = Object.values(next).some(Boolean)
          return hasAny ? next : defaults
        }
      } catch {
        // Fall through to legacy key parsing.
      }
    }

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
  }, [defaultVisibleColumnKeys, gridColumns, layoutStorageKey, requiredColumnKeys, storageKey])

  const [visibleColumns, setVisibleColumns] = useState<Record<string, boolean>>(resolveDefaultVisibility)
  const [columnWidths, setColumnWidths] = useState<Record<string, number>>(() => {
    if (!widthStorageKey) {
      return {}
    }

    const raw = localStorage.getItem(widthStorageKey)
    const layoutRaw = layoutStorageKey ? localStorage.getItem(layoutStorageKey) : null
    if (layoutRaw) {
      try {
        const parsedLayout = JSON.parse(layoutRaw) as { columnWidths?: Record<string, number> }
        if (parsedLayout.columnWidths) {
          return parsedLayout.columnWidths
        }
      } catch {
        // Fall through to legacy key parsing.
      }
    }

    if (!raw) {
      return {}
    }

    try {
      return JSON.parse(raw) as Record<string, number>
    } catch {
      return {}
    }
  })

  useEffect(() => {
    liveWidthsRef.current = columnWidths
  }, [columnWidths])

  const effectiveVisibleColumns = useMemo(() => {
    const next = Object.fromEntries(
      gridColumns.map((column) => {
        const key = String(column.key)
        return [key, key in visibleColumns ? visibleColumns[key] : true]
      }),
    ) as Record<string, boolean>

    const hasVisible = gridColumns.some((column) => next[String(column.key)])
    return hasVisible ? next : Object.fromEntries(gridColumns.map((column) => [String(column.key), true]))
  }, [gridColumns, visibleColumns])

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
        gridColumns.some((column) => String(row[column.key] ?? '').toLowerCase().includes(q)),
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
  }, [rows, effectiveSearch, effectiveSort, gridColumns, isControlled])

  const paged = useMemo(() => {
    if (isControlled) {
      return filtered
    }

    const start = (effectivePage - 1) * effectivePageSize
    return filtered.slice(start, start + effectivePageSize)
  }, [filtered, effectivePage, effectivePageSize, isControlled])

  const totalRows = isControlled ? totalCount ?? rows.length : filtered.length
  const pages = Math.max(1, Math.ceil(totalRows / effectivePageSize))
  const selectedRowIds = useMemo(() => Object.keys(selected).filter((id) => selected[id]), [selected])
  const selectedRows = useMemo(() => rows.filter((row) => selected[row.id]), [rows, selected])
  const selectedCount = selectedRows.length
  const activeRow = useMemo(() => rows.find((row) => row.id === activeRowId) ?? null, [activeRowId, rows])
  const visibleDataColumns = gridColumns.filter((column) => effectiveVisibleColumns[String(column.key)])
  const defaultColumnVisibility = useMemo(
    () =>
      Object.fromEntries(
        gridColumns.map((column) => {
          const key = String(column.key)
          const isDefault = defaultVisibleColumnKeys && defaultVisibleColumnKeys.length > 0
            ? defaultVisibleColumnKeys.includes(key)
            : true
          return [key, requiredColumnKeys.includes(key) ? true : isDefault]
        }),
      ) as Record<string, boolean>,
    [defaultVisibleColumnKeys, gridColumns, requiredColumnKeys],
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

  useEffect(() => {
    if (!widthStorageKey) {
      return
    }

    localStorage.setItem(widthStorageKey, JSON.stringify(columnWidths))
  }, [columnWidths, widthStorageKey])

  useEffect(() => {
    if (!layoutStorageKey) {
      return
    }

    localStorage.setItem(layoutStorageKey, JSON.stringify({
      visibleColumns,
      columnWidths,
    }))
  }, [columnWidths, layoutStorageKey, visibleColumns])

  useEffect(() => {
    onSelectionChange?.({
      selectedRowIds,
      selectedRows,
      selectedCount,
      activeRow,
    })
  }, [activeRow, onSelectionChange, selectedCount, selectedRowIds, selectedRows])

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
    const visible = gridColumns.filter((column) => visibleColumns[String(column.key)])
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

  const legacySelectionActions = useMemo(() => {
    const actions: DenseCommandAction<T>[] = []

    if (onView) {
      actions.push({ key: 'view', label: 'View', onClick: (items) => onView(items[0]), requiresSelection: 'single' })
    }

    if (onEdit) {
      actions.push({ key: 'edit', label: 'Edit', onClick: (items) => onEdit(items[0]), requiresSelection: 'single' })
    }

    if (onDelete) {
      actions.push({
        key: 'delete',
        label: 'Delete',
        onClick: (items) => {
          items.forEach((item) => onDelete(item))
        },
        requiresSelection: 'any',
        allowBulk: true,
      })
    }

    if (customActions?.length) {
      customActions.forEach((action) => {
        actions.push({
          key: action.key,
          label: action.label,
          onClick: (items) => {
            if (action.allowBulk) {
              items.forEach((item) => {
                if (!action.disabled?.(item)) {
                  action.onClick(item)
                }
              })
              return
            }

            action.onClick(items[0])
          },
          requiresSelection: action.allowBulk ? 'any' : 'single',
          disabled: (items) => {
            if (items.length === 0) {
              return true
            }

            if (action.allowBulk) {
              return items.every((item) => action.disabled?.(item))
            }

            return Boolean(action.disabled?.(items[0]))
          },
          allowBulk: action.allowBulk,
        })
      })
    }

    return actions
  }, [customActions, onDelete, onEdit, onView])

  const selectionActions: SelectionAction[] = useMemo(() => {
    const source = commandActions ?? legacySelectionActions

    return source.map((action) => ({
      key: action.key,
      label: action.label,
      appearance: action.appearance,
      requiresSelection: action.requiresSelection ?? (action.allowBulk ? 'any' : 'single'),
      onClick: () => {
        void action.onClick(selectedRows)
      },
      disabled: () => Boolean(action.disabled?.(selectedRows)),
    }))
  }, [commandActions, legacySelectionActions, selectedRows])

  const openPrimaryRecord = useMemo(() => {
    if (onView) {
      return (row: T) => {
        onView(row)
      }
    }

    const source = commandActions ?? legacySelectionActions
    const viewAction = source.find((action) => action.key === 'view')
    if (!viewAction) {
      return undefined
    }

    return (row: T) => {
      void viewAction.onClick([row])
    }
  }, [commandActions, legacySelectionActions, onView])

  const applyLiveColumnWidth = (columnKey: string, width: number) => {
    const table = tableRef.current
    if (!table) {
      return
    }

    const selectorKey = CSS.escape(columnKey)
    const col = table.querySelector(`col[data-column-key="${selectorKey}"]`) as HTMLTableColElement | null
    if (col) {
      col.style.width = `${width}px`
    }
  }

  const commitColumnWidth = (columnKey: string, width: number) => {
    setColumnWidths((state) => {
      if (state[columnKey] === width) {
        return state
      }

      return { ...state, [columnKey]: width }
    })
  }

  const queueLiveResize = () => {
    if (rafRef.current != null) {
      return
    }

    rafRef.current = window.requestAnimationFrame(() => {
      rafRef.current = null
      const resize = resizeRef.current
      if (!resize) {
        return
      }

      applyLiveColumnWidth(resize.key, resize.nextWidth)
    })
  }

  function onResizeMouseMove(event: MouseEvent) {
    const resize = resizeRef.current
    if (!resize) {
      return
    }

    const delta = event.clientX - resize.startX
    const minWidth = 120
    const maxWidth = 800
    resize.nextWidth = Math.max(minWidth, Math.min(maxWidth, resize.startWidth + delta))
    queueLiveResize()
  }

  function onResizeMouseUp() {
    stopResize()
  }

  function stopResize() {
    const resize = resizeRef.current
    resizeRef.current = null

    document.body.style.cursor = ''
    document.body.style.userSelect = ''

    if (rafRef.current != null) {
      window.cancelAnimationFrame(rafRef.current)
      rafRef.current = null
    }

    window.removeEventListener('mousemove', onResizeMouseMove)
    window.removeEventListener('mouseup', onResizeMouseUp)

    if (!resize) {
      return
    }

    commitColumnWidth(resize.key, resize.nextWidth)
  }

  // Cleanup is intentionally unmount-only; listeners are attached/removed by resize lifecycle methods.
  // eslint-disable-next-line react-hooks/exhaustive-deps
  useEffect(() => () => {
    resizeRef.current = null
    document.body.style.cursor = ''
    document.body.style.userSelect = ''
    if (rafRef.current != null) {
      window.cancelAnimationFrame(rafRef.current)
      rafRef.current = null
    }
    window.removeEventListener('mousemove', onResizeMouseMove)
    window.removeEventListener('mouseup', onResizeMouseUp)
  }, [])

  const startResize = (columnKey: string, event: ReactMouseEvent<HTMLSpanElement>) => {
    event.preventDefault()
    event.stopPropagation()

    stopResize()

    const startWidth = liveWidthsRef.current[columnKey] ?? 220
    resizeRef.current = {
      key: columnKey,
      startX: event.clientX,
      startWidth,
      nextWidth: startWidth,
    }

    document.body.style.cursor = 'col-resize'
    document.body.style.userSelect = 'none'

    window.addEventListener('mousemove', onResizeMouseMove)
    window.addEventListener('mouseup', onResizeMouseUp)
  }

  const autoFitColumn = (column: DenseColumn<T>) => {
    const key = String(column.key)
    const values = paged.map((row) => String(row[column.key] ?? ''))
    const maxTextLength = values.reduce((max, value) => Math.max(max, value.length), String(column.label).length)
    const padding = 44
    const estimatedWidth = maxTextLength * 8 + padding
    const minWidth = 120
    const maxWidth = 800
    const nextWidth = Math.max(minWidth, Math.min(maxWidth, estimatedWidth))

    applyLiveColumnWidth(key, nextWidth)
    commitColumnWidth(key, nextWidth)
  }

  const isInteractiveTarget = (target: EventTarget | null) => {
    if (!(target instanceof HTMLElement)) {
      return false
    }

    return Boolean(target.closest('a,button,input,textarea,select,[role="button"],[role="checkbox"],label'))
  }

  const setSingleSelection = useCallback((rowId: string) => {
    setSelected({ [rowId]: true })
    setActiveRowId(rowId)
    setLastClickedRowId(rowId)
  }, [])

  const handleRowClick = useCallback((row: T, rowIndex: number, event: ReactMouseEvent<HTMLTableRowElement>) => {
    if (isInteractiveTarget(event.target)) {
      return
    }

    if (event.shiftKey && lastClickedRowId) {
      const currentIndex = rowIndex
      const anchorIndex = paged.findIndex((item) => item.id === lastClickedRowId)
      if (anchorIndex >= 0) {
        const [start, end] = anchorIndex <= currentIndex ? [anchorIndex, currentIndex] : [currentIndex, anchorIndex]
        const next = event.ctrlKey || event.metaKey ? { ...selected } : {}
        for (let index = start; index <= end; index += 1) {
          next[paged[index].id] = true
        }
        setSelected(next)
        setActiveRowId(row.id)
        return
      }
    }

    if (event.ctrlKey || event.metaKey) {
      setSelected((state) => ({ ...state, [row.id]: !state[row.id] }))
      setActiveRowId(row.id)
      setLastClickedRowId(row.id)
      return
    }

    setSingleSelection(row.id)
  }, [lastClickedRowId, paged, selected, setSingleSelection])

  const resolvedPrimaryColumnKey = useMemo(() => {
    if (primaryColumnKey) {
      return String(primaryColumnKey)
    }

    const preferredKeys = [
      'leadNumber',
      'opportunityNumber',
      'accountNumber',
      'fullName',
      'productCode',
      'productName',
      'quoteNumber',
      'orderNumber',
      'invoiceNumber',
      'caseNumber',
      'email',
      'documentName',
      'title',
      'name',
      'id',
    ]

    for (const key of preferredKeys) {
      if (visibleDataColumns.some((column) => String(column.key) === key)) {
        return key
      }
    }

    return visibleDataColumns.length > 0 ? String(visibleDataColumns[0].key) : ''
  }, [primaryColumnKey, visibleDataColumns])

  return (
    <div className={styles.wrap}>
      <EntityBulkActionsBar
        createAction={createAction}
        actions={selectionActions}
        selectedCount={selectedCount}
      />

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
              options={gridColumns.map((column) => {
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
              Export
            </Button>
          </>
        }
      />

      <div className={styles.tableWrap}>
        <table className={styles.table} ref={tableRef}>
          <colgroup>
            <col className={styles.selectionCol} />
            {visibleDataColumns.map((column) => {
              const key = String(column.key)
              return (
                <col
                  key={`col-${key}`}
                  data-column-key={key}
                  style={columnWidths[key] ? { width: `${columnWidths[key]}px` } : undefined}
                />
              )
            })}
          </colgroup>
          <thead>
            <tr>
              <th className={styles.checkboxCell}>
                <div className={styles.checkboxInner}>
                  <Checkbox
                    checked={paged.length > 0 && paged.every((row) => selected[row.id])}
                    onChange={(_, data) => {
                      const checked = Boolean(data.checked)
                      const clone = { ...selected }
                      paged.forEach((row) => {
                        clone[row.id] = checked
                      })
                      setSelected(clone)
                      if (checked && paged.length > 0) {
                        setActiveRowId(paged[0].id)
                        setLastClickedRowId(paged[0].id)
                      }
                    }}
                  />
                </div>
              </th>
              {visibleDataColumns.map((column) => (
                <th key={String(column.key)} className={styles.resizableHeaderCell}>
                  <div className={styles.headerCellInner}>
                    <Button
                      size="small"
                      appearance="subtle"
                      icon={column.sortable ? <ArrowSortRegular /> : undefined}
                      onClick={() => column.sortable && toggleSort(column.key)}
                      className={styles.headerSortButton}
                    >
                      {column.label}
                    </Button>
                    <span
                      role="separator"
                      aria-orientation="vertical"
                      aria-label={`Resize ${column.label}`}
                      className={styles.columnResizeHandle}
                      onMouseDown={(event) => startResize(String(column.key), event)}
                      onDoubleClick={(event) => {
                        event.preventDefault()
                        event.stopPropagation()
                        autoFitColumn(column)
                      }}
                      onClick={(event) => event.stopPropagation()}
                    />
                  </div>
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {loading
              ? Array.from({ length: 6 }).map((_, index) => (
                <tr key={`s-${index}`} className={styles.rowFixed}>
                  <td className={styles.checkboxCell}>
                    <div className={styles.checkboxInner}>
                      <Skeleton>
                        <SkeletonItem size={16} />
                      </Skeleton>
                    </div>
                  </td>
                  <td colSpan={Math.max(1, gridColumns.length)}>
                    <Skeleton>
                      <SkeletonItem size={16} />
                    </Skeleton>
                  </td>
                </tr>
              ))
              : paged.map((row, rowIndex) => (
                <tr
                  key={row.id}
                  className={`${styles.rowHover} ${styles.rowFixed} ${selected[row.id] ? styles.rowSelected : ''} ${activeRowId === row.id ? styles.rowActive : ''}`}
                  onClick={(event) => handleRowClick(row, rowIndex, event)}
                >
                  <td className={styles.checkboxCell}>
                    <div className={styles.checkboxInner}>
                      <Checkbox
                        checked={Boolean(selected[row.id])}
                        onChange={(_, data) => {
                          const checked = Boolean(data.checked)
                          setSelected((current) => ({ ...current, [row.id]: checked }))
                          if (checked) {
                            setActiveRowId(row.id)
                            setLastClickedRowId(row.id)
                          }
                        }}
                      />
                    </div>
                  </td>
                  {visibleDataColumns.map((column) => (
                    <td key={String(column.key)} className={styles.dataCell}>
                      {openPrimaryRecord && String(column.key) === resolvedPrimaryColumnKey ? (
                        <Link
                          className={styles.cellLink}
                          onClick={(event) => {
                            event.preventDefault()
                            event.stopPropagation()
                            openPrimaryRecord(row)
                          }}
                        >
                          {column.render
                            ? column.render(row)
                            : (
                              <GridCellText
                                value={row[column.key]}
                                maxLength={resolveCellTextLength(String(column.key), row[column.key], column.textMaxLength) ?? 40}
                              />
                            )}
                        </Link>
                      ) : (
                        column.render
                          ? column.render(row)
                          : (
                            <GridCellText
                              value={row[column.key]}
                              maxLength={resolveCellTextLength(String(column.key), row[column.key], column.textMaxLength) ?? 40}
                            />
                          )
                      )}
                    </td>
                  ))}
                </tr>
              ))}
          </tbody>
        </table>

        {!loading && paged.length === 0 ? <div className={styles.empty}>{emptyMessage ?? 'No data found for current filters.'}</div> : null}
      </div>

      <div className={styles.footer}>
        <div>{selectedCount > 0 ? `${selectedCount} records selected` : 'No records selected'}</div>

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
