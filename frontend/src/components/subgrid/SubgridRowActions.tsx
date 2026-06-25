import { Button, Menu, MenuItem, MenuList, MenuPopover, MenuTrigger } from '@fluentui/react-components'
import { MoreHorizontalRegular } from '@fluentui/react-icons'
import { getActionIcon } from '../actions/actionIcons'

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
          {onView ? <MenuItem icon={getActionIcon('view', 'View')} onClick={onView}>View</MenuItem> : null}
          {onEdit ? <MenuItem icon={getActionIcon('edit', 'Edit')} onClick={onEdit} disabled={disableEdit}>Edit</MenuItem> : null}
          {onDelete ? <MenuItem icon={getActionIcon('delete', 'Delete')} onClick={onDelete} disabled={disableDelete}>Delete</MenuItem> : null}
        </MenuList>
      </MenuPopover>
    </Menu>
  )
}
