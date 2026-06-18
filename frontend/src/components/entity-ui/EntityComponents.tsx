import { useEffect, useMemo, useState } from 'react'
import {
  Badge,
  Button,
  Combobox,
  MessageBar,
  MessageBarBody,
  Option,
  Tab,
  TabList,
  type ButtonProps,
  type SelectTabData,
  type SelectTabEvent,
} from '@fluentui/react-components'
import { Breadcrumbs } from '../../layout/components/Breadcrumbs'
import styles from './EntityDesign.module.css'
import { loadLookupOptions, type LookupOption } from './referenceData'

export type EntityHeaderAction = {
  key: string
  label: string
  onClick?: () => void
  appearance?: ButtonProps['appearance']
  disabled?: boolean
}

export type EntityTabItem = {
  key: string
  label: string
}

export function StatusBadge({ status }: { status?: string }) {
  if (!status) {
    return null
  }

  const normalized = status.toLowerCase()
  const appearance = normalized === 'active' ? 'filled' : 'outline'
  const color = normalized === 'active' ? 'success' : normalized === 'inactive' ? 'warning' : 'informative'

  return (
    <Badge appearance={appearance} color={color} size="small">
      {status}
    </Badge>
  )
}

export function EntityHeader({
  icon,
  title,
  subtitle,
  status,
  actions,
}: {
  icon?: React.ReactNode
  title: string
  subtitle?: string
  status?: string
  actions?: EntityHeaderAction[]
}) {
  return (
    <div className={styles.headerSurface}>
      <div className={styles.compactBreadcrumbs}>
        <Breadcrumbs />
      </div>
      <div className={styles.titleRow}>
        <div className={styles.titleLeft}>
          {icon}
          <div>
            <div className={styles.titleText}>
              <h1>{title}</h1>
              <StatusBadge status={status} />
            </div>
            {subtitle ? <p className={styles.subtitle}>{subtitle}</p> : null}
          </div>
        </div>
        {actions && actions.length > 0 ? (
          <div className={styles.actions}>
            {actions.map((action) => (
              <Button
                key={action.key}
                size="small"
                appearance={action.appearance ?? 'subtle'}
                onClick={action.onClick}
                disabled={action.disabled}
              >
                {action.label}
              </Button>
            ))}
          </div>
        ) : null}
      </div>
    </div>
  )
}

export function EntityTabs({
  tabs,
  activeTab,
  onTabChange,
}: {
  tabs: EntityTabItem[]
  activeTab: string
  onTabChange: (tab: string) => void
}) {
  return (
    <div className={styles.tabsRow}>
      <TabList
        selectedValue={activeTab}
        onTabSelect={(_: SelectTabEvent, data: SelectTabData) => onTabChange(String(data.value))}
        size="small"
      >
        {tabs.map((tab) => (
          <Tab key={tab.key} value={tab.key}>
            {tab.label}
          </Tab>
        ))}
      </TabList>
    </div>
  )
}

export function FormSectionCard({
  title,
  icon,
  children,
}: {
  title: string
  icon?: React.ReactNode
  children: React.ReactNode
}) {
  return (
    <section className={styles.card}>
      <div className={styles.sectionTitle}>
        {icon}
        <span>{title}</span>
      </div>
      <div className={styles.formGrid}>{children}</div>
    </section>
  )
}

export function StickySaveBar({
  onSave,
  onSaveAndClose,
  onCancel,
  onDelete,
  disableActions,
  showDelete,
}: {
  onSave: () => void
  onSaveAndClose: () => void
  onCancel: () => void
  onDelete?: () => void
  disableActions?: boolean
  showDelete?: boolean
}) {
  return (
    <div className={styles.stickyBar}>
      <Button size="small" appearance="primary" onClick={onSave} disabled={disableActions}>
        Save
      </Button>
      <Button size="small" appearance="secondary" onClick={onSaveAndClose} disabled={disableActions}>
        Save and Close
      </Button>
      {showDelete && onDelete ? (
        <Button size="small" appearance="subtle" onClick={onDelete} disabled={disableActions}>
          Delete
        </Button>
      ) : null}
      <Button size="small" appearance="subtle" onClick={onCancel}>
        Cancel
      </Button>
    </div>
  )
}

