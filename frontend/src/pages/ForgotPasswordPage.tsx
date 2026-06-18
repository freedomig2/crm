import { useState } from 'react'
import type { FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { Button, Card, Field, Input, MessageBar, MessageBarBody, Text } from '@fluentui/react-components'
import { api } from '../api/client'

export function ForgotPasswordPage() {
  const [email, setEmail] = useState('')
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setMessage('')
    setError('')
    try {
      const { data } = await api.post('api/auth/forgot-password', { email })
      setMessage(`Request processed. Reset token (dev): ${data.resetToken ?? 'hidden'}`)
    } catch {
      setError('Unable to process request.')
    }
  }

  return (
    <div className="authPage">
      <Card className="authCard">
        <form onSubmit={submit}>
          <Text size={500} weight="semibold">Forgot Password</Text>
          {message && (
            <MessageBar intent="success" style={{ marginTop: 8 }}>
              <MessageBarBody>{message}</MessageBarBody>
            </MessageBar>
          )}
          {error && (
            <MessageBar intent="error" style={{ marginTop: 8 }}>
              <MessageBarBody>{error}</MessageBarBody>
            </MessageBar>
          )}
          <Field label="Email" required>
            <Input type="email" value={email} onChange={(_, d) => setEmail(d.value)} />
          </Field>
          <Button appearance="primary" type="submit">Send Reset Token</Button>
          <div style={{ marginTop: 8, display: 'flex', gap: 12 }}>
            <Link to="/reset-password">I already have a reset token</Link>
            <Link to="/login">Back to login</Link>
          </div>
        </form>
      </Card>
    </div>
  )
}
