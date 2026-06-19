import { useEffect, useState } from 'react'
import { MessageBar, MessageBarBody, Spinner } from '@fluentui/react-components'
import { CalendarAgendaRegular } from '@fluentui/react-icons'
import { useNavigate, useParams } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { EntityHeader, EntityPageLayout } from '../components/entity-ui/EntityComponents'
import type { Opportunity } from '../types/models'
import { OpportunityTimelinePanel } from './OpportunityRelatedPanels'

export function OpportunityTimelinePage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const { hasPermission } = useAuth()
  const canView = hasPermission('Opportunities.ViewTimeline')
  const [opportunity, setOpportunity] = useState<Opportunity | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!id || !canView) return
    void (async () => {
      setLoading(true)
      setError('')
      try {
        const { data } = await api.get<Opportunity>(`api/opportunities/${id}`)
        setOpportunity(data)
      } catch {
        setError('Failed to load opportunity.')
      } finally {
        setLoading(false)
      }
    })()
  }, [canView, id])

  return (
    <EntityPageLayout
      header={<EntityHeader icon={<CalendarAgendaRegular />} title={opportunity?.topic ?? 'Opportunity Timeline'} subtitle={opportunity?.opportunityNumber} actions={[{ key: 'back', label: 'Back to Opportunity', onClick: () => navigate(id ? `/opportunities/${id}` : '/opportunities'), appearance: 'subtle' }]} />}
      alerts={error ? [{ intent: 'error', text: error }] : undefined}
    >
      {!canView ? (
        <MessageBar intent="error"><MessageBarBody>You do not have permission to view opportunity timelines.</MessageBarBody></MessageBar>
      ) : null}
      {loading ? <Spinner size="small" label="Loading timeline..." /> : null}
      {!loading && id && canView ? <OpportunityTimelinePanel opportunityId={id} /> : null}
    </EntityPageLayout>
  )
}
