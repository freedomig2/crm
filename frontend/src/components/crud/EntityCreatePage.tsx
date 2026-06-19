import { useEffect, useMemo, useState } from 'react'
import { Dropdown, Field, Input, Option, Switch, Textarea } from '@fluentui/react-components'
import { useNavigate } from 'react-router-dom'
import { api } from '../../api/client'
import type { EntityConfig, EntityField, FormState } from './adminConfig'
import { useAuth } from '../../auth/AuthContext'
import {
  EntityHeader,
  EntityPageLayout,
  EntityTabPlaceholder,
  EntityTabs,
  FormSectionCard,
  LookupCombobox,
  StickySaveBar,
} from '../entity-ui/EntityComponents'
import {
  friendlyLabel,
  getEntityIcon,
  getPageTitle,
  isDateLikeField,
  isLookupLikeField,
  isMultilineField,
  sectionMap,
  tabsForEntity,
  wideField,
} from '../entity-ui/entityMeta'
import designStyles from '../entity-ui/EntityDesign.module.css'
import { loadNumberSequencePreview } from '../../configuration/numberSequenceUtils'

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
  const [activeTab, setActiveTab] = useState('general')

  const canCreate = config.permissions.create ? hasPermission(config.permissions.create) : false

  const validationSummary = useMemo(() => Object.values(fieldErrors), [fieldErrors])
  const sections = useMemo(() => sectionMap(config.key, config.fields), [config.key, config.fields])
  const tabs = useMemo(() => tabsForEntity(config.key), [config.key])
  const pageTitle = useMemo(() => getPageTitle(config.title, 'create'), [config.title])
  const entityIcon = useMemo(() => getEntityIcon(config.key), [config.key])

  useEffect(() => {
    const generatedNumber = config.generatedNumber
    if (!generatedNumber) {
      return
    }

    let active = true
    void loadNumberSequencePreview(generatedNumber.sequenceCode)
      .then((preview) => {
        if (active) {
          setForm((current) => ({ ...current, [generatedNumber.fieldKey]: preview || 'Generated on save' }))
        }
      })
      .catch(() => {
        if (active) {
          setForm((current) => ({ ...current, [generatedNumber.fieldKey]: 'Generated on save' }))
        }
      })

    return () => {
      active = false
    }
  }, [config.generatedNumber])

  const renderField = (field: EntityField) => {
    const value = form[field.key]
    const isWide = wideField(field.key)
    const label = friendlyLabel(field)
    const isReadOnly = Boolean(field.readOnlyOnCreate)

    return (
      <Field
        key={field.key}
        className={isWide ? designStyles.fieldWide : undefined}
        label={label}
        required={field.required}
        validationMessage={fieldErrors[field.key]}
      >
        {field.kind === 'checkbox' ? (
          <Switch
            checked={Boolean(value)}
            onChange={(_, data) => setForm((current) => ({ ...current, [field.key]: Boolean(data.checked) }))}
            disabled={isReadOnly}
          />
        ) : null}

        {field.kind === 'select' ? (
          <Dropdown
            size="small"
            selectedOptions={[String(value ?? '')]}
            value={field.options?.find((option) => option.value === String(value ?? ''))?.label ?? ''}
            onOptionSelect={(_, data) => setForm((current) => ({ ...current, [field.key]: data.optionValue ?? '' }))}
            disabled={isReadOnly}
          >
            {(field.options ?? []).map((option) => (
              <Option key={option.value} value={option.value} text={option.label}>
                {option.label}
              </Option>
            ))}
          </Dropdown>
        ) : null}

        {field.kind !== 'select' && field.kind !== 'checkbox' && isLookupLikeField(field.key) ? (
          <LookupCombobox
            fieldKey={field.key}
            value={String(value ?? '')}
            disabled={isReadOnly}
            onChange={(nextValue) => setForm((current) => ({ ...current, [field.key]: nextValue }))}
          />
        ) : null}

        {field.kind !== 'select' && field.kind !== 'checkbox' && !isLookupLikeField(field.key) && (field.kind === 'textarea' || isMultilineField(field.key)) ? (
          <Textarea
            value={String(value ?? '')}
            readOnly={isReadOnly}
            onChange={(_, data) => setForm((current) => ({ ...current, [field.key]: data.value }))}
          />
        ) : null}

        {field.kind !== 'select' && field.kind !== 'checkbox' && !isLookupLikeField(field.key) && !(field.kind === 'textarea' || isMultilineField(field.key)) ? (
          <Input
            size="small"
            type={
              field.kind === 'number'
                ? 'number'
                : isDateLikeField(field.key)
                  ? 'date'
                  : 'text'
            }
            value={
              isDateLikeField(field.key)
                ? String(value ?? '').slice(0, 10)
                : String(value ?? (field.kind === 'number' ? 0 : ''))
            }
            onChange={(_, data) =>
              setForm((current) => ({
                ...current,
                [field.key]: field.kind === 'number' ? Number(data.value || 0) : data.value,
              }))
            }
            readOnly={isReadOnly}
          />
        ) : null}
      </Field>
    )
  }

  const validate = () => {
    const next: Record<string, string> = {}
    for (const field of config.fields) {
      if (field.required && !String(form[field.key] ?? '').trim()) {
        next[field.key] = `${friendlyLabel(field)} is required.`
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
      <EntityPageLayout
        header={
          <EntityHeader
            icon={entityIcon}
            title={pageTitle}
            subtitle={config.subtitle}
            actions={[{ key: 'cancel', label: 'Cancel', onClick: () => navigate(config.listPath) }]}
          />
        }
        alerts={[{ intent: 'error', text: 'You do not have permission to create records.' }]}
      >
        <EntityTabPlaceholder text="Access denied." />
      </EntityPageLayout>
    )
  }

  const alerts = [
    ...(error ? [{ intent: 'error' as const, text: error }] : []),
    ...validationSummary.map((message) => ({ intent: 'warning' as const, text: message })),
  ]

  const headerActions = [
    { key: 'save', label: 'Save', onClick: () => void save(false), appearance: 'primary' as const, disabled: loading },
    { key: 'save-close', label: 'Save & Close', onClick: () => void save(true), appearance: 'secondary' as const, disabled: loading },
    { key: 'cancel', label: 'Cancel', onClick: () => navigate(config.listPath), appearance: 'subtle' as const },
  ]

  return (
    <EntityPageLayout
      header={<EntityHeader icon={entityIcon} title={pageTitle} subtitle={config.subtitle} actions={headerActions} />}
      tabs={<EntityTabs tabs={tabs} activeTab={activeTab} onTabChange={setActiveTab} />}
      alerts={alerts}
      stickyBar={
        <StickySaveBar
          onSave={() => void save(false)}
          onSaveAndClose={() => void save(true)}
          onCancel={() => navigate(config.listPath)}
          disableActions={loading}
        />
      }
    >
      {activeTab === 'general' ? (
        sections.map((section) => (
          <FormSectionCard key={section.key} title={section.title} icon={section.icon}>
            {section.fields
              .map((fieldKey) => config.fields.find((field) => field.key === fieldKey))
              .filter((field): field is EntityField => Boolean(field))
              .map((field) => renderField(field))}
          </FormSectionCard>
        ))
      ) : (
        <EntityTabPlaceholder text={`The ${tabs.find((tab) => tab.key === activeTab)?.label ?? 'selected'} workspace will be available as related data is connected to this record.`} />
      )}
    </EntityPageLayout>
  )
}
