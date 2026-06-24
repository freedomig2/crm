import { useCallback, useEffect, useMemo, useState } from 'react'
import { Button, Field, Input, MessageBar, MessageBarBody, Switch, Textarea } from '@fluentui/react-components'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { FormSectionCard, LookupCombobox } from '../components/entity-ui/EntityComponents'
import { RelatedRecordsSubgrid } from '../components/subgrid/RelatedRecordsSubgrid'
import { SubgridDeleteConfirmDialog } from '../components/subgrid/SubgridDeleteConfirmDialog'
import { SubgridModalForm } from '../components/subgrid/SubgridModalForm'
import { SubgridRowActions } from '../components/subgrid/SubgridRowActions'
import { loadLookupOptionsByCategoryCode } from '../components/entity-ui/referenceData'
import type { OpportunityActivity, OpportunityCompetitor, OpportunityProduct, OpportunityTimelineItem, PagedResult } from '../types/models'
import { formatCurrency, formatDateTime, nullIfBlank } from './opportunityUtils'
import { toDateInput, toDateTimeInput } from '../leads/leadUtils'
import styles from '../contacts/Contacts.module.css'

type ProductForm = {
  productId: string
  productName: string
  description: string
  quantity: string
  unitPrice: string
  discountPercent: string
  discountAmount: string
  taxAmount: string
  sortOrder: string
}

type CompetitorForm = {
  competitorName: string
  strengths: string
  weaknesses: string
  threatLevelId: string
  isPrimaryCompetitor: boolean
  notes: string
}

