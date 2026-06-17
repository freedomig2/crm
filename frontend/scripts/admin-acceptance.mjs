import { chromium, request } from 'playwright'

const baseUrl = 'http://localhost:5253'
const credentials = {
  email: 'admin@crm.local',
  password: 'Admin@12345',
}

const normalizePath = (value) => value.replace(/\/$/, '')

const routes = [
  { path: '/dashboard' },
  { path: '/admin/users', expectCreateRoute: '/admin/users/create' },
  { path: '/admin/roles', expectCreateRoute: '/admin/roles/create' },
  { path: '/admin/permissions', expectCreateRoute: '/admin/permissions/create' },
  { path: '/admin/teams', expectCreateRoute: '/admin/teams/create' },
  { path: '/admin/departments', expectCreateRoute: '/admin/departments/create' },
  { path: '/admin/system-settings', expectCreateRoute: '/admin/system-settings/create' },
  { path: '/admin/lookup-categories', expectCreateRoute: '/admin/lookup-categories/create' },
  { path: '/admin/lookup-values', expectCreateRoute: '/admin/lookup-values/create' },
  { path: '/admin/audit-logs' },
  { path: '/reference-data', expectCreateRoute: '/admin/lookup-categories/create' },
]

const crudModules = [
  {
    name: 'users',
    path: '/admin/users',
    createdKey: (suffix) => `ui-${suffix}@crm.local`,
    updatedKey: (suffix) => `ui-${suffix}@crm.local`,
    fillCreate: async (page, suffix) => {
      await fillInput(page, 'Email', `ui-${suffix}@crm.local`)
      await fillInput(page, 'Password', 'Temp@12345')
      await fillInput(page, 'First Name', 'Ui')
      await fillInput(page, 'Last Name', suffix)
      await setSwitch(page, 'Enabled', true)
    },
    fillEdit: async (page, suffix) => {
      await fillInput(page, 'Password', 'Temp@12345')
      await fillInput(page, 'First Name', 'UiUpdated')
      await fillInput(page, 'Last Name', `${suffix}-2`)
      await setSwitch(page, 'Enabled', false)
    },
  },
  {
    name: 'roles',
    path: '/admin/roles',
    createdKey: (suffix) => `UI Role ${suffix}`,
    updatedKey: (suffix) => `UI Role ${suffix} Updated`,
    fillCreate: async (page, suffix) => {
      await fillInput(page, 'Name', `UI Role ${suffix}`)
      await fillInput(page, 'Description', 'Created by UI acceptance')
    },
    fillEdit: async (page, suffix) => {
      await fillInput(page, 'Name', `UI Role ${suffix} Updated`)
      await fillInput(page, 'Description', 'Edited by UI acceptance')
    },
  },
  {
    name: 'permissions',
    path: '/admin/permissions',
    createdKey: (suffix) => `UiModule${suffix}`,
    updatedKey: (suffix) => `UiModule${suffix}Updated`,
    fillCreate: async (page, suffix) => {
      await fillInput(page, 'Module', `UiModule${suffix}`)
      await fillInput(page, 'Action', 'View')
    },
    fillEdit: async (page, suffix) => {
      await fillInput(page, 'Module', `UiModule${suffix}Updated`)
      await fillInput(page, 'Action', 'Edit')
    },
  },
  {
    name: 'teams',
    path: '/admin/teams',
    createdKey: (suffix) => `UI Team ${suffix}`,
    updatedKey: (suffix) => `UI Team ${suffix} Updated`,
    fillCreate: async (page, suffix) => {
      await fillInput(page, 'Name', `UI Team ${suffix}`)
      await fillInput(page, 'Description', 'Created by UI acceptance')
      await fillInput(page, 'Owner User Id', '')
      await setSwitch(page, 'Active', true)
    },
    fillEdit: async (page, suffix) => {
      await fillInput(page, 'Name', `UI Team ${suffix} Updated`)
      await fillInput(page, 'Description', 'Edited by UI acceptance')
      await setSwitch(page, 'Active', false)
    },
  },
  {
    name: 'departments',
    path: '/admin/departments',
    createdKey: (suffix) => `UI Dept ${suffix}`,
    updatedKey: (suffix) => `UI Dept ${suffix} Updated`,
    fillCreate: async (page, suffix) => {
      await fillInput(page, 'Name', `UI Dept ${suffix}`)
      await fillInput(page, 'Description', 'Created by UI acceptance')
      await fillInput(page, 'Parent Department Id', '')
      await setSwitch(page, 'Active', true)
    },
    fillEdit: async (page, suffix) => {
      await fillInput(page, 'Name', `UI Dept ${suffix} Updated`)
      await fillInput(page, 'Description', 'Edited by UI acceptance')
      await setSwitch(page, 'Active', false)
    },
  },
  {
    name: 'system-settings',
    path: '/admin/system-settings',
    createdKey: (suffix) => `ui_key_${suffix}`,
    updatedKey: (suffix) => `ui_key_${suffix}_updated`,
    fillCreate: async (page, suffix) => {
      await fillInput(page, 'Category', 'UI Acceptance')
      await fillInput(page, 'Key', `ui_key_${suffix}`)
      await fillInput(page, 'Value', 'value-1')
      await fillInput(page, 'Description', 'Created by UI acceptance')
    },
    fillEdit: async (page, suffix) => {
      await fillInput(page, 'Category', 'UI Acceptance')
      await fillInput(page, 'Key', `ui_key_${suffix}_updated`)
      await fillInput(page, 'Value', 'value-2')
      await fillInput(page, 'Description', 'Edited by UI acceptance')
    },
  },
  {
    name: 'lookup-categories',
    path: '/admin/lookup-categories',
    createdKey: (suffix) => `UI_CAT_${suffix}`,
    updatedKey: (suffix) => `UI_CAT_${suffix}_U`,
    fillCreate: async (page, suffix) => {
      await fillInput(page, 'Name', `UI Category ${suffix}`)
      await fillInput(page, 'Code', `UI_CAT_${suffix}`)
      await fillInput(page, 'Description', 'Created by UI acceptance')
      await setSwitch(page, 'Active', true)
    },
    fillEdit: async (page, suffix) => {
      await fillInput(page, 'Name', `UI Category ${suffix} Updated`)
      await fillInput(page, 'Code', `UI_CAT_${suffix}_U`)
      await fillInput(page, 'Description', 'Edited by UI acceptance')
      await setSwitch(page, 'Active', true)
    },
  },
  {
    name: 'lookup-values',
    path: '/admin/lookup-values',
    createdKey: (suffix) => `UI_VAL_${suffix}`,
    updatedKey: (suffix) => `UI_VAL_${suffix}_U`,
    fillCreate: async (page, suffix, context) => {
      await fillInput(page, 'Lookup Category Id', context.lookupCategoryId)
      await fillInput(page, 'Name', `UI Value ${suffix}`)
      await fillInput(page, 'Code', `UI_VAL_${suffix}`)
      await fillInput(page, 'Sort Order', '10')
      await setSwitch(page, 'Default', false)
      await setSwitch(page, 'Active', true)
    },
    fillEdit: async (page, suffix, context) => {
      await fillInput(page, 'Lookup Category Id', context.lookupCategoryId)
      await fillInput(page, 'Name', `UI Value ${suffix} Updated`)
      await fillInput(page, 'Code', `UI_VAL_${suffix}_U`)
      await fillInput(page, 'Sort Order', '20')
      await setSwitch(page, 'Default', true)
      await setSwitch(page, 'Active', true)
    },
  },
]

