import { useMemo, useState } from 'react'
import { Field, Input, Switch, Textarea, Dropdown, Option } from '@fluentui/react-components'
import { KeyRegular, PersonRegular, ShieldRegular } from '@fluentui/react-icons'
import { useNavigate } from 'react-router-dom'
import { api } from '../../api/client'
import { EntityFormLayout } from './EntityFormLayout'
import type { EntityConfig, EntityField, FormState } from './adminConfig'
import { useAuth } from '../../auth/AuthContext'
import styles from './EntityFormLayout.module.css'

type FormSection = {
  key: string
  title: string
  icon: React.ReactNode
  fields: EntityField[]
  wide?: boolean
}

function splitCreateSections(configKey: string, fields: EntityField[]): FormSection[] {
  if (configKey === 'users') {
    const account = fields.filter((field) => field.key === 'email' || field.key === 'password')
    const personal = fields.filter((field) => field.key === 'firstName' || field.key === 'lastName')
    const security = fields.filter((field) => field.key === 'isEnabled')
    return [
      { key: 'account', title: 'Account Information', icon: <KeyRegular />, fields: account },
      { key: 'personal', title: 'Personal Information', icon: <PersonRegular />, fields: personal },
      { key: 'security', title: 'Security', icon: <ShieldRegular />, fields: security },
    ].filter((section) => section.fields.length > 0)
  }

  const securityFields = fields.filter((field) => ['isEnabled', 'isActive', 'isDefault', 'dataType'].includes(field.key))
  const generalFields = fields.filter((field) => !securityFields.some((securityField) => securityField.key === field.key))

  return [
    { key: 'general', title: 'General Information', icon: <PersonRegular />, fields: generalFields, wide: generalFields.length >= 6 },
    { key: 'security', title: 'Security', icon: <ShieldRegular />, fields: securityFields },
  ].filter((section) => section.fields.length > 0)
}

export function EntityCreatePage<TItem extends { id: string }>({
  config,
}: {
  config: EntityConfig<TItem>
}) {
  const navigate = useNavigate()
  const { hasPermission } = useAuth()
  const [form, setForm] = useState<FormState>(config.defaultForm)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({})

  const canCreate = config.permissions.create ? hasPermission(config.permissions.create) : false

  const validationSummary = useMemo(() => Object.values(fieldErrors), [fieldErrors])
  const sections = useMemo(() => splitCreateSections(config.key, config.fields), [config.key, config.fields])

  const renderField = (field: EntityField) => (
    <Field key={field.key} className={styles.fieldItem} label={field.label} required={field.required} validationMessage={fieldErrors[field.key]}>
      {field.kind === 'textarea' ? (
        <Textarea value={String(form[field.key] ?? '')} onChange={(_, data) => setForm((current) => ({ ...current, [field.key]: data.value }))} />
      ) : null}
      {field.kind === 'text' ? (
        <Input size="small" value={String(form[field.key] ?? '')} onChange={(_, data) => setForm((current) => ({ ...current, [field.key]: data.value }))} />
      ) : null}
      {field.kind === 'number' ? (
        <Input size="small" type="number" value={String(form[field.key] ?? 0)} onChange={(_, data) => setForm((current) => ({ ...current, [field.key]: Number(data.value || 0) }))} />
      ) : null}
      {field.kind === 'checkbox' ? (
        <Switch checked={Boolean(form[field.key])} onChange={(_, data) => setForm((current) => ({ ...current, [field.key]: Boolean(data.checked) }))} />
      ) : null}
      {field.kind === 'select' ? (
        <Dropdown
          size="small"
          selectedOptions={[String(form[field.key] ?? '')]}
          value={field.options?.find((x) => x.value === String(form[field.key] ?? ''))?.label ?? ''}
          onOptionSelect={(_, data) => setForm((current) => ({ ...current, [field.key]: data.optionValue ?? '' }))}
        >
          {(field.options ?? []).map((option) => (
            <Option key={option.value} value={option.value} text={option.label}>{option.label}</Option>
          ))}
        </Dropdown>
      ) : null}
    </Field>
  )

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
    if (!canCreate || !validate()) {
      return
    }

    setLoading(true)
    setError('')
    try {
      const { data } = await api.post<TItem>(config.endpoint, config.mapCreatePayload(form))
      navigate(closeAfterSave ? config.listPath : config.editPath(data.id))
    } catch {
      setError('Create failed. Please review input values.')
    } finally {
      setLoading(false)
    }
  }

  if (!canCreate) {
    return (
      <EntityFormLayout
        title={`Create ${config.title.replace(/s$/, '')}`}
        subtitle={config.subtitle}
        loading={false}
        error="You do not have permission to create records."
        validationSummary={[]}
        onSave={() => {}}
        onSaveAndClose={() => {}}
        onCancel={() => navigate(config.listPath)}
      >
        <div />
      </EntityFormLayout>
    )
  }

  return (
    <EntityFormLayout
      title={`Create ${config.title.replace(/s$/, '')}`}
      subtitle={config.subtitle}
      loading={loading}
      error={error}
      validationSummary={validationSummary}
      onSave={() => void save(false)}
      onSaveAndClose={() => void save(true)}
      onCancel={() => navigate(config.listPath)}
      stickyHeader
    >
      {sections.map((section) => (
        <section key={section.key} className={styles.section}>
          <div className={styles.sectionHeader}>
            {section.icon}
            <span>{section.title}</span>
          </div>
          <div className={`${styles.sectionGrid} ${section.wide ? styles.sectionGridWide : ''}`}>
            {section.fields.map((field) => renderField(field))}
          </div>
        </section>
      ))}
    </EntityFormLayout>
  )
}
