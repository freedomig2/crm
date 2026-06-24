const fs = require('node:fs')
const path = require('node:path')
const { test, expect } = require('../../frontend/node_modules/@playwright/test')

const screenshotDir = path.resolve(__dirname, '../reports/screenshots')

const expectedGroupKeys = [
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
]

const filterRouteScreenshots = [
  { route: '/crm/accounts', file: 'accounts-filter-open.png' },
  { route: '/leads', file: 'leads-filter-open.png' },
  { route: '/opportunities', file: 'opportunities-filter-open.png' },
  { route: '/sales/products', file: 'products-filter-open.png' },
  { route: '/sales/quotes', file: 'quotes-filter-open.png' },
  { route: '/sales/orders', file: 'orders-filter-open.png' },
  { route: '/sales/invoices', file: 'invoices-filter-open.png' },
  { route: '/service/cases', file: 'cases-filter-open.png' },
]

function saveScreenshot(page, filename) {
  fs.mkdirSync(screenshotDir, { recursive: true })
  return page.screenshot({ path: path.join(screenshotDir, filename), fullPage: true })
}

async function clearMenuStorageAndLogin(page) {
  await page.goto('/login')
  await page.evaluate(() => {
    localStorage.removeItem('crm.menu.state.version')
    localStorage.removeItem('crm.sidebar.collapsed')
    localStorage.removeItem('crm.sidebar.expanded-groups')
    localStorage.removeItem('crm.sidebar.expanded-groups.v2')
    localStorage.removeItem('crm.sidebar.expanded-groups.v3')
  })

  await page.getByTestId('login-email').fill('admin@crm.local')
  await page.getByTestId('login-password').fill('Admin@12345')
  await page.getByTestId('login-submit').click()
  await page.waitForURL(/\/dashboard/)
}

async function ensureGroupExpanded(page, groupKey) {
  const links = page.getByTestId(`nav-group-links-${groupKey}`)
  if ((await links.count()) === 0) {
    await page.getByTestId(`nav-group-${groupKey}`).click()
  }
  await expect(page.getByTestId(`nav-group-links-${groupKey}`)).toBeVisible()
}

async function waitForGridIdle(page) {
  await expect(page.locator('[role="progressbar"]')).toHaveCount(0, { timeout: 15000 })
}

function getFilterButton(page) {
  return page.locator('main [data-testid="grid-filter-button"]').first()
}

async function openFilters(page) {
  const popover = page.getByTestId('grid-filter-popover')
  const drawer = page.getByTestId('grid-filter-drawer')

  const becameVisible = async () => {
    const popoverVisible = await popover.isVisible().catch(() => false)
    const drawerVisible = await drawer.isVisible().catch(() => false)
    return popoverVisible || drawerVisible
  }

  if (await becameVisible()) {
    return
  }

  const buttons = page.locator('main [data-testid="grid-filter-button"]')
  const count = await buttons.count()

  for (let index = 0; index < count; index += 1) {
    const button = buttons.nth(index)
    const isVisible = await button.isVisible().catch(() => false)
    if (!isVisible) {
      continue
    }

    try {
      await button.click({ force: true, timeout: 2000 })
    } catch {
      continue
    }

    const opened = await expect.poll(becameVisible, { timeout: 3000 }).toBe(true).then(
      () => true,
      () => false,
    )

    if (opened) {
      return
    }
  }

  throw new Error('Unable to open filter surface')
}

async function getOpenFilterSurface(page) {
  const popover = page.getByTestId('grid-filter-popover')
  if (await popover.isVisible().catch(() => false)) {
    return popover
  }

  return page.getByTestId('grid-filter-drawer')
}

test.describe('CRM sidebar, navigation, and filters', () => {
  test('sidebar default state and group toggle behavior', async ({ page }) => {
    await clearMenuStorageAndLogin(page)

    await expect(page.getByTestId('app-shell')).toHaveAttribute('data-collapsed', 'false')
    await expect(page.getByTestId('crm-sidebar')).toHaveAttribute('data-collapsed', 'false')

    for (const key of expectedGroupKeys) {
      await expect(page.getByTestId(`nav-group-${key}`)).toBeVisible()
      await expect(page.getByTestId(`nav-group-links-${key}`)).toHaveCount(0)
    }

    await saveScreenshot(page, 'sidebar-initial-expanded-groups-collapsed.png')

    await page.getByTestId('nav-group-customers').click()
    await expect(page.getByTestId('nav-group-links-customers')).toBeVisible()
    await expect(page.getByTestId('nav-group-links-sales')).toHaveCount(0)
    await saveScreenshot(page, 'customers-group-expanded.png')

    await page.getByTestId('nav-group-sales').click()
    await expect(page.getByTestId('nav-group-links-sales')).toBeVisible()
    await expect(page.getByTestId('nav-group-links-customers')).toBeVisible()
    await saveScreenshot(page, 'sales-group-expanded.png')

    await page.getByTestId('nav-group-customers').click()
    await expect(page.getByTestId('nav-group-links-customers')).toHaveCount(0)
  })

  test('every visible nav item resolves to a real route and supports browser navigation', async ({ page }) => {
    await clearMenuStorageAndLogin(page)

    const groups = await page.locator('[data-testid^="nav-group-"]').all()
    for (const group of groups) {
      const groupTestId = await group.getAttribute('data-testid')
      if (!groupTestId) {
        continue
      }

      const groupKey = groupTestId.replace('nav-group-', '')
      await ensureGroupExpanded(page, groupKey)

      const items = await page.locator(`[data-testid="nav-group-links-${groupKey}"] [data-testid^="nav-item-"]`).all()
      for (const item of items) {
        const href = await item.getAttribute('href')
        if (!href) {
          continue
        }

        await item.click()
        await page.waitForURL(new RegExp(`${href.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}(?:$|/)`))

        await expect(page.getByTestId('app-shell')).toBeVisible()
        await expect(page.locator('main')).toContainText(/./)
        await expect(page.getByText('Page not found')).toHaveCount(0)

        const landedUrl = page.url()
        await page.reload()
        await page.waitForURL(landedUrl)
        await expect(page.getByTestId('app-shell')).toBeVisible()

        await page.goBack()
        await expect(page.getByTestId('app-shell')).toBeVisible()

        await page.goForward()
        await page.waitForURL(landedUrl)
        await expect(page.getByTestId('app-shell')).toBeVisible()

        await ensureGroupExpanded(page, groupKey)
      }
    }
  })

  for (const entry of filterRouteScreenshots) {
    test(`filters: ${entry.route}`, async ({ page }) => {
      test.setTimeout(120000)
      await clearMenuStorageAndLogin(page)

      await page.goto(entry.route)
      await expect(page.getByTestId('app-shell')).toBeVisible()
      await waitForGridIdle(page)

      await expect(page.getByTestId('grid-filter-popover')).toHaveCount(0)
      await expect(page.getByTestId('grid-filter-drawer')).toHaveCount(0)
      const filterButton = getFilterButton(page)
      await expect(filterButton).toBeVisible()

      await openFilters(page)
      const surface = await getOpenFilterSurface(page)
      await saveScreenshot(page, entry.file)

      await surface.getByRole('button', { name: /^Apply$/i }).first().click()
      await expect(page.getByTestId('grid-filter-popover')).toHaveCount(0)
      await expect(page.getByTestId('grid-filter-drawer')).toHaveCount(0)
      await waitForGridIdle(page)

      await openFilters(page)
      await (await getOpenFilterSurface(page)).getByRole('button', { name: /^Clear$/i }).first().click()
      await waitForGridIdle(page)
    })
  }
})
