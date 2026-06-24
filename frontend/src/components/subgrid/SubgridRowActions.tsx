import { Button, Menu, MenuItem, MenuList, MenuPopover, MenuTrigger } from '@fluentui/react-components'
import { MoreHorizontalRegular } from '@fluentui/react-icons'

export function SubgridRowActions({
  onView,
  onEdit,
  onDelete,
  disableEdit,
  disableDelete,
}: {
  onView?: () => void
  onEdit?: () => void
  onDelete?: () => void
  disableEdit?: boolean
  disableDelete?: boolean
}) {
  return (
    <Menu>
      <MenuTrigger disableButtonEnhancement>
        <Button size="small" appearance="subtle" icon={<MoreHorizontalRegular />} />
      </MenuTrigger>
      <MenuPopover>
        <MenuList>
          {onView ? <MenuItem onClick={onView}>View</MenuItem> : null}
          {onEdit ? <MenuItem onClick={onEdit} disabled={disableEdit}>Edit</MenuItem> : null}
          {onDelete ? <MenuItem onClick={onDelete} disabled={disableDelete}>Delete</MenuItem> : null}
        </MenuList>
      </MenuPopover>
    </Menu>
  )
}
