import { useEffect, useMemo, useState } from 'react'
import { Dropdown, Field, MessageBar, MessageBarBody, Option, Spinner } from '@fluentui/react-components'
import { useNavigate } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { DeleteConfirmDialog } from '../components/crud/DeleteConfirmDialog'
import { LookupCombobox } from '../components/entity-ui/EntityComponents'
import { DenseDataGrid, statusCell, type DenseColumn, type DenseSort } from '../components/grid/DenseDataGrid'
import { CommandBar } from '../layout/components/CommandBar'
import { PageHeader } from '../layout/components/PageHeader'
import type { Contact, PagedResult } from '../types/models'
import styles from './Contacts.module.css'

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
  const [query, setQuery] = useState<ContactQuery>({
    page: 1,
    pageSize: 20,
    search: '',
    sortBy: '',
    sortDir: 'asc',
    accountId: '',
    contactRoleId: '',
    preferredContactMethodId: '',
    isActive: '',
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

      <section className={styles.filterBar}>
        <Field label="Account">
          <LookupCombobox fieldKey="accountId" value={query.accountId} onChange={(value) => setQuery((current) => ({ ...current, accountId: value, page: 1 }))} />
        </Field>
        <Field label="Role">
          <LookupCombobox fieldKey="contactRoleId" value={query.contactRoleId} onChange={(value) => setQuery((current) => ({ ...current, contactRoleId: value, page: 1 }))} />
        </Field>
        <Field label="Preferred Contact Method">
          <LookupCombobox fieldKey="preferredContactMethodId" value={query.preferredContactMethodId} onChange={(value) => setQuery((current) => ({ ...current, preferredContactMethodId: value, page: 1 }))} />
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
