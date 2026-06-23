import { useEffect, useMemo, useState } from 'react'
import { Dropdown, MessageBar, MessageBarBody, Option, Spinner } from '@fluentui/react-components'
import { useNavigate } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { DeleteConfirmDialog } from '../components/crud/DeleteConfirmDialog'
import { DenseDataGrid, statusCell, type DenseColumn, type DenseSort } from '../components/grid/DenseDataGrid'
import { FilterField } from '../components/filters/FilterField'
import { LookupFilterField } from '../components/filters/LookupFilterField'
import { useListQueryState } from '../hooks/useListQueryState'
import { CommandBar } from '../layout/components/CommandBar'
import { PageHeader } from '../layout/components/PageHeader'
import type { Contact, PagedResult } from '../types/models'

type ContactQuery = {
  page: number
  pageSize: number
  search: string
  sortBy: string
  sortDir: 'asc' | 'desc'
  accountId: string
  contactRoleId: string
  preferredContactMethodId: string
  isActive: string
}

export function ContactsListPage() {
  const navigate = useNavigate()
  const { hasPermission } = useAuth()
  const canView = hasPermission('Contacts.View')
  const canCreate = hasPermission('Contacts.Create')
  const canEdit = hasPermission('Contacts.Update')
  const canDelete = hasPermission('Contacts.Delete')
  const [rows, setRows] = useState<Contact[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [deleteTarget, setDeleteTarget] = useState<Contact | null>(null)
  const defaultQuery: ContactQuery = {
    page: 1,
    pageSize: 20,
    search: '',
    sortBy: '',
    sortDir: 'asc',
    accountId: '',
    contactRoleId: '',
    preferredContactMethodId: '',
    isActive: '',
  }
  const { query, setQuery } = useListQueryState<ContactQuery>({ defaults: defaultQuery, numberKeys: ['page', 'pageSize'] })
  const [draftFilters, setDraftFilters] = useState<Pick<ContactQuery, 'accountId' | 'contactRoleId' | 'preferredContactMethodId' | 'isActive'>>({
    accountId: query.accountId,
    contactRoleId: query.contactRoleId,
    preferredContactMethodId: query.preferredContactMethodId,
    isActive: query.isActive,
  })

  const columns = useMemo<DenseColumn<Contact>[]>(
    () => [
      { key: 'contactNumber', label: 'Contact Number', sortable: true },
      { key: 'fullName', label: 'Full Name', sortable: true },
      { key: 'accountName', label: 'Account', sortable: true },
      { key: 'jobTitle', label: 'Job Title', sortable: true },
      { key: 'email', label: 'Email', sortable: true },
      { key: 'mobilePhone', label: 'Mobile', sortable: true },
      { key: 'preferredContactMethodName', label: 'Preferred Contact Method', sortable: true },
      { key: 'contactRoleName', label: 'Role', sortable: true },
      { key: 'isPrimaryContact', label: 'Primary Contact', sortable: true, render: (row) => statusCell(row.isPrimaryContact ? 'Yes' : 'No') },
      { key: 'isActive', label: 'Active', sortable: true, render: (row) => statusCell(row.isActive ? 'Active' : 'Inactive') },
    ],
    [],
  )

  useEffect(() => {
    if (!canView) {
      return
    }

    const run = async () => {
      setLoading(true)
      setError('')
      try {
        const { data } = await api.get<PagedResult<Contact>>('api/contacts', {
          params: {
            page: query.page,
            pageSize: query.pageSize,
            search: query.search || undefined,
            sortBy: query.sortBy || undefined,
            sortDir: query.sortDir,
            accountId: query.accountId || undefined,
            contactRoleId: query.contactRoleId || undefined,
            preferredContactMethodId: query.preferredContactMethodId || undefined,
            isActive: query.isActive || undefined,
          },
        })
        setRows(data.items)
        setTotalCount(data.totalCount)
      } catch {
        setError('Failed to load contacts.')
      } finally {
        setLoading(false)
      }
    }

    void run()
  }, [canView, query])

  const activeFilterCount = [query.accountId, query.contactRoleId, query.preferredContactMethodId, query.isActive].filter(Boolean).length

  const deleteContact = async () => {
    if (!deleteTarget) {
      return
    }

    setLoading(true)
    setError('')
    try {
      await api.delete(`api/contacts/${deleteTarget.id}`)
      setDeleteTarget(null)
      setQuery((current) => ({ ...current }))
    } catch {
      setError('Failed to delete contact.')
    } finally {
      setLoading(false)
    }
  }

  if (!canView) {
    return (
      <div>
        <PageHeader title="Contacts" subtitle="Manage individual customer contacts." />
        <MessageBar intent="error">
          <MessageBarBody>You do not have permission to view contacts.</MessageBarBody>
        </MessageBar>
      </div>
    )
  }

  return (
    <div>
      <PageHeader
        title="Contacts"
        subtitle="Manage individuals, communication preferences, relationships, and interactions."
        quickAction={canCreate ? 'Create Contact' : undefined}
        onQuickAction={canCreate ? () => navigate('/contacts/create') : undefined}
      />
      <CommandBar actions={canCreate ? [{ key: 'create', label: 'Create Contact', onClick: () => navigate('/contacts/create') }] : []} />

      {loading ? <Spinner size="small" label="Loading contacts..." style={{ margin: '8px 0' }} /> : null}
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
        sort={query.sortBy ? ({ key: query.sortBy as keyof Contact, dir: query.sortDir }) : null}
        onPageChange={(page) => setQuery((current) => ({ ...current, page }))}
        onPageSizeChange={(pageSize) => setQuery((current) => ({ ...current, pageSize, page: 1 }))}
        onSearchChange={(search) => setQuery((current) => ({ ...current, search, page: 1 }))}
        onSortChange={(sort: DenseSort<Contact> | null) =>
          setQuery((current) => ({
            ...current,
            sortBy: sort ? String(sort.key) : '',
            sortDir: sort?.dir ?? 'asc',
            page: 1,
          }))
        }
        onView={(row) => navigate(`/contacts/${row.id}`)}
        onEdit={canEdit ? (row) => navigate(`/contacts/${row.id}/edit`) : undefined}
        onDelete={canDelete ? (row) => setDeleteTarget(row) : undefined}
        emptyMessage="No contacts match the current filters."
        activeFilterCount={activeFilterCount}
        filterPanel={
          <>
            <LookupFilterField
              label="Account"
              fieldKey="accountId"
              value={draftFilters.accountId}
              onChange={(value) => setDraftFilters((current) => ({ ...current, accountId: value }))}
            />
            <LookupFilterField
              label="Role"
              fieldKey="contactRoleId"
              value={draftFilters.contactRoleId}
              onChange={(value) => setDraftFilters((current) => ({ ...current, contactRoleId: value }))}
            />
            <LookupFilterField
              label="Preferred Contact Method"
              fieldKey="preferredContactMethodId"
              value={draftFilters.preferredContactMethodId}
              onChange={(value) => setDraftFilters((current) => ({ ...current, preferredContactMethodId: value }))}
            />
            <FilterField label="Active">
              <Dropdown
                size="small"
                selectedOptions={draftFilters.isActive ? [draftFilters.isActive] : []}
                value={draftFilters.isActive === 'true' ? 'Active' : draftFilters.isActive === 'false' ? 'Inactive' : ''}
                onOptionSelect={(_, data) =>
                  setDraftFilters((current) => ({
                    ...current,
                    isActive: data.optionValue ?? '',
                  }))
                }
              >
                <Option value="">All</Option>
                <Option value="true">Active</Option>
                <Option value="false">Inactive</Option>
              </Dropdown>
            </FilterField>
          </>
        }
        onApplyFilters={() =>
          setQuery((current) => ({
            ...current,
            ...draftFilters,
            page: 1,
          }))
        }
        onCancelFilters={() =>
          setDraftFilters({
            accountId: query.accountId,
            contactRoleId: query.contactRoleId,
            preferredContactMethodId: query.preferredContactMethodId,
            isActive: query.isActive,
          })
        }
        onClearFilters={() =>
          setDraftFilters({
            accountId: '',
            contactRoleId: '',
            preferredContactMethodId: '',
            isActive: '',
          })
        }
      />

      <DeleteConfirmDialog
        open={Boolean(deleteTarget)}
        title="Delete Contact"
        message={`Delete ${deleteTarget?.fullName ?? 'this contact'}?`}
        onConfirm={() => void deleteContact()}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  )
}