async function loginByApi() {
  const api = await request.newContext({ baseURL: baseUrl })
  const response = await api.post('/api/auth/login', { data: credentials })
  if (!response.ok()) {
    throw new Error(`Login failed: ${response.status()} ${response.statusText()}`)
  }

  const auth = await response.json()
  await api.dispose()
  return auth
}

async function getFirstLookupCategoryId(accessToken) {
  const api = await request.newContext({
    baseURL: baseUrl,
    extraHTTPHeaders: {
      Authorization: `Bearer ${accessToken}`,
    },
  })

  try {
    const response = await api.get('/api/lookup-categories?page=1&pageSize=1')
    if (!response.ok()) {
      throw new Error(`Lookup category fetch failed: ${response.status()} ${response.statusText()}`)
    }

    const data = await response.json()
    const first = data?.items?.[0]
    if (!first?.id) {
      throw new Error('No lookup category exists for lookup-values CRUD test.')
    }

    return String(first.id)
  } finally {
    await api.dispose()
  }
}

function createRouteProbe(page) {
  const state = {
    pageErrors: [],
    consoleErrors: [],
    requestFailures: [],
  }

  page.on('pageerror', (error) => {
    state.pageErrors.push(error.message)
  })

  page.on('console', (msg) => {
    if (msg.type() === 'error') {
      state.consoleErrors.push(msg.text())
    }
  })

  page.on('requestfailed', (request) => {
    const errorText = request.failure()?.errorText ?? 'unknown'
    if (errorText.includes('ERR_ABORTED')) {
      return
    }

    state.requestFailures.push(`${request.method()} ${request.url()} :: ${errorText}`)
  })

  return state
}

