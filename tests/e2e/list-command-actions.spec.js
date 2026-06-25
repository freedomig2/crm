const fs = require('node:fs')
const path = require('node:path')
const { test, expect } = require('../../frontend/node_modules/@playwright/test')

const screenshotDir = path.resolve(__dirname, '../reports/screenshots')

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

async function waitForGridIdle(page) {
  await expect(page.locator('[role="progressbar"]')).toHaveCount(0, { timeout: 15000 })
  await expect(page.locator('main table')).toBeVisible()
}

async function assertNoActionsColumn(page) {
  await expect(page.locator('main table thead th', { hasText: /^Actions$/i })).toHaveCount(0)
  await expect(page.locator('main table thead th', { hasText: /lead actions/i })).toHaveCount(0)
}

async function assertSelectionStates(page, screenshotPrefix) {
  const viewButton = page.getByRole('button', { name: /^View$/i }).first()

  const selectedBadge = page.getByText(/record selected$/i)
  if (await selectedBadge.count()) {
    const firstRowCheckbox = page.locator('main table tbody tr').first().locator('td').first().getByRole('checkbox')
    await firstRowCheckbox.click()
    await expect(selectedBadge).toHaveCount(0)
  }

  await expect(viewButton).toBeDisabled()
  await saveScreenshot(page, `${screenshotPrefix}-none-selected.png`)

  const rows = page.locator('main table tbody tr')
  const rowCount = await rows.count()
  expect(rowCount).toBeGreaterThan(1)

  await rows.nth(0).locator('td').first().getByRole('checkbox').click()
  await expect(viewButton).toBeEnabled()
  await saveScreenshot(page, `${screenshotPrefix}-single-selected.png`)

  await rows.nth(1).locator('td').first().getByRole('checkbox').click()
  await expect(viewButton).toBeDisabled()
  await saveScreenshot(page, `${screenshotPrefix}-multi-selected.png`)
}

async function assertRowClickSelection(page) {
  const viewButton = page.getByRole('button', { name: /^View$/i }).first()
  await expect(viewButton).toBeDisabled()

  const firstRow = page.locator('main table tbody tr').first()
  await firstRow.click()
  await expect(viewButton).toBeEnabled()
  await expect(page.getByText(/^1 record selected$/i)).toBeVisible()
}

async function assertPrimaryLinkOpensRecord(page, route, expectedUrlPattern) {
  await page.goto(route)
  await waitForGridIdle(page)

  const rowCount = await page.locator('main table tbody tr').count()
  if (rowCount === 0) {
    await saveScreenshot(page, `primary-link-empty-${route.replaceAll('/', '_')}.png`)
    return
  }

  const firstRow = page.locator('main table tbody tr').first()
  const firstRowCheckbox = firstRow.locator('td').first().getByRole('checkbox')
  const firstPrimaryLink = firstRow.locator('a').first()

  await expect(firstPrimaryLink).toBeVisible()
  await firstPrimaryLink.click()
  await page.waitForURL(expectedUrlPattern)

  await page.goBack()
  await waitForGridIdle(page)
  await firstRowCheckbox.click()
  await expect(page.getByText(/^1 record selected$/i)).toBeVisible()
}

async function assertEntityCommand(page, route, buttonName, screenshotName) {
  await page.goto(route)
  await waitForGridIdle(page)
  await assertNoActionsColumn(page)
  await expect(page.getByRole('button', { name: buttonName }).first()).toBeVisible()
  await saveScreenshot(page, screenshotName)
}

async function expectButtonHasIcon(page, buttonName) {
  const button = page.getByRole('button', { name: buttonName }).first()
  await expect(button).toBeVisible()
  await expect(button.locator('[class*="fui-Button__icon"]').first()).toBeVisible()
}

async function expectToolbarButtonHasIcon(page, buttonName) {
  const button = page.getByRole('button', { name: buttonName }).first()
  await expect(button).toBeVisible()
  await expect(button.locator('[class*="fui-Button__icon"]').first()).toBeVisible()
}

