import type { ReactNode } from 'react'
import { Button, Drawer, DrawerBody, DrawerHeader, DrawerHeaderTitle } from '@fluentui/react-components'
import { FilterRegular } from '@fluentui/react-icons'
import { ActiveFilterBadge } from './ActiveFilterBadge'

export function FilterDrawer({
  open,
  onOpenChange,
  onClose,
  activeCount,
  children,
}: {
  open: boolean
  onOpenChange: (open: boolean) => void
  onClose: () => void
  activeCount: number
  children: ReactNode
}) {
  return (
    <>
      <Button size="small" appearance="subtle" icon={<FilterRegular />} onClick={() => onOpenChange(true)}>
        Filters
        <ActiveFilterBadge count={activeCount} />
      </Button>
      <Drawer open={open} onOpenChange={(_, data) => onOpenChange(data.open)} position="end" size="small">
        <DrawerHeader>
          <DrawerHeaderTitle
            action={
              <Button size="small" appearance="subtle" onClick={onClose}>
                Close
              </Button>
            }
          >
            Filters
          </DrawerHeaderTitle>
        </DrawerHeader>
        <DrawerBody>{children}</DrawerBody>
      </Drawer>
    </>
  )
}
