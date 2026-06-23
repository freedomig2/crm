import { Button, MessageBar, MessageBarBody, Spinner } from '@fluentui/react-components'
import { useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { api } from '../api/client'
import { PageHeader } from '../layout/components/PageHeader'

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

  return (
    <div>
      <PageHeader
        title={action === 'test' ? 'Test Integration Connection' : 'Run Integration Sync'}
        subtitle={action === 'test' ? 'Validate external endpoint reachability for this connection.' : 'Execute a manual integration sync and write run history.'}
      />

      <div style={{ display: 'flex', gap: 8, marginBottom: 12 }}>
        <Button appearance="primary" onClick={runAction} disabled={running}>
          {action === 'test' ? 'Run Connection Test' : 'Run Sync'}
        </Button>
        <Button appearance="secondary" onClick={() => navigate('/integrations/connections')}>
          Back To Connections
        </Button>
      </div>

      {running ? <Spinner size="small" label="Processing integration action..." style={{ marginBottom: 10 }} /> : null}

      {message ? (
        <MessageBar intent={message.success ? 'success' : 'error'}>
          <MessageBarBody>{message.message}</MessageBarBody>
        </MessageBar>
      ) : null}
    </div>
  )
}
