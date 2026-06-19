import { Button } from '@fluentui/react-components'
import { useEffect, useState } from 'react'
import { Area, AreaChart, Bar, BarChart, CartesianGrid, Pie, PieChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { api } from '../api/client'
import { DashboardCard } from '../components/common/DashboardCard'
import { PageHeader } from '../layout/components/PageHeader'
import { CommandBar } from '../layout/components/CommandBar'
import type { LeadDashboardSummary } from '../types/models'
import styles from './DashboardPage.module.css'

const userGrowth = [
  { month: 'Jan', users: 92 },
  { month: 'Feb', users: 108 },
  { month: 'Mar', users: 126 },
  { month: 'Apr', users: 132 },
  { month: 'May', users: 141 },
  { month: 'Jun', users: 158 },
]

const loginActivity = [
  { day: 'Mon', success: 88, failed: 6 },
  { day: 'Tue', success: 92, failed: 5 },
  { day: 'Wed', success: 97, failed: 8 },
  { day: 'Thu', success: 102, failed: 3 },
  { day: 'Fri', success: 95, failed: 7 },
]

const roleDistribution = [
  { name: 'Admin', value: 12 },
  { name: 'Sales', value: 54 },
  { name: 'Support', value: 31 },
  { name: 'Read Only', value: 24 },
]

const departmentCounts = [
  { dept: 'Sales', count: 48 },
  { dept: 'Service', count: 37 },
  { dept: 'Operations', count: 22 },
  { dept: 'Finance', count: 14 },
]

export function DashboardPage() {
  const [leadSummary, setLeadSummary] = useState<LeadDashboardSummary | null>(null)

  useEffect(() => {
    void (async () => {
      try {
        const { data } = await api.get<LeadDashboardSummary>('api/leads/dashboard-summary')
        setLeadSummary(data)
      } catch {
        setLeadSummary(null)
      }
    })()
  }, [])

  return (
    <div>
      <PageHeader
        title="Dashboard"
        subtitle="Enterprise CRM administration overview"
      />

      <CommandBar
        actions={[
          { key: 'refresh', label: 'Refresh' },
          { key: 'alerts', label: 'View Security Alerts' },
          { key: 'export', label: 'Export Dashboard' },
        ]}
      />

      <section className={styles.kpiGrid}>
        <DashboardCard label="Total Leads" value={leadSummary?.totalLeads ?? 0} />
        <DashboardCard label="New Leads" value={leadSummary?.newLeads ?? 0} />
        <DashboardCard label="Qualified Leads" value={leadSummary?.qualifiedLeads ?? 0} />
        <DashboardCard label="Converted Leads" value={leadSummary?.convertedLeads ?? 0} />
        <DashboardCard label="Disqualified Leads" value={leadSummary?.disqualifiedLeads ?? 0} />
        <DashboardCard label="Average Lead Score" value={leadSummary?.averageLeadScore ?? 0} />
        <DashboardCard label="Hot Leads" value={leadSummary?.hotLeads ?? 0} />
      </section>

      <section className={styles.chartGrid}>
        <article className={styles.chartCard}>
          <p className={styles.sectionTitle}>Leads by Source</p>
          <ResponsiveContainer width="100%" height={190}>
            <BarChart data={leadSummary?.leadsBySource ?? []}>
              <CartesianGrid strokeDasharray="2 2" />
              <XAxis dataKey="name" tick={{ fontSize: 11 }} />
              <YAxis tick={{ fontSize: 11 }} />
              <Tooltip />
              <Bar dataKey="count" fill="#0d9488" />
            </BarChart>
          </ResponsiveContainer>
        </article>

        <article className={styles.chartCard}>
          <p className={styles.sectionTitle}>Leads by Status</p>
          <ResponsiveContainer width="100%" height={190}>
            <PieChart>
              <Pie data={leadSummary?.leadsByStatus ?? []} dataKey="count" nameKey="name" outerRadius={70} fill="#7c3aed" />
              <Tooltip />
            </PieChart>
          </ResponsiveContainer>
        </article>
      </section>

      <section className={styles.widgetGrid}>
        <article className={styles.widgetCard}>
          <p className={styles.sectionTitle}>Recent Leads</p>
          <ul className={styles.widgetList}>
            {(leadSummary?.recentLeads ?? []).length === 0 ? <li>No recent leads</li> : null}
            {(leadSummary?.recentLeads ?? []).map((lead) => (
              <li key={lead.id}>{lead.leadNumber} - {lead.topic}</li>
            ))}
          </ul>
        </article>
        <article className={styles.widgetCard}>
          <p className={styles.sectionTitle}>Recently Converted Leads</p>
          <ul className={styles.widgetList}>
            {(leadSummary?.recentlyConvertedLeads ?? []).length === 0 ? <li>No recently converted leads</li> : null}
            {(leadSummary?.recentlyConvertedLeads ?? []).map((lead) => (
              <li key={lead.id}>{lead.leadNumber} - {lead.topic}</li>
            ))}
          </ul>
        </article>
      </section>

      <section className={styles.kpiGrid}>
        <DashboardCard label="Total Users" value={158} delta="+8.3% this month" />
        <DashboardCard label="Active Users" value={149} delta="94.3% active ratio" />
        <DashboardCard label="Locked Users" value={5} delta="-2 from yesterday" />
        <DashboardCard label="Roles" value={6} />
        <DashboardCard label="Teams" value={14} />
        <DashboardCard label="Departments" value={9} />
        <DashboardCard label="Open Audit Events" value={23} delta="5 critical" />
        <DashboardCard label="System Settings Changed" value={11} delta="24h window" />
      </section>

      <section className={styles.chartGrid}>
        <article className={styles.chartCard}>
          <p className={styles.sectionTitle}>User Growth</p>
          <ResponsiveContainer width="100%" height={190}>
            <AreaChart data={userGrowth}>
              <CartesianGrid strokeDasharray="2 2" />
              <XAxis dataKey="month" tick={{ fontSize: 11 }} />
              <YAxis tick={{ fontSize: 11 }} />
              <Tooltip />
              <Area type="monotone" dataKey="users" stroke="#0f6cbd" fill="#dbeafe" />
            </AreaChart>
          </ResponsiveContainer>
        </article>

        <article className={styles.chartCard}>
          <p className={styles.sectionTitle}>Login Activity</p>
          <ResponsiveContainer width="100%" height={190}>
            <BarChart data={loginActivity}>
              <CartesianGrid strokeDasharray="2 2" />
              <XAxis dataKey="day" tick={{ fontSize: 11 }} />
              <YAxis tick={{ fontSize: 11 }} />
              <Tooltip />
              <Bar dataKey="success" fill="#0d9488" />
              <Bar dataKey="failed" fill="#dc2626" />
            </BarChart>
          </ResponsiveContainer>
        </article>

        <article className={styles.chartCard}>
          <p className={styles.sectionTitle}>Role Distribution</p>
          <ResponsiveContainer width="100%" height={190}>
            <PieChart>
              <Pie data={roleDistribution} dataKey="value" nameKey="name" outerRadius={70} fill="#0f6cbd" />
              <Tooltip />
            </PieChart>
          </ResponsiveContainer>
        </article>

        <article className={styles.chartCard}>
          <p className={styles.sectionTitle}>Department User Count</p>
          <ResponsiveContainer width="100%" height={190}>
            <BarChart data={departmentCounts}>
              <CartesianGrid strokeDasharray="2 2" />
              <XAxis dataKey="dept" tick={{ fontSize: 11 }} />
              <YAxis tick={{ fontSize: 11 }} />
              <Tooltip />
              <Bar dataKey="count" fill="#2563eb" />
            </BarChart>
          </ResponsiveContainer>
        </article>
      </section>

      <section className={styles.widgetGrid}>
        <article className={styles.widgetCard}>
          <p className={styles.sectionTitle}>Recent Audit Logs</p>
          <ul className={styles.widgetList}>
            <li>Role permissions updated for Sales Manager</li>
            <li>System setting "PasswordExpiryDays" changed</li>
            <li>Department hierarchy updated for Service</li>
          </ul>
        </article>
        <article className={styles.widgetCard}>
          <p className={styles.sectionTitle}>Recent Logins</p>
          <ul className={styles.widgetList}>
            <li>alex.smith - 2 min ago</li>
            <li>rachel.owens - 7 min ago</li>
            <li>admin@crm.local - 11 min ago</li>
          </ul>
        </article>
        <article className={styles.widgetCard}>
          <p className={styles.sectionTitle}>Failed Login Attempts</p>
          <ul className={styles.widgetList}>
            <li>5 attempts for support.temp</li>
            <li>3 attempts from IP 10.10.3.41</li>
            <li>2 attempts for sales.contractor</li>
          </ul>
        </article>
        <article className={styles.widgetCard}>
          <p className={styles.sectionTitle}>Users Pending Activation</p>
          <ul className={styles.widgetList}>
            <li>luna.bishop</li>
            <li>omar.khan</li>
            <li>mia.carter</li>
          </ul>
        </article>
        <article className={styles.widgetCard}>
          <p className={styles.sectionTitle}>Recently Updated Settings</p>
          <ul className={styles.widgetList}>
            <li>PasswordExpiryDays</li>
            <li>SessionTimeoutMinutes</li>
            <li>CaseAutoAssignment</li>
          </ul>
        </article>
        <article className={styles.widgetCard}>
          <p className={styles.sectionTitle}>Security Alerts / Top Active Users</p>
          <ul className={styles.widgetList}>
            <li>2 high-risk geolocation sign-ins detected</li>
            <li>Top users: alex.smith, rachel.owens, eva.kim</li>
          </ul>
        </article>
      </section>

      <div className={styles.quickActions}>
        <Button size="small" appearance="secondary">Create User</Button>
        <Button size="small" appearance="secondary">Create Role</Button>
        <Button size="small" appearance="secondary">Create Team</Button>
        <Button size="small" appearance="secondary">Add Department</Button>
        <Button size="small" appearance="secondary">Manage Permissions</Button>
        <Button size="small" appearance="secondary">View Audit Logs</Button>
        <Button size="small" appearance="secondary">Open System Settings</Button>
      </div>
    </div>
  )
}
