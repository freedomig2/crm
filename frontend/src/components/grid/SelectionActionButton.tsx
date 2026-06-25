import { Button } from '@fluentui/react-components'
import type { ButtonProps } from '@fluentui/react-components'
import styles from './DenseDataGrid.module.css'

export type SelectionActionRequirement = 'none' | 'single' | 'multiple' | 'any'

export type SelectionActionButtonProps = {
  label: string
  onClick: () => void
  selectedCount: number
  requiresSelection?: SelectionActionRequirement
  disabled?: boolean
  appearance?: 'primary' | 'secondary' | 'subtle'
  icon?: ButtonProps['icon']
}

function isSelectionSatisfied(requiresSelection: SelectionActionRequirement, selectedCount: number): boolean {
  if (requiresSelection === 'none') {
    return true
  }

  if (requiresSelection === 'single') {
    return selectedCount === 1
  }

  if (requiresSelection === 'multiple') {
    return selectedCount > 1
  }

  return selectedCount > 0
}

export function SelectionActionButton({
  label,
  onClick,
  selectedCount,
  requiresSelection = 'single',
  disabled,
  appearance = 'subtle',
  icon,
}: SelectionActionButtonProps) {
  const isEnabled = isSelectionSatisfied(requiresSelection, selectedCount) && !disabled

  return (
    <Button
      size="small"
      appearance={appearance}
      icon={icon}
      onClick={onClick}
      disabled={!isEnabled}
      className={styles.selectionButton}
    >
      {label}
    </Button>
  )
}