async function fillInput(page, label, value) {
  await page.getByLabel(label).first().fill(value)
}

async function setSwitch(page, label, expectedChecked) {
  const control = page.getByRole('switch', { name: label }).first()
  if (!await control.isVisible().catch(() => false)) {
    return
  }

  const current = (await control.getAttribute('aria-checked')) === 'true'
  if (current !== expectedChecked) {
    await control.click()
  }
}

async function selectFirstDropdownOption(page, label) {
  const combo = page.getByRole('combobox', { name: label }).first()
  if (!await combo.isVisible().catch(() => false)) {
    return
  }

  await combo.click()
  const option = page.getByRole('option').first()
  if (await option.isVisible().catch(() => false)) {
    await option.click()
  }
}

async function searchRows(page, text) {
  const search = page.getByPlaceholder('Search rows').first()
  await search.fill(text)
  await page.waitForLoadState('networkidle')
}

async function waitForRow(page, text) {
  const row = page.locator('tbody tr').filter({ hasText: text }).first()
  await row.waitFor({ state: 'visible', timeout: 10000 })
  return row
}

async function openRowAction(page, rowText, actionLabel) {
  const row = await waitForRow(page, rowText)
  const actionButton = row.locator('td:last-child button').first()
  await actionButton.click()
  await page.getByRole('menuitem', { name: actionLabel }).first().click()
}

async function ensureRowMissing(page, text) {
  for (let attempt = 0; attempt < 20; attempt += 1) {
    const count = await page.locator('tbody tr').filter({ hasText: text }).count()
    if (count === 0) {
      return true
    }

    await new Promise((resolve) => setTimeout(resolve, 200))
  }

  return false
}

async function waitForListRoute(page, listPath) {
  const expected = normalizePath(`${baseUrl}${listPath}`)
  for (let attempt = 0; attempt < 20; attempt += 1) {
    const current = normalizePath(page.url().split('?')[0])
    if (current === expected) {
      return true
    }

    await page.waitForTimeout(200)
  }

  return false
}

async function checkRoute(page, routeConfig) {
  const probe = createRouteProbe(page)
  const url = `${baseUrl}${routeConfig.path}`
  const started = Date.now()

  await page.goto(url, { waitUntil: 'networkidle' })

  const title = await page.title()
  const h1 = await page.locator('h1').first().textContent().catch(() => null)
  const hasUnauthorized = await page.getByText(/unauthorized|forbidden/i).first().isVisible().catch(() => false)
  let quickActionVerified = true

  if (routeConfig.expectCreateRoute) {
    const actionButton = page.getByRole('button', { name: /create /i }).first()
    if (await actionButton.isVisible().catch(() => false)) {
      await actionButton.click()
      await page.waitForLoadState('networkidle')
      quickActionVerified = page.url().includes(routeConfig.expectCreateRoute)
      await page.goto(`${baseUrl}${routeConfig.path}`, { waitUntil: 'networkidle' })
    } else {
      quickActionVerified = false
    }
  }

  const elapsedMs = Date.now() - started

  return {
    route: routeConfig.path,
    url: page.url(),
    title,
    header: h1?.trim() ?? '',
    quickAction: routeConfig.expectCreateRoute ?? null,
    quickActionVerified,
    hasUnauthorized,
    pageErrors: probe.pageErrors,
    consoleErrors: probe.consoleErrors,
    requestFailures: probe.requestFailures,
    elapsedMs,
  }
}

