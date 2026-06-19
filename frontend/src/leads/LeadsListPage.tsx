import { useEffect, useState } from 'react'
import { Button, Dropdown, Field, Input, MessageBar, MessageBarBody, Option, Spinner } from '@fluentui/react-components'
import { useNavigate } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { DeleteConfirmDialog } from '../components/crud/DeleteConfirmDialog'
import { LookupCombobox } from '../components/entity-ui/EntityComponents'
import { loadLookupOptionsByCategoryCode } from '../components/entity-ui/referenceData'
import { DenseDataGrid, statusCell, type DenseColumn, type DenseSort } from '../components/grid/DenseDataGrid'
import { CommandBar } from '../layout/components/CommandBar'
import { PageHeader } from '../layout/components/PageHeader'
import type { Lead, PagedResult } from '../types/models'
import { formatCurrency, formatDate, formatDateTime } from './leadUtils'
import styles from '../contacts/Contacts.module.css'

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
  const [query, setQuery] = useState<LeadQuery>({
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
    {
      key: 'id',
      label: 'Lead Actions',
      render: (row) => (
        <div className={styles.inlineActions}>
          <Button size="small" appearance="subtle" onClick={() => void assignToMe(row)} disabled={!canAssign}>Assign</Button>
          <Button size="small" appearance="subtle" onClick={() => void qualify(row)} disabled={!canQualify || row.leadStatusName === 'Converted'}>Qualify</Button>
          <Button size="small" appearance="subtle" onClick={() => void disqualify(row)} disabled={!canDisqualify || row.leadStatusName === 'Converted'}>Disqualify</Button>
          <Button size="small" appearance="subtle" onClick={() => navigate(`/leads/${row.id}/convert`)} disabled={!canConvert || row.leadStatusName !== 'Qualified'}>Convert</Button>
          <Button size="small" appearance="subtle" onClick={() => void calculateScore(row)} disabled={!canScore}>Score</Button>
        </div>
      ),
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
        quickAction={canCreate ? 'New Lead' : undefined}
        onQuickAction={canCreate ? () => navigate('/leads/create') : undefined}
      />
      <CommandBar actions={canCreate ? [{ key: 'create', label: 'New Lead', onClick: () => navigate('/leads/create') }] : []} />

      <section className={styles.filterBar}>
        <Field label="Status">
          <LookupCombobox fieldKey="leadStatusId" value={query.leadStatusId} onChange={(value) => setQuery((current) => ({ ...current, leadStatusId: value, page: 1 }))} />
        </Field>
        <Field label="Qualification">
          <LookupCombobox fieldKey="qualificationStatusId" value={query.qualificationStatusId} onChange={(value) => setQuery((current) => ({ ...current, qualificationStatusId: value, page: 1 }))} />
        </Field>
        <Field label="Rating">
          <LookupCombobox fieldKey="ratingId" value={query.ratingId} onChange={(value) => setQuery((current) => ({ ...current, ratingId: value, page: 1 }))} />
        </Field>
        <Field label="Lead Source">
          <LookupCombobox fieldKey="leadSourceId" value={query.leadSourceId} onChange={(value) => setQuery((current) => ({ ...current, leadSourceId: value, page: 1 }))} />
        </Field>
        <Field label="Owner">
          <LookupCombobox fieldKey="ownerUserId" value={query.ownerUserId} onChange={(value) => setQuery((current) => ({ ...current, ownerUserId: value, page: 1 }))} />
        </Field>
        <Field label="Assigned User">
          <LookupCombobox fieldKey="assignedToUserId" value={query.assignedToUserId} onChange={(value) => setQuery((current) => ({ ...current, assignedToUserId: value, page: 1 }))} />
        </Field>
        <Field label="Created From">
          <Input size="small" type="date" value={query.createdFrom} onChange={(_, data) => setQuery((current) => ({ ...current, createdFrom: data.value, page: 1 }))} />
        </Field>
        <Field label="Created To">
          <Input size="small" type="date" value={query.createdTo} onChange={(_, data) => setQuery((current) => ({ ...current, createdTo: data.value, page: 1 }))} />
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
        onView={(row) => navigate(`/leads/${row.id}`)}
        onEdit={canEdit ? (row) => navigate(`/leads/${row.id}/edit`) : undefined}
        onDelete={canDelete ? (row) => setDeleteTarget(row) : undefined}
        emptyMessage="No leads match the current filters."
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
