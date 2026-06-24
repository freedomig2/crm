import { Field, Input, MessageBar, MessageBarBody } from '@fluentui/react-components'
import { useEffect, useMemo, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { FormSectionCard, LookupCombobox } from '../components/entity-ui/EntityComponents'
import { FilterField } from '../components/filters/FilterField'
import { DenseDataGrid, type DenseColumn, type DenseSort } from '../components/grid/DenseDataGrid'
import { RelatedRecordsSubgrid } from '../components/subgrid/RelatedRecordsSubgrid'
import { SubgridDeleteConfirmDialog } from '../components/subgrid/SubgridDeleteConfirmDialog'
import { SubgridModalForm } from '../components/subgrid/SubgridModalForm'
import { useListQueryState } from '../hooks/useListQueryState'
import { CommandBar } from '../layout/components/CommandBar'
import { PageHeader } from '../layout/components/PageHeader'
import type { Invoice, InvoiceLine, PagedResult } from '../types/models'
import styles from './Sales.module.css'

type InvoiceLineQuery = {
  page: number
  pageSize: number
  search: string
  sortBy: string
  sortDir: 'asc' | 'desc'
  productId: string
  productBundleId: string
}

type InvoiceLineForm = {
  productId: string
  productBundleId: string
  productName: string
  description: string
  unitOfMeasureId: string
  quantity: string
  unitPrice: string
  discountPercent: string
  taxRate: string
  sortOrder: string
}

const emptyForm: InvoiceLineForm = {
  productId: '',
  productBundleId: '',
  productName: '',
  description: '',
  unitOfMeasureId: '',
  quantity: '1',
  unitPrice: '0',
  discountPercent: '0',
  taxRate: '0',
  sortOrder: '0',
}

export function InvoiceLinesPage() {
  const navigate = useNavigate()
  const { id: invoiceId } = useParams()
  const { hasPermission } = useAuth()
  const canView = hasPermission('InvoiceLines.View')
  const canCreate = hasPermission('InvoiceLines.Create')
  const canUpdate = hasPermission('InvoiceLines.Update')
  const canDelete = hasPermission('InvoiceLines.Delete')
  const [invoice, setInvoice] = useState<Invoice | null>(null)
  const [rows, setRows] = useState<InvoiceLine[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [editingId, setEditingId] = useState<string | null>(null)
  const [formOpen, setFormOpen] = useState(false)
  const [deleteTarget, setDeleteTarget] = useState<InvoiceLine | null>(null)
  const [form, setForm] = useState<InvoiceLineForm>(emptyForm)

  const defaultQuery: InvoiceLineQuery = {
    page: 1,
    pageSize: 20,
    search: '',
    sortBy: '',
    sortDir: 'asc',
    productId: '',
    productBundleId: '',
  }

  const { query, setQuery } = useListQueryState<InvoiceLineQuery>({ defaults: defaultQuery, numberKeys: ['page', 'pageSize'] })
  const [draftFilters, setDraftFilters] = useState<Pick<InvoiceLineQuery, 'productId' | 'productBundleId'>>({
    productId: query.productId,
    productBundleId: query.productBundleId,
  })

  const loadInvoice = async () => {
    if (!invoiceId) {
      return
    }

    try {
      const { data } = await api.get<Invoice>(`api/invoices/${invoiceId}`)
      setInvoice(data)
    } catch {
      setInvoice(null)
    }
  }

  const loadLines = async () => {
    if (!invoiceId || !canView) {
      return
    }

    setLoading(true)
    setError('')
    try {
      const { data } = await api.get<PagedResult<InvoiceLine>>(`api/invoices/${invoiceId}/lines`, {
        params: {
          page: query.page,
          pageSize: query.pageSize,
          search: query.search || undefined,
          sortBy: query.sortBy || undefined,
          sortDir: query.sortDir,
          productId: query.productId || undefined,
          productBundleId: query.productBundleId || undefined,
        },
      })
      setRows(data.items)
      setTotalCount(data.totalCount)
    } catch {
      setError('Failed to load invoice lines.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void Promise.resolve().then(async () => {
      await Promise.all([loadInvoice(), loadLines()])
    })
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [invoiceId, canView, query])

  const resetForm = () => {
    setEditingId(null)
    setForm(emptyForm)
  }

  const openAddForm = () => {
    resetForm()
    setFormOpen(true)
  }

  const editRow = (row: InvoiceLine) => {
    setEditingId(row.id)
    setForm({
      productId: row.productId ?? '',
      productBundleId: row.productBundleId ?? '',
      productName: row.productName,
      description: row.description ?? '',
      unitOfMeasureId: row.unitOfMeasureId ?? '',
      quantity: String(row.quantity),
      unitPrice: String(row.unitPrice),
      discountPercent: String(row.discountPercent),
      taxRate: String(row.taxRate),
      sortOrder: String(row.sortOrder),
    })
    setFormOpen(true)
  }

  const saveLine = async () => {
    if (!invoiceId || (!canCreate && !canUpdate)) {
      return
    }

    if (!form.productName.trim()) {
      setError('Product name is required.')
      return
    }

    setLoading(true)
    setError('')
    setSuccess('')
    try {
      const payload = {
        productId: form.productId || null,
        productBundleId: form.productBundleId || null,
        productName: form.productName.trim(),
        description: form.description.trim() || null,
        unitOfMeasureId: form.unitOfMeasureId || null,
        quantity: Number(form.quantity || 0),
        unitPrice: Number(form.unitPrice || 0),
        discountPercent: Number(form.discountPercent || 0),
        taxRate: Number(form.taxRate || 0),
        sortOrder: Number(form.sortOrder || 0),
      }

      if (editingId) {
        await api.put(`api/invoices/lines/${editingId}`, payload)
      } else {
        await api.post(`api/invoices/${invoiceId}/lines`, payload)
      }

      resetForm()
      setFormOpen(false)
      setSuccess(editingId ? 'Invoice line updated successfully.' : 'Invoice line created successfully.')
      await loadLines()
    } catch {
      setError('Failed to save invoice line.')
    } finally {
      setLoading(false)
    }
  }

  const removeLine = async () => {
    if (!deleteTarget || !canDelete) {
      return
    }

    setLoading(true)
    setError('')
    setSuccess('')
    try {
      await api.delete(`api/invoices/lines/${deleteTarget.id}`)
      setDeleteTarget(null)
      setSuccess('Invoice line deleted successfully.')
      await loadLines()
    } catch {
      setError('Failed to delete invoice line.')
    } finally {
      setLoading(false)
    }
  }

  const activeFilterCount = [query.productId, query.productBundleId].filter(Boolean).length

  const columns = useMemo<DenseColumn<InvoiceLine>[]>(
    () => [
      { key: 'productName', label: 'Product', sortable: true },
      { key: 'unitOfMeasureName', label: 'UOM', sortable: true },
      { key: 'quantity', label: 'Qty', sortable: true },
      { key: 'unitPrice', label: 'Unit Price', sortable: true },
      { key: 'discountPercent', label: 'Discount %', sortable: true },
      { key: 'taxRate', label: 'Tax %', sortable: true },
      { key: 'lineTotal', label: 'Line Total', sortable: true },
      { key: 'sortOrder', label: 'Sort', sortable: true },
    ],
    [],
  )

  if (!canView) {
    return (
      <div>
        <PageHeader title="Invoice Lines" subtitle="Manage line items for a selected invoice." />
        <MessageBar intent="error"><MessageBarBody>You do not have permission to view invoice lines.</MessageBarBody></MessageBar>
      </div>
    )
  }

  return (
    <div>
      <PageHeader
        title="Invoice Lines"
        subtitle={invoice ? `${invoice.invoiceNumber} - ${invoice.accountName ?? ''}` : 'Manage line items for a selected invoice.'}
        actions={[
          { key: 'back', label: 'Back to Invoices', onClick: () => navigate('/sales/invoices'), appearance: 'subtle' },
          { key: 'edit-invoice', label: 'Edit Invoice', onClick: () => invoiceId && navigate(`/sales/invoices/${invoiceId}/edit`), appearance: 'secondary' },
        ]}
      />
      <CommandBar actions={[{ key: 'back', label: 'Back to Invoices', onClick: () => navigate('/sales/invoices') }]} />

      {invoice ? (
        <div className={styles.metricGrid} style={{ marginBottom: 10 }}>
          <div className={styles.metricCard}><p className={styles.metricLabel}>Subtotal</p><p className={styles.metricValue}>{invoice.subtotalAmount}</p></div>
          <div className={styles.metricCard}><p className={styles.metricLabel}>Discount</p><p className={styles.metricValue}>{invoice.discountAmount}</p></div>
          <div className={styles.metricCard}><p className={styles.metricLabel}>Tax</p><p className={styles.metricValue}>{invoice.taxAmount}</p></div>
          <div className={styles.metricCard}><p className={styles.metricLabel}>Total</p><p className={styles.metricValue}>{invoice.totalAmount}</p></div>
        </div>
      ) : null}

      {success ? <MessageBar intent="success" style={{ marginBottom: 10 }}><MessageBarBody>{success}</MessageBarBody></MessageBar> : null}
      {error ? <MessageBar intent="error" style={{ marginBottom: 10 }}><MessageBarBody>{error}</MessageBarBody></MessageBar> : null}

      <RelatedRecordsSubgrid
        title="Invoice Lines"
        addLabel={canCreate ? 'Add Line' : undefined}
        onAdd={canCreate ? openAddForm : undefined}
        onRefresh={() => void loadLines()}
        loading={loading}
        error={error}
        hasRows={rows.length > 0}
        emptyMessage="No invoice lines match the current filters."
        emptyActionLabel={canCreate ? 'Add Line' : undefined}
        onEmptyAction={canCreate ? openAddForm : undefined}
      >
        <DenseDataGrid
          rows={rows}
          columns={columns}
          loading={loading}
          totalCount={totalCount}
          page={query.page}
          pageSize={query.pageSize}
          search={query.search}
          sort={query.sortBy ? ({ key: query.sortBy as keyof InvoiceLine, dir: query.sortDir }) : null}
          onPageChange={(page) => setQuery((current) => ({ ...current, page }))}
          onPageSizeChange={(pageSize) => setQuery((current) => ({ ...current, pageSize, page: 1 }))}
          onSearchChange={(search) => setQuery((current) => ({ ...current, search, page: 1 }))}
          onSortChange={(sort: DenseSort<InvoiceLine> | null) => setQuery((current) => ({ ...current, sortBy: sort ? String(sort.key) : '', sortDir: sort?.dir ?? 'asc', page: 1 }))}
          onEdit={canUpdate ? (row) => editRow(row) : undefined}
          onDelete={canDelete ? (row) => setDeleteTarget(row) : undefined}
          emptyMessage="No invoice lines match the current filters."
          activeFilterCount={activeFilterCount}
          filterPanel={
            <>
              <FilterField label="Product">
                <LookupCombobox fieldKey="productId" value={draftFilters.productId} onChange={(value) => setDraftFilters((current) => ({ ...current, productId: value }))} />
              </FilterField>
              <FilterField label="Product Bundle">
                <LookupCombobox fieldKey="productBundleId" value={draftFilters.productBundleId} onChange={(value) => setDraftFilters((current) => ({ ...current, productBundleId: value }))} />
              </FilterField>
            </>
          }
          onApplyFilters={() => setQuery((current) => ({ ...current, ...draftFilters, page: 1 }))}
          onCancelFilters={() => setDraftFilters({ productId: query.productId, productBundleId: query.productBundleId })}
          onClearFilters={() => setDraftFilters({ productId: '', productBundleId: '' })}
        />
      </RelatedRecordsSubgrid>

      <SubgridModalForm
        open={formOpen}
        title={editingId ? 'Edit Invoice Line' : 'Add Invoice Line'}
        submitLabel={editingId ? 'Update Line' : 'Add Line'}
        loading={loading}
        onOpenChange={(open) => {
          setFormOpen(open)
          if (!open) {
            resetForm()
          }
        }}
        onSubmit={() => void saveLine()}
      >
        <FormSectionCard title="Line Details">
          <Field label="Product">
            <LookupCombobox fieldKey="productId" value={form.productId} disabled={loading} onChange={(value) => setForm((current) => ({ ...current, productId: value }))} />
          </Field>
          <Field label="Product Bundle">
            <LookupCombobox fieldKey="productBundleId" value={form.productBundleId} disabled={loading} onChange={(value) => setForm((current) => ({ ...current, productBundleId: value }))} />
          </Field>
          <Field label="Product Name" required>
            <Input size="small" value={form.productName} onChange={(_, data) => setForm((current) => ({ ...current, productName: data.value }))} />
          </Field>
          <Field label="Description">
            <Input size="small" value={form.description} onChange={(_, data) => setForm((current) => ({ ...current, description: data.value }))} />
          </Field>
          <Field label="Unit Of Measure">
            <LookupCombobox fieldKey="unitOfMeasureId" value={form.unitOfMeasureId} disabled={loading} onChange={(value) => setForm((current) => ({ ...current, unitOfMeasureId: value }))} />
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
          <Field label="Tax Rate">
            <Input size="small" type="number" value={form.taxRate} onChange={(_, data) => setForm((current) => ({ ...current, taxRate: data.value }))} />
          </Field>
          <Field label="Sort Order">
            <Input size="small" type="number" value={form.sortOrder} onChange={(_, data) => setForm((current) => ({ ...current, sortOrder: data.value }))} />
          </Field>
        </FormSectionCard>
      </SubgridModalForm>

      <SubgridDeleteConfirmDialog
        open={Boolean(deleteTarget)}
        title="Delete Invoice Line"
        message={`Delete ${deleteTarget?.productName ?? 'this line'} from the invoice?`}
        onConfirm={() => void removeLine()}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  )
}