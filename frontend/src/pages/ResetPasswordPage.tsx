import { useState } from 'react'
import type { FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { Button, Card, Field, Input, MessageBar, MessageBarBody, Text } from '@fluentui/react-components'
import { api } from '../api/client'

export function ResetPasswordPage() {
  const [email, setEmail] = useState('')
  const [token, setToken] = useState('')
  const [newPassword, setNewPassword] = useState('')
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setMessage('')
    setError('')
    try {
      await api.post('api/auth/reset-password', { email, token, newPassword })
      setMessage('Password reset successfully.')
    } catch {
      setError('Reset failed. Check token and email.')
    }
  }

  return (
    <div className="authPage">
      <Card className="authCard">
        <form onSubmit={submit}>
          <Text size={500} weight="semibold">Reset Password</Text>
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
          <Field label="Token" required>
            <Input value={token} onChange={(_, d) => setToken(d.value)} />
          </Field>
          <Field label="New Password" required>
            <Input type="password" value={newPassword} onChange={(_, d) => setNewPassword(d.value)} />
          </Field>
          <Button appearance="primary" type="submit">Reset Password</Button>
          <div style={{ marginTop: 8, display: 'flex', gap: 12 }}>
            <Link to="/forgot-password">Request a new token</Link>
            <Link to="/login">Back to login</Link>
          </div>
        </form>
      </Card>
    </div>
  )
}
