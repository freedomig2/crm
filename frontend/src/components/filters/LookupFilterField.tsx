import { LookupCombobox } from '../entity-ui/EntityComponents'
import { FilterField } from './FilterField'

export function LookupFilterField({
  label,
  fieldKey,
  value,
  onChange,
}: {
  label: string
  fieldKey: string
  value: string
  onChange: (value: string) => void
}) {
  return (
    <FilterField label={label}>
      <LookupCombobox fieldKey={fieldKey} value={value} onChange={onChange} />
    </FilterField>
  )
}
