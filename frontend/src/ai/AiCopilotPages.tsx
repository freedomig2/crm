import { Button, Card, CardHeader, MessageBar, MessageBarBody, Spinner, Textarea } from '@fluentui/react-components'
import { useEffect, useState } from 'react'
import { api } from '../api/client'
import { PageHeader } from '../layout/components/PageHeader'
import type { AiDashboardSummary, AiRecommendation } from '../types/models'

export function AiDashboardPage() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [summary, setSummary] = useState<AiDashboardSummary | null>(null)

  useEffect(() => {
    const run = async () => {
      setLoading(true)
      setError('')
      try {
        const { data } = await api.get<AiDashboardSummary>('api/ai/dashboard')
        setSummary(data)
      } catch {
        setError('Failed to load AI dashboard summary.')
      } finally {
        setLoading(false)
      }
    }

    void run()
  }, [])

  return (
    <div>
      <PageHeader title="AI Dashboard" subtitle="Operational AI signals across sales and service workloads." />
      {loading ? <Spinner size="small" label="Loading AI insights..." style={{ marginBottom: 12 }} /> : null}
      {error ? (
        <MessageBar intent="error" style={{ marginBottom: 12 }}>
          <MessageBarBody>{error}</MessageBarBody>
        </MessageBar>
      ) : null}

      {summary ? (
        <div style={{ display: 'grid', gap: 10 }}>
          <Card>
            <CardHeader header={<strong>Open Leads</strong>} description={String(summary.openLeads)} />
          </Card>
          <Card>
            <CardHeader header={<strong>Open Opportunities</strong>} description={String(summary.openOpportunities)} />
          </Card>
          <Card>
            <CardHeader header={<strong>Open Cases</strong>} description={String(summary.openCases)} />
          </Card>
          <Card>
            <CardHeader header={<strong>AI Insights</strong>} />
            <ul>
              {summary.insights.map((line) => (
                <li key={line}>{line}</li>
              ))}
            </ul>
          </Card>
        </div>
      ) : null}
    </div>
  )
}

export function NextBestActionsPage() {
  const [scenarioCode, setScenarioCode] = useState('LEAD_FOLLOW_UP')
  const [contextText, setContextText] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [result, setResult] = useState<AiRecommendation | null>(null)

  const run = async () => {
    setLoading(true)
    setError('')

    try {
      const { data } = await api.post<AiRecommendation>('api/ai/recommendations', {
        scenarioCode,
        contextText: contextText || null,
      })
      setResult(data)
    } catch {
      setError('Failed to generate AI recommendations.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div>
      <PageHeader title="Next Best Actions" subtitle="Generate deterministic AI recommendations by scenario." />

      <div style={{ display: 'grid', gap: 10, maxWidth: 720 }}>
        <label>
          Scenario Code
          <input
            value={scenarioCode}
            onChange={(event) => setScenarioCode(event.target.value)}
            style={{ width: '100%', padding: 8, marginTop: 4 }}
          />
        </label>

        <label>
          Context
          <Textarea value={contextText} onChange={(_, data) => setContextText(data.value)} style={{ width: '100%' }} />
        </label>

        <div>
          <Button appearance="primary" onClick={run} disabled={loading}>
            Generate Recommendations
          </Button>
        </div>
      </div>

      {loading ? <Spinner size="small" label="Generating recommendations..." style={{ marginTop: 12 }} /> : null}
      {error ? (
        <MessageBar intent="error" style={{ marginTop: 12 }}>
          <MessageBarBody>{error}</MessageBarBody>
        </MessageBar>
      ) : null}

      {result ? (
        <Card style={{ marginTop: 12 }}>
          <CardHeader header={<strong>Scenario: {result.scenarioCode}</strong>} />
          <ul>
            {result.actions.map((item) => (
              <li key={item}>{item}</li>
            ))}
          </ul>
        </Card>
      ) : null}
    </div>
  )
}
