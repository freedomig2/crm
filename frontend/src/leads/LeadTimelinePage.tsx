import { useEffect, useState } from 'react'
import { MessageBar, MessageBarBody, Spinner } from '@fluentui/react-components'
import { CalendarAgendaRegular } from '@fluentui/react-icons'
import { useNavigate, useParams } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { EntityHeader, EntityPageLayout } from '../components/entity-ui/EntityComponents'
import type { Lead } from '../types/models'
import { LeadTimelinePanel } from './LeadRelatedPanels'

export function LeadTimelinePage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const { hasPermission } = useAuth()
  const canView = hasPermission('Leads.ViewTimeline')
  const [lead, setLead] = useState<Lead | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!id || !canView) {
      return
    }

    void (async () => {
      setLoading(true)
      setError('')
      try {
        const { data } = await api.get<Lead>(`api/leads/${id}`)
        setLead(data)
      } catch {
        setError('Failed to load lead.')
      } finally {
        setLoading(false)
      }
    })()
  }, [canView, id])

  return (
    <EntityPageLayout
      header={<EntityHeader icon={<CalendarAgendaRegular />} title={lead?.topic ?? 'Lead Timeline'} subtitle={lead?.leadNumber} actions={[{ key: 'back', label: 'Back to Lead', onClick: () => navigate(id ? `/leads/${id}` : '/leads'), appearance: 'subtle' }]} />}
      alerts={error ? [{ intent: 'error', text: error }] : undefined}
    >
      {!canView ? (
        <MessageBar intent="error">
          <MessageBarBody>You do not have permission to view lead timelines.</MessageBarBody>
        </MessageBar>
      ) : null}
      {loading ? <Spinner size="small" label="Loading timeline..." /> : null}
      {!loading && id && canView ? <LeadTimelinePanel leadId={id} /> : null}
    </EntityPageLayout>
  )
}
