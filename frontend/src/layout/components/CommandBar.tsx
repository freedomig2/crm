import { Button, Toolbar } from '@fluentui/react-components'
import styles from './PageChrome.module.css'

export type CommandAction = {
  key: string
  label: string
  onClick?: () => void
}

export function CommandBar({ actions }: { actions: CommandAction[] }) {
  if (actions.length === 0) {
    return null
  }

  return (
    <div className={styles.commandBar}>
      <Toolbar size="small" aria-label="Page command bar">
        {actions.map((action) => (
          <Button key={action.key} size="small" appearance="subtle" onClick={action.onClick}>
            {action.label}
          </Button>
        ))}
      </Toolbar>
    </div>
  )
}
