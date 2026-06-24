import { Button, Input, Spinner } from '@fluentui/react-components'
import { useEffect, useMemo, useState } from 'react'
import { api } from '../api/client'
import { CommandBar } from '../layout/components/CommandBar'
import { PageHeader } from '../layout/components/PageHeader'
import type { DashboardMyOpenTasks } from '../types/models'
import styles from './DashboardWorkspace.module.css'

const formatDate = (value?: string) => (value ? new Date(value).toLocaleDateString() : 'Not set')

export function MyOpenTasksPage() {
  const [data, setData] = useState<DashboardMyOpenTasks | null>(null)
  const [search, setSearch] = useState('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const params = useMemo(() => ({
    page: 1,
    pageSize: 75,
    search: search.trim() || undefined,
  }), [search])

  const load = async () => {
    setLoading(true)
    setError(null)
    try {
      const response = await api.get<DashboardMyOpenTasks>('api/dashboard/my-open-tasks', { params })
      setData(response.data)
    } catch {
      setError('My Open Tasks data could not be loaded.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void (async () => {
      setLoading(true)
      setError(null)
      try {
        const response = await api.get<DashboardMyOpenTasks>('api/dashboard/my-open-tasks', { params })
        setData(response.data)
      } catch {
        setError('My Open Tasks data could not be loaded.')
      } finally {
        setLoading(false)
      }
    })()
  }, [params])

  return (
    <div className={styles.layout}>
      <PageHeader title="My Open Tasks" subtitle="Actionable items requiring execution" />
      <CommandBar actions={[{ key: 'refresh', label: 'Refresh', onClick: () => void load() }]} />

      <section className={styles.panel}>
        <div className={styles.filterBar}>
          <Input value={search} onChange={(_, v) => setSearch(v.value)} placeholder="Search tasks by number, subject, or notes" />
          <div />
          <Button appearance="primary" onClick={() => void load()}>Apply Filters</Button>
        </div>
      </section>

      {error ? <div className={styles.error}>{error}</div> : null}
      {loading ? <Spinner label="Loading open tasks" /> : null}

      <section className={styles.statsGrid}>
        <article className={styles.statCard}>
          <p className={styles.statLabel}>Total Open Tasks</p>
          <p className={styles.statValue}>{data?.totalCount ?? 0}</p>
        </article>
        <article className={styles.statCard}>
          <p className={styles.statLabel}>Overdue Tasks</p>
          <p className={styles.statValue}>{data?.overdueCount ?? 0}</p>
        </article>
      </section>

      <article className={styles.panel}>
        <div className={styles.panelHeader}>
          <h3 className={styles.panelTitle}>Task Queue</h3>
        </div>
        <table className={styles.table}>
          <thead>
            <tr>
              <th>Subject</th>
              <th>Status</th>
              <th>Priority</th>
              <th>Due Date</th>
              <th>Related Record</th>
            </tr>
          </thead>
          <tbody>
            {(data?.items ?? []).map((item) => (
              <tr key={item.id}>
                <td>{item.subject}</td>
                <td>{item.statusName ?? 'Not set'}</td>
                <td>{item.priorityName ?? 'Normal'}</td>
                <td className={item.isOverdue ? styles.warning : ''}>{formatDate(item.dueDate)}</td>
                <td>{item.relatedRecord}</td>
              </tr>
            ))}
          </tbody>
        </table>
        {(data?.items ?? []).length === 0 ? <p className={styles.empty}>No open tasks found</p> : null}
      </article>
    </div>
  )
}
