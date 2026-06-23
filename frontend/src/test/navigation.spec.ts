import { describe, expect, it } from 'vitest'
import { navGroups } from '../layout/navigation'

describe('navigation defaults', () => {
  it('includes required implemented groups in canonical order', () => {
    const keys = navGroups.map((x) => x.key)

    const required = [
      'sales',
      'service',
      'activities',
      'documents',
      'configuration',
      'reporting',
      'integrations',
      'ai-copilot',
    ]

    for (const key of required) {
      expect(keys).toContain(key)
    }

    const positions = required.map((key) => keys.indexOf(key))
    for (let index = 1; index < positions.length; index += 1) {
      expect(positions[index]).toBeGreaterThan(positions[index - 1])
    }
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
