import { useEffect, useMemo, useState } from 'react'
import { Dropdown, Field, Input, Option, Switch, Textarea } from '@fluentui/react-components'
import { useNavigate, useParams } from 'react-router-dom'
import { api } from '../../api/client'
import type { EntityConfig, EntityField, FormState } from './adminConfig'
import { useAuth } from '../../auth/AuthContext'
import { DeleteConfirmDialog } from './DeleteConfirmDialog'
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
  statusFromItem,
  tabsForEntity,
  wideField,
} from '../entity-ui/entityMeta'
import designStyles from '../entity-ui/EntityDesign.module.css'

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
  const [activeTab, setActiveTab] = useState('general')

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
  const sections = useMemo(() => sectionMap(config.key, config.fields), [config.key, config.fields])
  const tabs = useMemo(() => tabsForEntity(config.key), [config.key])
  const pageTitle = useMemo(() => getPageTitle(config.title, 'edit'), [config.title])
  const entityIcon = useMemo(() => getEntityIcon(config.key), [config.key])

  const renderField = (field: EntityField) => {
    const value = form[field.key]
    const isWide = wideField(field.key)
    const label = friendlyLabel(field)

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
            disabled={field.readOnlyOnEdit || !canUpdate}
          />
        ) : null}

        {field.kind === 'select' ? (
          <Dropdown
            size="small"
            selectedOptions={[String(value ?? '')]}
            value={field.options?.find((option) => option.value === String(value ?? ''))?.label ?? ''}
            onOptionSelect={(_, data) => setForm((current) => ({ ...current, [field.key]: data.optionValue ?? '' }))}
            disabled={field.readOnlyOnEdit || !canUpdate}
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
            disabled={field.readOnlyOnEdit || !canUpdate}
            onChange={(nextValue) => setForm((current) => ({ ...current, [field.key]: nextValue }))}
          />
        ) : null}

        {field.kind !== 'select' && field.kind !== 'checkbox' && !isLookupLikeField(field.key) && (field.kind === 'textarea' || isMultilineField(field.key)) ? (
          <Textarea
            value={String(value ?? '')}
            onChange={(_, data) => setForm((current) => ({ ...current, [field.key]: data.value }))}
            readOnly={field.readOnlyOnEdit || !canUpdate}
          />
        ) : null}

        {field.kind !== 'select' && field.kind !== 'checkbox' && !isLookupLikeField(field.key) && !(field.kind === 'textarea' || isMultilineField(field.key)) ? (
          <Input
            size="small"
            type={field.kind === 'number' ? 'number' : isDateLikeField(field.key) ? 'date' : 'text'}
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
            readOnly={field.readOnlyOnEdit || !canUpdate}
          />
        ) : null}
      </Field>
    )
  }

  const userWithArrays = item as (TItem & { roles?: string[]; teams?: string[] }) | null

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

  const alerts = [
    ...(!canUpdate ? [{ intent: 'error' as const, text: 'You do not have permission to update records.' }] : []),
    ...(error ? [{ intent: 'error' as const, text: error }] : []),
    ...validationSummary.map((message) => ({ intent: 'warning' as const, text: message })),
  ]

  const headerActions = [
    { key: 'save', label: 'Save', onClick: () => void save(false), appearance: 'primary' as const, disabled: loading || saving || !canUpdate },
    { key: 'save-close', label: 'Save & Close', onClick: () => void save(true), appearance: 'secondary' as const, disabled: loading || saving || !canUpdate },
    ...(canDelete ? [{ key: 'delete', label: 'Delete', onClick: () => setConfirmDeleteOpen(true), appearance: 'subtle' as const, disabled: loading || saving }] : []),
    { key: 'cancel', label: 'Cancel', onClick: () => navigate(config.listPath), appearance: 'subtle' as const },
  ]

  const currentStatus = statusFromItem(item as Record<string, unknown> | null)

  return (
    <>
      <EntityPageLayout
        header={<EntityHeader icon={entityIcon} title={pageTitle} subtitle={config.subtitle} status={currentStatus} actions={headerActions} />}
        tabs={<EntityTabs tabs={tabs} activeTab={activeTab} onTabChange={setActiveTab} />}
        alerts={alerts}
        stickyBar={
          <StickySaveBar
            onSave={() => void save(false)}
            onSaveAndClose={() => void save(true)}
            onCancel={() => navigate(config.listPath)}
            onDelete={canDelete ? () => setConfirmDeleteOpen(true) : undefined}
            showDelete={canDelete}
            disableActions={loading || saving || !canUpdate}
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
        ) : null}

        {activeTab === 'roles' ? (
          <EntityTabPlaceholder
            text={
              (userWithArrays?.roles ?? []).length > 0
                ? `Assigned Roles: ${(userWithArrays?.roles ?? []).join(', ')}`
                : 'No roles are currently assigned to this user.'
            }
          />
        ) : null}

        {activeTab === 'teams' ? (
          <EntityTabPlaceholder
            text={
              (userWithArrays?.teams ?? []).length > 0
                ? `Assigned Teams: ${(userWithArrays?.teams ?? []).join(', ')}`
                : 'No teams are currently assigned to this user.'
            }
          />
        ) : null}

        {activeTab !== 'general' && activeTab !== 'roles' && activeTab !== 'teams' ? (
          <EntityTabPlaceholder
            text={`The ${tabs.find((tab) => tab.key === activeTab)?.label ?? 'selected'} workspace is represented in dedicated related modules and audit screens.`}
          />
        ) : null}
      </EntityPageLayout>

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
