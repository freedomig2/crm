import { useEffect, useMemo, useState } from 'react'
import { Spinner } from '@fluentui/react-components'
import { useNavigate, useParams } from 'react-router-dom'
import { api } from '../../api/client'
import type { EntityConfig } from './adminConfig'
import { useAuth } from '../../auth/AuthContext'
import { EntityDetailsGrid, EntityHeader, EntityPageLayout, EntitySummaryCard, EntityTabPlaceholder, EntityTabs } from '../entity-ui/EntityComponents'
import { friendlyLabel, getEntityIcon, getPageTitle, statusFromItem, tabsForEntity } from '../entity-ui/entityMeta'
import { resolveDisplayValue } from '../entity-ui/referenceData'

export function EntityDetailsPage<TItem extends { id: string }>({
  config,
}: {
  config: EntityConfig<TItem>
}) {
  const navigate = useNavigate()
  const { id } = useParams()
  const { hasPermission } = useAuth()
  const [item, setItem] = useState<TItem | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [activeTab, setActiveTab] = useState('general')
  const [resolvedRows, setResolvedRows] = useState<Array<{ label: string; value: string }>>([])

  const canView = hasPermission(config.permissions.view)
  const canEdit = config.permissions.update ? hasPermission(config.permissions.update) : false
  const tabs = useMemo(() => tabsForEntity(config.key), [config.key])
  const pageTitle = useMemo(() => getPageTitle(config.title, 'details'), [config.title])
  const entityIcon = useMemo(() => getEntityIcon(config.key), [config.key])

  useEffect(() => {
    const run = async () => {
      if (!id) {
        setError('Missing record id.')
        setLoading(false)
        return
      }

      try {
        const { data } = await api.get<TItem>(`${config.endpoint}/${id}`)
        setItem(data)
      } catch {
        setError('Failed to load details.')
      } finally {
        setLoading(false)
      }
    }

    void run()
  }, [config.endpoint, id])

  useEffect(() => {
    const run = async () => {
      if (!item) {
        setResolvedRows([])
        return
      }

      const plain = item as Record<string, unknown>
      const rows = await Promise.all(
        config.fields.map(async (field) => {
          const value = plain[field.key]
          const resolvedValue = await resolveDisplayValue(field.key, value)
          return {
            label: friendlyLabel(field),
            value: resolvedValue,
          }
        }),
      )

      const fallback = config
        .details(item)
        .map((line) => ({
          label: line.label.replace(/Lookup Id/gi, '').replace(/ Id$/gi, '').trim(),
          value: /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(line.value)
            ? 'Linked record'
            : line.value,
        }))

      const merged = [...rows, ...fallback.filter((line) => !rows.some((row) => row.label.toLowerCase() === line.label.toLowerCase()))]
      setResolvedRows(merged)
    }

    void run()
  }, [config, item])

  if (!canView) {
    return (
      <EntityPageLayout
        header={
          <EntityHeader
            icon={entityIcon}
            title={pageTitle}
            subtitle={config.subtitle}
            actions={[{ key: 'back', label: 'Back to List', onClick: () => navigate(config.listPath) }]}
          />
        }
        alerts={[{ intent: 'error', text: 'You do not have permission to view details.' }]}
      >
        <EntityTabPlaceholder text="Access denied." />
      </EntityPageLayout>
    )
  }

  const actions = [] as Array<{ key: string; label: string; onClick?: () => void; appearance?: 'subtle' | 'secondary' | 'primary' }>
  if (canEdit && id && !config.readOnly) {
    actions.push({ key: 'edit', label: 'Edit', onClick: () => navigate(config.editPath(id)), appearance: 'primary' })
  }
  actions.push({ key: 'back', label: 'Back to List', onClick: () => navigate(config.listPath), appearance: 'subtle' })

  const summaryRows = [
    ...resolvedRows.filter((row) => ['Status', 'Owner', 'Primary Contact', 'Industry', 'Customer Segment', 'Account Type'].includes(row.label)).slice(0, 6),
  ]

  const otherRows = resolvedRows.filter((row) => !summaryRows.some((summary) => summary.label === row.label))

  return (
    <EntityPageLayout
      header={
        <EntityHeader
          icon={entityIcon}
          title={pageTitle}
          subtitle={config.subtitle}
          status={statusFromItem(item as Record<string, unknown> | null)}
          actions={actions}
        />
      }
      tabs={<EntityTabs tabs={tabs} activeTab={activeTab} onTabChange={setActiveTab} />}
      alerts={error ? [{ intent: 'error', text: error }] : undefined}
    >
      {loading ? <Spinner size="small" label="Loading details..." /> : null}

      {!loading && item && activeTab === 'general' ? (
        <>
          <EntitySummaryCard title="Overview" rows={summaryRows.length > 0 ? summaryRows : resolvedRows.slice(0, 6)} />
          <EntityDetailsGrid rows={otherRows.length > 0 ? otherRows : resolvedRows} />
        </>
      ) : null}

      {!loading && activeTab !== 'general' ? (
        <EntityTabPlaceholder text={`The ${tabs.find((tab) => tab.key === activeTab)?.label ?? 'selected'} view is shown through related records and dedicated modules.`} />
      ) : null}
    </EntityPageLayout>
  )
}
