import type { ReactNode } from 'react'
import { Input } from '@fluentui/react-components'
import { SearchRegular } from '@fluentui/react-icons'
import styles from './DenseDataGrid.module.css'

export function ListCommandBar({
  searchValue,
  onSearchChange,
  rightActions,
  searchPlaceholder = 'Search rows',
}: {
  searchValue: string
  onSearchChange: (value: string) => void
  rightActions: ReactNode
  searchPlaceholder?: string
}) {
  return (
    <div className={styles.toolbar}>
      <Input
        size="small"
        contentBefore={<SearchRegular />}
        placeholder={searchPlaceholder}
        className={styles.searchInput}
        value={searchValue}
        onChange={(_, data) => onSearchChange(data.value)}
      />

      <div className={styles.toolbarActions}>{rightActions}</div>
    </div>
  )
}