async function checkCrudModule(page, moduleConfig, context) {
  const started = Date.now()
  const suffix = Date.now().toString().slice(-6)
  const createdKey = moduleConfig.createdKey(suffix)
  const updatedKey = moduleConfig.updatedKey(suffix)
  const errors = []

  try {
    await page.goto(`${baseUrl}${moduleConfig.path}`, { waitUntil: 'networkidle' })

    const createButton = page.getByRole('button', { name: /create /i }).first()
    await createButton.click()
    await page.waitForLoadState('networkidle')
    if (!page.url().includes('/create')) {
      errors.push(`Did not navigate to create route for ${moduleConfig.name}`)
      return {
        module: moduleConfig.name,
        path: moduleConfig.path,
        elapsedMs: Date.now() - started,
        errors,
      }
    }

    await moduleConfig.fillCreate(page, suffix, context)
    await page.getByRole('button', { name: 'Save and Close' }).first().click()
    await page.waitForLoadState('networkidle')
    if (!await waitForListRoute(page, moduleConfig.path)) {
      errors.push(`Save and Close did not return to list route for ${moduleConfig.name}`)
    }

    await searchRows(page, createdKey)
    await waitForRow(page, createdKey)

    await openRowAction(page, createdKey, 'Edit')
    await page.waitForLoadState('networkidle')
    if (!page.url().includes('/edit')) {
      errors.push(`Did not navigate to edit route for ${moduleConfig.name}`)
    }

    await moduleConfig.fillEdit(page, suffix, context)
    await page.getByRole('button', { name: 'Save and Close' }).first().click()
    await page.waitForLoadState('networkidle')
    if (!await waitForListRoute(page, moduleConfig.path)) {
      errors.push(`Edit Save and Close did not return to list route for ${moduleConfig.name}`)
    }

    await searchRows(page, updatedKey)
    await waitForRow(page, updatedKey)

    await openRowAction(page, updatedKey, 'Edit')
    await page.waitForLoadState('networkidle')
    await page.getByRole('button', { name: 'Delete' }).first().click()
    await page.getByRole('button', { name: 'Delete' }).last().click()
    await page.waitForLoadState('networkidle')
    if (!await waitForListRoute(page, moduleConfig.path)) {
      errors.push(`Delete did not navigate back to list route for ${moduleConfig.name}`)
    }

    await searchRows(page, updatedKey)
    const removed = await ensureRowMissing(page, updatedKey)
    if (!removed) {
      errors.push(`Row still present after delete: ${updatedKey}`)
    }
  } catch (error) {
    errors.push(error instanceof Error ? error.message : String(error))
  }

  return {
    module: moduleConfig.name,
    path: moduleConfig.path,
    elapsedMs: Date.now() - started,
    errors,
  }
}

async function run() {
  const auth = await loginByApi()
  const lookupCategoryId = await getFirstLookupCategoryId(auth.accessToken)
  const testContext = { lookupCategoryId }

  const browser = await chromium.launch({ headless: true })
  const context = await browser.newContext()
  await context.addInitScript((authPayload) => {
    localStorage.setItem('crm.accessToken', authPayload.accessToken)
    localStorage.setItem('crm.refreshToken', authPayload.refreshToken)
    localStorage.setItem('crm.user', JSON.stringify(authPayload.user))
  }, auth)

  const page = await context.newPage()

  const results = []
  for (const routeConfig of routes) {
    // eslint-disable-next-line no-await-in-loop
    const result = await checkRoute(page, routeConfig)
    results.push(result)
  }

  const crudResults = []
  for (const moduleConfig of crudModules) {
    // eslint-disable-next-line no-await-in-loop
    const result = await checkCrudModule(page, moduleConfig, testContext)
    crudResults.push(result)
  }

  await browser.close()

  const failures = results.filter(
    (r) =>
      r.pageErrors.length > 0 ||
      r.consoleErrors.length > 0 ||
      r.requestFailures.length > 0 ||
        r.hasUnauthorized ||
        (r.quickAction !== null && !r.quickActionVerified),
  )

  for (const result of results) {
    const status = failures.includes(result) ? 'FAIL' : 'OK'
    console.log(`${status} ${result.route} (${result.elapsedMs}ms) -> ${result.url}`)
    if (result.header) {
      console.log(`  header: ${result.header}`)
    }
    if (result.consoleErrors.length > 0) {
      console.log(`  consoleErrors: ${result.consoleErrors.join(' | ')}`)
    }
    if (result.pageErrors.length > 0) {
      console.log(`  pageErrors: ${result.pageErrors.join(' | ')}`)
    }
    if (result.requestFailures.length > 0) {
      console.log(`  requestFailures: ${result.requestFailures.join(' | ')}`)
    }
    if (result.hasUnauthorized) {
      console.log('  unauthorized text detected')
    }
    if (result.quickAction && !result.quickActionVerified) {
      console.log(`  quick action failed: ${result.quickAction}`)
    }
  }

  if (failures.length > 0) {
    process.exitCode = 1
  }

  for (const result of crudResults) {
    const status = result.errors.length === 0 ? 'OK' : 'FAIL'
    console.log(`${status} CRUD ${result.module} (${result.elapsedMs}ms) -> ${result.path}`)
    if (result.errors.length > 0) {
      console.log(`  errors: ${result.errors.join(' | ')}`)
    }
  }

  if (crudResults.some((x) => x.errors.length > 0)) {
    process.exitCode = 1
  }
}

run().catch((error) => {
  console.error(error)
  process.exit(1)
})
