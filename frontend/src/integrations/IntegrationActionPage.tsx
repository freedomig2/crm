import { MessageBar, MessageBarBody, Spinner } from '@fluentui/react-components'
import { useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { api } from '../api/client'
import { EntityHeader, EntityPageLayout, StickyActionBar } from '../components/entity-ui/EntityComponents'

type IntegrationActionPageProps = {
  action: 'test' | 'sync'
}

type ActionResult = {
  success: boolean
  message: string
}

export function IntegrationActionPage({ action }: IntegrationActionPageProps) {
  const { id } = useParams()
  const navigate = useNavigate()
  const [running, setRunning] = useState(false)
  const [message, setMessage] = useState<ActionResult | null>(null)

  const runAction = async () => {
    if (!id) {
      setMessage({ success: false, message: 'Integration connection id is required.' })
      return
    }

    setRunning(true)
    setMessage(null)

    try {
      const endpoint = action === 'test' ? `api/integration-connections/${id}/test` : `api/integration-connections/${id}/sync`
      const { data } = await api.post<ActionResult>(endpoint)
      setMessage(data)
    } catch {
      setMessage({ success: false, message: `Failed to ${action === 'test' ? 'test connection' : 'run sync'}.` })
    } finally {
      setRunning(false)
    }
  }

  const title = action === 'test' ? 'Test Integration Connection' : 'Run Integration Sync'
  const subtitle = action === 'test'
    ? 'Validate external endpoint reachability for this connection.'
    : 'Execute a manual integration sync and write run history.'

  const actionBarActions = [
    {
      key: 'cancel',
      label: 'Cancel',
      onClick: () => navigate('/integrations/connections'),
      appearance: 'subtle' as const,
    },
    {
      key: 'submit',
      label: action === 'test' ? 'Run Connection Test' : 'Run Sync',
      onClick: () => { void runAction() },
      appearance: 'primary' as const,
      disabled: running,
    },
  ]

  return (
    <EntityPageLayout
      header={<EntityHeader title={title} subtitle={subtitle} actions={[{ key: 'back', label: 'Back to Connections', onClick: () => navigate('/integrations/connections') }]} />}
      stickyBar={<StickyActionBar actions={actionBarActions} message={running ? 'Processing integration action...' : undefined} />}
    >

      {running ? <Spinner size="small" label="Processing integration action..." style={{ marginBottom: 10 }} /> : null}

      {message ? (
        <MessageBar intent={message.success ? 'success' : 'error'}>
          <MessageBarBody>{message.message}</MessageBarBody>
        </MessageBar>
      ) : null}
    </EntityPageLayout>
  )
}