export function EntitySummaryCard({
  title,
  rows,
}: {
  title: string
  rows: Array<{ label: string; value: string }>
}) {
  return (
    <section className={styles.card}>
      <div className={styles.sectionTitle}>{title}</div>
      <div className={styles.summaryGrid}>
        {rows.map((row) => (
          <div key={row.label} className={styles.detailCell}>
            <p className={styles.detailLabel}>{row.label}</p>
            <p className={styles.detailValue}>{row.value || 'Not set'}</p>
          </div>
        ))}
      </div>
    </section>
  )
}

export function EntityDetailsGrid({
  rows,
}: {
  rows: Array<{ label: string; value: string }>
}) {
  return (
    <section className={styles.card}>
      <div className={styles.sectionTitle}>Details</div>
      <div className={styles.detailsGrid}>
        {rows.map((row) => (
          <div key={`${row.label}-${row.value}`} className={styles.detailCell}>
            <p className={styles.detailLabel}>{row.label}</p>
            <p className={styles.detailValue}>{row.value || 'Not set'}</p>
          </div>
        ))}
      </div>
    </section>
  )
}

export function EntityPageLayout({
  header,
  tabs,
  alerts,
  children,
  stickyBar,
}: {
  header: React.ReactNode
  tabs?: React.ReactNode
  alerts?: Array<{ intent: 'error' | 'warning'; text: string }>
  children: React.ReactNode
  stickyBar?: React.ReactNode
}) {
  return (
    <div className={styles.page}>
      {header}
      {tabs}
      {alerts && alerts.length > 0 ? (
        <div className={styles.alertStack}>
          {alerts.map((alert) => (
            <MessageBar key={`${alert.intent}-${alert.text}`} intent={alert.intent}>
              <MessageBarBody>{alert.text}</MessageBarBody>
            </MessageBar>
          ))}
        </div>
      ) : null}
      <div className={styles.body}>{children}</div>
      {stickyBar}
    </div>
  )
}

export function LookupCombobox({
  fieldKey,
  value,
  onChange,
  disabled,
  placeholder,
}: {
  fieldKey: string
  value: string
  onChange: (nextValue: string) => void
  disabled?: boolean
  placeholder?: string
}) {
  const [query, setQuery] = useState('')
  const [options, setOptions] = useState<LookupOption[]>([])

  useEffect(() => {
    let active = true
    void loadLookupOptions(fieldKey).then((items) => {
      if (active) {
        setOptions(items)
      }
    })

    return () => {
      active = false
    }
  }, [fieldKey])

  const selectedLabel = useMemo(() => options.find((option) => option.value === value)?.label ?? '', [options, value])

  const visibleOptions = useMemo(() => {
    const term = query.trim().toLowerCase()
    if (!term) {
      return options.slice(0, 100)
    }

    return options.filter((option) => option.label.toLowerCase().includes(term)).slice(0, 100)
  }, [options, query])

  return (
    <Combobox
      size="small"
      freeform
      disabled={disabled}
      selectedOptions={value ? [value] : []}
      value={query || selectedLabel}
      placeholder={placeholder ?? 'Search and select'}
      onChange={(event) => setQuery((event.target as HTMLInputElement).value)}
      onOptionSelect={(_, data) => {
        const next = String(data.optionValue ?? '')
        onChange(next)
        setQuery('')
      }}
    >
      {visibleOptions.map((option) => (
        <Option key={option.value} value={option.value} text={option.label}>
          {option.label}
        </Option>
      ))}
    </Combobox>
  )
}

export function EntityTabPlaceholder({ text }: { text: string }) {
  return <div className={styles.placeholder}>{text}</div>
}
