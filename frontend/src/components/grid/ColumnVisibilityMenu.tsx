import { Button, Checkbox, MenuItem } from '@fluentui/react-components'

export type ColumnVisibilityOption = {
  key: string
  label: string
  checked: boolean
  disabled?: boolean
}

export function ColumnVisibilityMenu({
  options,
  onToggle,
  onReset,
}: {
  options: ColumnVisibilityOption[]
  onToggle: (key: string, nextChecked: boolean) => void
  onReset: () => void
}) {
  return (
    <>
      <MenuItem disabled>
        <strong>Columns</strong>
      </MenuItem>
      <MenuItem disabled>
        <Button size="small" appearance="subtle" onClick={onReset}>
          Reset to default
        </Button>
      </MenuItem>
      {options.map((option) => (
        <MenuItem key={option.key}>
          <Checkbox
            label={option.label}
            checked={option.checked}
            disabled={option.disabled}
            onChange={(_, data) => onToggle(option.key, Boolean(data.checked))}
          />
        </MenuItem>
      ))}
    </>
  )
}
