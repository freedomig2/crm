import { useEffect, useMemo, useState } from 'react'
import { Field, Input, Switch, Textarea, Dropdown, Option, Tab, TabList, type SelectTabData } from '@fluentui/react-components'
import { HistoryRegular, KeyRegular, PersonRegular, PeopleTeamRegular, ShieldRegular } from '@fluentui/react-icons'
import { useNavigate, useParams } from 'react-router-dom'
import { api } from '../../api/client'
import { EntityFormLayout } from './EntityFormLayout'
import type { EntityConfig, EntityField, FormState } from './adminConfig'
import { useAuth } from '../../auth/AuthContext'
import { DeleteConfirmDialog } from './DeleteConfirmDialog'
import styles from './EntityFormLayout.module.css'

type FormSection = {
  key: string
  title: string
  icon: React.ReactNode
  fields: EntityField[]
}

type EditTab = 'general' | 'security' | 'roles' | 'teams' | 'audit'

function splitEditSections(configKey: string, fields: EntityField[]) {
  if (configKey === 'users') {
    const account = fields.filter((field) => field.key === 'email' || field.key === 'password')
    const personal = fields.filter((field) => field.key === 'firstName' || field.key === 'lastName')
    const security = fields.filter((field) => field.key === 'isEnabled')
    return {
      general: [
        { key: 'account', title: 'Account Information', icon: <KeyRegular />, fields: account },
        { key: 'personal', title: 'Personal Information', icon: <PersonRegular />, fields: personal },
      ].filter((section) => section.fields.length > 0),
      security: [{ key: 'security', title: 'Security', icon: <ShieldRegular />, fields: security }].filter((section) => section.fields.length > 0),
    }
  }

  const securityFields = fields.filter((field) => ['isEnabled', 'isActive', 'isDefault', 'dataType'].includes(field.key))
  const generalFields = fields.filter((field) => !securityFields.some((securityField) => securityField.key === field.key))

  return {
    general: [{ key: 'general', title: 'General Information', icon: <PersonRegular />, fields: generalFields }].filter((section) => section.fields.length > 0),
    security: [{ key: 'security', title: 'Security', icon: <ShieldRegular />, fields: securityFields }].filter((section) => section.fields.length > 0),
  }
}

