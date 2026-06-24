import { useCallback, useEffect, useMemo, useState } from 'react'
import { Field, Input, MessageBar, MessageBarBody, Switch, Textarea } from '@fluentui/react-components'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { FormSectionCard, LookupCombobox } from '../components/entity-ui/EntityComponents'
import { RelatedRecordsSubgrid } from '../components/subgrid/RelatedRecordsSubgrid'
import { SubgridDeleteConfirmDialog } from '../components/subgrid/SubgridDeleteConfirmDialog'
import { SubgridModalForm } from '../components/subgrid/SubgridModalForm'
import { SubgridRowActions } from '../components/subgrid/SubgridRowActions'
import type { Account, AuditLog, ContactCommunication, ContactInteraction, PagedResult } from '../types/models'
import { formatDateTime, nullIfBlank, toDateInput } from './contactUtils'
import styles from './Contacts.module.css'

type CommunicationForm = {
  communicationTypeId: string
  value: string
  isPrimary: boolean
  isVerified: boolean
  verificationDate: string
  notes: string
}

type InteractionForm = {
  interactionTypeId: string
  subject: string
  description: string
  interactionDate: string
  outcome: string
  followUpDate: string
  assignedToUserId: string
}

const emptyCommunication: CommunicationForm = {
  communicationTypeId: '',
  value: '',
  isPrimary: false,
  isVerified: false,
  verificationDate: '',
  notes: '',
}

const emptyInteraction: InteractionForm = {
  interactionTypeId: '',
  subject: '',
  description: '',
  interactionDate: '',
  outcome: '',
  followUpDate: '',
  assignedToUserId: '',
}

const toDateTimeInput = (value?: string) => {
  if (!value) {
    return ''
  }

  const parsed = new Date(value)
  if (Number.isNaN(parsed.getTime())) {
    return value.slice(0, 16)
  }

  const local = new Date(parsed.getTime() - parsed.getTimezoneOffset() * 60000)
  return local.toISOString().slice(0, 16)
}

