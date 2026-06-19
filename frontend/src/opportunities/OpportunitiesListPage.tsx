import { useEffect, useMemo, useState } from 'react'
import { Dropdown, Field, Input, MessageBar, MessageBarBody, Option, Spinner } from '@fluentui/react-components'
import { useNavigate } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { DeleteConfirmDialog } from '../components/crud/DeleteConfirmDialog'
import { LookupCombobox } from '../components/entity-ui/EntityComponents'
import { DenseDataGrid, statusCell, type DenseColumn, type DenseSort } from '../components/grid/DenseDataGrid'
import { CommandBar } from '../layout/components/CommandBar'
import { PageHeader } from '../layout/components/PageHeader'
import type { Opportunity, PagedResult } from '../types/models'
import { formatCurrency, formatDate } from './opportunityUtils'
import styles from '../contacts/Contacts.module.css'

type OpportunityQuery = {
  page: number
  pageSize: number
  search: string
  sortBy: string
  sortDir: 'asc' | 'desc'
  accountId: string
  opportunityStageId: string
  opportunityStatusId: string
  ratingId: string
  sourceId: string
  ownerUserId: string
  estimatedCloseFrom: string
  estimatedCloseTo: string
  minRevenue: string
  maxRevenue: string
  isActive: string
}

export function OpportunitiesListPage() {
  const navigate = useNavigate()
  const { hasPermission, user } = useAuth()
  const canView = hasPermission('Opportunities.View')
  const canCreate = hasPermission('Opportunities.Create')
  const canEdit = hasPermission('Opportunities.Update')
  const canDelete = hasPermission('Opportunities.Delete')
  const canAssign = hasPermission('Opportunities.AssignOwner')
  const canMarkWon = hasPermission('Opportunities.MarkWon')
  const canMarkLost = hasPermission('Opportunities.MarkLost')
  const canViewPipeline = hasPermission('Opportunities.ViewPipeline')
  const [rows, setRows] = useState<Opportunity[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [deleteTarget, setDeleteTarget] = useState<Opportunity | null>(null)
  const [query, setQuery] = useState<OpportunityQuery>({
    page: 1,
    pageSize: 20,
    search: '',
    sortBy: '',
    sortDir: 'asc',
    accountId: '',
    opportunityStageId: '',
    opportunityStatusId: '',
    ratingId: '',
    sourceId: '',
    ownerUserId: '',
    estimatedCloseFrom: '',
    estimatedCloseTo: '',
    minRevenue: '',
    maxRevenue: '',
    isActive: '',
  })

  const load = async () => {
    if (!canView) return
    setLoading(true)
    setError('')
    try {
      const { data } = await api.get<PagedResult<Opportunity>>('api/opportunities', {
        params: {
          page: query.page,
          pageSize: query.pageSize,
          search: query.search || undefined,
          sortBy: query.sortBy || undefined,
          sortDir: query.sortDir,
          accountId: query.accountId || undefined,
          opportunityStageId: query.opportunityStageId || undefined,
          opportunityStatusId: query.opportunityStatusId || undefined,
          ratingId: query.ratingId || undefined,
          sourceId: query.sourceId || undefined,
          ownerUserId: query.ownerUserId || undefined,
          estimatedCloseFrom: query.estimatedCloseFrom || undefined,
          estimatedCloseTo: query.estimatedCloseTo || undefined,
          minRevenue: query.minRevenue || undefined,
          maxRevenue: query.maxRevenue || undefined,
          isActive: query.isActive || undefined,
        },
      })
      setRows(data.items)
      setTotalCount(data.totalCount)
    } catch {
      setError('Failed to load opportunities.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void Promise.resolve().then(load)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [canView, query])

  const refresh = () => setQuery((current) => ({ ...current }))

  const deleteOpportunity = async () => {
    if (!deleteTarget) return
    setLoading(true)
    setError('')
    try {
      await api.delete(`api/opportunities/${deleteTarget.id}`)
      setDeleteTarget(null)
      refresh()
    } catch {
      setError('Failed to delete opportunity.')
    } finally {
      setLoading(false)
    }
  }

  const assignToMe = async (opportunity: Opportunity) => {
    if (!user?.id) {
      setError('Current user could not be resolved.')
      return
    }

    setError('')
    try {
      await api.post(`api/opportunities/${opportunity.id}/assign-owner`, { ownerUserId: user.id, ownerTeamId: null })
      refresh()
    } catch {
      setError('Failed to assign opportunity.')
    }
  }

  const columns = useMemo<DenseColumn<Opportunity>[]>(
    () => [
      { key: 'opportunityNumber', label: 'Opportunity Number', sortable: true },
      { key: 'topic', label: 'Topic', sortable: true },
      { key: 'accountName', label: 'Account', sortable: true, render: (row) => row.accountName ?? 'Not set' },
      { key: 'contactName', label: 'Contact', sortable: true, render: (row) => row.contactName ?? 'Not set' },
      { key: 'opportunityStageName', label: 'Stage', sortable: true, render: (row) => statusCell(row.opportunityStageName ?? 'Not set') },
      { key: 'opportunityStatusName', label: 'Status', sortable: true, render: (row) => statusCell(row.opportunityStatusName ?? 'Not set') },
      { key: 'ratingName', label: 'Rating', sortable: true, render: (row) => row.ratingName ?? 'Not set' },
      { key: 'probability', label: 'Probability', sortable: true, render: (row) => `${row.probability}%` },
      { key: 'estimatedRevenue', label: 'Estimated Revenue', sortable: true, render: (row) => formatCurrency(row.estimatedRevenue) || 'Not set' },
      { key: 'weightedRevenue', label: 'Weighted Revenue', sortable: true, render: (row) => formatCurrency(row.weightedRevenue) || 'Not set' },
      { key: 'estimatedCloseDate', label: 'Estimated Close', sortable: true, render: (row) => formatDate(row.estimatedCloseDate) || 'Not set' },
      { key: 'ownerUserName', label: 'Owner', sortable: true, render: (row) => row.ownerUserName ?? row.ownerTeamName ?? 'Not set' },
    ],
    [],
  )

  if (!canView) {
    return (
      <div>
        <PageHeader title="Opportunities" subtitle="Manage deals, pipeline value, competitors, products, and sales activities." />
        <MessageBar intent="error"><MessageBarBody>You do not have permission to view opportunities.</MessageBarBody></MessageBar>
      </div>
    )
  }

  return (
    <div>
      <PageHeader
        title="Opportunities"
        subtitle="Manage deals, pipeline value, competitors, products, and sales activities."
        quickAction={canCreate ? 'New Opportunity' : undefined}
        onQuickAction={canCreate ? () => navigate('/opportunities/create') : undefined}
      />
      <CommandBar
        actions={[
          ...(canCreate ? [{ key: 'create', label: 'New Opportunity', onClick: () => navigate('/opportunities/create') }] : []),
          ...(canViewPipeline ? [{ key: 'pipeline', label: 'Pipeline', onClick: () => navigate('/opportunities/pipeline') }] : []),
        ]}
      />

      <section className={styles.filterBar}>
        <Field label="Account">
          <LookupCombobox fieldKey="accountId" value={query.accountId} onChange={(value) => setQuery((current) => ({ ...current, accountId: value, page: 1 }))} />
        </Field>
        <Field label="Stage">
          <LookupCombobox fieldKey="opportunityStageId" value={query.opportunityStageId} onChange={(value) => setQuery((current) => ({ ...current, opportunityStageId: value, page: 1 }))} />
        </Field>
        <Field label="Status">
          <LookupCombobox fieldKey="opportunityStatusId" value={query.opportunityStatusId} onChange={(value) => setQuery((current) => ({ ...current, opportunityStatusId: value, page: 1 }))} />
        </Field>
        <Field label="Rating">
          <LookupCombobox fieldKey="opportunityRatingId" value={query.ratingId} onChange={(value) => setQuery((current) => ({ ...current, ratingId: value, page: 1 }))} />
        </Field>
        <Field label="Source">
          <LookupCombobox fieldKey="opportunitySourceId" value={query.sourceId} onChange={(value) => setQuery((current) => ({ ...current, sourceId: value, page: 1 }))} />
        </Field>
        <Field label="Owner">
          <LookupCombobox fieldKey="ownerUserId" value={query.ownerUserId} onChange={(value) => setQuery((current) => ({ ...current, ownerUserId: value, page: 1 }))} />
        </Field>
        <Field label="Close From">
          <Input size="small" type="date" value={query.estimatedCloseFrom} onChange={(_, data) => setQuery((current) => ({ ...current, estimatedCloseFrom: data.value, page: 1 }))} />
        </Field>
        <Field label="Close To">
          <Input size="small" type="date" value={query.estimatedCloseTo} onChange={(_, data) => setQuery((current) => ({ ...current, estimatedCloseTo: data.value, page: 1 }))} />
        </Field>
        <Field label="Min Revenue">
          <Input size="small" type="number" value={query.minRevenue} onChange={(_, data) => setQuery((current) => ({ ...current, minRevenue: data.value, page: 1 }))} />
        </Field>
        <Field label="Max Revenue">
          <Input size="small" type="number" value={query.maxRevenue} onChange={(_, data) => setQuery((current) => ({ ...current, maxRevenue: data.value, page: 1 }))} />
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

      {loading ? <Spinner size="small" label="Loading opportunities..." style={{ margin: '8px 0' }} /> : null}
      {error ? <MessageBar intent="error" style={{ marginBottom: 10 }}><MessageBarBody>{error}</MessageBarBody></MessageBar> : null}

      <DenseDataGrid
        rows={rows}
        columns={columns}
        loading={loading}
        totalCount={totalCount}
        page={query.page}
        pageSize={query.pageSize}
        search={query.search}
        sort={query.sortBy ? ({ key: query.sortBy as keyof Opportunity, dir: query.sortDir }) : null}
        onPageChange={(page) => setQuery((current) => ({ ...current, page }))}
        onPageSizeChange={(pageSize) => setQuery((current) => ({ ...current, pageSize, page: 1 }))}
        onSearchChange={(search) => setQuery((current) => ({ ...current, search, page: 1 }))}
        onSortChange={(sort: DenseSort<Opportunity> | null) =>
          setQuery((current) => ({ ...current, sortBy: sort ? String(sort.key) : '', sortDir: sort?.dir ?? 'asc', page: 1 }))
        }
        onView={(row) => navigate(`/opportunities/${row.id}`)}
        onEdit={canEdit ? (row) => navigate(`/opportunities/${row.id}/edit`) : undefined}
        onDelete={canDelete ? (row) => setDeleteTarget(row) : undefined}
        customActions={[
          { key: 'assign', label: 'Assign to Me', onClick: (row) => void assignToMe(row), disabled: () => !canAssign },
          { key: 'won', label: 'Mark Won', onClick: (row) => navigate(`/opportunities/${row.id}/mark-won`), disabled: (row) => !canMarkWon || row.opportunityStatusCode === 'WON' },
          { key: 'lost', label: 'Mark Lost', onClick: (row) => navigate(`/opportunities/${row.id}/mark-lost`), disabled: (row) => !canMarkLost || row.opportunityStatusCode === 'LOST' },
          { key: 'timeline', label: 'Timeline', onClick: (row) => navigate(`/opportunities/${row.id}/timeline`) },
        ]}
        emptyMessage="No opportunities match the current filters."
      />

      <DeleteConfirmDialog
        open={Boolean(deleteTarget)}
        title="Delete Opportunity"
        message={`Delete ${deleteTarget?.topic ?? 'this opportunity'}?`}
        onConfirm={() => void deleteOpportunity()}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  )
}
