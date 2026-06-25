import { useEffect, useState } from 'react'
import { Dropdown, MessageBar, MessageBarBody, Option, Spinner } from '@fluentui/react-components'
import { useNavigate } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { DeleteConfirmDialog } from '../components/crud/DeleteConfirmDialog'
import { DateRangeFilterField } from '../components/filters/DateRangeFilterField'
import { FilterField } from '../components/filters/FilterField'
import { LookupFilterField } from '../components/filters/LookupFilterField'
import { loadLookupOptionsByCategoryCode } from '../components/entity-ui/referenceData'
import { DenseDataGrid, statusCell, type DenseColumn, type DenseCommandAction, type DenseSort } from '../components/grid/DenseDataGrid'
import { useListQueryState } from '../hooks/useListQueryState'
import { PageHeader } from '../layout/components/PageHeader'
import type { Lead, PagedResult } from '../types/models'
import { formatCurrency, formatDate, formatDateTime } from './leadUtils'

type LeadQuery = {
  page: number
  pageSize: number
  search: string
  sortBy: string
  sortDir: 'asc' | 'desc'
  leadStatusId: string
  qualificationStatusId: string
  ratingId: string
  leadSourceId: string
  ownerUserId: string
  assignedToUserId: string
  createdFrom: string
  createdTo: string
  isActive: string
}

