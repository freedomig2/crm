import { Badge } from '@fluentui/react-components'

type Tone = 'success' | 'warning' | 'danger' | 'neutral'

const toneToAppearance: Record<Tone, 'filled' | 'tint'> = {
  success: 'tint',
  warning: 'tint',
  danger: 'filled',
  neutral: 'tint',
}

const toneToColor: Record<Tone, 'success' | 'warning' | 'danger' | 'brand' | 'important' | 'informative' | 'severe'> = {
  success: 'success',
  warning: 'warning',
  danger: 'danger',
  neutral: 'informative',
}

export function StatusBadge({ label, tone = 'neutral' }: { label: string; tone?: Tone }) {
  return (
    <Badge size="small" appearance={toneToAppearance[tone]} color={toneToColor[tone]}>
      {label}
    </Badge>
  )
}
