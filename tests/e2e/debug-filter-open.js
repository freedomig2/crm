const { chromium } = require('../../frontend/node_modules/playwright')

const BASE_URL = process.env.BASE_URL || 'http://localhost:5253'

async function login(page) {
  await page.goto(`${BASE_URL}/login`)
  await page.fill('[data-testid="login-email"]', 'admin@crm.local')
  await page.fill('[data-testid="login-password"]', 'Admin@12345')
  await page.click('[data-testid="login-submit"]')
  await page.waitForURL(/\/dashboard/)
}

async function probe(page, route) {
  await page.goto(`${BASE_URL}${route}`)
  await page.waitForSelector('[data-testid="app-shell"]')
  await page.waitForTimeout(1000)

  const button = page.locator('main [data-testid="grid-filter-button"]').first()
  const visible = await button.isVisible().catch(() => false)
  const count = await page.locator('main [data-testid="grid-filter-button"]').count()

  await button.evaluate((element) => {
    window.__filterClicks = 0
    element.addEventListener('click', () => {
      window.__filterClicks += 1
    })
  }).catch(() => undefined)

  await button.click({ force: true }).catch(() => undefined)
  await page.waitForTimeout(800)

  const popoverVisible = await page.locator('[data-testid="grid-filter-popover"]').isVisible().catch(() => false)
  const drawerVisible = await page.locator('[data-testid="grid-filter-drawer"]').isVisible().catch(() => false)
  const clicks = await page.evaluate(() => window.__filterClicks ?? -1).catch(() => -1)

  console.log(JSON.stringify({ route, visible, count, clicks, popoverVisible, drawerVisible }))
}

async function run() {
  const browser = await chromium.launch({ headless: true })
  const context = await browser.newContext()
  const page = await context.newPage()

  page.on('console', (msg) => {
    console.log('[console]', msg.type(), msg.text())
  })
  page.on('pageerror', (err) => {
    console.log('[pageerror]', err.message)
  })

  await login(page)
  await probe(page, '/crm/accounts')
  await probe(page, '/leads')
  await probe(page, '/opportunities')

  await browser.close()
}

run().catch((err) => {
  console.error(err)
  process.exit(1)
})
