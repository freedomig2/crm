import { Badge } from '@fluentui/react-components'

export function ActiveFilterBadge({ count }: { count: number }) {
  if (count <= 0) {
    return null
  }

  return (
    <Badge appearance="filled" size="small" color="informative">
      {count}
    </Badge>
  )
}
