import { Button } from '@fluentui/react-components'
import {
  AddRegular,
} from '@fluentui/react-icons'
import styles from './DenseDataGrid.module.css'
import { SelectionActionButton, type SelectionActionRequirement } from './SelectionActionButton'
import { getActionIcon } from '../actions/actionIcons'

export type SelectionAction = {
  key: string
  label: string
  onClick: () => void
  requiresSelection?: SelectionActionRequirement
  disabled?: () => boolean
  appearance?: 'primary' | 'secondary' | 'subtle'
}

export function EntityBulkActionsBar({
  createAction,
  actions,
  selectedCount,
}: {
  createAction?: { label: string; onClick: () => void }
  actions: SelectionAction[]
  selectedCount: number
}) {
  return (
    <div className={styles.selectionBar} data-testid="grid-selection-action-bar">
      <div className={styles.selectionActionsLeft}>
        {createAction ? (
          <Button size="small" appearance="primary" icon={<AddRegular />} onClick={createAction.onClick} className={styles.selectionButton}>
            {createAction.label}
          </Button>
        ) : null}

        {actions.map((action) => (
          <SelectionActionButton
            key={action.key}
            label={action.label}
            selectedCount={selectedCount}
            requiresSelection={action.requiresSelection}
            disabled={action.disabled?.()}
            appearance={action.appearance}
            icon={getActionIcon(action.key, action.label)}
            onClick={action.onClick}
          />
        ))}
      </div>

      <div className={styles.selectionState}>
        {selectedCount === 0 ? 'No records selected' : `${selectedCount} record${selectedCount > 1 ? 's' : ''} selected`}
      </div>
    </div>
  )
}
