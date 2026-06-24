import { Button } from '@fluentui/react-components'
import styles from './Subgrid.module.css'

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
          <Button size="small" appearance="subtle" onClick={onRefresh}>
            Refresh
          </Button>
        ) : null}
        {addLabel && onAdd ? (
          <Button size="small" appearance="primary" onClick={onAdd} disabled={disableAdd}>
            {addLabel}
          </Button>
        ) : null}
      </div>
    </div>
  )
}
