import type { ReactNode } from 'react'
import { Field } from '@fluentui/react-components'

export function FilterField({
  label,
  children,
}: {
  label: string
  children: ReactNode
}) {
  return <Field label={label}>{children}</Field>
}
