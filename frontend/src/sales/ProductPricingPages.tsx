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
import type { PagedResult, PriceList, PriceListItem, ProductBundle, ProductBundleItem } from '../types/models'
import styles from './Sales.module.css'

type PriceListItemQuery = {
  page: number
  pageSize: number
  search: string
  sortBy: string
  sortDir: 'asc' | 'desc'
  productId: string
}

type BundleItemQuery = {
  page: number
  pageSize: number
  search: string
  sortBy: string
  sortDir: 'asc' | 'desc'
}

type PriceListItemForm = {
  productId: string
  unitPrice: string
  minimumQuantity: string
  maximumQuantity: string
  discountPercent: string
  effectiveFrom: string
  effectiveTo: string
}

type BundleItemForm = {
  productId: string
  quantity: string
  sortOrder: string
}

const emptyPriceListItemForm: PriceListItemForm = {
  productId: '',
  unitPrice: '0',
  minimumQuantity: '',
  maximumQuantity: '',
  discountPercent: '',
  effectiveFrom: '',
  effectiveTo: '',
}

const emptyBundleItemForm: BundleItemForm = {
  productId: '',
  quantity: '1',
  sortOrder: '0',
}

export function PriceListItemsPage() {
  const navigate = useNavigate()
  const { id: priceListId } = useParams()
  const { hasPermission } = useAuth()
  const canView = hasPermission('PriceLists.View')
  const canUpdate = hasPermission('PriceLists.Update')
  const canDelete = hasPermission('PriceLists.Delete')
  const [priceList, setPriceList] = useState<PriceList | null>(null)
  const [rows, setRows] = useState<PriceListItem[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [editingId, setEditingId] = useState<string | null>(null)
  const [deleteTarget, setDeleteTarget] = useState<PriceListItem | null>(null)
  const [form, setForm] = useState<PriceListItemForm>(emptyPriceListItemForm)

  const defaultQuery: PriceListItemQuery = {
    page: 1,
    pageSize: 20,
    search: '',
    sortBy: '',
    sortDir: 'asc',
    productId: '',
  }

  const { query, setQuery } = useListQueryState<PriceListItemQuery>({ defaults: defaultQuery, numberKeys: ['page', 'pageSize'] })
  const [draftFilters, setDraftFilters] = useState<Pick<PriceListItemQuery, 'productId'>>({ productId: query.productId })

  const loadPriceList = async () => {
    if (!priceListId) {
      return
    }

    try {
      const { data } = await api.get<PriceList>(`api/price-lists/${priceListId}`)
      setPriceList(data)
    } catch {
      setPriceList(null)
    }
  }

  const loadItems = async () => {
    if (!priceListId || !canView) {
      return
    }

    setLoading(true)
    setError('')
    try {
      const { data } = await api.get<PagedResult<PriceListItem>>(`api/price-lists/${priceListId}/items`, {
        params: {
          page: query.page,
          pageSize: query.pageSize,
          search: query.search || undefined,
          sortBy: query.sortBy || undefined,
          sortDir: query.sortDir,
          productId: query.productId || undefined,
        },
      })
      setRows(data.items)
      setTotalCount(data.totalCount)
    } catch {
      setError('Failed to load price list items.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void Promise.resolve().then(async () => {
      await Promise.all([loadPriceList(), loadItems()])
    })
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [priceListId, canView, query])

  const resetForm = () => {
    setEditingId(null)
    setForm(emptyPriceListItemForm)
  }

  const editRow = (row: PriceListItem) => {
    setEditingId(row.id)
    setForm({
      productId: row.productId,
      unitPrice: String(row.unitPrice),
      minimumQuantity: row.minimumQuantity === undefined || row.minimumQuantity === null ? '' : String(row.minimumQuantity),
      maximumQuantity: row.maximumQuantity === undefined || row.maximumQuantity === null ? '' : String(row.maximumQuantity),
      discountPercent: row.discountPercent === undefined || row.discountPercent === null ? '' : String(row.discountPercent),
      effectiveFrom: row.effectiveFrom ? row.effectiveFrom.slice(0, 10) : '',
      effectiveTo: row.effectiveTo ? row.effectiveTo.slice(0, 10) : '',
    })
  }

  const saveItem = async () => {
    if (!priceListId || !canUpdate) {
      return
    }

    if (!form.productId) {
      setError('Product is required.')
      return
    }

    setLoading(true)
    setError('')
    try {
      const payload = {
        productId: form.productId,
        unitPrice: Number(form.unitPrice || 0),
        minimumQuantity: form.minimumQuantity.trim() ? Number(form.minimumQuantity) : null,
        maximumQuantity: form.maximumQuantity.trim() ? Number(form.maximumQuantity) : null,
        discountPercent: form.discountPercent.trim() ? Number(form.discountPercent) : null,
        effectiveFrom: form.effectiveFrom || null,
        effectiveTo: form.effectiveTo || null,
      }

      if (editingId) {
        await api.put(`api/price-list-items/${editingId}`, payload)
      } else {
        await api.post(`api/price-lists/${priceListId}/items`, payload)
      }

      resetForm()
      await loadItems()
    } catch {
      setError('Failed to save price list item.')
    } finally {
      setLoading(false)
    }
  }

  const removeItem = async () => {
    if (!deleteTarget || !canDelete) {
      return
    }

    setLoading(true)
    setError('')
    try {
      await api.delete(`api/price-list-items/${deleteTarget.id}`)
      setDeleteTarget(null)
      await loadItems()
    } catch {
      setError('Failed to delete price list item.')
    } finally {
      setLoading(false)
    }
  }

  const activeFilterCount = [query.productId].filter(Boolean).length

  const columns = useMemo<DenseColumn<PriceListItem>[]>(
    () => [
      { key: 'productCode', label: 'Product #', sortable: true },
      { key: 'productName', label: 'Product', sortable: true },
      { key: 'unitPrice', label: 'Unit Price', sortable: true },
      { key: 'minimumQuantity', label: 'Min Qty', sortable: true },
      { key: 'maximumQuantity', label: 'Max Qty', sortable: true },
      { key: 'discountPercent', label: 'Discount %', sortable: true },
      { key: 'effectiveFrom', label: 'Effective From', sortable: true },
      { key: 'effectiveTo', label: 'Effective To', sortable: true },
    ],
    [],
  )

  if (!canView) {
    return (
      <div>
        <PageHeader title="Price List Items" subtitle="Manage product prices for a selected price list." />
        <MessageBar intent="error"><MessageBarBody>You do not have permission to view price list items.</MessageBarBody></MessageBar>
      </div>
    )
  }

  return (
    <div>
      <PageHeader
        title="Price List Items"
        subtitle={priceList ? `${priceList.priceListNumber} - ${priceList.name}` : 'Manage product prices for a selected price list.'}
        actions={[{ key: 'back', label: 'Back to Price Lists', onClick: () => navigate('/sales/price-lists'), appearance: 'subtle' }]}
      />
      <CommandBar actions={[{ key: 'back', label: 'Back to Price Lists', onClick: () => navigate('/sales/price-lists') }]} />

      {canUpdate ? (
        <FormSectionCard title={editingId ? 'Edit Price List Item' : 'Add Price List Item'}>
          <Field label="Product" required>
            <LookupCombobox fieldKey="productId" value={form.productId} disabled={loading} onChange={(value) => setForm((current) => ({ ...current, productId: value }))} />
          </Field>
          <Field label="Unit Price" required>
            <Input size="small" type="number" value={form.unitPrice} onChange={(_, data) => setForm((current) => ({ ...current, unitPrice: data.value }))} />
          </Field>
          <Field label="Minimum Quantity">
            <Input size="small" type="number" value={form.minimumQuantity} onChange={(_, data) => setForm((current) => ({ ...current, minimumQuantity: data.value }))} />
          </Field>
          <Field label="Maximum Quantity">
            <Input size="small" type="number" value={form.maximumQuantity} onChange={(_, data) => setForm((current) => ({ ...current, maximumQuantity: data.value }))} />
          </Field>
          <Field label="Discount Percent">
            <Input size="small" type="number" value={form.discountPercent} onChange={(_, data) => setForm((current) => ({ ...current, discountPercent: data.value }))} />
          </Field>
          <Field label="Effective From">
            <Input size="small" type="date" value={form.effectiveFrom} onChange={(_, data) => setForm((current) => ({ ...current, effectiveFrom: data.value }))} />
          </Field>
          <Field label="Effective To">
            <Input size="small" type="date" value={form.effectiveTo} onChange={(_, data) => setForm((current) => ({ ...current, effectiveTo: data.value }))} />
          </Field>
          <div className={styles.inlineActions}>
            <Button size="small" appearance="primary" disabled={loading} onClick={() => void saveItem()}>{editingId ? 'Update Item' : 'Add Item'}</Button>
            <Button size="small" appearance="subtle" disabled={loading} onClick={resetForm}>Reset</Button>
          </div>
        </FormSectionCard>
      ) : null}

      {loading ? <Spinner size="small" label="Loading price list items..." style={{ margin: '8px 0' }} /> : null}
      {error ? <MessageBar intent="error" style={{ marginBottom: 10 }}><MessageBarBody>{error}</MessageBarBody></MessageBar> : null}

      <DenseDataGrid
        rows={rows}
        columns={columns}
        loading={loading}
        totalCount={totalCount}
        page={query.page}
        pageSize={query.pageSize}
        search={query.search}
        sort={query.sortBy ? ({ key: query.sortBy as keyof PriceListItem, dir: query.sortDir }) : null}
        onPageChange={(page) => setQuery((current) => ({ ...current, page }))}
        onPageSizeChange={(pageSize) => setQuery((current) => ({ ...current, pageSize, page: 1 }))}
        onSearchChange={(search) => setQuery((current) => ({ ...current, search, page: 1 }))}
        onSortChange={(sort: DenseSort<PriceListItem> | null) => setQuery((current) => ({ ...current, sortBy: sort ? String(sort.key) : '', sortDir: sort?.dir ?? 'asc', page: 1 }))}
        onEdit={canUpdate ? (row) => editRow(row) : undefined}
        onDelete={canDelete ? (row) => setDeleteTarget(row) : undefined}
        emptyMessage="No price list items match the current filters."
        activeFilterCount={activeFilterCount}
        filterPanel={<FilterField label="Product"><LookupCombobox fieldKey="productId" value={draftFilters.productId} onChange={(value) => setDraftFilters((current) => ({ ...current, productId: value }))} /></FilterField>}
        onApplyFilters={() => setQuery((current) => ({ ...current, ...draftFilters, page: 1 }))}
        onCancelFilters={() => setDraftFilters({ productId: query.productId })}
        onClearFilters={() => setDraftFilters({ productId: '' })}
      />

      <DeleteConfirmDialog
        open={Boolean(deleteTarget)}
        title="Delete Price List Item"
        message={`Delete ${deleteTarget?.productName ?? 'this item'} from the price list?`}
        onConfirm={() => void removeItem()}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  )
}

export function ProductBundleItemsPage() {
  const navigate = useNavigate()
  const { id: bundleId } = useParams()
  const { hasPermission } = useAuth()
  const canView = hasPermission('ProductBundles.View')
  const canUpdate = hasPermission('ProductBundles.Update')
  const canDelete = hasPermission('ProductBundles.Delete')
  const [bundle, setBundle] = useState<ProductBundle | null>(null)
  const [rows, setRows] = useState<ProductBundleItem[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [editingId, setEditingId] = useState<string | null>(null)
  const [deleteTarget, setDeleteTarget] = useState<ProductBundleItem | null>(null)
  const [form, setForm] = useState<BundleItemForm>(emptyBundleItemForm)

  const defaultQuery: BundleItemQuery = {
    page: 1,
    pageSize: 20,
    search: '',
    sortBy: '',
    sortDir: 'asc',
  }

  const { query, setQuery } = useListQueryState<BundleItemQuery>({ defaults: defaultQuery, numberKeys: ['page', 'pageSize'] })

  const loadBundle = async () => {
    if (!bundleId) {
      return
    }

    try {
      const { data } = await api.get<ProductBundle>(`api/product-bundles/${bundleId}`)
      setBundle(data)
    } catch {
      setBundle(null)
    }
  }

  const loadItems = async () => {
    if (!bundleId || !canView) {
      return
    }

    setLoading(true)
    setError('')
    try {
      const { data } = await api.get<PagedResult<ProductBundleItem>>(`api/product-bundles/${bundleId}/items`, {
        params: {
          page: query.page,
          pageSize: query.pageSize,
          search: query.search || undefined,
          sortBy: query.sortBy || undefined,
          sortDir: query.sortDir,
        },
      })
      setRows(data.items)
      setTotalCount(data.totalCount)
    } catch {
      setError('Failed to load bundle items.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void Promise.resolve().then(async () => {
      await Promise.all([loadBundle(), loadItems()])
    })
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [bundleId, canView, query])

  const resetForm = () => {
    setEditingId(null)
    setForm(emptyBundleItemForm)
  }

  const editRow = (row: ProductBundleItem) => {
    setEditingId(row.id)
    setForm({
      productId: row.productId,
      quantity: String(row.quantity),
      sortOrder: String(row.sortOrder),
    })
  }

  const saveItem = async () => {
    if (!bundleId || !canUpdate) {
      return
    }

    if (!form.productId) {
      setError('Product is required.')
      return
    }

    setLoading(true)
    setError('')
    try {
      const payload = {
        productId: form.productId,
        quantity: Number(form.quantity || 0),
        sortOrder: Number(form.sortOrder || 0),
      }

      if (editingId) {
        await api.put(`api/product-bundle-items/${editingId}`, payload)
      } else {
        await api.post(`api/product-bundles/${bundleId}/items`, payload)
      }

      resetForm()
      await loadItems()
    } catch {
      setError('Failed to save bundle item.')
    } finally {
      setLoading(false)
    }
  }

  const removeItem = async () => {
    if (!deleteTarget || !canDelete) {
      return
    }

    setLoading(true)
    setError('')
    try {
      await api.delete(`api/product-bundle-items/${deleteTarget.id}`)
      setDeleteTarget(null)
      await loadItems()
    } catch {
      setError('Failed to delete bundle item.')
    } finally {
      setLoading(false)
    }
  }

  const columns = useMemo<DenseColumn<ProductBundleItem>[]>(
    () => [
      { key: 'productCode', label: 'Product #', sortable: true },
      { key: 'productName', label: 'Product', sortable: true },
      { key: 'quantity', label: 'Quantity', sortable: true },
      { key: 'sortOrder', label: 'Sort Order', sortable: true },
      { key: 'unitPrice', label: 'Unit Price', sortable: true },
      { key: 'lineTotal', label: 'Line Total', sortable: true },
    ],
    [],
  )

  if (!canView) {
    return (
      <div>
        <PageHeader title="Bundle Items" subtitle="Manage component products for a bundle." />
        <MessageBar intent="error"><MessageBarBody>You do not have permission to view bundle items.</MessageBarBody></MessageBar>
      </div>
    )
  }

  return (
    <div>
      <PageHeader
        title="Bundle Items"
        subtitle={bundle ? `${bundle.bundleCode} - ${bundle.name}` : 'Manage component products for a bundle.'}
        actions={[{ key: 'back', label: 'Back to Product Bundles', onClick: () => navigate('/sales/product-bundles'), appearance: 'subtle' }]}
      />
      <CommandBar actions={[{ key: 'back', label: 'Back to Product Bundles', onClick: () => navigate('/sales/product-bundles') }]} />

      {canUpdate ? (
        <FormSectionCard title={editingId ? 'Edit Bundle Item' : 'Add Bundle Item'}>
          <Field label="Product" required>
            <LookupCombobox fieldKey="productId" value={form.productId} disabled={loading} onChange={(value) => setForm((current) => ({ ...current, productId: value }))} />
          </Field>
          <Field label="Quantity" required>
            <Input size="small" type="number" value={form.quantity} onChange={(_, data) => setForm((current) => ({ ...current, quantity: data.value }))} />
          </Field>
          <Field label="Sort Order">
            <Input size="small" type="number" value={form.sortOrder} onChange={(_, data) => setForm((current) => ({ ...current, sortOrder: data.value }))} />
          </Field>
          <div className={styles.inlineActions}>
            <Button size="small" appearance="primary" disabled={loading} onClick={() => void saveItem()}>{editingId ? 'Update Item' : 'Add Item'}</Button>
            <Button size="small" appearance="subtle" disabled={loading} onClick={resetForm}>Reset</Button>
          </div>
        </FormSectionCard>
      ) : null}

      {loading ? <Spinner size="small" label="Loading bundle items..." style={{ margin: '8px 0' }} /> : null}
      {error ? <MessageBar intent="error" style={{ marginBottom: 10 }}><MessageBarBody>{error}</MessageBarBody></MessageBar> : null}

      <DenseDataGrid
        rows={rows}
        columns={columns}
        loading={loading}
        totalCount={totalCount}
        page={query.page}
        pageSize={query.pageSize}
        search={query.search}
        sort={query.sortBy ? ({ key: query.sortBy as keyof ProductBundleItem, dir: query.sortDir }) : null}
        onPageChange={(page) => setQuery((current) => ({ ...current, page }))}
        onPageSizeChange={(pageSize) => setQuery((current) => ({ ...current, pageSize, page: 1 }))}
        onSearchChange={(search) => setQuery((current) => ({ ...current, search, page: 1 }))}
        onSortChange={(sort: DenseSort<ProductBundleItem> | null) => setQuery((current) => ({ ...current, sortBy: sort ? String(sort.key) : '', sortDir: sort?.dir ?? 'asc', page: 1 }))}
        onEdit={canUpdate ? (row) => editRow(row) : undefined}
        onDelete={canDelete ? (row) => setDeleteTarget(row) : undefined}
        emptyMessage="No bundle items match the current filters."
        activeFilterCount={0}
        filterPanel={<FilterField label="Bundle Status"><Input size="small" value="All" readOnly /></FilterField>}
        onApplyFilters={() => undefined}
        onCancelFilters={() => undefined}
        onClearFilters={() => undefined}
      />

      <DeleteConfirmDialog
        open={Boolean(deleteTarget)}
        title="Delete Bundle Item"
        message={`Delete ${deleteTarget?.productName ?? 'this item'} from the bundle?`}
        onConfirm={() => void removeItem()}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  )
}
