import { useCallback, useEffect, useMemo, useState } from 'react'
import { MessageBar, MessageBarBody, Spinner } from '@fluentui/react-components'
import { PersonCallRegular } from '@fluentui/react-icons'
import { useNavigate, useParams } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import {
  EntityDetailsGrid,
  EntityHeader,
  EntityPageLayout,
  EntitySummaryCard,
  EntityTabs,
} from '../components/entity-ui/EntityComponents'
import type { Contact, ContactInteraction, PagedResult } from '../types/models'
import { AuditHistoryPanel, ContactCommunicationsPanel, ContactInteractionsPanel, RelatedAccountPanel } from './ContactRelatedPanels'
import { formatDateTime } from './contactUtils'

const tabs = [
  { key: 'summary', label: 'Summary' },
  { key: 'communications', label: 'Communications' },
  { key: 'interaction-history', label: 'Interaction History' },
  { key: 'related-account', label: 'Related Account' },
  { key: 'audit-history', label: 'Audit History' },
]

export function ContactDetailsPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const { hasPermission } = useAuth()
  const canView = hasPermission('Contacts.View')
  const canEdit = hasPermission('Contacts.Update')
  const canSetPrimary = hasPermission('Contacts.SetPrimary')
  const [contact, setContact] = useState<Contact | null>(null)
  const [lastInteraction, setLastInteraction] = useState<ContactInteraction | null>(null)
  const [activeTab, setActiveTab] = useState('summary')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const load = useCallback(async () => {
    if (!id || !canView) {
      setLoading(false)
      return
    }

    setError('')
    try {
      const { data } = await api.get<Contact>(`api/contacts/${id}`)
      setContact(data)
      const interactions = await api.get<PagedResult<ContactInteraction>>('api/contact-interactions', {
        params: { contactId: id, page: 1, pageSize: 1 },
      })
      setLastInteraction(interactions.data.items[0] ?? null)
    } catch {
      setError('Failed to load contact details.')
    } finally {
      setLoading(false)
    }
  }, [id, canView])

  useEffect(() => {
    void Promise.resolve().then(load)
  }, [load])

  const setPrimary = async () => {
    if (!id) {
      return
    }

    setError('')
    try {
      await api.post(`api/contacts/${id}/set-primary`)
      await load()
    } catch {
      setError('Failed to set primary contact.')
    }
  }

  const summaryRows = useMemo(
    () => contact ? [
      { label: 'Name', value: contact.fullName },
      { label: 'Account', value: contact.accountName ?? '' },
      { label: 'Role', value: contact.contactRoleName ?? '' },
      { label: 'Email', value: contact.email ?? '' },
      { label: 'Mobile', value: contact.mobilePhone ?? '' },
      { label: 'Preferred Method', value: contact.preferredContactMethodName ?? '' },
      { label: 'Last Interaction', value: lastInteraction ? `${lastInteraction.interactionTypeName ?? 'Interaction'} - ${formatDateTime(lastInteraction.interactionDate)}` : '' },
      { label: 'Marketing Consent', value: contact.marketingConsent ? 'Yes' : 'No' },
    ] : [],
    [contact, lastInteraction],
  )

  const detailsRows = useMemo(
    () => contact ? [
      { label: 'Contact Number', value: contact.contactNumber },
      { label: 'Salutation', value: contact.salutationName ?? '' },
      { label: 'Gender', value: contact.genderName ?? '' },
      { label: 'Preferred Name', value: contact.preferredName ?? '' },
      { label: 'Job Title', value: contact.jobTitle ?? '' },
      { label: 'Department', value: contact.department ?? '' },
      { label: 'Alternate Email', value: contact.alternateEmail ?? '' },
      { label: 'Work Phone', value: contact.workPhone ?? '' },
      { label: 'Home Phone', value: contact.homePhone ?? '' },
      { label: 'Fax', value: contact.fax ?? '' },
      { label: 'Preferred Language', value: contact.preferredLanguageName ?? '' },
      { label: 'Preferred Time Zone', value: contact.preferredTimeZoneName ?? '' },
      { label: 'Email Opt-In', value: contact.emailOptIn ? 'Yes' : 'No' },
      { label: 'SMS Opt-In', value: contact.smsOptIn ? 'Yes' : 'No' },
      { label: 'Phone Opt-In', value: contact.phoneOptIn ? 'Yes' : 'No' },
      { label: 'Primary Contact', value: contact.isPrimaryContact ? 'Yes' : 'No' },
      { label: 'Active', value: contact.isActive ? 'Yes' : 'No' },
      { label: 'Notes', value: contact.notes ?? '' },
    ] : [],
    [contact],
  )

  if (!canView) {
    return (
      <EntityPageLayout
        header={<EntityHeader icon={<PersonCallRegular />} title="Contact Details" actions={[{ key: 'back', label: 'Back to List', onClick: () => navigate('/contacts') }]} />}
      >
        <MessageBar intent="error">
          <MessageBarBody>You do not have permission to view contacts.</MessageBarBody>
        </MessageBar>
      </EntityPageLayout>
    )
  }

  const actions = [
    ...(canEdit && id ? [{ key: 'edit', label: 'Edit', onClick: () => navigate(`/contacts/${id}/edit`), appearance: 'primary' as const }] : []),
    ...(canSetPrimary && contact && !contact.isPrimaryContact ? [{ key: 'primary', label: 'Set Primary', onClick: () => void setPrimary(), appearance: 'secondary' as const }] : []),
    { key: 'back', label: 'Back to List', onClick: () => navigate('/contacts'), appearance: 'subtle' as const },
  ]

  return (
    <EntityPageLayout
      header={<EntityHeader icon={<PersonCallRegular />} title={contact?.fullName ?? 'Contact Details'} subtitle={contact?.contactNumber} status={contact?.isActive ? 'Active' : contact ? 'Inactive' : undefined} actions={actions} />}
      tabs={<EntityTabs tabs={tabs} activeTab={activeTab} onTabChange={setActiveTab} />}
      alerts={error ? [{ intent: 'error', text: error }] : undefined}
    >
      {loading ? <Spinner size="small" label="Loading contact..." /> : null}

      {!loading && contact && activeTab === 'summary' ? (
        <>
          <EntitySummaryCard title="Summary" rows={summaryRows} />
          <EntityDetailsGrid rows={detailsRows} />
        </>
      ) : null}

      {!loading && contact && activeTab === 'communications' ? <ContactCommunicationsPanel contactId={contact.id} editable /> : null}
      {!loading && contact && activeTab === 'interaction-history' ? <ContactInteractionsPanel contactId={contact.id} accountId={contact.accountId} editable /> : null}
      {!loading && contact && activeTab === 'related-account' ? <RelatedAccountPanel accountId={contact.accountId} accountName={contact.accountName} /> : null}
      {!loading && contact && activeTab === 'audit-history' ? <AuditHistoryPanel entityName="Contact" entityId={contact.id} /> : null}
    </EntityPageLayout>
  )
}
