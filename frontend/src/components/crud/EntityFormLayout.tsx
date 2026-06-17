import { MessageBar, MessageBarBody, Skeleton, SkeletonItem } from '@fluentui/react-components'
import { PageHeader } from '../../layout/components/PageHeader'
import styles from './EntityFormLayout.module.css'

export function EntityFormLayout({
  title,
  subtitle,
  loading,
  error,
  validationSummary,
  onSave,
  onSaveAndClose,
  onCancel,
  onDelete,
  showDelete,
  stickyHeader,
  children,
}: {
  title: string
  subtitle?: string
  loading?: boolean
  error?: string
  validationSummary: string[]
  onSave: () => void
  onSaveAndClose: () => void
  onCancel: () => void
  onDelete?: () => void
  showDelete?: boolean
  stickyHeader?: boolean
  children: React.ReactNode
}) {
  const headerActions = [
    { key: 'save', label: 'Save', onClick: onSave, appearance: 'primary' as const, disabled: Boolean(loading) },
    { key: 'save-close', label: 'Save and Close', onClick: onSaveAndClose, appearance: 'secondary' as const, disabled: Boolean(loading) },
    ...(showDelete && onDelete ? [{ key: 'delete', label: 'Delete', onClick: onDelete, appearance: 'subtle' as const, disabled: Boolean(loading) }] : []),
    { key: 'cancel', label: 'Cancel / Back', onClick: onCancel, appearance: 'subtle' as const },
  ]

  return (
    <div>
      <div className={stickyHeader ? styles.stickyHeader : undefined}>
        <PageHeader title={title} subtitle={subtitle} actions={headerActions} />
      </div>

      <div className={styles.formSurface}>
        <div className={styles.summaryStack}>
          {error ? (
            <MessageBar intent="error">
              <MessageBarBody>{error}</MessageBarBody>
            </MessageBar>
          ) : null}
          {validationSummary.length > 0 ? (
            <MessageBar intent="warning">
              <MessageBarBody>
                {validationSummary.map((item) => (
                  <div key={item}>{item}</div>
                ))}
              </MessageBarBody>
            </MessageBar>
          ) : null}
        </div>

        {loading ? (
          <div className={styles.skeletonGrid}>
            {Array.from({ length: 6 }).map((_, index) => (
              <Skeleton key={index}>
                <SkeletonItem size={16} />
              </Skeleton>
            ))}
          </div>
        ) : (
          children
        )}
      </div>
    </div>
  )
}