export function EntityEditPage<TItem extends { id: string }>({
  config,
}: {
  config: EntityConfig<TItem>
}) {
  const navigate = useNavigate()
  const { id } = useParams()
  const { hasPermission } = useAuth()

  const [form, setForm] = useState<FormState>(config.defaultForm)
  const [item, setItem] = useState<TItem | null>(null)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({})
  const [confirmDeleteOpen, setConfirmDeleteOpen] = useState(false)
  const [activeTab, setActiveTab] = useState<EditTab>('general')

  const canUpdate = config.permissions.update ? hasPermission(config.permissions.update) : false
  const canDelete = config.permissions.delete ? hasPermission(config.permissions.delete) : false

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
        setForm(config.mapItemToForm(data))
      } catch {
        setError('Failed to load record.')
      } finally {
        setLoading(false)
      }
    }

    void run()
  }, [config, id])

  const validationSummary = useMemo(() => Object.values(fieldErrors), [fieldErrors])
  const sections = useMemo(() => splitEditSections(config.key, config.fields), [config.key, config.fields])
  const tabs = useMemo(() => {
    const base: EditTab[] = ['general']

    if (sections.security.length > 0) {
      base.push('security')
    }
    if (config.key === 'users') {
      base.push('roles', 'teams', 'audit')
    }

    return base
  }, [config.key, sections.security.length])

  const renderField = (field: EntityField) => (
    <Field key={field.key} className={styles.fieldItem} label={field.label} required={field.required} validationMessage={fieldErrors[field.key]}>
      {field.kind === 'textarea' ? (
        <Textarea value={String(form[field.key] ?? '')} onChange={(_, data) => setForm((current) => ({ ...current, [field.key]: data.value }))} readOnly={field.readOnlyOnEdit} />
      ) : null}
      {field.kind === 'text' ? (
        <Input size="small" value={String(form[field.key] ?? '')} onChange={(_, data) => setForm((current) => ({ ...current, [field.key]: data.value }))} readOnly={field.readOnlyOnEdit} />
      ) : null}
      {field.kind === 'number' ? (
        <Input size="small" type="number" value={String(form[field.key] ?? 0)} onChange={(_, data) => setForm((current) => ({ ...current, [field.key]: Number(data.value || 0) }))} readOnly={field.readOnlyOnEdit} />
      ) : null}
      {field.kind === 'checkbox' ? (
        <Switch checked={Boolean(form[field.key])} onChange={(_, data) => setForm((current) => ({ ...current, [field.key]: Boolean(data.checked) }))} disabled={field.readOnlyOnEdit} />
      ) : null}
      {field.kind === 'select' ? (
        <Dropdown
          size="small"
          selectedOptions={[String(form[field.key] ?? '')]}
          value={field.options?.find((x) => x.value === String(form[field.key] ?? ''))?.label ?? ''}
          onOptionSelect={(_, data) => setForm((current) => ({ ...current, [field.key]: data.optionValue ?? '' }))}
          disabled={field.readOnlyOnEdit}
        >
          {(field.options ?? []).map((option) => (
            <Option key={option.value} value={option.value} text={option.label}>{option.label}</Option>
          ))}
        </Dropdown>
      ) : null}
    </Field>
  )

  const renderSections = (group: FormSection[]) => (
    group.map((section) => (
      <section key={section.key} className={styles.section}>
        <div className={styles.sectionHeader}>
          {section.icon}
          <span>{section.title}</span>
        </div>
        <div className={styles.sectionGrid}>
          {section.fields.map((field) => renderField(field))}
        </div>
      </section>
    ))
  )

  const userWithArrays = item as (TItem & { roles?: string[]; teams?: string[] }) | null

  const validate = () => {
    const next: Record<string, string> = {}
    for (const field of config.fields) {
      if (field.required && !String(form[field.key] ?? '').trim()) {
        next[field.key] = `${field.label} is required.`
      }
    }
    setFieldErrors(next)
    return Object.keys(next).length === 0
  }

  const save = async (closeAfterSave: boolean) => {
    if (!id || !item || !canUpdate || !validate()) {
      return
    }

    setSaving(true)
    setError('')
    try {
      await api.put(`${config.endpoint}/${id}`, config.mapUpdatePayload(form, item))
      navigate(closeAfterSave ? config.listPath : config.editPath(id))
    } catch {
      setError('Update failed. Please review input values.')
    } finally {
      setSaving(false)
    }
  }

  const deleteRecord = async () => {
    if (!id || !canDelete) {
      return
    }

    setSaving(true)
    setError('')
    try {
      await api.delete(`${config.endpoint}/${id}`)
      navigate(config.listPath)
    } catch {
      setError('Delete failed.')
    } finally {
      setSaving(false)
      setConfirmDeleteOpen(false)
    }
  }

  return (
    <>
      <EntityFormLayout
        title={`Edit ${config.title.replace(/s$/, '')}`}
        subtitle={config.subtitle}
        loading={loading || saving}
        error={!canUpdate ? 'You do not have permission to update records.' : error}
        validationSummary={validationSummary}
        onSave={() => void save(false)}
        onSaveAndClose={() => void save(true)}
        onCancel={() => navigate(config.listPath)}
        onDelete={canDelete ? () => setConfirmDeleteOpen(true) : undefined}
        showDelete={canDelete}
        stickyHeader
      >
        <TabList
          size="small"
          selectedValue={activeTab}
          className={styles.tabs}
          onTabSelect={(_, data: SelectTabData) => setActiveTab(data.value as EditTab)}
        >
          {tabs.includes('general') ? <Tab id="general" icon={<PersonRegular />} value="general">General</Tab> : null}
          {tabs.includes('security') ? <Tab id="security" icon={<ShieldRegular />} value="security">Security</Tab> : null}
          {tabs.includes('roles') ? <Tab id="roles" icon={<PeopleTeamRegular />} value="roles">Roles</Tab> : null}
          {tabs.includes('teams') ? <Tab id="teams" icon={<PeopleTeamRegular />} value="teams">Teams</Tab> : null}
          {tabs.includes('audit') ? <Tab id="audit" icon={<HistoryRegular />} value="audit">Audit History</Tab> : null}
        </TabList>

        <div className={styles.tabPanel}>
          {activeTab === 'general' ? renderSections(sections.general) : null}
          {activeTab === 'security' ? renderSections(sections.security) : null}
          {activeTab === 'roles' ? (
            <div className={styles.placeholderPanel}>
              {(userWithArrays?.roles ?? []).length > 0
                ? `Assigned Roles: ${(userWithArrays?.roles ?? []).join(', ')}`
                : 'No roles are currently assigned to this user.'}
            </div>
          ) : null}
          {activeTab === 'teams' ? (
            <div className={styles.placeholderPanel}>
              {(userWithArrays?.teams ?? []).length > 0
                ? `Assigned Teams: ${(userWithArrays?.teams ?? []).join(', ')}`
                : 'No teams are currently assigned to this user.'}
            </div>
          ) : null}
          {activeTab === 'audit' ? (
            <div className={styles.placeholderPanel}>
              Audit history is available in the Audit Logs module. Use filters by user id to trace changes.
            </div>
          ) : null}
        </div>
      </EntityFormLayout>

      <DeleteConfirmDialog
        open={confirmDeleteOpen}
        title={`Delete ${config.title.replace(/s$/, '')}`}
        message="This action cannot be undone."
        onConfirm={() => void deleteRecord()}
        onCancel={() => setConfirmDeleteOpen(false)}
      />
    </>
  )
}
