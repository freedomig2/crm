import { Button, Field, Input, MessageBar, MessageBarBody, Spinner } from '@fluentui/react-components'
import { useEffect, useMemo, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { DeleteConfirmDialog } from '../components/crud/DeleteConfirmDialog'
import { FormSectionCard, LookupCombobox } from '../components/entity-ui/EntityComponents'
import { FilterField } from '../components/filters/FilterField'
import { DenseDataGrid, type DenseColumn, type DenseSort } from '../components/grid/DenseDataGrid'
import { useListQueryState } from '../hooks/useListQueryState'
import { CommandBar } from '../layout/components/CommandBar'
import { PageHeader } from '../layout/components/PageHeader'
import type { PagedResult, Quote, QuoteLine } from '../types/models'
import styles from './Sales.module.css'

type QuoteLineQuery = {
  page: number
  pageSize: number
  search: string
  sortBy: string
  sortDir: 'asc' | 'desc'
  productId: string
  productBundleId: string
}

type QuoteLineForm = {
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

const emptyForm: QuoteLineForm = {
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

export function QuoteLinesPage() {
  const navigate = useNavigate()
  const { id: quoteId } = useParams()
  const { hasPermission } = useAuth()
  const canView = hasPermission('QuoteLines.View')
  const canCreate = hasPermission('QuoteLines.Create')
  const canUpdate = hasPermission('QuoteLines.Update')
  const canDelete = hasPermission('QuoteLines.Delete')
  const [quote, setQuote] = useState<Quote | null>(null)
  const [rows, setRows] = useState<QuoteLine[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [editingId, setEditingId] = useState<string | null>(null)
  const [deleteTarget, setDeleteTarget] = useState<QuoteLine | null>(null)
  const [form, setForm] = useState<QuoteLineForm>(emptyForm)

  const defaultQuery: QuoteLineQuery = {
    page: 1,
    pageSize: 20,
    search: '',
    sortBy: '',
    sortDir: 'asc',
    productId: '',
    productBundleId: '',
  }

  const { query, setQuery } = useListQueryState<QuoteLineQuery>({ defaults: defaultQuery, numberKeys: ['page', 'pageSize'] })
  const [draftFilters, setDraftFilters] = useState<Pick<QuoteLineQuery, 'productId' | 'productBundleId'>>({
    productId: query.productId,
    productBundleId: query.productBundleId,
  })

  const loadQuote = async () => {
    if (!quoteId) {
      return
    }

    try {
      const { data } = await api.get<Quote>(`api/quotes/${quoteId}`)
      setQuote(data)
    } catch {
      setQuote(null)
    }
  }

  const loadLines = async () => {
    if (!quoteId || !canView) {
      return
    }

    setLoading(true)
    setError('')
    try {
      const { data } = await api.get<PagedResult<QuoteLine>>(`api/quotes/${quoteId}/lines`, {
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
      setError('Failed to load quote lines.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void Promise.resolve().then(async () => {
      await Promise.all([loadQuote(), loadLines()])
    })
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [quoteId, canView, query])

  const resetForm = () => {
    setEditingId(null)
    setForm(emptyForm)
  }

  const editRow = (row: QuoteLine) => {
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
  }

  const saveLine = async () => {
    if (!quoteId || (!canCreate && !canUpdate)) {
      return
    }

    if (!form.productName.trim()) {
      setError('Product name is required.')
      return
    }

    setLoading(true)
    setError('')
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
        await api.put(`api/quotes/lines/${editingId}`, payload)
      } else {
        await api.post(`api/quotes/${quoteId}/lines`, payload)
      }

      resetForm()
      await Promise.all([loadLines(), loadQuote()])
    } catch {
      setError('Failed to save quote line.')
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
    try {
      await api.delete(`api/quotes/lines/${deleteTarget.id}`)
      setDeleteTarget(null)
      await Promise.all([loadLines(), loadQuote()])
    } catch {
      setError('Failed to delete quote line.')
    } finally {
      setLoading(false)
    }
  }

  const activeFilterCount = [query.productId, query.productBundleId].filter(Boolean).length

  const columns = useMemo<DenseColumn<QuoteLine>[]>(
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
        <PageHeader title="Quote Lines" subtitle="Manage line items for a selected quote." />
        <MessageBar intent="error"><MessageBarBody>You do not have permission to view quote lines.</MessageBarBody></MessageBar>
      </div>
    )
  }

  return (
    <div>
      <PageHeader
        title="Quote Lines"
        subtitle={quote ? `${quote.quoteNumber} - ${quote.accountName ?? ''}` : 'Manage line items for a selected quote.'}
        actions={[
          { key: 'back', label: 'Back to Quotes', onClick: () => navigate('/sales/quotes'), appearance: 'subtle' },
          { key: 'edit-quote', label: 'Edit Quote', onClick: () => quoteId && navigate(`/sales/quotes/${quoteId}/edit`), appearance: 'secondary' },
        ]}
      />
      <CommandBar actions={[{ key: 'back', label: 'Back to Quotes', onClick: () => navigate('/sales/quotes') }]} />

      {quote ? (
        <div className={styles.metricGrid} style={{ marginBottom: 10 }}>
          <div className={styles.metricCard}><p className={styles.metricLabel}>Subtotal</p><p className={styles.metricValue}>{quote.subtotalAmount}</p></div>
          <div className={styles.metricCard}><p className={styles.metricLabel}>Discount</p><p className={styles.metricValue}>{quote.discountAmount}</p></div>
          <div className={styles.metricCard}><p className={styles.metricLabel}>Tax</p><p className={styles.metricValue}>{quote.taxAmount}</p></div>
          <div className={styles.metricCard}><p className={styles.metricLabel}>Total</p><p className={styles.metricValue}>{quote.totalAmount}</p></div>
        </div>
      ) : null}

      {(canCreate || canUpdate) ? (
        <FormSectionCard title={editingId ? 'Edit Quote Line' : 'Add Quote Line'}>
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
          <div className={styles.inlineActions}>
            <Button size="small" appearance="primary" disabled={loading} onClick={() => void saveLine()}>{editingId ? 'Update Line' : 'Add Line'}</Button>
            <Button size="small" appearance="subtle" disabled={loading} onClick={resetForm}>Reset</Button>
          </div>
        </FormSectionCard>
      ) : null}

      {loading ? <Spinner size="small" label="Loading quote lines..." style={{ margin: '8px 0' }} /> : null}
      {error ? <MessageBar intent="error" style={{ marginBottom: 10 }}><MessageBarBody>{error}</MessageBarBody></MessageBar> : null}

      <DenseDataGrid
        rows={rows}
        columns={columns}
        loading={loading}
        totalCount={totalCount}
        page={query.page}
        pageSize={query.pageSize}
        search={query.search}
        sort={query.sortBy ? ({ key: query.sortBy as keyof QuoteLine, dir: query.sortDir }) : null}
        onPageChange={(page) => setQuery((current) => ({ ...current, page }))}
        onPageSizeChange={(pageSize) => setQuery((current) => ({ ...current, pageSize, page: 1 }))}
        onSearchChange={(search) => setQuery((current) => ({ ...current, search, page: 1 }))}
        onSortChange={(sort: DenseSort<QuoteLine> | null) => setQuery((current) => ({ ...current, sortBy: sort ? String(sort.key) : '', sortDir: sort?.dir ?? 'asc', page: 1 }))}
        onEdit={canUpdate ? (row) => editRow(row) : undefined}
        onDelete={canDelete ? (row) => setDeleteTarget(row) : undefined}
        emptyMessage="No quote lines match the current filters."
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

      <DeleteConfirmDialog
        open={Boolean(deleteTarget)}
        title="Delete Quote Line"
        message={`Delete ${deleteTarget?.productName ?? 'this line'} from the quote?`}
        onConfirm={() => void removeLine()}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  )
}
