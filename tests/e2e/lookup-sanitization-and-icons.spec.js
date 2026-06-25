const { test, expect } = require('../../frontend/node_modules/@playwright/test')

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

async function getLookupOptions(page, label) {
  const combobox = page.getByRole('combobox', { name: new RegExp(`^${label}$`, 'i') }).first()

  await expect(combobox).toBeVisible()
  await combobox.click()

  const namedListbox = page.getByRole('listbox', { name: new RegExp(`^${label}$`, 'i') }).first()
  const listbox = (await namedListbox.count()) > 0 ? namedListbox : page.locator('[role="listbox"]').last()
  await expect(listbox).toBeVisible()
  await expect(listbox.getByRole('option').first()).toBeVisible()

  const options = await listbox.getByRole('option').allTextContents()
  return options.map((text) => text.trim()).filter(Boolean)
}

async function assertLookupDoesNotShowCodes(options) {
  for (const option of options) {
    expect(option).not.toMatch(/\([A-Z0-9_]+\)$/)
  }
}

async function expectButtonHasIcon(page, buttonName) {
  const button = page.getByRole('button', { name: buttonName }).first()
  await expect(button).toBeVisible()
  await expect(button.locator('[class*="fui-Button__icon"]').first()).toBeVisible()
}

test('lookup options are category-clean and form actions have icons', async ({ page }) => {
  test.setTimeout(180000)
  await clearMenuStorageAndLogin(page)

  await page.goto('/crm/accounts/create')
  let options = await getLookupOptions(page, 'Customer Status')
  expect(options).toContain('Prospect')
  expect(options).toContain('Active Customer')
  expect(options).not.toContain('Female')
  expect(options).not.toContain('WhatsApp')
  expect(options).not.toContain('Decision Maker')
  await assertLookupDoesNotShowCodes(options)

  options = await getLookupOptions(page, 'Ownership Type')
  expect(options).toContain('Private Company')
  expect(options).toContain('Government')
  expect(options).not.toContain('Decision Maker')
  await assertLookupDoesNotShowCodes(options)

  options = await getLookupOptions(page, 'Industry')
  expect(options).toContain('Banking')
  expect(options).toContain('Healthcare')
  await assertLookupDoesNotShowCodes(options)

  await expectButtonHasIcon(page, /^Save$/i)
  await expectButtonHasIcon(page, /^Save & Close$/i)
  await expectButtonHasIcon(page, /^Cancel$/i)

  await page.goto('/contacts/create')
  options = await getLookupOptions(page, 'Gender')
  expect(options).toContain('Male')
  expect(options).toContain('Non-Binary')
  expect(options).not.toContain('Mr')
  expect(options).not.toContain('Email')
  await assertLookupDoesNotShowCodes(options)

  await page.getByRole('tab', { name: /^Preferences$/i }).click()
  options = await getLookupOptions(page, 'Preferred Contact Method')
  expect(options).toContain('Email')
  expect(options).toContain('WhatsApp')
  expect(options).not.toContain('Decision Maker')
  await assertLookupDoesNotShowCodes(options)

  await page.getByRole('tab', { name: /^Relationships$/i }).click()
  options = await getLookupOptions(page, 'Contact Role')
  expect(options).toContain('Decision Maker')
  expect(options).toContain('Technical Contact')
  expect(options).not.toContain('Female')
  await assertLookupDoesNotShowCodes(options)

  await page.goto('/leads/create')
  options = await getLookupOptions(page, 'Lead Source')
  expect(options).toContain('Website')
  expect(options).toContain('Trade Show')
  expect(options).not.toContain('Male')
  await assertLookupDoesNotShowCodes(options)

  await page.goto('/opportunities/create')
  options = await getLookupOptions(page, 'Stage')
  expect(options).toContain('Qualify')
  expect(options).toContain('Won')
  expect(options).toContain('Lost')
  await assertLookupDoesNotShowCodes(options)

  options = await getLookupOptions(page, 'Rating')
  expect(options).toEqual(expect.arrayContaining(['Hot', 'Warm', 'Cold']))
  await assertLookupDoesNotShowCodes(options)

  await page.goto('/service/cases/create')
  options = await getLookupOptions(page, 'Severity')
  expect(options).toContain('Critical')
  expect(options).not.toContain('Female')
  expect(options).not.toContain('Decision Maker')
  await assertLookupDoesNotShowCodes(options)

  await page.goto('/activities/tasks/create')
  options = await getLookupOptions(page, 'Status')
  expect(options).toContain('Open')
  expect(options).toContain('Completed')
  await assertLookupDoesNotShowCodes(options)

  await page.goto('/leads')
  await expect(page.locator('main table')).toBeVisible()
  const firstPrimary = page.locator('main table tbody tr').first().locator('td').nth(1).locator('button').first()
  await firstPrimary.click()
  await page.waitForURL(/\/leads\/[^/]+$/)
  await page.getByRole('button', { name: /^Edit$/i }).click()
  await page.waitForURL(/\/leads\/[^/]+\/edit$/)

  await expectButtonHasIcon(page, /^Save$/i)
})
