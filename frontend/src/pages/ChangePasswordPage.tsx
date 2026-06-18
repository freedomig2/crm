import { useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { Button, Card, Field, Input, MessageBar, MessageBarBody, Text } from '@fluentui/react-components'
import { api } from '../api/client'

export function ChangePasswordPage() {
  const navigate = useNavigate()
  const [currentPassword, setCurrentPassword] = useState('')
  const [newPassword, setNewPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setMessage('')
    setError('')

    if (newPassword !== confirmPassword) {
      setError('New password and confirmation do not match.')
      return
    }

    setLoading(true)
    try {
      await api.post('api/auth/change-password', { currentPassword, newPassword })
      setMessage('Password changed successfully.')
      setCurrentPassword('')
      setNewPassword('')
      setConfirmPassword('')
    } catch {
      setError('Change password failed. Please verify your current password.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="authPage">
      <Card className="authCard">
        <form onSubmit={submit}>
          <Text size={500} weight="semibold">Change Password</Text>
          {message ? (
            <MessageBar intent="success" style={{ marginTop: 8 }}>
              <MessageBarBody>{message}</MessageBarBody>
            </MessageBar>
          ) : null}
          {error ? (
            <MessageBar intent="error" style={{ marginTop: 8 }}>
              <MessageBarBody>{error}</MessageBarBody>
            </MessageBar>
          ) : null}

          <Field label="Current Password" required>
            <Input type="password" value={currentPassword} onChange={(_, d) => setCurrentPassword(d.value)} />
          </Field>
          <Field label="New Password" required>
            <Input type="password" value={newPassword} onChange={(_, d) => setNewPassword(d.value)} />
          </Field>
          <Field label="Confirm New Password" required>
            <Input type="password" value={confirmPassword} onChange={(_, d) => setConfirmPassword(d.value)} />
          </Field>

          <div style={{ display: 'flex', gap: 8, marginTop: 4 }}>
            <Button appearance="primary" type="submit" disabled={loading}>
              {loading ? 'Saving...' : 'Update Password'}
            </Button>
            <Button appearance="secondary" type="button" onClick={() => navigate(-1)}>
              Cancel
            </Button>
          </div>
        </form>
      </Card>
    </div>
  )
}
