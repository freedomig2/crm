import { Badge, Button, Spinner } from '@fluentui/react-components'
import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../api/client'
import { CommandBar } from '../layout/components/CommandBar'
import { PageHeader } from '../layout/components/PageHeader'
import type { DashboardMyWork } from '../types/models'
import styles from './DashboardWorkspace.module.css'

const formatMoney = (value?: number) => new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(value ?? 0)
const formatDate = (value?: string) => (value ? new Date(value).toLocaleDateString() : 'Not set')

export function MyWorkPage() {
  const navigate = useNavigate()
  const [data, setData] = useState<DashboardMyWork | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const load = async () => {
    setLoading(true)
    setError(null)
    try {
      const response = await api.get<DashboardMyWork>('api/dashboard/my-work')
      setData(response.data)
    } catch {
      setError('My Work data could not be loaded.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void (async () => {
      setLoading(true)
      setError(null)
      try {
        const response = await api.get<DashboardMyWork>('api/dashboard/my-work')
        setData(response.data)
      } catch {
        setError('My Work data could not be loaded.')
      } finally {
        setLoading(false)
      }
    })()
  }, [])

  return (
    <div className={styles.layout}>
      <PageHeader title="My Work" subtitle="Your assigned records and immediate priorities" />
      <CommandBar
        actions={[
          { key: 'refresh', label: 'Refresh', onClick: () => void load() },
          { key: 'my-activities', label: 'My Activities', onClick: () => navigate('/dashboard/my-activities') },
          { key: 'my-open-tasks', label: 'My Open Tasks', onClick: () => navigate('/dashboard/my-open-tasks') },
        ]}
      />

      {error ? <div className={styles.error}>{error}</div> : null}
      {loading ? <Spinner label="Loading my work" /> : null}

      <section className={styles.contentGrid}>
        <div className={styles.leftColumn}>
          <article className={styles.panel}>
            <div className={styles.panelHeader}>
              <h3 className={styles.panelTitle}>Assigned Opportunities</h3>
              <Badge appearance="outline">{data?.assignedOpportunities.length ?? 0}</Badge>
            </div>
            <ul className={styles.rowList}>
              {(data?.assignedOpportunities ?? []).map((item) => (
                <li key={item.id} className={styles.rowItem}>
                  <p className={styles.rowTitle}>{item.opportunityNumber} - {item.topic}</p>
                  <p className={styles.rowMeta}>
                    <span>{item.stageName ?? 'Not set'}</span>
                    <span>{formatMoney(item.estimatedRevenue)}</span>
                    <span>{formatDate(item.estimatedCloseDate)}</span>
                  </p>
                </li>
              ))}
            </ul>
            {(data?.assignedOpportunities ?? []).length === 0 ? <p className={styles.empty}>No assigned opportunities</p> : null}
          </article>

          <article className={styles.panel}>
            <div className={styles.panelHeader}>
              <h3 className={styles.panelTitle}>Assigned Cases</h3>
              <Badge appearance="outline">{data?.assignedCases.length ?? 0}</Badge>
            </div>
            <ul className={styles.rowList}>
              {(data?.assignedCases ?? []).map((item) => (
                <li key={item.id} className={styles.rowItem}>
                  <p className={styles.rowTitle}>{item.caseNumber} - {item.subject}</p>
                  <p className={styles.rowMeta}>
                    <span>{item.priorityName ?? 'Not set'}</span>
                    <span>{item.statusName ?? 'Not set'}</span>
                    <span>{formatDate(item.dueAt)}</span>
                  </p>
                </li>
              ))}
            </ul>
            {(data?.assignedCases ?? []).length === 0 ? <p className={styles.empty}>No assigned cases</p> : null}
          </article>
        </div>

        <div className={styles.rightColumn}>
          <article className={styles.panel}>
            <div className={styles.panelHeader}>
              <h3 className={styles.panelTitle}>Pending Approvals</h3>
              <Badge appearance="tint">{data?.pendingApprovals.length ?? 0}</Badge>
            </div>
            <ul className={styles.rowList}>
              {(data?.pendingApprovals ?? []).map((item) => (
                <li key={item.id} className={styles.rowItem}>
                  <p className={styles.rowTitle}>{item.referenceNumber} ({item.type})</p>
                  <p className={styles.rowMeta}>
                    <span>{item.accountName ?? 'No account'}</span>
                    <span>{formatMoney(item.totalAmount)}</span>
                    <span>{item.approvalStatusName ?? 'Pending'}</span>
                  </p>
                </li>
              ))}
            </ul>
            {(data?.pendingApprovals ?? []).length === 0 ? <p className={styles.empty}>No pending approvals</p> : null}
          </article>

          <article className={styles.panel}>
            <div className={styles.panelHeader}>
              <h3 className={styles.panelTitle}>Overdue Tasks</h3>
              <Button size="small" appearance="subtle" onClick={() => navigate('/dashboard/my-open-tasks')}>Open Task View</Button>
            </div>
            <ul className={styles.rowList}>
              {(data?.overdueTasks ?? []).map((item) => (
                <li key={item.id} className={styles.rowItem}>
                  <p className={styles.rowTitle}>{item.subject}</p>
                  <p className={styles.rowMeta}>
                    <span>{item.priorityName ?? 'Normal'}</span>
                    <span>{item.relatedRecord}</span>
                    <span className={styles.warning}>{formatDate(item.dueDate)}</span>
                  </p>
                </li>
              ))}
            </ul>
            {(data?.overdueTasks ?? []).length === 0 ? <p className={styles.empty}>No overdue tasks</p> : null}
          </article>
        </div>
      </section>
    </div>
  )
}
