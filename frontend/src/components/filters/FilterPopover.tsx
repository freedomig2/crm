import type { ReactNode } from 'react'
import { Button, Popover, PopoverSurface, PopoverTrigger } from '@fluentui/react-components'
import { FilterRegular } from '@fluentui/react-icons'
import { ActiveFilterBadge } from './ActiveFilterBadge'
import styles from './Filters.module.css'

export function FilterPopover({
  open,
  onOpenChange,
  activeCount,
  children,
}: {
  open: boolean
  onOpenChange: (open: boolean) => void
  activeCount: number
  children: ReactNode
}) {
  return (
    <Popover open={open} onOpenChange={(_, data) => onOpenChange(data.open)} positioning="below-end" withArrow>
      <PopoverTrigger disableButtonEnhancement>
        <Button data-testid="grid-filter-button" size="small" appearance="subtle" icon={<FilterRegular />}>
          Filters
          <ActiveFilterBadge count={activeCount} />
        </Button>
      </PopoverTrigger>
      <PopoverSurface className={styles.filterPopoverSurface} data-testid="grid-filter-popover">{children}</PopoverSurface>
    </Popover>
  )
}
