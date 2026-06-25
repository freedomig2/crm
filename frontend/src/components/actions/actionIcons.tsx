import type { ButtonProps } from '@fluentui/react-components'
import {
  AddRegular,
  ArrowClockwiseRegular,
  ArrowDownloadRegular,
  ArrowSwapRegular,
  ArrowUploadRegular,
  CheckmarkCircleRegular,
  CloudArrowUpRegular,
  DeleteRegular,
  DismissCircleRegular,
  DismissRegular,
  EditRegular,
  EyeRegular,
  FilterRegular,
  PersonAddRegular,
  SaveRegular,
  SearchRegular,
  StarRegular,
  TableRegular,
  WarningRegular,
} from '@fluentui/react-icons'

const actionIconMap: Record<string, ButtonProps['icon']> = {
  save: <SaveRegular />,
  saveandclose: <SaveRegular />,
  cancel: <DismissRegular />,
  back: <DismissRegular />,
  close: <DismissRegular />,
  dismiss: <DismissRegular />,
  delete: <DeleteRegular />,
  remove: <DeleteRegular />,
  create: <AddRegular />,
  new: <AddRegular />,
  add: <AddRegular />,
  edit: <EditRegular />,
  update: <EditRegular />,
  view: <EyeRegular />,
  open: <EyeRegular />,
  refresh: <ArrowClockwiseRegular />,
  export: <ArrowDownloadRegular />,
  import: <ArrowUploadRegular />,
  upload: <CloudArrowUpRegular />,
  download: <ArrowDownloadRegular />,
  filter: <FilterRegular />,
  filters: <FilterRegular />,
  columns: <TableRegular />,
  search: <SearchRegular />,
  assign: <PersonAddRegular />,
  approve: <CheckmarkCircleRegular />,
  reject: <DismissCircleRegular />,
  convert: <ArrowSwapRegular />,
  qualify: <CheckmarkCircleRegular />,
  disqualify: <DismissCircleRegular />,
  score: <StarRegular />,
  resolve: <CheckmarkCircleRegular />,
  escalate: <WarningRegular />,
}

const tokenize = (value: string) =>
  value
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, ' ')
    .trim()
    .split(/\s+/)
    .filter(Boolean)

export function getActionIcon(actionKey?: string, label?: string): ButtonProps['icon'] {
  const candidates = [...tokenize(actionKey ?? ''), ...tokenize(label ?? '')]

  for (const token of candidates) {
    const icon = actionIconMap[token]
    if (icon) {
      return icon
    }
  }

  const compact = `${actionKey ?? ''}${label ?? ''}`.toLowerCase().replace(/[^a-z0-9]/g, '')
  if (compact in actionIconMap) {
    return actionIconMap[compact]
  }

  return null
}
