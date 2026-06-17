import { useMemo } from 'react'
import { MessageBar, MessageBarBody } from '@fluentui/react-components'
import { DenseDataGrid, type DenseColumn, statusCell } from '../components/grid/DenseDataGrid'
import { CommandBar } from '../layout/components/CommandBar'
import { PageHeader } from '../layout/components/PageHeader'

type Row = {
  id: string
  name: string
  code: string
  owner: string
  status: string
  updatedAt: string
}

function seedRows(prefix: string): Row[] {
  return Array.from({ length: 34 }).map((_, index) => ({
    id: `${prefix}-${index + 1}`,
    name: `${prefix} Item ${index + 1}`,
    code: `${prefix.toUpperCase()}-${String(index + 1).padStart(3, '0')}`,
    owner: index % 2 === 0 ? 'Operations' : 'Administration',
    status: index % 8 === 0 ? 'Locked' : index % 3 === 0 ? 'Pending' : 'Active',
    updatedAt: `2026-06-${String((index % 28) + 1).padStart(2, '0')}`,
  }))
}

export function ModuleGridPage({
  title,
  subtitle,
  quickAction,
  moduleKey,
  showErrorDemo,
}: {
  title: string
  subtitle: string
  quickAction: string
  moduleKey: string
  showErrorDemo?: boolean
}) {
  const rows = useMemo(() => seedRows(moduleKey), [moduleKey])
  const columns: DenseColumn<Row>[] = [
    { key: 'name', label: 'Name', sortable: true },
    { key: 'code', label: 'Code', sortable: true },
    { key: 'owner', label: 'Owner', sortable: true },
    { key: 'status', label: 'Status', sortable: true, render: (row) => statusCell(row.status) },
    { key: 'updatedAt', label: 'Updated', sortable: true },
  ]

  return (
    <div>
      <PageHeader title={title} subtitle={subtitle} quickAction={quickAction} />
      <CommandBar actions={[{ key: 'bulk', label: 'Bulk Actions' }, { key: 'assign', label: 'Assign' }, { key: 'archive', label: 'Archive' }]} />

      {showErrorDemo ? (
        <MessageBar intent="warning" style={{ margin: '10px 0' }}>
          <MessageBarBody>One data source is temporarily delayed. Showing cached results.</MessageBarBody>
        </MessageBar>
      ) : null}

      <DenseDataGrid rows={rows} columns={columns} />
    </div>
  )
}
