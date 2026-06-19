import { useCallback, useEffect, useMemo, useState } from 'react'
import { Button, Field, Input, MessageBar, MessageBarBody, Textarea } from '@fluentui/react-components'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { FormSectionCard, LookupCombobox } from '../components/entity-ui/EntityComponents'
import type { Account, Contact, LeadActivity, LeadTimelineItem, PagedResult } from '../types/models'
import { formatDateTime, nullIfBlank, toDateInput, toDateTimeInput } from './leadUtils'
import styles from '../contacts/Contacts.module.css'

type ActivityForm = {
  activityTypeId: string
  subject: string
  description: string
  activityDate: string
  dueDate: string
  completedDate: string
  statusId: string
  priorityId: string
  assignedToUserId: string
}

const emptyActivity: ActivityForm = {
  activityTypeId: '',
  subject: '',
  description: '',
  activityDate: '',
  dueDate: '',
  completedDate: '',
  statusId: '',
  priorityId: '',
  assignedToUserId: '',
}

export function LeadActivitiesPanel({ leadId, editable }: { leadId: string; editable?: boolean }) {
  const { hasPermission } = useAuth()
  const canView = hasPermission('LeadActivities.View')
  const canCreate = editable && hasPermission('LeadActivities.Create')
  const canUpdate = editable && hasPermission('LeadActivities.Update')
  const canDelete = editable && hasPermission('LeadActivities.Delete')
  const canComplete = editable && hasPermission('LeadActivities.Complete')
  const [rows, setRows] = useState<LeadActivity[]>([])
  const [form, setForm] = useState<ActivityForm>(emptyActivity)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  const sortedRows = useMemo(
    () => [...rows].sort((a, b) => new Date(b.activityDate).getTime() - new Date(a.activityDate).getTime()),
    [rows],
  )

  const load = useCallback(async () => {
    if (!canView || !leadId) {
      return
    }

    setLoading(true)
    setError('')
    try {
      const { data } = await api.get<PagedResult<LeadActivity>>(`api/leads/${leadId}/activities`, {
        params: { page: 1, pageSize: 100 },
      })
      setRows(data.items)
    } catch {
      setError('Failed to load lead activities.')
    } finally {
      setLoading(false)
    }
  }, [canView, leadId])

  useEffect(() => {
    void Promise.resolve().then(load)
  }, [load])

  const reset = () => {
    setEditingId(null)
    setForm(emptyActivity)
  }

  const edit = (row: LeadActivity) => {
    setEditingId(row.id)
    setForm({
      activityTypeId: row.activityTypeId,
      subject: row.subject,
      description: row.description ?? '',
      activityDate: toDateTimeInput(row.activityDate),
      dueDate: toDateInput(row.dueDate),
      completedDate: toDateTimeInput(row.completedDate),
      statusId: row.statusId,
      priorityId: row.priorityId ?? '',
      assignedToUserId: row.assignedToUserId ?? '',
    })
  }

  const save = async () => {
    if (!form.activityTypeId || !form.statusId || !form.subject.trim()) {
      setError('Activity type, status, and subject are required.')
      return
    }

    const payload = {
      leadId,
      activityTypeId: form.activityTypeId,
      subject: form.subject.trim(),
      description: nullIfBlank(form.description),
      activityDate: form.activityDate ? new Date(form.activityDate).toISOString() : new Date().toISOString(),
      dueDate: nullIfBlank(form.dueDate),
      completedDate: form.completedDate ? new Date(form.completedDate).toISOString() : null,
      statusId: form.statusId,
      priorityId: nullIfBlank(form.priorityId),
      assignedToUserId: nullIfBlank(form.assignedToUserId),
    }

    setLoading(true)
    setError('')
    try {
      if (editingId) {
        await api.put(`api/lead-activities/${editingId}`, payload)
      } else {
        await api.post(`api/leads/${leadId}/activities`, payload)
      }
      reset()
      await load()
    } catch {
      setError('Failed to save lead activity.')
    } finally {
      setLoading(false)
    }
  }

  const remove = async (id: string) => {
    setLoading(true)
    setError('')
    try {
      await api.delete(`api/lead-activities/${id}`)
      await load()
    } catch {
      setError('Failed to delete lead activity.')
    } finally {
      setLoading(false)
    }
  }

  const complete = async (id: string) => {
    setLoading(true)
    setError('')
    try {
      await api.post(`api/lead-activities/${id}/complete`, {})
      await load()
    } catch {
      setError('Failed to complete lead activity.')
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

      {canCreate || (editingId && canUpdate) ? (
        <FormSectionCard title={editingId ? 'Edit Activity' : 'Add Activity'}>
          <Field label="Activity Type" required>
            <LookupCombobox fieldKey="activityTypeId" value={form.activityTypeId} onChange={(value) => setForm((current) => ({ ...current, activityTypeId: value }))} />
          </Field>
          <Field label="Status" required>
            <LookupCombobox fieldKey="statusId" value={form.statusId} onChange={(value) => setForm((current) => ({ ...current, statusId: value }))} />
          </Field>
          <Field label="Subject" required>
            <Input size="small" value={form.subject} onChange={(_, data) => setForm((current) => ({ ...current, subject: data.value }))} />
          </Field>
          <Field label="Activity Date">
            <Input size="small" type="datetime-local" value={form.activityDate} onChange={(_, data) => setForm((current) => ({ ...current, activityDate: data.value }))} />
          </Field>
          <Field label="Due Date">
            <Input size="small" type="date" value={form.dueDate} onChange={(_, data) => setForm((current) => ({ ...current, dueDate: data.value }))} />
          </Field>
          <Field label="Completed Date">
            <Input size="small" type="datetime-local" value={form.completedDate} onChange={(_, data) => setForm((current) => ({ ...current, completedDate: data.value }))} />
          </Field>
          <Field label="Priority">
            <LookupCombobox fieldKey="priorityId" value={form.priorityId} onChange={(value) => setForm((current) => ({ ...current, priorityId: value }))} />
          </Field>
          <Field label="Assigned To">
            <LookupCombobox fieldKey="assignedToUserId" value={form.assignedToUserId} onChange={(value) => setForm((current) => ({ ...current, assignedToUserId: value }))} />
          </Field>
          <Field label="Description">
            <Textarea value={form.description} onChange={(_, data) => setForm((current) => ({ ...current, description: data.value }))} />
          </Field>
          <div className={styles.inlineActions}>
            <Button size="small" appearance="primary" onClick={() => void save()} disabled={loading || Boolean(editingId && !canUpdate)}>
              {editingId ? 'Update' : 'Add'}
            </Button>
            <Button size="small" appearance="subtle" onClick={reset}>Reset</Button>
          </div>
        </FormSectionCard>
      ) : null}

      <div className={styles.timeline}>
        {sortedRows.length === 0 ? <div className={styles.emptyState}>No lead activities.</div> : sortedRows.map((row) => (
          <article className={styles.timelineItem} key={row.id}>
            <div className={styles.timelineTop}>
              <div>
                <p className={styles.timelineTitle}>{row.subject}</p>
                <p className={styles.timelineMeta}>
                  {[row.activityTypeName, row.statusName, formatDateTime(row.activityDate), row.assignedToUserName].filter(Boolean).join(' | ')}
                </p>
              </div>
              <div className={styles.inlineActions}>
                <Button size="small" appearance="subtle" onClick={() => edit(row)} disabled={!canUpdate}>Edit</Button>
                <Button size="small" appearance="subtle" onClick={() => void complete(row.id)} disabled={!canComplete || Boolean(row.completedDate)}>Complete</Button>
                <Button size="small" appearance="subtle" onClick={() => void remove(row.id)} disabled={!canDelete}>Delete</Button>
              </div>
            </div>
            {row.completedDate ? <p className={styles.timelineMeta}>Completed: {formatDateTime(row.completedDate)}</p> : null}
            {row.description ? <p className={styles.timelineBody}>{row.description}</p> : null}
          </article>
        ))}
      </div>
    </div>
  )
}

export function LeadTimelinePanel({ leadId }: { leadId: string }) {
  const { hasPermission } = useAuth()
  const [rows, setRows] = useState<LeadTimelineItem[]>([])
  const [error, setError] = useState('')

  useEffect(() => {
    if (!hasPermission('Leads.ViewTimeline')) {
      return
    }

    void (async () => {
      setError('')
      try {
        const { data } = await api.get<LeadTimelineItem[]>(`api/leads/${leadId}/timeline`)
        setRows(data)
      } catch {
        setError('Failed to load lead timeline.')
      }
    })()
  }, [hasPermission, leadId])

  if (!hasPermission('Leads.ViewTimeline')) {
    return <div className={styles.emptyState}>Access denied.</div>
  }

  return (
    <div className={styles.timeline}>
      {error ? (
        <MessageBar intent="error">
          <MessageBarBody>{error}</MessageBarBody>
        </MessageBar>
      ) : null}
      {rows.length === 0 ? <div className={styles.emptyState}>No timeline records.</div> : rows.map((row) => (
        <article className={styles.timelineItem} key={row.id}>
          <div className={styles.timelineTop}>
            <div>
              <p className={styles.timelineTitle}>{row.title}</p>
              <p className={styles.timelineMeta}>
                {[row.itemType, row.status, row.priority, row.assignedToName, formatDateTime(row.occurredAt)].filter(Boolean).join(' | ')}
              </p>
            </div>
          </div>
          {row.description ? <p className={styles.timelineBody}>{row.description}</p> : null}
        </article>
      ))}
    </div>
  )
}

export function RelatedConversionPanel({ accountId, contactId }: { accountId?: string; contactId?: string }) {
  const [account, setAccount] = useState<Account | null>(null)
  const [contact, setContact] = useState<Contact | null>(null)
  const [error, setError] = useState('')

  useEffect(() => {
    void (async () => {
      setError('')
      try {
        if (accountId) {
          const { data } = await api.get<Account>(`api/accounts/${accountId}`)
          setAccount(data)
        }
        if (contactId) {
          const { data } = await api.get<Contact>(`api/contacts/${contactId}`)
          setContact(data)
        }
      } catch {
        setError('Failed to load converted account or contact.')
      }
    })()
  }, [accountId, contactId])

  return (
    <FormSectionCard title="Related Records">
      {error ? (
        <MessageBar intent="error">
          <MessageBarBody>{error}</MessageBarBody>
        </MessageBar>
      ) : null}
      <table className={styles.recordTable}>
        <tbody>
          <tr><th>Account</th><td>{account?.name ?? 'Not converted'}</td></tr>
          <tr><th>Account Number</th><td>{account?.accountNumber ?? ''}</td></tr>
          <tr><th>Contact</th><td>{contact?.fullName ?? 'Not converted'}</td></tr>
          <tr><th>Contact Number</th><td>{contact?.contactNumber ?? ''}</td></tr>
        </tbody>
      </table>
    </FormSectionCard>
  )
}
