import { Button } from '@fluentui/react-components'
import styles from './Subgrid.module.css'

export function SubgridEmptyState({
  message,
  actionLabel,
  onAction,
}: {
  message: string
  actionLabel?: string
  onAction?: () => void
}) {
  return (
    <div className={styles.emptyState}>
      <span>{message}</span>
      {actionLabel && onAction ? (
        <Button size="small" appearance="secondary" onClick={onAction}>
          {actionLabel}
        </Button>
      ) : null}
    </div>
  )
}
