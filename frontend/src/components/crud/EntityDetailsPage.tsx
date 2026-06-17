import { useEffect, useState } from 'react'
import { MessageBar, MessageBarBody, Spinner } from '@fluentui/react-components'
import { useNavigate, useParams } from 'react-router-dom'
import { api } from '../../api/client'
import { PageHeader } from '../../layout/components/PageHeader'
import type { EntityConfig } from './adminConfig'
import { useAuth } from '../../auth/AuthContext'
import styles from './EntityFormLayout.module.css'

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

  const canView = hasPermission(config.permissions.view)
  const canEdit = config.permissions.update ? hasPermission(config.permissions.update) : false

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

  if (!canView) {
    return (
      <div>
        <PageHeader title={`${config.title} Details`} subtitle={config.subtitle} actions={[{ key: 'back', label: 'Back to List', onClick: () => navigate(config.listPath) }]} />
        <MessageBar intent="error"><MessageBarBody>You do not have permission to view details.</MessageBarBody></MessageBar>
      </div>
    )
  }

  const actions = [] as Array<{ key: string; label: string; onClick?: () => void; appearance?: 'subtle' | 'secondary' | 'primary' }>
  if (canEdit && id && !config.readOnly) {
    actions.push({ key: 'edit', label: 'Edit', onClick: () => navigate(config.editPath(id)), appearance: 'primary' })
  }
  actions.push({ key: 'back', label: 'Back to List', onClick: () => navigate(config.listPath), appearance: 'subtle' })

  return (
    <div>
      <div className={styles.stickyHeader}>
        <PageHeader title={`${config.title} Details`} subtitle={config.subtitle} actions={actions} />
      </div>

      {loading ? <Spinner size="small" label="Loading..." style={{ margin: '10px 0' }} /> : null}
      {error ? <MessageBar intent="error"><MessageBarBody>{error}</MessageBarBody></MessageBar> : null}

      {!loading && item ? (
        <div className={styles.formSurface}>
          {config.details(item).map((line) => (
            <div key={line.label} style={{ border: '1px solid #e5e7eb', padding: 8, borderRadius: 6, marginBottom: 6 }}>
              <strong>{line.label}: </strong>
              <span>{line.value}</span>
            </div>
          ))}
        </div>
      ) : null}
    </div>
  )
}
