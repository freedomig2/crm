import { Button, type ButtonProps } from '@fluentui/react-components'
import { AddRegular } from '@fluentui/react-icons'
import styles from './PageChrome.module.css'
import { Breadcrumbs } from './Breadcrumbs'

export type HeaderAction = {
  key: string
  label: string
  onClick?: () => void
  appearance?: ButtonProps['appearance']
  disabled?: boolean
}

export function PageHeader({
  title,
  subtitle,
  quickAction,
  onQuickAction,
  actions,
}: {
  title: string
  subtitle?: string
  quickAction?: string
  onQuickAction?: () => void
  actions?: HeaderAction[]
}) {
  const resolvedActions = actions ?? (quickAction
    ? [{ key: 'quick-action', label: quickAction, onClick: onQuickAction, appearance: 'primary' as const }]
    : [])

  return (
    <header className={styles.header}>
      <Breadcrumbs />
      <div className={styles.headerTop}>
        <div className={styles.titleBlock}>
          <h1>{title}</h1>
          {subtitle ? <p className={styles.subtitle}>{subtitle}</p> : null}
        </div>
        {resolvedActions.length > 0 ? (
          <div className={styles.headerActions}>
            {resolvedActions.map((action, index) => (
              <Button
                key={action.key}
                size="small"
                appearance={action.appearance ?? 'subtle'}
                icon={index === 0 && action.appearance === 'primary' ? <AddRegular /> : undefined}
                onClick={action.onClick}
                disabled={action.disabled}
              >
                {action.label}
              </Button>
            ))}
          </div>
        ) : null}
      </div>
    </header>
  )
}
