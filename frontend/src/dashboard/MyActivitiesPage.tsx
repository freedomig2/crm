import { Button, Input, Spinner } from '@fluentui/react-components'
import { useEffect, useMemo, useState } from 'react'
import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { api } from '../api/client'
import { CommandBar } from '../layout/components/CommandBar'
import { PageHeader } from '../layout/components/PageHeader'
import type { DashboardMyActivities } from '../types/models'
import styles from './DashboardWorkspace.module.css'

const formatDate = (value?: string) => (value ? new Date(value).toLocaleDateString() : 'Not set')

export function MyActivitiesPage() {
  const [data, setData] = useState<DashboardMyActivities | null>(null)
  const [search, setSearch] = useState('')
  const [relatedRecord, setRelatedRecord] = useState('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const params = useMemo(() => ({
    page: 1,
    pageSize: 50,
    search: search.trim() || undefined,
    relatedRecord: relatedRecord.trim() || undefined,
  }), [search, relatedRecord])

  const load = async () => {
    setLoading(true)
    setError(null)
    try {
      const response = await api.get<DashboardMyActivities>('api/dashboard/my-activities', { params })
      setData(response.data)
    } catch {
      setError('My Activities data could not be loaded.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void (async () => {
      setLoading(true)
      setError(null)
      try {
        const response = await api.get<DashboardMyActivities>('api/dashboard/my-activities', { params })
        setData(response.data)
      } catch {
        setError('My Activities data could not be loaded.')
      } finally {
        setLoading(false)
      }
    })()
  }, [params])

  return (
    <div className={styles.layout}>
      <PageHeader title="My Activities" subtitle="Activity queue, context, and execution load" />
      <CommandBar actions={[{ key: 'refresh', label: 'Refresh', onClick: () => void load() }]} />

      <section className={styles.panel}>
        <div className={styles.filterBar}>
          <Input value={search} onChange={(_, v) => setSearch(v.value)} placeholder="Search by number, subject, or notes" />
          <Input value={relatedRecord} onChange={(_, v) => setRelatedRecord(v.value)} placeholder="Filter by related record" />
          <Button appearance="primary" onClick={() => void load()}>Apply Filters</Button>
        </div>
      </section>

      {error ? <div className={styles.error}>{error}</div> : null}
      {loading ? <Spinner label="Loading activities" /> : null}

      <section className={styles.contentGrid}>
        <div className={styles.leftColumn}>
          <article className={styles.panel}>
            <div className={styles.panelHeader}>
              <h3 className={styles.panelTitle}>My Activities ({data?.totalCount ?? 0})</h3>
            </div>
            <table className={styles.table}>
              <thead>
                <tr>
                  <th>Number</th>
                  <th>Subject</th>
                  <th>Status</th>
                  <th>Priority</th>
                  <th>Due Date</th>
                  <th>Related</th>
                </tr>
              </thead>
              <tbody>
                {(data?.items ?? []).map((item) => (
                  <tr key={item.id}>
                    <td>{item.activityNumber}</td>
                    <td>{item.subject}</td>
                    <td>{item.statusName ?? 'Not set'}</td>
                    <td>{item.priorityName ?? 'Normal'}</td>
                    <td>{formatDate(item.dueDate)}</td>
                    <td>{item.relatedRecord}</td>
                  </tr>
                ))}
              </tbody>
            </table>
            {(data?.items ?? []).length === 0 ? <p className={styles.empty}>No activities found</p> : null}
          </article>
        </div>

        <div className={styles.rightColumn}>
          <article className={styles.panel}>
            <div className={styles.panelHeader}>
              <h3 className={styles.panelTitle}>Activities by Status</h3>
            </div>
            <ResponsiveContainer width="100%" height={240}>
              <BarChart data={data?.activitiesByStatus ?? []}>
                <CartesianGrid strokeDasharray="2 2" />
                <XAxis dataKey="name" tick={{ fontSize: 11 }} />
                <YAxis tick={{ fontSize: 11 }} />
                <Tooltip />
                <Bar dataKey="count" fill="#0f6cbd" />
              </BarChart>
            </ResponsiveContainer>
          </article>
        </div>
      </section>
    </div>
  )
}
