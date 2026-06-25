const fs = require('node:fs')
const path = require('node:path')
const { test, expect } = require('../../frontend/node_modules/@playwright/test')

const screenshotDir = path.resolve(__dirname, '../reports/screenshots')

const viewports = [
  { name: '1920x1080', width: 1920, height: 1080 },
  { name: '1366x768', width: 1366, height: 768 },
  { name: '1440x900', width: 1440, height: 900 },
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

async function scrollMainContent(page, ratio) {
  await page.getByTestId('app-page-content').evaluate((element, value) => {
    const maxScroll = element.scrollHeight - element.clientHeight
    element.scrollTop = Math.max(0, Math.floor(maxScroll * value))
  }, ratio)
}

async function waitForGridIdle(page) {
  await expect(page.locator('[role="progressbar"]')).toHaveCount(0, { timeout: 15000 })
}

test.describe('Global layout shell behavior', () => {
  for (const viewport of viewports) {
    test.describe(viewport.name, () => {
      test.use({ viewport: { width: viewport.width, height: viewport.height } })

      test('keeps sidebar fixed while content scrolls and keeps sticky CRUD actions visible', async ({ page }) => {
        test.setTimeout(180000)
        await clearMenuStorageAndLogin(page)

        await page.goto('/dashboard')
        await expect(page.getByTestId('app-shell')).toBeVisible()

        const sidebar = page.getByTestId('crm-sidebar')
        const mainContent = page.getByTestId('app-page-content')
        const before = await sidebar.boundingBox()

        await scrollMainContent(page, 0.85)
        await page.waitForTimeout(200)

        const after = await sidebar.boundingBox()
        expect(before).not.toBeNull()
        expect(after).not.toBeNull()
        expect(Math.abs((before?.y ?? 0) - (after?.y ?? 0))).toBeLessThanOrEqual(1)
        expect(Math.abs((before?.x ?? 0) - (after?.x ?? 0))).toBeLessThanOrEqual(1)

        await saveScreenshot(page, `layout-dashboard-fixed-sidebar-${viewport.name}.png`)

        await page.goto('/leads')
        await expect(page.getByTestId('app-shell')).toBeVisible()
        await expect(page.locator('main table')).toBeVisible()
        await waitForGridIdle(page)

        const rowCount = await page.locator('main table tbody tr').count()
        expect(rowCount).toBeGreaterThan(0)

        const firstRowPrimaryButton = page
          .locator('main table tbody tr')
          .first()
          .locator('td')
          .nth(1)
          .locator('button')
          .first()

        await firstRowPrimaryButton.click()
        await page.waitForURL(/\/leads\/[^/]+$/)
        await expect(page.getByRole('button', { name: /^Edit$/i })).toBeVisible()

        await page.getByRole('button', { name: /^Edit$/i }).click()
        await page.waitForURL(/\/leads\/[^/]+\/edit$/)

        const stickyBar = page.getByTestId('sticky-action-bar')
        await expect(stickyBar).toBeVisible()
        await expect(stickyBar.getByRole('button', { name: /^Save$/i })).toBeVisible()
        await expect(stickyBar.getByRole('button', { name: /^Save & Close$/i })).toBeVisible()
        await expect(stickyBar.getByRole('button', { name: /^Cancel$/i })).toBeVisible()

        const sidebarBox = await sidebar.boundingBox()
        const stickyBox = await stickyBar.boundingBox()
        const mainBox = await mainContent.boundingBox()

        expect(sidebarBox).not.toBeNull()
        expect(stickyBox).not.toBeNull()
        expect(mainBox).not.toBeNull()
        expect((stickyBox?.x ?? 0) + 1).toBeGreaterThan((sidebarBox?.x ?? 0) + (sidebarBox?.width ?? 0))
        expect((stickyBox?.x ?? 0) + (stickyBox?.width ?? 0)).toBeLessThanOrEqual((mainBox?.x ?? 0) + (mainBox?.width ?? 0) + 2)

        await scrollMainContent(page, 0.5)
        await expect(stickyBar).toBeVisible()

        await scrollMainContent(page, 0.98)
        await expect(stickyBar).toBeVisible()

        await stickyBar.getByRole('button', { name: /^Save$/i }).click()
        await page.waitForURL(/\/leads\/[^/]+\/edit$/)
        await expect(stickyBar).toBeVisible()

        await saveScreenshot(page, `layout-edit-sticky-bar-${viewport.name}.png`)

        await page.goto('/leads/create')
        await expect(page.getByTestId('sticky-action-bar')).toBeVisible()
        await scrollMainContent(page, 0.95)
        await expect(page.getByTestId('sticky-action-bar')).toBeVisible()

        await page.goto('/opportunities')
        await expect(page.getByTestId('app-shell')).toBeVisible()
        await expect(page.locator('main table')).toBeVisible()
        await waitForGridIdle(page)
        await page
          .locator('main table tbody tr')
          .first()
          .locator('td')
          .nth(1)
          .locator('button')
          .first()
          .click()

        const detailsUrl = page.url()
        const opportunityMatch = detailsUrl.match(/\/opportunities\/([^/]+)$/)
        expect(opportunityMatch).not.toBeNull()
        const opportunityId = opportunityMatch?.[1]

        await page.goto(`/opportunities/${opportunityId}/mark-lost`)
        await expect(page.getByTestId('sticky-action-bar')).toBeVisible()
        await expect(page.getByTestId('sticky-action-bar').getByRole('button', { name: /^Mark Lost$/i })).toBeVisible()
        await expect(page.getByTestId('sticky-action-bar').getByRole('button', { name: /^Cancel$/i })).toBeVisible()

        await saveScreenshot(page, `layout-action-sticky-bar-${viewport.name}.png`)
      })
    })
  }
})
