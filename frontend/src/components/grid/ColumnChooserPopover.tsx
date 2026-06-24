import { Button, Menu, MenuList, MenuPopover, MenuTrigger } from '@fluentui/react-components'
import { SettingsRegular } from '@fluentui/react-icons'
import { ColumnVisibilityMenu, type ColumnVisibilityOption } from './ColumnVisibilityMenu'

export function ColumnChooserPopover({
  options,
  onToggle,
  onReset,
}: {
  options: ColumnVisibilityOption[]
  onToggle: (key: string, nextChecked: boolean) => void
  onReset: () => void
}) {
  return (
    <Menu>
      <MenuTrigger disableButtonEnhancement>
        <Button size="small" appearance="subtle" icon={<SettingsRegular />} data-testid="grid-columns-button">
          Columns
        </Button>
      </MenuTrigger>
      <MenuPopover>
        <MenuList>
          <ColumnVisibilityMenu options={options} onToggle={onToggle} onReset={onReset} />
        </MenuList>
      </MenuPopover>
    </Menu>
  )
}