async function verifyColumnResizePersistence(page) {
  await page.goto('/admin/users')
  await waitForGridIdle(page)

  const firstRow = page.locator('main table tbody tr').first()
  await firstRow.click()
  await expect(page.getByText(/^1 record selected$/i)).toBeVisible()

  const firstDataHeader = page.locator('main table thead th').nth(1)
  const widthBefore = (await firstDataHeader.boundingBox())?.width ?? 0

  const handle = page.locator('main table thead th [role="separator"]').first()
  const handleBox = await handle.boundingBox()
  expect(handleBox).not.toBeNull()

  await page.mouse.move((handleBox?.x ?? 0) + (handleBox?.width ?? 0) / 2, (handleBox?.y ?? 0) + (handleBox?.height ?? 0) / 2)
  await page.mouse.down()
  await page.mouse.move((handleBox?.x ?? 0) + 120, (handleBox?.y ?? 0) + (handleBox?.height ?? 0) / 2)
  await page.mouse.up()

  const widthAfter = (await firstDataHeader.boundingBox())?.width ?? 0
  expect(widthAfter).toBeGreaterThan(widthBefore + 30)
  await expect(page.getByText(/^1 record selected$/i)).toBeVisible()

  await page.locator('main table').evaluate((table) => {
    const container = table.parentElement
    if (container) {
      container.scrollLeft = 260
    }
  })
  await expect(page.getByText(/^1 record selected$/i)).toBeVisible()

  await page.getByRole('button', { name: /^Email$/i }).first().click()
  await expect(page.getByText(/^1 record selected$/i)).toBeVisible()

  await page.reload()
  await waitForGridIdle(page)
  const widthReloaded = (await page.locator('main table thead th').nth(1).boundingBox())?.width ?? 0
  expect(widthReloaded).toBeGreaterThan(widthBefore + 20)

  await saveScreenshot(page, 'users-column-resize-persisted.png')
}

test('global list command actions use top selection bar', async ({ page }) => {
  test.setTimeout(180000)
  await clearMenuStorageAndLogin(page)

  await page.goto('/opportunities')
  await waitForGridIdle(page)
  await assertNoActionsColumn(page)

  await page.goto('/admin/users')
  await waitForGridIdle(page)
  await assertNoActionsColumn(page)
  await assertRowClickSelection(page)
  await expectButtonHasIcon(page, /^View$/i)
  await expectButtonHasIcon(page, /^Edit$/i)
  await expectButtonHasIcon(page, /^Delete$/i)
  await expectToolbarButtonHasIcon(page, /^Export$/i)
  await expectToolbarButtonHasIcon(page, /^Filters/i)
  await expectToolbarButtonHasIcon(page, /^Columns$/i)
  await assertSelectionStates(page, 'users-command-bar')

  await page.goto('/leads')
  await waitForGridIdle(page)
  await assertNoActionsColumn(page)
  await expect(page.getByRole('button', { name: /^Assign to Me$/i }).first()).toBeVisible()
  await expect(page.getByRole('button', { name: /^Qualify$/i }).first()).toBeVisible()
  await expect(page.getByRole('button', { name: /^Disqualify$/i }).first()).toBeVisible()
  await expect(page.getByRole('button', { name: /^Convert$/i }).first()).toBeVisible()
  await expect(page.getByRole('button', { name: /^Score$/i }).first()).toBeVisible()
  await expectButtonHasIcon(page, /^Assign to Me$/i)
  await expectButtonHasIcon(page, /^Qualify$/i)
  await expectButtonHasIcon(page, /^Disqualify$/i)
  await expectButtonHasIcon(page, /^Convert$/i)
  await expectButtonHasIcon(page, /^Score$/i)
  await saveScreenshot(page, 'leads-command-actions.png')

  await assertEntityCommand(page, '/sales/quotes', /^Manage Lines$/i, 'quotes-command-actions.png')
  await assertEntityCommand(page, '/sales/orders', /^Manage Lines$/i, 'orders-command-actions.png')
  await assertEntityCommand(page, '/sales/invoices', /^Manage Lines$/i, 'invoices-command-actions.png')
  await assertEntityCommand(page, '/service/cases', /^Manage Comments$/i, 'cases-command-actions.png')

  await verifyColumnResizePersistence(page)

  await assertPrimaryLinkOpensRecord(page, '/leads', /\/leads\/[^/]+$/)
  await assertPrimaryLinkOpensRecord(page, '/opportunities', /\/opportunities\/[^/]+$/)
  await assertPrimaryLinkOpensRecord(page, '/crm/accounts', /\/crm\/accounts\/[^/]+$/)
  await assertPrimaryLinkOpensRecord(page, '/contacts', /\/contacts\/[^/]+$/)
  await assertPrimaryLinkOpensRecord(page, '/sales/products', /\/sales\/products\/[^/]+$/)
  await assertPrimaryLinkOpensRecord(page, '/sales/quotes', /\/sales\/quotes\/[^/]+/)
  await assertPrimaryLinkOpensRecord(page, '/sales/orders', /\/sales\/orders\/[^/]+/)
  await assertPrimaryLinkOpensRecord(page, '/sales/invoices', /\/sales\/invoices\/[^/]+/)
  await assertPrimaryLinkOpensRecord(page, '/service/cases', /\/service\/cases\/[^/]+/)
})
