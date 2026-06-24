import { DeleteConfirmDialog } from '../crud/DeleteConfirmDialog'

export function SubgridDeleteConfirmDialog({
  open,
  title,
  message,
  onConfirm,
  onCancel,
}: {
  open: boolean
  title: string
  message: string
  onConfirm: () => void
  onCancel: () => void
}) {
  return (
    <DeleteConfirmDialog
      open={open}
      title={title}
      message={message}
      onConfirm={onConfirm}
      onCancel={onCancel}
    />
  )
}
