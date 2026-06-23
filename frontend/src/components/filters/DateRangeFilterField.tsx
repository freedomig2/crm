import { Input } from '@fluentui/react-components'
import { FilterField } from './FilterField'

export function DateRangeFilterField({
  fromLabel,
  toLabel,
  fromValue,
  toValue,
  onFromChange,
  onToChange,
}: {
  fromLabel: string
  toLabel: string
  fromValue: string
  toValue: string
  onFromChange: (value: string) => void
  onToChange: (value: string) => void
}) {
  return (
    <>
      <FilterField label={fromLabel}>
        <Input size="small" type="date" value={fromValue} onChange={(_, data) => onFromChange(data.value)} />
      </FilterField>
      <FilterField label={toLabel}>
        <Input size="small" type="date" value={toValue} onChange={(_, data) => onToChange(data.value)} />
      </FilterField>
    </>
  )
}
