import { describe, expect, it } from 'vitest'
import { navGroups } from '../layout/navigation'

describe('navigation defaults', () => {
  it('follows canonical sidebar group order', () => {
    const keys = navGroups.map((x) => x.key)
    expect(keys).toEqual([
      'dashboard',
      'customers',
      'sales',
      'marketing',
      'service',
      'projects',
      'documents',
      'activities',
      'finance',
      'reporting',
      'administration',
      'security',
      'configuration',
      'data-management',
      'audit',
      'integrations',
      'ai-copilot',
      'personal',
    ])
  })

  it('shows implemented groups and hides future groups by default', () => {
    const enabledGroups = navGroups.filter((group) => group.enabled !== false).map((group) => group.key)
    expect(enabledGroups).toEqual([
      'dashboard',
      'customers',
      'sales',
      'service',
      'reporting',
      'administration',
      'security',
      'configuration',
      'audit',
      'integrations',
      'ai-copilot',
    ])
  })

  it('exposes implemented AI menu items and hides future placeholders by default', () => {
    const aiGroup = navGroups.find((x) => x.key === 'ai-copilot')
    expect(aiGroup).toBeDefined()

    const enabledItems = (aiGroup?.items ?? []).filter((item) => item.enabled !== false).map((item) => item.key)
    expect(enabledItems).toContain('ai-dashboard')
    expect(enabledItems).toContain('next-best-actions')
    expect(enabledItems).toContain('ai-prompt-templates')
    expect(enabledItems).not.toContain('predictive-analytics')
  })
})
