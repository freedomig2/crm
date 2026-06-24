import { useCallback, useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Dropdown, Input, MessageBar, MessageBarBody, Option, Spinner } from '@fluentui/react-components'
import { api } from '../../api/client'
import { DenseDataGrid, type DenseColumn, type DenseSort } from '../grid/DenseDataGrid'
import { getEntityColumnConfig, getEntityFilters, type EntityFilterDefinition } from '../grid/EntityColumnRegistry'
import { DateRangeFilterField } from '../filters/DateRangeFilterField'
import { FilterField } from '../filters/FilterField'
import { LookupFilterField } from '../filters/LookupFilterField'
import { useListQueryState } from '../../hooks/useListQueryState'
import { CommandBar } from '../../layout/components/CommandBar'
import { PageHeader } from '../../layout/components/PageHeader'
import { useAuth } from '../../auth/AuthContext'
import type { PagedResult } from '../../types/models'
import type { EntityConfig } from './adminConfig'

const EMPTY_FILTER_KEYS: string[] = []

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
  config?: EntityConfig<TItem>
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

  const entityKey = config?.key
  const listFilterKeys = config?.listFilterKeys ?? EMPTY_FILTER_KEYS

  const configuredFilterDefinitions = useMemo(() => {
    const allDefinitions = getEntityFilters(entityKey)
    if (listFilterKeys.length === 0) {
      return allDefinitions
    }

    const allowed = new Set(listFilterKeys)
    return allDefinitions.filter((definition) => {
      if (definition.type === 'dateRange') {
        return allowed.has(definition.fromKey) && allowed.has(definition.toKey)
      }

      return allowed.has(definition.key)
    })
  }, [entityKey, listFilterKeys])

  const filterKeys = useMemo(() => {
    const keys = new Set(listFilterKeys)

    configuredFilterDefinitions.forEach((definition) => {
      if (definition.type === 'dateRange') {
        keys.add(definition.fromKey)
        keys.add(definition.toKey)
        return
      }

      keys.add(definition.key)
    })

    return Array.from(keys)
  }, [configuredFilterDefinitions, listFilterKeys])

  const dateFilterKeys = useMemo(() => {
    const keys = new Set<string>()

    configuredFilterDefinitions.forEach((definition) => {
      if (definition.type === 'dateRange') {
        keys.add(definition.fromKey)
        keys.add(definition.toKey)
      }
    })

    return keys
  }, [configuredFilterDefinitions])

  const queryDefaults = useMemo(() => {
    const next: Record<string, string | number> = {
      page: 1,
      pageSize: 20,
      search: '',
      sortBy: '',
      sortDir: 'asc',
    }

    filterKeys.forEach((key) => {
      next[key] = ''
    })

    return next
  }, [filterKeys])

  const { query, setQuery } = useListQueryState<Record<string, string | number>>({
    defaults: queryDefaults,
    numberKeys: ['page', 'pageSize'] as Array<keyof Record<string, string | number>>,
  })

  const queryPage = Number(query.page ?? 1)
  const queryPageSize = Number(query.pageSize ?? 20)
  const querySearch = String(query.search ?? '')
  const querySortBy = String(query.sortBy ?? '')
  const querySortDir = query.sortDir === 'desc' ? 'desc' : 'asc'

  const readFilterValues = useCallback(
    (source: Record<string, string | number>) =>
      Object.fromEntries(filterKeys.map((key) => [key, String(source[key] ?? '')])) as Record<string, string>,
    [filterKeys],
  )

  const [draftFilters, setDraftFilters] = useState<Record<string, string>>(() => readFilterValues(queryDefaults))

  const toApiFilterValue = useCallback((key: string, value: string): string => {
    if (!dateFilterKeys.has(key)) {
      return value
    }

    if (!/^\d{4}-\d{2}-\d{2}$/.test(value)) {
      return value
    }

    const suffix = key.toLowerCase().endsWith('to') ? 'T23:59:59.999' : 'T00:00:00.000'
    return new Date(`${value}${suffix}`).toISOString()
  }, [dateFilterKeys])

  const activeFilterCount = filterKeys.reduce((count, key) => (String(query[key] ?? '').trim() ? count + 1 : count), 0)

  const columnConfig = useMemo(() => getEntityColumnConfig<TItem>(entityKey, columns), [columns, entityKey])

  useEffect(() => {
    const run = async () => {
      setLoading(true)
      setError('')
      try {
        const filterParams = Object.fromEntries(
          filterKeys.flatMap((key) => {
            const raw = String(query[key] ?? '').trim()
            if (!raw) {
              return []
            }

            return [[key, toApiFilterValue(key, raw)]]
          }),
        )

        const { data } = await api.get<PagedResult<TItem>>(endpoint, {
          params: {
            page: queryPage,
            pageSize: queryPageSize,
            search: querySearch || undefined,
            sortBy: querySortBy || undefined,
            sortDir: querySortDir,
            ...filterParams,
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
  }, [endpoint, filterKeys, query, queryPage, queryPageSize, querySearch, querySortBy, querySortDir, toApiFilterValue])

  const renderFilter = (definition: EntityFilterDefinition) => {
    if (definition.type === 'lookup') {
      return (
        <LookupFilterField
          key={definition.key}
          label={definition.label}
          fieldKey={definition.lookupFieldKey}
          value={draftFilters[definition.key] ?? ''}
          onChange={(value) => setDraftFilters((current) => ({ ...current, [definition.key]: value }))}
        />
      )
    }

    if (definition.type === 'boolean') {
      const value = draftFilters[definition.key] ?? ''
      return (
        <FilterField key={definition.key} label={definition.label}>
          <Dropdown
            size="small"
            selectedOptions={value ? [value] : []}
            value={value === 'true' ? (definition.trueLabel ?? 'Yes') : value === 'false' ? (definition.falseLabel ?? 'No') : ''}
            onOptionSelect={(_, data) => setDraftFilters((current) => ({ ...current, [definition.key]: String(data.optionValue ?? '') }))}
          >
            <Option value="">All</Option>
            <Option value="true">{definition.trueLabel ?? 'Yes'}</Option>
            <Option value="false">{definition.falseLabel ?? 'No'}</Option>
          </Dropdown>
        </FilterField>
      )
    }

    if (definition.type === 'text') {
      return (
        <FilterField key={definition.key} label={definition.label}>
          <Input
            size="small"
            value={draftFilters[definition.key] ?? ''}
            onChange={(_, data) => setDraftFilters((current) => ({ ...current, [definition.key]: data.value }))}
          />
        </FilterField>
      )
    }

    if (definition.type === 'dateRange') {
      return (
        <DateRangeFilterField
          key={`${definition.fromKey}-${definition.toKey}`}
          fromLabel={definition.fromLabel}
          toLabel={definition.toLabel}
          fromValue={draftFilters[definition.fromKey] ?? ''}
          toValue={draftFilters[definition.toKey] ?? ''}
          onFromChange={(value) => setDraftFilters((current) => ({ ...current, [definition.fromKey]: value }))}
          onToChange={(value) => setDraftFilters((current) => ({ ...current, [definition.toKey]: value }))}
        />
      )
    }

    return null
  }

  const clearFilters = Object.fromEntries(filterKeys.map((key) => [key, ''])) as Record<string, string>

  const actions = useMemo(
    () => (canCreate ? [{ key: 'create', label: `Create ${title.replace(/s$/, '')}`, onClick: () => navigate(createPath) }] : []),
    [canCreate, createPath, navigate, title],
  )

  const rowActions = useMemo(
    () =>
      (config?.listRowActions ?? []).map((action) => ({
        key: action.key,
        label: action.label,
        onClick: (row: TItem) => navigate(action.to(row)),
      })),
    [config, navigate],
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
        columns={columnConfig.availableColumns}
        loading={loading}
        totalCount={totalCount}
        page={queryPage}
        pageSize={queryPageSize}
        search={querySearch}
        sort={querySortBy ? ({ key: querySortBy as keyof TItem, dir: querySortDir }) : null}
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
        customActions={rowActions.length > 0 ? rowActions : undefined}
        emptyMessage={emptyMessage}
        entityKey={entityKey}
        defaultVisibleColumnKeys={columnConfig.defaultVisibleColumnKeys}
        requiredColumnKeys={columnConfig.requiredColumnKeys}
        activeFilterCount={activeFilterCount}
        filterPanel={configuredFilterDefinitions.length > 0 ? <>{configuredFilterDefinitions.map(renderFilter)}</> : undefined}
        onApplyFilters={() => setQuery((current) => ({ ...current, ...draftFilters, page: 1 }))}
        onCancelFilters={() => setDraftFilters(readFilterValues(query))}
        onClearFilters={() => setDraftFilters(clearFilters)}
      />
    </div>
  )
}
