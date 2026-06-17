import { Button, Card, Text } from '@fluentui/react-components'
import { PageHeader } from '../layout/components/PageHeader'
import { CommandBar } from '../layout/components/CommandBar'

export function SimpleStatePage({ title, subtitle }: { title: string; subtitle: string }) {
  return (
    <div>
      <PageHeader title={title} subtitle={subtitle} quickAction="Create" />
      <CommandBar actions={[{ key: 'refresh', label: 'Refresh' }]} />

      <div style={{ marginTop: 12 }}>
        <Card>
          <Text size={500} weight="semibold">No records yet</Text>
          <Text>Configure this module to start collecting data and events.</Text>
          <div style={{ marginTop: 12 }}>
            <Button appearance="primary" size="small">Create First Record</Button>
          </div>
        </Card>
      </div>
    </div>
  )
}
