const { test, expect } = require('../../frontend/node_modules/@playwright/test')

async function loginAsAdmin(page) {
  await page.goto('/login')
  await page.getByTestId('login-email').fill('admin@crm.local')
  await page.getByTestId('login-password').fill('Admin@12345')
  await page.getByTestId('login-submit').click()
  await page.waitForURL(/\/dashboard/)
}

test.describe('CRM navigation and filter UX', () => {
  test('sidebar/menu defaults and navigation are functional @auth @permission', async ({ page }) => {
    await loginAsAdmin(page)

    await expect(page.getByTestId('app-shell')).toHaveAttribute('data-collapsed', 'true')
    await expect(page.getByTestId('crm-sidebar')).toHaveAttribute('data-collapsed', 'true')

    await expect(page.getByTestId('nav-group-sales')).toBeVisible()
    await expect(page.getByTestId('nav-group-service')).toBeVisible()

    await page.getByTestId('toggle-sidebar').click()
    await expect(page.getByTestId('app-shell')).toHaveAttribute('data-collapsed', 'false')

    await expect(page.getByTestId('nav-group-links-sales')).toHaveCount(0)
    await page.getByTestId('nav-group-sales').click()
    await expect(page.getByTestId('nav-group-links-sales')).toBeVisible()

    await page.getByTestId('nav-item-leads').click()
    await page.waitForURL(/\/leads/)
    await expect(page.getByRole('heading', { name: 'Leads' })).toBeVisible()
  })

  test('list pages use popup filter controls @crud @lookup @sequence', async ({ page }) => {
    await loginAsAdmin(page)
    await page.goto('/contacts')

    const filterButton = page.getByTestId('grid-filter-button').first()
    await expect(filterButton).toBeVisible()
    await filterButton.evaluate((node) => {
      node.click()
    })

    const popover = page.getByTestId('grid-filter-popover')
    const drawer = page.getByTestId('grid-filter-drawer')
    const filterHeading = page.getByText('Filters', { exact: true })
    const hasPopover = await popover.count()
    const hasDrawer = await drawer.count()

    if (hasPopover > 0) {
      await expect(popover).toBeVisible()
    } else if (hasDrawer > 0) {
      await expect(drawer).toBeVisible()
    } else {
      await expect(filterHeading.first()).toBeVisible()
    }

    await page.getByRole('button', { name: 'Cancel' }).first().click()
  })
})
