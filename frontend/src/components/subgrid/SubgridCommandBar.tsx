import { Button } from '@fluentui/react-components'
import styles from './Subgrid.module.css'
import { getActionIcon } from '../actions/actionIcons'

export function SubgridCommandBar({
  title,
  addLabel,
  onAdd,
  onRefresh,
  disableAdd,
}: {
  title: string
  addLabel?: string
  onAdd?: () => void
  onRefresh?: () => void
  disableAdd?: boolean
}) {
  return (
    <div className={styles.headerRow}>
      <h3 className={styles.title}>{title}</h3>
      <div className={styles.headerActions}>
        {onRefresh ? (
          <Button size="small" appearance="subtle" onClick={onRefresh} icon={getActionIcon('refresh', 'Refresh')}>
            Refresh
          </Button>
        ) : null}
        {addLabel && onAdd ? (
          <Button size="small" appearance="primary" onClick={onAdd} disabled={disableAdd} icon={getActionIcon('create', addLabel)}>
            {addLabel}
          </Button>
        ) : null}
      </div>
    </div>
  )
}
