import { Button, Dialog, DialogActions, DialogBody, DialogContent, DialogSurface, DialogTitle, DialogTrigger } from '@fluentui/react-components'

export function SubgridModalForm({
  open,
  title,
  submitLabel,
  cancelLabel = 'Cancel',
  loading,
  onOpenChange,
  onSubmit,
  children,
}: {
  open: boolean
  title: string
  submitLabel: string
  cancelLabel?: string
  loading?: boolean
  onOpenChange: (open: boolean) => void
  onSubmit: () => void
  children: React.ReactNode
}) {
  return (
    <Dialog open={open} onOpenChange={(_, data) => onOpenChange(Boolean(data.open))}>
      <DialogSurface>
        <DialogBody>
          <DialogTitle>{title}</DialogTitle>
          <DialogContent>{children}</DialogContent>
          <DialogActions>
            <DialogTrigger disableButtonEnhancement>
              <Button size="small" appearance="subtle" disabled={loading}>
                {cancelLabel}
              </Button>
            </DialogTrigger>
            <Button size="small" appearance="primary" disabled={loading} onClick={onSubmit}>
              {submitLabel}
            </Button>
          </DialogActions>
        </DialogBody>
      </DialogSurface>
    </Dialog>
  )
}