export function LeadsListPage() {
  const navigate = useNavigate()
  const { hasPermission, user } = useAuth()
  const canView = hasPermission('Leads.View')
  const canCreate = hasPermission('Leads.Create')
  const canEdit = hasPermission('Leads.Update')
  const canDelete = hasPermission('Leads.Delete')
  const canAssign = hasPermission('Leads.Assign')
  const canQualify = hasPermission('Leads.Qualify')
  const canDisqualify = hasPermission('Leads.Disqualify')
  const canConvert = hasPermission('Leads.Convert')
  const canScore = hasPermission('Leads.Score')
  const [rows, setRows] = useState<Lead[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [deleteTarget, setDeleteTarget] = useState<Lead | null>(null)
  const defaultQuery: LeadQuery = {
    page: 1,
    pageSize: 20,
    search: '',
    sortBy: '',
    sortDir: 'asc',
    leadStatusId: '',
    qualificationStatusId: '',
    ratingId: '',
    leadSourceId: '',
    ownerUserId: '',
    assignedToUserId: '',
    createdFrom: '',
    createdTo: '',
    isActive: '',
  }
  const { query, setQuery } = useListQueryState<LeadQuery>({ defaults: defaultQuery, numberKeys: ['page', 'pageSize'] })
  const [draftFilters, setDraftFilters] = useState<Pick<LeadQuery, 'leadStatusId' | 'qualificationStatusId' | 'ratingId' | 'leadSourceId' | 'ownerUserId' | 'assignedToUserId' | 'createdFrom' | 'createdTo' | 'isActive'>>({
    leadStatusId: query.leadStatusId,
    qualificationStatusId: query.qualificationStatusId,
    ratingId: query.ratingId,
    leadSourceId: query.leadSourceId,
    ownerUserId: query.ownerUserId,
    assignedToUserId: query.assignedToUserId,
    createdFrom: query.createdFrom,
    createdTo: query.createdTo,
    isActive: query.isActive,
  })

  const load = async () => {
    if (!canView) {
      return
    }

    setLoading(true)
    setError('')
    try {
      const { data } = await api.get<PagedResult<Lead>>('api/leads', {
        params: {
          page: query.page,
          pageSize: query.pageSize,
          search: query.search || undefined,
          sortBy: query.sortBy || undefined,
          sortDir: query.sortDir,
          leadStatusId: query.leadStatusId || undefined,
          qualificationStatusId: query.qualificationStatusId || undefined,
          ratingId: query.ratingId || undefined,
          leadSourceId: query.leadSourceId || undefined,
          ownerUserId: query.ownerUserId || undefined,
          assignedToUserId: query.assignedToUserId || undefined,
          createdFrom: query.createdFrom || undefined,
          createdTo: query.createdTo || undefined,
          isActive: query.isActive || undefined,
        },
      })
      setRows(data.items)
      setTotalCount(data.totalCount)
    } catch {
      setError('Failed to load leads.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void Promise.resolve().then(load)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [canView, query])

  const activeFilterCount = [
    query.leadStatusId,
    query.qualificationStatusId,
    query.ratingId,
    query.leadSourceId,
    query.ownerUserId,
    query.assignedToUserId,
    query.createdFrom,
    query.createdTo,
    query.isActive,
  ].filter(Boolean).length

  const refresh = () => setQuery((current) => ({ ...current }))

  const deleteLead = async () => {
    if (!deleteTarget) {
      return
    }

    setLoading(true)
    setError('')
    try {
      await api.delete(`api/leads/${deleteTarget.id}`)
      setDeleteTarget(null)
      refresh()
    } catch {
      setError('Failed to delete lead.')
    } finally {
      setLoading(false)
    }
  }

  const assignToMe = async (lead: Lead) => {
    if (!user?.id) {
      setError('Current user could not be resolved.')
      return
    }

    setError('')
    try {
      await api.post(`api/leads/${lead.id}/assign`, { assignedToUserId: user.id, assignedToTeamId: null })
      refresh()
    } catch {
      setError('Failed to assign lead.')
    }
  }

  const qualify = async (lead: Lead) => {
    setError('')
    try {
      await api.post(`api/leads/${lead.id}/qualify`)
      refresh()
    } catch {
      setError('Failed to qualify lead.')
    }
  }

  const disqualify = async (lead: Lead) => {
    setError('')
    try {
      const reasons = await loadLookupOptionsByCategoryCode('LEAD_DISQUALIFICATION_REASON')
      const reason = reasons.find((item) => item.label.toLowerCase().includes('other')) ?? reasons[0]
      if (!reason) {
        setError('No disqualification reason lookup values are configured.')
        return
      }
      await api.post(`api/leads/${lead.id}/disqualify`, { disqualifiedReasonId: reason.value })
      refresh()
    } catch {
      setError('Failed to disqualify lead.')
    }
  }

  const calculateScore = async (lead: Lead) => {
    setError('')
    try {
      await api.post(`api/leads/${lead.id}/calculate-score`)
      refresh()
    } catch {
      setError('Failed to calculate lead score.')
    }
  }

  const columns: DenseColumn<Lead>[] = [
    { key: 'leadNumber', label: 'Lead Number', sortable: true },
    { key: 'topic', label: 'Topic', sortable: true },
    { key: 'fullName', label: 'Full Name', sortable: true, render: (row) => row.fullName || 'Not set' },
    { key: 'companyName', label: 'Company Name', sortable: true, render: (row) => row.companyName || 'Not set' },
    { key: 'leadSourceName', label: 'Lead Source', sortable: true, render: (row) => row.leadSourceName || 'Not set' },
    { key: 'leadStatusName', label: 'Status', sortable: true, render: (row) => statusCell(row.leadStatusName ?? 'Not set') },
    { key: 'qualificationStatusName', label: 'Qualification Status', sortable: true, render: (row) => row.qualificationStatusName || 'Not set' },
    { key: 'ratingName', label: 'Rating', sortable: true, render: (row) => statusCell(row.ratingName ?? 'Not set') },
    { key: 'score', label: 'Score', sortable: true },
    { key: 'scoreGrade', label: 'Score Grade', sortable: true, render: (row) => statusCell(row.scoreGrade ?? 'Cold') },
    { key: 'ownerUserName', label: 'Owner', sortable: true, render: (row) => row.ownerUserName ?? row.ownerTeamName ?? 'Not set' },
    { key: 'assignedToUserName', label: 'Assigned To', sortable: true, render: (row) => row.assignedToUserName ?? row.assignedToTeamName ?? 'Not set' },
    { key: 'estimatedValue', label: 'Estimated Value', sortable: true, render: (row) => formatCurrency(row.estimatedValue) || 'Not set' },
    { key: 'estimatedCloseDate', label: 'Estimated Close Date', sortable: true, render: (row) => formatDate(row.estimatedCloseDate) || 'Not set' },
    { key: 'createdAt', label: 'Created At', sortable: true, render: (row) => formatDateTime(row.createdAt) },
  ]

  const commandActions: DenseCommandAction<Lead>[] = [
    { key: 'view', label: 'View', onClick: (items) => navigate(`/leads/${items[0].id}`), requiresSelection: 'single' },
    ...(canEdit ? [{ key: 'edit', label: 'Edit', onClick: (items: Lead[]) => navigate(`/leads/${items[0].id}/edit`), requiresSelection: 'single' as const }] : []),
    ...(canDelete ? [{ key: 'delete', label: 'Delete', onClick: (items: Lead[]) => setDeleteTarget(items[0]), requiresSelection: 'single' as const }] : []),
    {
      key: 'assign',
      label: 'Assign to Me',
      onClick: (items) => {
        items.forEach((item) => {
          void assignToMe(item)
        })
      },
      requiresSelection: 'any',
      allowBulk: true,
      disabled: () => !canAssign,
    },
    {
      key: 'qualify',
      label: 'Qualify',
      onClick: (items) => {
        items.forEach((item) => {
          void qualify(item)
        })
      },
      requiresSelection: 'any',
      allowBulk: true,
      disabled: (items) => !canQualify || items.some((item) => item.leadStatusName === 'Converted'),
    },
    {
      key: 'disqualify',
      label: 'Disqualify',
      onClick: (items) => {
        items.forEach((item) => {
          void disqualify(item)
        })
      },
      requiresSelection: 'any',
      allowBulk: true,
      disabled: (items) => !canDisqualify || items.some((item) => item.leadStatusName === 'Converted'),
    },
    {
      key: 'convert',
      label: 'Convert',
      onClick: (items) => navigate(`/leads/${items[0].id}/convert`),
      requiresSelection: 'single',
      disabled: (items) => !canConvert || items[0]?.leadStatusName !== 'Qualified',
    },
    {
      key: 'score',
      label: 'Score',
      onClick: (items) => {
        items.forEach((item) => {
          void calculateScore(item)
        })
      },
      requiresSelection: 'any',
      allowBulk: true,
      disabled: () => !canScore,
    },
  ]

  if (!canView) {
    return (
      <div>
        <PageHeader title="Leads" subtitle="Capture, qualify, score, assign, and convert sales leads." />
        <MessageBar intent="error">
          <MessageBarBody>You do not have permission to view leads.</MessageBarBody>
        </MessageBar>
      </div>
    )
  }

  return (
    <div>
      <PageHeader
        title="Leads"
        subtitle="Capture, qualify, score, assign, and convert sales leads."
      />

      {loading ? <Spinner size="small" label="Loading leads..." style={{ margin: '8px 0' }} /> : null}
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
        sort={query.sortBy ? ({ key: query.sortBy as keyof Lead, dir: query.sortDir }) : null}
        onPageChange={(page) => setQuery((current) => ({ ...current, page }))}
        onPageSizeChange={(pageSize) => setQuery((current) => ({ ...current, pageSize, page: 1 }))}
        onSearchChange={(search) => setQuery((current) => ({ ...current, search, page: 1 }))}
        onSortChange={(sort: DenseSort<Lead> | null) =>
          setQuery((current) => ({
            ...current,
            sortBy: sort ? String(sort.key) : '',
            sortDir: sort?.dir ?? 'asc',
            page: 1,
          }))
        }
        createAction={canCreate ? { label: 'New Lead', onClick: () => navigate('/leads/create') } : undefined}
        commandActions={commandActions}
        emptyMessage="No leads match the current filters."
        activeFilterCount={activeFilterCount}
        filterPanel={
          <>
            <LookupFilterField label="Status" fieldKey="leadStatusId" value={draftFilters.leadStatusId} onChange={(value) => setDraftFilters((current) => ({ ...current, leadStatusId: value }))} />
            <LookupFilterField label="Qualification" fieldKey="qualificationStatusId" value={draftFilters.qualificationStatusId} onChange={(value) => setDraftFilters((current) => ({ ...current, qualificationStatusId: value }))} />
            <LookupFilterField label="Rating" fieldKey="ratingId" value={draftFilters.ratingId} onChange={(value) => setDraftFilters((current) => ({ ...current, ratingId: value }))} />
            <LookupFilterField label="Lead Source" fieldKey="leadSourceId" value={draftFilters.leadSourceId} onChange={(value) => setDraftFilters((current) => ({ ...current, leadSourceId: value }))} />
            <LookupFilterField label="Owner" fieldKey="ownerUserId" value={draftFilters.ownerUserId} onChange={(value) => setDraftFilters((current) => ({ ...current, ownerUserId: value }))} />
            <LookupFilterField label="Assigned User" fieldKey="assignedToUserId" value={draftFilters.assignedToUserId} onChange={(value) => setDraftFilters((current) => ({ ...current, assignedToUserId: value }))} />
            <DateRangeFilterField
              fromLabel="Created From"
              toLabel="Created To"
              fromValue={draftFilters.createdFrom}
              toValue={draftFilters.createdTo}
              onFromChange={(value) => setDraftFilters((current) => ({ ...current, createdFrom: value }))}
              onToChange={(value) => setDraftFilters((current) => ({ ...current, createdTo: value }))}
            />
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
            leadStatusId: query.leadStatusId,
            qualificationStatusId: query.qualificationStatusId,
            ratingId: query.ratingId,
            leadSourceId: query.leadSourceId,
            ownerUserId: query.ownerUserId,
            assignedToUserId: query.assignedToUserId,
            createdFrom: query.createdFrom,
            createdTo: query.createdTo,
            isActive: query.isActive,
          })
        }
        onClearFilters={() =>
          setDraftFilters({
            leadStatusId: '',
            qualificationStatusId: '',
            ratingId: '',
            leadSourceId: '',
            ownerUserId: '',
            assignedToUserId: '',
            createdFrom: '',
            createdTo: '',
            isActive: '',
          })
        }
      />

      <DeleteConfirmDialog
        open={Boolean(deleteTarget)}
        title="Delete Lead"
        message={`Delete ${deleteTarget?.topic ?? 'this lead'}?`}
        onConfirm={() => void deleteLead()}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  )
}