export function ContactCommunicationsPanel({ contactId, editable }: { contactId: string; editable?: boolean }) {
  const { hasPermission } = useAuth()
  const canView = hasPermission('ContactCommunications.View')
  const canCreate = editable && hasPermission('ContactCommunications.Create')
  const canUpdate = editable && hasPermission('ContactCommunications.Update')
  const canDelete = editable && hasPermission('ContactCommunications.Delete')
  const [rows, setRows] = useState<ContactCommunication[]>([])
  const [form, setForm] = useState<CommunicationForm>(emptyCommunication)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [formOpen, setFormOpen] = useState(false)
  const [success, setSuccess] = useState('')
  const [deleteTarget, setDeleteTarget] = useState<ContactCommunication | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  const load = useCallback(async () => {
    if (!canView || !contactId) {
      return
    }

    setLoading(true)
    setError('')
    try {
      const { data } = await api.get<PagedResult<ContactCommunication>>('api/contact-communications', {
        params: { contactId, page: 1, pageSize: 100 },
      })
      setRows(data.items)
    } catch {
      setError('Failed to load communication records.')
    } finally {
      setLoading(false)
    }
  }, [canView, contactId])

  useEffect(() => {
    void Promise.resolve().then(load)
  }, [load])

  const reset = () => {
    setEditingId(null)
    setForm(emptyCommunication)
  }

  const openAdd = () => {
    reset()
    setFormOpen(true)
  }

  const edit = (row: ContactCommunication) => {
    setEditingId(row.id)
    setForm({
      communicationTypeId: row.communicationTypeId ?? '',
      value: row.value,
      isPrimary: row.isPrimary,
      isVerified: row.isVerified,
      verificationDate: toDateInput(row.verificationDate),
      notes: row.notes ?? '',
    })
    setFormOpen(true)
  }

  const save = async () => {
    if (!form.value.trim()) {
      setError('Communication value is required.')
      return
    }

    const payload = {
      contactId,
      communicationTypeId: nullIfBlank(form.communicationTypeId),
      value: form.value.trim(),
      isPrimary: form.isPrimary,
      isVerified: form.isVerified,
      verificationDate: nullIfBlank(form.verificationDate),
      notes: nullIfBlank(form.notes),
    }

    setLoading(true)
    setError('')
    setSuccess('')
    try {
      if (editingId) {
        await api.put(`api/contact-communications/${editingId}`, payload)
      } else {
        await api.post('api/contact-communications', payload)
      }
      reset()
      setFormOpen(false)
      setSuccess(editingId ? 'Communication updated successfully.' : 'Communication created successfully.')
      await load()
    } catch {
      setError('Failed to save communication record.')
    } finally {
      setLoading(false)
    }
  }

  const remove = async () => {
    if (!deleteTarget) {
      return
    }
    setLoading(true)
    setError('')
    setSuccess('')
    try {
      await api.delete(`api/contact-communications/${deleteTarget.id}`)
      setDeleteTarget(null)
      setSuccess('Communication deleted successfully.')
      await load()
    } catch {
      setError('Failed to delete communication record.')
    } finally {
      setLoading(false)
    }
  }

  if (!canView) {
    return <div className={styles.emptyState}>Access denied.</div>
  }

  return (
    <div className={styles.sectionStack}>
      {error ? (
        <MessageBar intent="error">
          <MessageBarBody>{error}</MessageBarBody>
        </MessageBar>
      ) : null}

      {success ? (
        <MessageBar intent="success">
          <MessageBarBody>{success}</MessageBarBody>
        </MessageBar>
      ) : null}

      <RelatedRecordsSubgrid
        title="Communications"
        addLabel={canCreate ? 'Add Communication' : undefined}
        onAdd={canCreate ? openAdd : undefined}
        onRefresh={() => void load()}
        loading={loading}
        error={error}
        hasRows={rows.length > 0}
        emptyMessage="No communication records."
        emptyActionLabel={canCreate ? 'Add Communication' : undefined}
        onEmptyAction={canCreate ? openAdd : undefined}
      >
        <table className={styles.recordTable}>
          <thead>
            <tr>
              <th>Type</th>
              <th>Value</th>
              <th>Primary</th>
              <th>Verified</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((row) => (
              <tr key={row.id}>
                <td>{row.communicationTypeName || 'Not set'}</td>
                <td>{row.value}</td>
                <td>{row.isPrimary ? 'Yes' : 'No'}</td>
                <td>{row.isVerified ? 'Yes' : 'No'}</td>
                <td>
                  <SubgridRowActions
                    onView={() => edit(row)}
                    onEdit={canUpdate ? () => edit(row) : undefined}
                    onDelete={canDelete ? () => setDeleteTarget(row) : undefined}
                    disableEdit={!canUpdate}
                    disableDelete={!canDelete}
                  />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </RelatedRecordsSubgrid>

      <SubgridModalForm
        open={formOpen}
        title={editingId ? 'Edit Communication' : 'Add Communication'}
        submitLabel={editingId ? 'Update' : 'Add'}
        loading={loading}
        onOpenChange={(open) => {
          setFormOpen(open)
          if (!open) {
            reset()
          }
        }}
        onSubmit={() => void save()}
      >
        <FormSectionCard title="Communication Details">
          <Field label="Communication Type">
            <LookupCombobox fieldKey="communicationTypeId" value={form.communicationTypeId} onChange={(value) => setForm((current) => ({ ...current, communicationTypeId: value }))} />
          </Field>
          <Field label="Value" required>
            <Input size="small" value={form.value} onChange={(_, data) => setForm((current) => ({ ...current, value: data.value }))} />
          </Field>
          <Field label="Verified Date">
            <Input size="small" type="date" value={form.verificationDate} onChange={(_, data) => setForm((current) => ({ ...current, verificationDate: data.value }))} />
          </Field>
          <Field label="Primary">
            <Switch checked={form.isPrimary} onChange={(_, data) => setForm((current) => ({ ...current, isPrimary: Boolean(data.checked) }))} />
          </Field>
          <Field label="Verified">
            <Switch checked={form.isVerified} onChange={(_, data) => setForm((current) => ({ ...current, isVerified: Boolean(data.checked) }))} />
          </Field>
          <Field label="Notes">
            <Textarea value={form.notes} onChange={(_, data) => setForm((current) => ({ ...current, notes: data.value }))} />
          </Field>
        </FormSectionCard>
      </SubgridModalForm>

      <SubgridDeleteConfirmDialog
        open={Boolean(deleteTarget)}
        title="Delete Communication"
        message="Delete this communication record?"
        onConfirm={() => void remove()}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  )
}

export function ContactInteractionsPanel({ contactId, accountId, editable }: { contactId: string; accountId: string; editable?: boolean }) {
  const { hasPermission } = useAuth()
  const canView = hasPermission('ContactInteractions.View')
  const canCreate = editable && hasPermission('ContactInteractions.Create')
  const canUpdate = editable && hasPermission('ContactInteractions.Update')
  const canDelete = editable && hasPermission('ContactInteractions.Delete')
  const [rows, setRows] = useState<ContactInteraction[]>([])
  const [form, setForm] = useState<InteractionForm>(emptyInteraction)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [formOpen, setFormOpen] = useState(false)
  const [success, setSuccess] = useState('')
  const [deleteTarget, setDeleteTarget] = useState<ContactInteraction | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  const sortedRows = useMemo(
    () => [...rows].sort((a, b) => new Date(b.interactionDate).getTime() - new Date(a.interactionDate).getTime()),
    [rows],
  )

  const load = useCallback(async () => {
    if (!canView || !contactId) {
      return
    }

    setLoading(true)
    setError('')
    try {
      const { data } = await api.get<PagedResult<ContactInteraction>>('api/contact-interactions', {
        params: { contactId, page: 1, pageSize: 100 },
      })
      setRows(data.items)
    } catch {
      setError('Failed to load interaction history.')
    } finally {
      setLoading(false)
    }
  }, [canView, contactId])

  useEffect(() => {
    void Promise.resolve().then(load)
  }, [load])

  const reset = () => {
    setEditingId(null)
    setForm(emptyInteraction)
  }

  const openAdd = () => {
    reset()
    setFormOpen(true)
  }

  const edit = (row: ContactInteraction) => {
    setEditingId(row.id)
    setForm({
      interactionTypeId: row.interactionTypeId ?? '',
      subject: row.subject,
      description: row.description ?? '',
      interactionDate: toDateTimeInput(row.interactionDate),
      outcome: row.outcome ?? '',
      followUpDate: toDateInput(row.followUpDate),
      assignedToUserId: row.assignedToUserId ?? '',
    })
    setFormOpen(true)
  }

  const save = async () => {
    if (!form.subject.trim()) {
      setError('Subject is required.')
      return
    }

    const payload = {
      contactId,
      accountId,
      interactionTypeId: nullIfBlank(form.interactionTypeId),
      subject: form.subject.trim(),
      description: nullIfBlank(form.description),
      interactionDate: form.interactionDate ? new Date(form.interactionDate).toISOString() : new Date().toISOString(),
      outcome: nullIfBlank(form.outcome),
      followUpDate: nullIfBlank(form.followUpDate),
      assignedToUserId: nullIfBlank(form.assignedToUserId),
    }

    setLoading(true)
    setError('')
    setSuccess('')
    try {
      if (editingId) {
        await api.put(`api/contact-interactions/${editingId}`, payload)
      } else {
        await api.post('api/contact-interactions', payload)
      }
      reset()
      setFormOpen(false)
      setSuccess(editingId ? 'Interaction updated successfully.' : 'Interaction created successfully.')
      await load()
    } catch {
      setError('Failed to save interaction.')
    } finally {
      setLoading(false)
    }
  }

  const remove = async () => {
    if (!deleteTarget) {
      return
    }
    setLoading(true)
    setError('')
    setSuccess('')
    try {
      await api.delete(`api/contact-interactions/${deleteTarget.id}`)
      setDeleteTarget(null)
      setSuccess('Interaction deleted successfully.')
      await load()
    } catch {
      setError('Failed to delete interaction.')
    } finally {
      setLoading(false)
    }
  }

  if (!canView) {
    return <div className={styles.emptyState}>Access denied.</div>
  }

  return (
    <div className={styles.sectionStack}>
      {error ? (
        <MessageBar intent="error">
          <MessageBarBody>{error}</MessageBarBody>
        </MessageBar>
      ) : null}

      {success ? (
        <MessageBar intent="success">
          <MessageBarBody>{success}</MessageBarBody>
        </MessageBar>
      ) : null}

      <RelatedRecordsSubgrid
        title="Interaction History"
        addLabel={canCreate ? 'Add Interaction' : undefined}
        onAdd={canCreate ? openAdd : undefined}
        onRefresh={() => void load()}
        loading={loading}
        error={error}
        hasRows={sortedRows.length > 0}
        emptyMessage="No interaction history."
        emptyActionLabel={canCreate ? 'Add Interaction' : undefined}
        onEmptyAction={canCreate ? openAdd : undefined}
      >
        <div className={styles.timeline}>
          {sortedRows.map((row) => (
            <article className={styles.timelineItem} key={row.id}>
              <div className={styles.timelineTop}>
                <div>
                  <p className={styles.timelineTitle}>{row.subject}</p>
                  <p className={styles.timelineMeta}>
                    {[row.interactionTypeName, formatDateTime(row.interactionDate), row.assignedToUserName].filter(Boolean).join(' | ')}
                  </p>
                </div>
                <SubgridRowActions
                  onView={() => edit(row)}
                  onEdit={canUpdate ? () => edit(row) : undefined}
                  onDelete={canDelete ? () => setDeleteTarget(row) : undefined}
                  disableEdit={!canUpdate}
                  disableDelete={!canDelete}
                />
              </div>
              {row.outcome ? <p className={styles.timelineMeta}>Outcome: {row.outcome}</p> : null}
              {row.description ? <p className={styles.timelineBody}>{row.description}</p> : null}
            </article>
          ))}
        </div>
      </RelatedRecordsSubgrid>

      <SubgridModalForm
        open={formOpen}
        title={editingId ? 'Edit Interaction' : 'Add Interaction'}
        submitLabel={editingId ? 'Update' : 'Add'}
        loading={loading}
        onOpenChange={(open) => {
          setFormOpen(open)
          if (!open) {
            reset()
          }
        }}
        onSubmit={() => void save()}
      >
        <FormSectionCard title="Interaction Details">
          <Field label="Interaction Type">
            <LookupCombobox fieldKey="interactionTypeId" value={form.interactionTypeId} onChange={(value) => setForm((current) => ({ ...current, interactionTypeId: value }))} />
          </Field>
          <Field label="Subject" required>
            <Input size="small" value={form.subject} onChange={(_, data) => setForm((current) => ({ ...current, subject: data.value }))} />
          </Field>
          <Field label="Interaction Date">
            <Input size="small" type="datetime-local" value={form.interactionDate} onChange={(_, data) => setForm((current) => ({ ...current, interactionDate: data.value }))} />
          </Field>
          <Field label="Outcome">
            <Input size="small" value={form.outcome} onChange={(_, data) => setForm((current) => ({ ...current, outcome: data.value }))} />
          </Field>
          <Field label="Follow Up Date">
            <Input size="small" type="date" value={form.followUpDate} onChange={(_, data) => setForm((current) => ({ ...current, followUpDate: data.value }))} />
          </Field>
          <Field label="Assigned To">
            <LookupCombobox fieldKey="assignedToUserId" value={form.assignedToUserId} onChange={(value) => setForm((current) => ({ ...current, assignedToUserId: value }))} />
          </Field>
          <Field label="Description">
            <Textarea value={form.description} onChange={(_, data) => setForm((current) => ({ ...current, description: data.value }))} />
          </Field>
        </FormSectionCard>
      </SubgridModalForm>

      <SubgridDeleteConfirmDialog
        open={Boolean(deleteTarget)}
        title="Delete Interaction"
        message="Delete this interaction?"
        onConfirm={() => void remove()}
        onCancel={() => setDeleteTarget(null)}
      />

    </div>
  )
}

export function AuditHistoryPanel({ entityName, entityId }: { entityName: string; entityId: string }) {
  const { hasPermission } = useAuth()
  const [rows, setRows] = useState<AuditLog[]>([])
  const [error, setError] = useState('')

  useEffect(() => {
    if (!hasPermission('AuditLogs.View')) {
      return
    }

    void (async () => {
      setError('')
      try {
        const { data } = await api.get<PagedResult<AuditLog>>('api/audit-logs', {
          params: { entityName, entityId, page: 1, pageSize: 50 },
        })
        setRows(data.items)
      } catch {
        setError('Failed to load audit history.')
      }
    })()
  }, [entityName, entityId, hasPermission])

  if (!hasPermission('AuditLogs.View')) {
    return <div className={styles.emptyState}>Access denied.</div>
  }

  return (
    <FormSectionCard title="Audit History">
      {error ? (
        <MessageBar intent="error">
          <MessageBarBody>{error}</MessageBarBody>
        </MessageBar>
      ) : null}
      {rows.length === 0 ? <div className={styles.emptyState}>No audit records.</div> : (
        <table className={styles.recordTable}>
          <thead>
            <tr>
              <th>Action</th>
              <th>Date</th>
              <th>Old Values</th>
              <th>New Values</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((row) => (
              <tr key={row.id}>
                <td>{row.action}</td>
                <td>{formatDateTime(row.createdAt)}</td>
                <td>{row.oldValues || ''}</td>
                <td>{row.newValues || ''}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </FormSectionCard>
  )
}

export function RelatedAccountPanel({ accountId, accountName }: { accountId: string; accountName?: string }) {
  const [account, setAccount] = useState<Account | null>(null)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!accountId) {
      return
    }

    void (async () => {
      setError('')
      try {
        const { data } = await api.get<Account>(`api/accounts/${accountId}`)
        setAccount(data)
      } catch {
        setError('Failed to load related account.')
      }
    })()
  }, [accountId])

  return (
    <FormSectionCard title="Related Account">
      {error ? (
        <MessageBar intent="error">
          <MessageBarBody>{error}</MessageBarBody>
        </MessageBar>
      ) : null}
      <table className={styles.recordTable}>
        <tbody>
          <tr><th>Account</th><td>{account?.name ?? accountName ?? ''}</td></tr>
          <tr><th>Account Number</th><td>{account?.accountNumber ?? ''}</td></tr>
          <tr><th>Email</th><td>{account?.email ?? ''}</td></tr>
          <tr><th>Main Phone</th><td>{account?.mainPhone ?? ''}</td></tr>
          <tr><th>Status</th><td>{account?.isActive ? 'Active' : account ? 'Inactive' : ''}</td></tr>
        </tbody>
      </table>
    </FormSectionCard>
  )
}