type ActivityForm = {
  contactId: string
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

const emptyProduct: ProductForm = {
  productId: '',
  productName: '',
  description: '',
  quantity: '1',
  unitPrice: '0',
  discountPercent: '',
  discountAmount: '',
  taxAmount: '',
  sortOrder: '0',
}

const emptyCompetitor: CompetitorForm = {
  competitorName: '',
  strengths: '',
  weaknesses: '',
  threatLevelId: '',
  isPrimaryCompetitor: false,
  notes: '',
}

const emptyActivity: ActivityForm = {
  contactId: '',
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

export function OpportunityProductsPanel({ opportunityId, editable }: { opportunityId: string; editable?: boolean }) {
  const { hasPermission } = useAuth()
  const canView = hasPermission('OpportunityProducts.View')
  const canCreate = editable && hasPermission('OpportunityProducts.Create')
  const canUpdate = editable && hasPermission('OpportunityProducts.Update')
  const canDelete = editable && hasPermission('OpportunityProducts.Delete')
  const [rows, setRows] = useState<OpportunityProduct[]>([])
  const [form, setForm] = useState<ProductForm>(emptyProduct)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [formOpen, setFormOpen] = useState(false)
  const [success, setSuccess] = useState('')
  const [deleteTarget, setDeleteTarget] = useState<OpportunityProduct | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  const load = useCallback(async () => {
    if (!canView) return
    setLoading(true)
    setError('')
    try {
      const { data } = await api.get<PagedResult<OpportunityProduct>>(`api/opportunities/${opportunityId}/products`, {
        params: { page: 1, pageSize: 100, sortBy: 'sortOrder', sortDir: 'asc' },
      })
      setRows(data.items)
    } catch {
      setError('Failed to load products.')
    } finally {
      setLoading(false)
    }
  }, [canView, opportunityId])

  useEffect(() => {
    void Promise.resolve().then(load)
  }, [load])

  const reset = () => {
    setEditingId(null)
    setForm(emptyProduct)
  }

  const openAdd = () => {
    reset()
    setFormOpen(true)
  }

  const edit = (row: OpportunityProduct) => {
    setEditingId(row.id)
    setForm({
      productId: row.productId ?? '',
      productName: row.productName,
      description: row.description ?? '',
      quantity: String(row.quantity),
      unitPrice: String(row.unitPrice),
      discountPercent: row.discountPercent === undefined || row.discountPercent === null ? '' : String(row.discountPercent),
      discountAmount: row.discountAmount === undefined || row.discountAmount === null ? '' : String(row.discountAmount),
      taxAmount: row.taxAmount === undefined || row.taxAmount === null ? '' : String(row.taxAmount),
      sortOrder: String(row.sortOrder),
    })
    setFormOpen(true)
  }

  const save = async () => {
    if (!form.productId && !form.productName.trim()) {
      setError('Product lookup or product name is required.')
      return
    }

    const payload = {
      productId: nullIfBlank(form.productId),
      productName: form.productName.trim(),
      description: nullIfBlank(form.description),
      quantity: Number(form.quantity || 0),
      unitPrice: Number(form.unitPrice || 0),
      discountPercent: form.discountPercent.trim() ? Number(form.discountPercent) : null,
      discountAmount: form.discountAmount.trim() ? Number(form.discountAmount) : null,
      taxAmount: form.taxAmount.trim() ? Number(form.taxAmount) : null,
      sortOrder: Number(form.sortOrder || 0),
    }

    setLoading(true)
    setError('')
    setSuccess('')
    try {
      if (editingId) {
        await api.put(`api/opportunity-products/${editingId}`, payload)
      } else {
        await api.post(`api/opportunities/${opportunityId}/products`, payload)
      }
      reset()
      setFormOpen(false)
      setSuccess(editingId ? 'Opportunity product updated successfully.' : 'Opportunity product created successfully.')
      await load()
    } catch {
      setError('Failed to save product.')
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
      await api.delete(`api/opportunity-products/${deleteTarget.id}`)
      setDeleteTarget(null)
      setSuccess('Opportunity product deleted successfully.')
      await load()
    } catch {
      setError('Failed to delete product.')
    } finally {
      setLoading(false)
    }
  }

  if (!canView) return <div className={styles.emptyState}>Access denied.</div>

  return (
    <div className={styles.sectionStack}>
      {success ? <MessageBar intent="success"><MessageBarBody>{success}</MessageBarBody></MessageBar> : null}
      {error ? <MessageBar intent="error"><MessageBarBody>{error}</MessageBarBody></MessageBar> : null}

      <RelatedRecordsSubgrid
        title="Products"
        addLabel={canCreate ? 'Add Product' : undefined}
        onAdd={canCreate ? openAdd : undefined}
        onRefresh={() => void load()}
        loading={loading}
        error={error}
        hasRows={rows.length > 0}
        emptyMessage="No products."
        emptyActionLabel={canCreate ? 'Add Product' : undefined}
        onEmptyAction={canCreate ? openAdd : undefined}
      >
        <table className={styles.recordTable}>
          <thead>
            <tr><th>Product</th><th>Qty</th><th>Unit Price</th><th>Discount</th><th>Tax</th><th>Total</th><th>Actions</th></tr>
          </thead>
          <tbody>
            {rows.length === 0 ? <tr><td colSpan={7}>No products.</td></tr> : rows.map((row) => (
              <tr key={row.id}>
                <td>{row.productName}</td>
                <td>{row.quantity}</td>
                <td>{formatCurrency(row.unitPrice)}</td>
                <td>{formatCurrency(row.discountAmount)}</td>
                <td>{formatCurrency(row.taxAmount)}</td>
                <td>{formatCurrency(row.lineTotal)}</td>
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
        title={editingId ? 'Edit Product' : 'Add Product'}
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
        <FormSectionCard title="Product Details">
          <Field label="Product Lookup">
            <LookupCombobox fieldKey="productId" value={form.productId} onChange={(value) => setForm((current) => ({ ...current, productId: value }))} />
          </Field>
          <Field label="Product Name (fallback)">
            <Input size="small" value={form.productName} onChange={(_, data) => setForm((current) => ({ ...current, productName: data.value }))} />
          </Field>
          <Field label="Quantity" required>
            <Input size="small" type="number" value={form.quantity} onChange={(_, data) => setForm((current) => ({ ...current, quantity: data.value }))} />
          </Field>
          <Field label="Unit Price" required>
            <Input size="small" type="number" value={form.unitPrice} onChange={(_, data) => setForm((current) => ({ ...current, unitPrice: data.value }))} />
          </Field>
          <Field label="Discount Percent">
            <Input size="small" type="number" value={form.discountPercent} onChange={(_, data) => setForm((current) => ({ ...current, discountPercent: data.value }))} />
          </Field>
          <Field label="Discount Amount">
            <Input size="small" type="number" value={form.discountAmount} onChange={(_, data) => setForm((current) => ({ ...current, discountAmount: data.value }))} />
          </Field>
          <Field label="Tax Amount">
            <Input size="small" type="number" value={form.taxAmount} onChange={(_, data) => setForm((current) => ({ ...current, taxAmount: data.value }))} />
          </Field>
          <Field label="Description">
            <Textarea value={form.description} onChange={(_, data) => setForm((current) => ({ ...current, description: data.value }))} />
          </Field>
          <Field label="Sort Order">
            <Input size="small" type="number" value={form.sortOrder} onChange={(_, data) => setForm((current) => ({ ...current, sortOrder: data.value }))} />
          </Field>
        </FormSectionCard>
      </SubgridModalForm>

      <SubgridDeleteConfirmDialog
        open={Boolean(deleteTarget)}
        title="Delete Product"
        message="Delete this opportunity product?"
        onConfirm={() => void remove()}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  )
}

export function OpportunityCompetitorsPanel({ opportunityId, editable }: { opportunityId: string; editable?: boolean }) {
  const { hasPermission } = useAuth()
  const canView = hasPermission('OpportunityCompetitors.View')
  const canCreate = editable && hasPermission('OpportunityCompetitors.Create')
  const canUpdate = editable && hasPermission('OpportunityCompetitors.Update')
  const canDelete = editable && hasPermission('OpportunityCompetitors.Delete')
  const canSetPrimary = editable && hasPermission('OpportunityCompetitors.SetPrimary')
  const [rows, setRows] = useState<OpportunityCompetitor[]>([])
  const [form, setForm] = useState<CompetitorForm>(emptyCompetitor)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [formOpen, setFormOpen] = useState(false)
  const [success, setSuccess] = useState('')
  const [deleteTarget, setDeleteTarget] = useState<OpportunityCompetitor | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  const sortedRows = useMemo(() => [...rows].sort((a, b) => Number(b.isPrimaryCompetitor) - Number(a.isPrimaryCompetitor) || a.competitorName.localeCompare(b.competitorName)), [rows])

  const load = useCallback(async () => {
    if (!canView) return
    setLoading(true)
    setError('')
    try {
      const { data } = await api.get<PagedResult<OpportunityCompetitor>>(`api/opportunities/${opportunityId}/competitors`, {
        params: { page: 1, pageSize: 100 },
      })
      setRows(data.items)
    } catch {
      setError('Failed to load competitors.')
    } finally {
      setLoading(false)
    }
  }, [canView, opportunityId])

  useEffect(() => {
    void Promise.resolve().then(load)
  }, [load])

  const reset = () => {
    setEditingId(null)
    setForm(emptyCompetitor)
  }

  const openAdd = () => {
    reset()
    setFormOpen(true)
  }

  const edit = (row: OpportunityCompetitor) => {
    setEditingId(row.id)
    setForm({
      competitorName: row.competitorName,
      strengths: row.strengths ?? '',
      weaknesses: row.weaknesses ?? '',
      threatLevelId: row.threatLevelId ?? '',
      isPrimaryCompetitor: row.isPrimaryCompetitor,
      notes: row.notes ?? '',
    })
    setFormOpen(true)
  }

  const save = async () => {
    if (!form.competitorName.trim()) {
      setError('Competitor name is required.')
      return
    }

    const payload = {
      competitorName: form.competitorName.trim(),
      strengths: nullIfBlank(form.strengths),
      weaknesses: nullIfBlank(form.weaknesses),
      threatLevelId: nullIfBlank(form.threatLevelId),
      isPrimaryCompetitor: form.isPrimaryCompetitor,
      notes: nullIfBlank(form.notes),
    }

    setLoading(true)
    setError('')
    setSuccess('')
    try {
      if (editingId) {
        await api.put(`api/opportunity-competitors/${editingId}`, payload)
      } else {
        await api.post(`api/opportunities/${opportunityId}/competitors`, payload)
      }
      reset()
      setFormOpen(false)
      setSuccess(editingId ? 'Competitor updated successfully.' : 'Competitor created successfully.')
      await load()
    } catch {
      setError('Failed to save competitor.')
    } finally {
      setLoading(false)
    }
  }

  const setPrimary = async (id: string) => {
    setLoading(true)
    setError('')
    setSuccess('')
    try {
      await api.post(`api/opportunity-competitors/${id}/set-primary`)
      setSuccess('Primary competitor updated successfully.')
      await load()
    } catch {
      setError('Failed to set primary competitor.')
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
      await api.delete(`api/opportunity-competitors/${deleteTarget.id}`)
      setDeleteTarget(null)
      setSuccess('Competitor deleted successfully.')
      await load()
    } catch {
      setError('Failed to delete competitor.')
    } finally {
      setLoading(false)
    }
  }

  if (!canView) return <div className={styles.emptyState}>Access denied.</div>

  return (
    <div className={styles.sectionStack}>
      {success ? <MessageBar intent="success"><MessageBarBody>{success}</MessageBarBody></MessageBar> : null}
      {error ? <MessageBar intent="error"><MessageBarBody>{error}</MessageBarBody></MessageBar> : null}

      <RelatedRecordsSubgrid
        title="Competitors"
        addLabel={canCreate ? 'Add Competitor' : undefined}
        onAdd={canCreate ? openAdd : undefined}
        onRefresh={() => void load()}
        loading={loading}
        error={error}
        hasRows={sortedRows.length > 0}
        emptyMessage="No competitors."
        emptyActionLabel={canCreate ? 'Add Competitor' : undefined}
        onEmptyAction={canCreate ? openAdd : undefined}
      >
        <div className={styles.timeline}>
          {sortedRows.map((row) => (
            <article className={styles.timelineItem} key={row.id}>
              <div className={styles.timelineTop}>
                <div>
                  <p className={styles.timelineTitle}>{row.competitorName}{row.isPrimaryCompetitor ? ' - Primary' : ''}</p>
                  <p className={styles.timelineMeta}>{row.threatLevelName ?? 'Threat not set'}</p>
                </div>
                <div className={styles.inlineActions}>
                  <Button size="small" appearance="subtle" onClick={() => void setPrimary(row.id)} disabled={!canSetPrimary || row.isPrimaryCompetitor}>Set Primary</Button>
                  <SubgridRowActions
                    onView={() => edit(row)}
                    onEdit={canUpdate ? () => edit(row) : undefined}
                    onDelete={canDelete ? () => setDeleteTarget(row) : undefined}
                    disableEdit={!canUpdate}
                    disableDelete={!canDelete}
                  />
                </div>
              </div>
              {row.strengths ? <p className={styles.timelineBody}>Strengths: {row.strengths}</p> : null}
              {row.weaknesses ? <p className={styles.timelineBody}>Weaknesses: {row.weaknesses}</p> : null}
              {row.notes ? <p className={styles.timelineBody}>{row.notes}</p> : null}
            </article>
          ))}
        </div>
      </RelatedRecordsSubgrid>

      <SubgridModalForm
        open={formOpen}
        title={editingId ? 'Edit Competitor' : 'Add Competitor'}
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
        <FormSectionCard title="Competitor Details">
          <Field label="Competitor Name" required>
            <Input size="small" value={form.competitorName} onChange={(_, data) => setForm((current) => ({ ...current, competitorName: data.value }))} />
          </Field>
          <Field label="Threat Level">
            <LookupCombobox fieldKey="threatLevelId" value={form.threatLevelId} onChange={(value) => setForm((current) => ({ ...current, threatLevelId: value }))} />
          </Field>
          <Field label="Primary Competitor">
            <Switch checked={form.isPrimaryCompetitor} onChange={(_, data) => setForm((current) => ({ ...current, isPrimaryCompetitor: Boolean(data.checked) }))} />
          </Field>
          <Field label="Strengths">
            <Textarea value={form.strengths} onChange={(_, data) => setForm((current) => ({ ...current, strengths: data.value }))} />
          </Field>
          <Field label="Weaknesses">
            <Textarea value={form.weaknesses} onChange={(_, data) => setForm((current) => ({ ...current, weaknesses: data.value }))} />
          </Field>
          <Field label="Notes">
            <Textarea value={form.notes} onChange={(_, data) => setForm((current) => ({ ...current, notes: data.value }))} />
          </Field>
        </FormSectionCard>
      </SubgridModalForm>

      <SubgridDeleteConfirmDialog
        open={Boolean(deleteTarget)}
        title="Delete Competitor"
        message="Delete this competitor?"
        onConfirm={() => void remove()}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  )
}

export function OpportunityActivitiesPanel({ opportunityId, editable }: { opportunityId: string; editable?: boolean }) {
  const { hasPermission } = useAuth()
  const canView = hasPermission('OpportunityActivities.View')
  const canCreate = editable && hasPermission('OpportunityActivities.Create')
  const canUpdate = editable && hasPermission('OpportunityActivities.Update')
  const canDelete = editable && hasPermission('OpportunityActivities.Delete')
  const canComplete = editable && hasPermission('OpportunityActivities.Complete')
  const [rows, setRows] = useState<OpportunityActivity[]>([])
  const [form, setForm] = useState<ActivityForm>(emptyActivity)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [formOpen, setFormOpen] = useState(false)
  const [success, setSuccess] = useState('')
  const [deleteTarget, setDeleteTarget] = useState<OpportunityActivity | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  const sortedRows = useMemo(() => [...rows].sort((a, b) => new Date(b.activityDate).getTime() - new Date(a.activityDate).getTime()), [rows])

  useEffect(() => {
    if (form.statusId) return
    void loadLookupOptionsByCategoryCode('ACTIVITY_STATUS').then((statuses) => {
      const open = statuses.find((item) => item.label.toLowerCase() === 'open') ?? statuses[0]
      if (open) {
        setForm((current) => ({ ...current, statusId: open.value }))
      }
    })
  }, [form.statusId])

  const load = useCallback(async () => {
    if (!canView) return
    setLoading(true)
    setError('')
    try {
      const { data } = await api.get<PagedResult<OpportunityActivity>>(`api/opportunities/${opportunityId}/activities`, {
        params: { page: 1, pageSize: 100 },
      })
      setRows(data.items)
    } catch {
      setError('Failed to load activities.')
    } finally {
      setLoading(false)
    }
  }, [canView, opportunityId])

  useEffect(() => {
    void Promise.resolve().then(load)
  }, [load])

  const reset = () => {
    setEditingId(null)
    setForm((current) => ({ ...emptyActivity, statusId: current.statusId }))
  }

  const openAdd = () => {
    reset()
    setFormOpen(true)
  }

  const edit = (row: OpportunityActivity) => {
    setEditingId(row.id)
    setForm({
      contactId: row.contactId ?? '',
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
    setFormOpen(true)
  }

  const save = async () => {
    if (!form.activityTypeId || !form.statusId || !form.subject.trim()) {
      setError('Activity type, status, and subject are required.')
      return
    }

    const payload = {
      contactId: nullIfBlank(form.contactId),
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
    setSuccess('')
    try {
      if (editingId) {
        await api.put(`api/opportunity-activities/${editingId}`, payload)
      } else {
        await api.post(`api/opportunities/${opportunityId}/activities`, payload)
      }
      reset()
      setFormOpen(false)
      setSuccess(editingId ? 'Activity updated successfully.' : 'Activity created successfully.')
      await load()
    } catch {
      setError('Failed to save activity.')
    } finally {
      setLoading(false)
    }
  }

  const complete = async (id: string) => {
    setLoading(true)
    setError('')
    setSuccess('')
    try {
      await api.post(`api/opportunity-activities/${id}/complete`, {})
      setSuccess('Activity marked as complete.')
      await load()
    } catch {
      setError('Failed to complete activity.')
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
      await api.delete(`api/opportunity-activities/${deleteTarget.id}`)
      setDeleteTarget(null)
      setSuccess('Activity deleted successfully.')
      await load()
    } catch {
      setError('Failed to delete activity.')
    } finally {
      setLoading(false)
    }
  }

  if (!canView) return <div className={styles.emptyState}>Access denied.</div>

  return (
    <div className={styles.sectionStack}>
      {success ? <MessageBar intent="success"><MessageBarBody>{success}</MessageBarBody></MessageBar> : null}
      {error ? <MessageBar intent="error"><MessageBarBody>{error}</MessageBarBody></MessageBar> : null}

      <RelatedRecordsSubgrid
        title="Activities"
        addLabel={canCreate ? 'Add Activity' : undefined}
        onAdd={canCreate ? openAdd : undefined}
        onRefresh={() => void load()}
        loading={loading}
        error={error}
        hasRows={sortedRows.length > 0}
        emptyMessage="No activities."
        emptyActionLabel={canCreate ? 'Add Activity' : undefined}
        onEmptyAction={canCreate ? openAdd : undefined}
      >
        <div className={styles.timeline}>
          {sortedRows.map((row) => (
            <article className={styles.timelineItem} key={row.id}>
              <div className={styles.timelineTop}>
                <div>
                  <p className={styles.timelineTitle}>{row.subject}</p>
                  <p className={styles.timelineMeta}>
                    {[row.activityTypeName, row.statusName, row.priorityName, row.contactName, formatDateTime(row.activityDate)].filter(Boolean).join(' | ')}
                  </p>
                </div>
                <div className={styles.inlineActions}>
                  {canComplete ? <Button size="small" appearance="subtle" onClick={() => void complete(row.id)} disabled={!canComplete || Boolean(row.completedDate)}>Complete</Button> : null}
                  <SubgridRowActions
                    onView={() => edit(row)}
                    onEdit={canUpdate ? () => edit(row) : undefined}
                    onDelete={canDelete ? () => setDeleteTarget(row) : undefined}
                    disableEdit={!canUpdate}
                    disableDelete={!canDelete}
                  />
                </div>
              </div>
              {row.completedDate ? <p className={styles.timelineMeta}>Completed: {formatDateTime(row.completedDate)}</p> : null}
              {row.description ? <p className={styles.timelineBody}>{row.description}</p> : null}
            </article>
          ))}
        </div>
      </RelatedRecordsSubgrid>

      <SubgridModalForm
        open={formOpen}
        title={editingId ? 'Edit Activity' : 'Add Activity'}
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
        <FormSectionCard title="Activity Details">
          <Field label="Activity Type" required>
            <LookupCombobox fieldKey="activityTypeId" value={form.activityTypeId} onChange={(value) => setForm((current) => ({ ...current, activityTypeId: value }))} />
          </Field>
          <Field label="Status" required>
            <LookupCombobox fieldKey="statusId" value={form.statusId} onChange={(value) => setForm((current) => ({ ...current, statusId: value }))} />
          </Field>
          <Field label="Subject" required>
            <Input size="small" value={form.subject} onChange={(_, data) => setForm((current) => ({ ...current, subject: data.value }))} />
          </Field>
          <Field label="Contact">
            <LookupCombobox fieldKey="contactId" value={form.contactId} onChange={(value) => setForm((current) => ({ ...current, contactId: value }))} />
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
        </FormSectionCard>
      </SubgridModalForm>

      <SubgridDeleteConfirmDialog
        open={Boolean(deleteTarget)}
        title="Delete Activity"
        message="Delete this opportunity activity?"
        onConfirm={() => void remove()}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  )
}

export function OpportunityTimelinePanel({ opportunityId }: { opportunityId: string }) {
  const { hasPermission } = useAuth()
  const canView = hasPermission('Opportunities.ViewTimeline')
  const [rows, setRows] = useState<OpportunityTimelineItem[]>([])
  const [error, setError] = useState('')

  useEffect(() => {
    if (!canView) return
    void (async () => {
      setError('')
      try {
        const { data } = await api.get<OpportunityTimelineItem[]>(`api/opportunities/${opportunityId}/timeline`)
        setRows(data)
      } catch {
        setError('Failed to load opportunity timeline.')
      }
    })()
  }, [canView, opportunityId])

  if (!canView) return <div className={styles.emptyState}>Access denied.</div>

  return (
    <div className={styles.timeline}>
      {error ? <MessageBar intent="error"><MessageBarBody>{error}</MessageBarBody></MessageBar> : null}
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
