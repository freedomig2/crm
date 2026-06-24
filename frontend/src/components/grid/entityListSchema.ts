import type { DenseColumn } from './DenseDataGrid'

export type EntityColumnType =
  | 'text'
  | 'number'
  | 'date'
  | 'money'
  | 'lookup'
  | 'boolean'
  | 'status'
  | 'actions'

export type EntityColumnDefinition = {
  key: string
  label: string
  field: string
  sortable?: boolean
  filterable?: boolean
  defaultVisible?: boolean
  width?: number
  type?: EntityColumnType
  required?: boolean
}

export type EntityFilterType = 'lookup' | 'text' | 'boolean' | 'date' | 'dateRange'

export type EntityFilterDefinition = {
  key: string
  label: string
  type: EntityFilterType
  fieldKey?: string
  fromKey?: string
  toKey?: string
  fromLabel?: string
  toLabel?: string
  trueLabel?: string
  falseLabel?: string
}

export type EntityColumnRegistryItem<TItem extends { id: string }> = {
  availableColumns: EntityColumnDefinition[]
  defaultVisibleColumnKeys: string[]
  requiredColumnKeys?: string[]
  toDenseColumns?: (columns: DenseColumn<TItem>[]) => DenseColumn<TItem>[]
}
