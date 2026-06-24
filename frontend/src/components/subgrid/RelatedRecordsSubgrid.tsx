import { MessageBar, MessageBarBody, Spinner } from '@fluentui/react-components'
import { SubgridCommandBar } from './SubgridCommandBar'
import { SubgridEmptyState } from './SubgridEmptyState'
import styles from './Subgrid.module.css'

export function RelatedRecordsSubgrid({
  title,
  addLabel,
  onAdd,
  onRefresh,
  loading,
  error,
  hasRows,
  emptyMessage,
  emptyActionLabel,
  onEmptyAction,
  disableAdd,
  children,
}: {
  title: string
  addLabel?: string
  onAdd?: () => void
  onRefresh?: () => void
  loading?: boolean
  error?: string
  hasRows: boolean
  emptyMessage: string
  emptyActionLabel?: string
  onEmptyAction?: () => void
  disableAdd?: boolean
  children: React.ReactNode
}) {
  return (
    <section className={styles.container}>
      <SubgridCommandBar
        title={title}
        addLabel={addLabel}
        onAdd={onAdd}
        onRefresh={onRefresh}
        disableAdd={disableAdd}
      />

      {loading ? <Spinner size="small" label="Loading..." style={{ marginBottom: 8 }} /> : null}
      {error ? (
        <MessageBar intent="error" style={{ marginBottom: 8 }}>
          <MessageBarBody>{error}</MessageBarBody>
        </MessageBar>
      ) : null}

      {hasRows ? children : <SubgridEmptyState message={emptyMessage} actionLabel={emptyActionLabel} onAction={onEmptyAction} />}
    </section>
  )
}
