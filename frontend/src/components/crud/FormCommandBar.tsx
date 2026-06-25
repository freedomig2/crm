import { Button, Toolbar, ToolbarDivider } from '@fluentui/react-components'
import styles from '../../layout/components/PageChrome.module.css'
import { getActionIcon } from '../actions/actionIcons'

export function FormCommandBar({
  onSave,
  onSaveAndClose,
  onCancel,
  onDelete,
  disableSave,
  showDelete,
}: {
  onSave: () => void
  onSaveAndClose: () => void
  onCancel: () => void
  onDelete?: () => void
  disableSave?: boolean
  showDelete?: boolean
}) {
  return (
    <div className={styles.commandBar}>
      <Toolbar size="small" aria-label="Form command bar">
        <Button size="small" appearance="primary" onClick={onSave} disabled={disableSave} icon={getActionIcon('save', 'Save')}>
          Save
        </Button>
        <Button size="small" appearance="secondary" onClick={onSaveAndClose} disabled={disableSave} icon={getActionIcon('saveAndClose', 'Save and Close')}>
          Save and Close
        </Button>
        <Button size="small" appearance="subtle" onClick={onCancel} icon={getActionIcon('cancel', 'Cancel')}>
          Cancel / Back
        </Button>
        {showDelete && onDelete ? (
          <>
            <ToolbarDivider />
            <Button size="small" appearance="outline" onClick={onDelete} icon={getActionIcon('delete', 'Delete')}>
              Delete
            </Button>
          </>
        ) : null}
      </Toolbar>
    </div>
  )
}
