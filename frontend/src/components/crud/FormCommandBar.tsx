import { Button, Toolbar, ToolbarDivider } from '@fluentui/react-components'
import styles from '../../layout/components/PageChrome.module.css'

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
        <Button size="small" appearance="primary" onClick={onSave} disabled={disableSave}>
          Save
        </Button>
        <Button size="small" appearance="secondary" onClick={onSaveAndClose} disabled={disableSave}>
          Save and Close
        </Button>
        <Button size="small" appearance="subtle" onClick={onCancel}>
          Cancel / Back
        </Button>
        {showDelete && onDelete ? (
          <>
            <ToolbarDivider />
            <Button size="small" appearance="subtle" onClick={onDelete}>
              Delete
            </Button>
          </>
        ) : null}
      </Toolbar>
    </div>
  )
}
