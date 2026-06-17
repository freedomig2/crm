import { Card, Text } from '@fluentui/react-components'
import styles from './DashboardCard.module.css'

export function DashboardCard({ label, value, delta }: { label: string; value: number | string; delta?: string }) {
  return (
    <Card className={styles.card}>
      <Text className={styles.label}>{label}</Text>
      <Text className={styles.value}>{value}</Text>
      {delta ? <Text className={styles.delta}>{delta}</Text> : null}
    </Card>
  )
}
