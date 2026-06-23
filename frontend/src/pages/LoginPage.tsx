import { useState } from 'react'
import type { FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { Button, Card, Field, Input, MessageBar, MessageBarBody, Text } from '@fluentui/react-components'
import { useAuth } from '../auth/AuthContext'

export function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [email, setEmail] = useState('admin@crm.local')
  const [password, setPassword] = useState('Admin@12345')
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setLoading(true)
    setError(null)
    try {
      await login(email, password)
      navigate('/dashboard')
    } catch {
      setError('Login failed. Please check your credentials.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="authPage" data-testid="login-page">
      <Card className="authCard">
        <form onSubmit={handleSubmit} data-testid="login-form">
          <Text size={500} weight="semibold">CRM Enterprise Sign-In</Text>
          {error && (
            <MessageBar intent="error" style={{ marginTop: 8 }}>
              <MessageBarBody>{error}</MessageBarBody>
            </MessageBar>
          )}
          <Field label="Email" required>
            <Input data-testid="login-email" type="email" value={email} onChange={(_, d) => setEmail(d.value)} />
          </Field>
          <Field label="Password" required>
            <Input data-testid="login-password" type="password" value={password} onChange={(_, d) => setPassword(d.value)} />
          </Field>
          <Button data-testid="login-submit" appearance="primary" type="submit" disabled={loading}>
            {loading ? 'Signing in...' : 'Login'}
          </Button>
          <div style={{ marginTop: 8 }}>
            <Link to="/forgot-password">Forgot password?</Link>
          </div>
        </form>
      </Card>
    </div>
  )
}
