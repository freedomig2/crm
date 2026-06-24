import { Field, Input, MessageBar, MessageBarBody } from '@fluentui/react-components'
import { useEffect, useMemo, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { FormSectionCard } from '../components/entity-ui/EntityComponents'
import { DateRangeFilterField } from '../components/filters/DateRangeFilterField'
import { FilterField } from '../components/filters/FilterField'
import { DenseDataGrid, type DenseColumn, type DenseSort } from '../components/grid/DenseDataGrid'
import { RelatedRecordsSubgrid } from '../components/subgrid/RelatedRecordsSubgrid'
import { SubgridModalForm } from '../components/subgrid/SubgridModalForm'
import { useListQueryState } from '../hooks/useListQueryState'
import { CommandBar } from '../layout/components/CommandBar'
import { PageHeader } from '../layout/components/PageHeader'
import type { Document, DocumentVersion, PagedResult } from '../types/models'
import styles from '../sales/Sales.module.css'

type VersionQuery = {
  page: number
  pageSize: number
  search: string
  sortBy: string
  sortDir: 'asc' | 'desc'
  createdFrom: string
  createdTo: string
  versionFrom: string
  versionTo: string
}

type AddVersionForm = {
  versionNumber: string
  fileName: string
  contentType: string
  fileSizeBytes: string
  storagePath: string
  changeSummary: string
}

const defaultQuery: VersionQuery = {
  page: 1,
  pageSize: 20,
  search: '',
  sortBy: '',
  sortDir: 'desc',
  createdFrom: '',
  createdTo: '',
  versionFrom: '',
  versionTo: '',
}

const emptyForm: AddVersionForm = {
  versionNumber: '',
  fileName: '',
  contentType: '',
  fileSizeBytes: '',
  storagePath: '',
  changeSummary: '',
}

const parseDate = (value: string): Date | null => {
  if (!value) {
    return null
  }

  const parsed = new Date(value)
  return Number.isNaN(parsed.getTime()) ? null : parsed
}

export function DocumentVersionsPage() {
  const navigate = useNavigate()
  const { id: documentId } = useParams()
  const { hasPermission } = useAuth()
  const canViewDocuments = hasPermission('Documents.View')
  const canViewVersions = hasPermission('DocumentVersions.View')
  const canCreateVersions = hasPermission('DocumentVersions.Create')

  const [document, setDocument] = useState<Document | null>(null)
  const [rows, setRows] = useState<DocumentVersion[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [formOpen, setFormOpen] = useState(false)
  const [form, setForm] = useState<AddVersionForm>(emptyForm)

  const { query, setQuery } = useListQueryState<VersionQuery>({
    defaults: defaultQuery,
    numberKeys: ['page', 'pageSize'],
  })

  const [draftFilters, setDraftFilters] = useState<Pick<VersionQuery, 'createdFrom' | 'createdTo' | 'versionFrom' | 'versionTo'>>({
    createdFrom: query.createdFrom,
    createdTo: query.createdTo,
    versionFrom: query.versionFrom,
    versionTo: query.versionTo,
  })

  const loadDocument = async () => {
    if (!documentId || !canViewDocuments) {
      return
    }

    try {
      const { data } = await api.get<Document>(`api/documents/${documentId}`)
      setDocument(data)
      setForm((current) => ({
        ...current,
        fileName: current.fileName || data.fileName || '',
        contentType: current.contentType || data.contentType || '',
        fileSizeBytes: current.fileSizeBytes || String(data.fileSizeBytes ?? ''),
        storagePath: current.storagePath || data.storagePath || '',
      }))
    } catch {
      setDocument(null)
    }
  }

  const loadVersions = async () => {
    if (!documentId || !canViewVersions) {
      return
    }

    setLoading(true)
    setError('')
    setSuccess('')
    try {
      const { data } = await api.get<PagedResult<DocumentVersion>>(`api/documents/${documentId}/versions`, {
        params: {
          page: query.page,
          pageSize: query.pageSize,
          search: query.search || undefined,
          sortBy: query.sortBy || undefined,
          sortDir: query.sortDir,
        },
      })

      let items = data.items

      if (query.versionFrom) {
        const minVersion = Number(query.versionFrom)
        if (!Number.isNaN(minVersion)) {
          items = items.filter((item) => item.versionNumber >= minVersion)
        }
      }

      if (query.versionTo) {
        const maxVersion = Number(query.versionTo)
        if (!Number.isNaN(maxVersion)) {
          items = items.filter((item) => item.versionNumber <= maxVersion)
        }
      }

      const fromDate = parseDate(query.createdFrom)
      if (fromDate) {
        items = items.filter((item) => parseDate(item.createdAt)?.getTime() && parseDate(item.createdAt)!.getTime() >= fromDate.getTime())
      }

      const toDate = parseDate(query.createdTo)
      if (toDate) {
        items = items.filter((item) => parseDate(item.createdAt)?.getTime() && parseDate(item.createdAt)!.getTime() <= toDate.getTime())
      }

      setRows(items)
      setTotalCount(items.length === data.items.length ? data.totalCount : items.length)
    } catch {
      setError('Failed to load document versions.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void Promise.resolve().then(async () => {
      await Promise.all([loadDocument(), loadVersions()])
    })
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [documentId, canViewDocuments, canViewVersions, query])

  const addVersion = async () => {
    if (!documentId || !canCreateVersions) {
      return
    }

    if (!form.fileName.trim()) {
      setError('File name is required.')
      return
    }

    if (!form.contentType.trim()) {
      setError('Content type is required.')
      return
    }

    if (!form.storagePath.trim()) {
      setError('Storage path is required.')
      return
    }

    const size = Number(form.fileSizeBytes || '0')
    const parsedVersionNumber = form.versionNumber ? Number(form.versionNumber) : null
    if (form.versionNumber && (parsedVersionNumber === null || Number.isNaN(parsedVersionNumber) || parsedVersionNumber <= 0)) {
      setError('Version number must be greater than zero.')
      return
    }

    if (Number.isNaN(size) || size < 0) {
      setError('File size must be zero or greater.')
      return
    }

    setLoading(true)
    setError('')
    try {
      await api.post(`api/documents/${documentId}/versions`, {
        versionNumber: parsedVersionNumber,
        fileName: form.fileName.trim(),
        contentType: form.contentType.trim(),
        fileSizeBytes: size,
        storagePath: form.storagePath.trim(),
        changeSummary: form.changeSummary.trim() || null,
      })

      setForm((current) => ({
        ...emptyForm,
        fileName: current.fileName,
        contentType: current.contentType,
        fileSizeBytes: current.fileSizeBytes,
        storagePath: current.storagePath,
      }))
      setFormOpen(false)
      setSuccess('Document version added successfully.')

      await loadVersions()
    } catch {
      setError('Failed to add document version.')
    } finally {
      setLoading(false)
    }
  }

  const activeFilterCount = [query.createdFrom, query.createdTo, query.versionFrom, query.versionTo].filter(Boolean).length

  const columns = useMemo<DenseColumn<DocumentVersion>[]>(
    () => [
      { key: 'versionNumber', label: 'Version', sortable: true },
      { key: 'fileName', label: 'File Name', sortable: true },
      { key: 'contentType', label: 'Content Type', sortable: true },
      { key: 'fileSizeBytes', label: 'Size (bytes)', sortable: true },
      { key: 'changeSummary', label: 'Change Summary', sortable: true },
      { key: 'createdAt', label: 'Created At', sortable: true },
    ],
    [],
  )

  if (!canViewVersions) {
    return (
      <div>
        <PageHeader title="Document Versions" subtitle="Manage version history for a selected document." />
        <MessageBar intent="error"><MessageBarBody>You do not have permission to view document versions.</MessageBarBody></MessageBar>
      </div>
    )
  }

  return (
    <div>
      <PageHeader
        title="Document Versions"
        subtitle={document ? `${document.documentNumber} - ${document.title}` : 'Manage version history for a selected document.'}
        actions={[
          { key: 'back-documents', label: 'Back to Documents', onClick: () => navigate('/documents'), appearance: 'subtle' },
          ...(documentId ? [{ key: 'edit-document', label: 'Edit Document', onClick: () => navigate(`/documents/${documentId}/edit`), appearance: 'secondary' as const }] : []),
        ]}
      />

      <CommandBar actions={[
        { key: 'back-documents', label: 'Back to Documents', onClick: () => navigate('/documents') },
      ]}
      />

      {document ? (
        <div className={styles.metricGrid} style={{ marginBottom: 10 }}>
          <div className={styles.metricCard}><p className={styles.metricLabel}>Status</p><p className={styles.metricValue}>{document.documentStatusName ?? ''}</p></div>
          <div className={styles.metricCard}><p className={styles.metricLabel}>Category</p><p className={styles.metricValue}>{document.documentCategoryName ?? ''}</p></div>
          <div className={styles.metricCard}><p className={styles.metricLabel}>Current Version</p><p className={styles.metricValue}>{String(document.currentVersion)}</p></div>
          <div className={styles.metricCard}><p className={styles.metricLabel}>Confidential</p><p className={styles.metricValue}>{document.isConfidential ? 'Yes' : 'No'}</p></div>
        </div>
      ) : null}

      {success ? <MessageBar intent="success" style={{ marginBottom: 10 }}><MessageBarBody>{success}</MessageBarBody></MessageBar> : null}
      {error ? <MessageBar intent="error" style={{ marginBottom: 10 }}><MessageBarBody>{error}</MessageBarBody></MessageBar> : null}

      <RelatedRecordsSubgrid
        title="Document Versions"
        addLabel={canCreateVersions ? 'Add Version' : undefined}
        onAdd={canCreateVersions ? () => setFormOpen(true) : undefined}
        onRefresh={() => void loadVersions()}
        loading={loading}
        error={error}
        hasRows={rows.length > 0}
        emptyMessage="No document versions match the current filters."
        emptyActionLabel={canCreateVersions ? 'Add Version' : undefined}
        onEmptyAction={canCreateVersions ? () => setFormOpen(true) : undefined}
      >
        <DenseDataGrid
          rows={rows}
          columns={columns}
          loading={loading}
          totalCount={totalCount}
          page={query.page}
          pageSize={query.pageSize}
          search={query.search}
          sort={query.sortBy ? ({ key: query.sortBy as keyof DocumentVersion, dir: query.sortDir }) : null}
          onPageChange={(page) => setQuery((current) => ({ ...current, page }))}
          onPageSizeChange={(pageSize) => setQuery((current) => ({ ...current, pageSize, page: 1 }))}
          onSearchChange={(search) => setQuery((current) => ({ ...current, search, page: 1 }))}
          onSortChange={(sort: DenseSort<DocumentVersion> | null) => setQuery((current) => ({ ...current, sortBy: sort ? String(sort.key) : '', sortDir: sort?.dir ?? 'desc', page: 1 }))}
          emptyMessage="No document versions match the current filters."
          activeFilterCount={activeFilterCount}
          filterPanel={
            <>
              <DateRangeFilterField
                fromLabel="Created From"
                toLabel="Created To"
                fromValue={draftFilters.createdFrom}
                toValue={draftFilters.createdTo}
                onFromChange={(value) => setDraftFilters((current) => ({ ...current, createdFrom: value }))}
                onToChange={(value) => setDraftFilters((current) => ({ ...current, createdTo: value }))}
              />
              <FilterField label="Version From">
                <Input size="small" value={draftFilters.versionFrom} onChange={(_, data) => setDraftFilters((current) => ({ ...current, versionFrom: data.value }))} />
              </FilterField>
              <FilterField label="Version To">
                <Input size="small" value={draftFilters.versionTo} onChange={(_, data) => setDraftFilters((current) => ({ ...current, versionTo: data.value }))} />
              </FilterField>
            </>
          }
          onApplyFilters={() => setQuery((current) => ({ ...current, ...draftFilters, page: 1 }))}
          onCancelFilters={() => setDraftFilters({
            createdFrom: query.createdFrom,
            createdTo: query.createdTo,
            versionFrom: query.versionFrom,
            versionTo: query.versionTo,
          })}
          onClearFilters={() => setDraftFilters({ createdFrom: '', createdTo: '', versionFrom: '', versionTo: '' })}
        />
      </RelatedRecordsSubgrid>

      <SubgridModalForm
        open={formOpen}
        title="Add Document Version"
        submitLabel="Add Version"
        loading={loading}
        onOpenChange={(open) => {
          setFormOpen(open)
          if (!open) {
            setForm(emptyForm)
          }
        }}
        onSubmit={() => void addVersion()}
      >
        <FormSectionCard title="Version Details">
          <Field label="Version Number (optional)">
            <Input size="small" value={form.versionNumber} onChange={(_, data) => setForm((current) => ({ ...current, versionNumber: data.value }))} />
          </Field>
          <Field label="File Name" required>
            <Input size="small" value={form.fileName} onChange={(_, data) => setForm((current) => ({ ...current, fileName: data.value }))} />
          </Field>
          <Field label="Content Type" required>
            <Input size="small" value={form.contentType} onChange={(_, data) => setForm((current) => ({ ...current, contentType: data.value }))} />
          </Field>
          <Field label="File Size (bytes)">
            <Input size="small" value={form.fileSizeBytes} onChange={(_, data) => setForm((current) => ({ ...current, fileSizeBytes: data.value }))} />
          </Field>
          <Field label="Storage Path" required>
            <Input size="small" value={form.storagePath} onChange={(_, data) => setForm((current) => ({ ...current, storagePath: data.value }))} />
          </Field>
          <Field label="Change Summary">
            <Input size="small" value={form.changeSummary} onChange={(_, data) => setForm((current) => ({ ...current, changeSummary: data.value }))} />
          </Field>
        </FormSectionCard>
      </SubgridModalForm>
    </div>
  )
}
