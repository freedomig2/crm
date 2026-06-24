import fs from 'node:fs'
import path from 'node:path'

const repoRoot = process.cwd()
const navPath = path.join(repoRoot, 'frontend/src/layout/navigation.tsx')
const appPath = path.join(repoRoot, 'frontend/src/App.tsx')
const outputPath = path.join(repoRoot, 'tests/reports/route-inventory.md')

const navSource = fs.readFileSync(navPath, 'utf8')
const appSource = fs.readFileSync(appPath, 'utf8')

const routeEntries = []
const routeRegex = /<Route\s+path="([^"]+)"\s+element=\{<([^>]+)>\}\s*\/?>/g
for (const match of appSource.matchAll(routeRegex)) {
  routeEntries.push({
    path: match[1],
    pageComponent: match[2].replace(/\s+/g, ' ').trim(),
  })
}

const routeMap = new Map(routeEntries.map((entry) => [entry.path, entry.pageComponent]))

const items = []
let currentGroup = ''
let currentGroupEnabled = true

for (const line of navSource.split(/\r?\n/)) {
  const groupMatch = line.match(/^\s*key:\s*'([^']+)',\s*$/)
  if (groupMatch && !line.includes('key: \'my-')) {
    currentGroup = groupMatch[1]
  }

  if (line.includes("enabled: isGroupEnabled('")) {
    currentGroupEnabled = true
  }
  if (line.includes('enabled: false')) {
    currentGroupEnabled = false
  }

  const itemMatch = line.match(/\{\s*key:\s*'([^']+)'\s*,\s*label:\s*'([^']+)'\s*,\s*to:\s*'([^']+)'[\s\S]*permission:\s*'([^']+)'[\s\S]*enabled:\s*([^,}]+)/)
  if (itemMatch) {
    items.push({
      group: currentGroup,
      itemKey: itemMatch[1],
      label: itemMatch[2],
      menuRoute: itemMatch[3],
      permission: itemMatch[4],
      itemEnabledExpression: itemMatch[5].trim(),
      groupEnabled: currentGroupEnabled,
    })
    continue
  }

  const noPermissionItemMatch = line.match(/\{\s*key:\s*'([^']+)'\s*,\s*label:\s*'([^']+)'\s*,\s*to:\s*'([^']+)'[\s\S]*enabled:\s*([^,}]+)/)
  if (noPermissionItemMatch && line.includes('permission:') === false) {
    items.push({
      group: currentGroup,
      itemKey: noPermissionItemMatch[1],
      label: noPermissionItemMatch[2],
      menuRoute: noPermissionItemMatch[3],
      permission: '-',
      itemEnabledExpression: noPermissionItemMatch[4].trim(),
      groupEnabled: currentGroupEnabled,
    })
  }
}

const rows = items
  .map((item) => {
    const pageComponent = routeMap.get(item.menuRoute) ?? '-'
    const implemented = item.itemEnabledExpression === 'false' || !item.groupEnabled ? 'No' : 'Yes'
    return {
      group: item.group,
      item: item.label,
      menuRoute: item.menuRoute,
      reactRoute: routeMap.has(item.menuRoute) ? item.menuRoute : '-',
      pageComponent,
      permission: item.permission,
      implemented,
    }
  })
  .sort((a, b) => (a.group + a.item).localeCompare(b.group + b.item))

const header = [
  '# Frontend Route Inventory',
  '',
  '| Group | Menu Item | Menu Config Route | React Router Route | Page Component | Permission | Implemented |',
  '|---|---|---|---|---|---|---|',
]

const table = rows.map((row) => `| ${row.group} | ${row.item} | ${row.menuRoute} | ${row.reactRoute} | ${row.pageComponent} | ${row.permission} | ${row.implemented} |`)

fs.mkdirSync(path.dirname(outputPath), { recursive: true })
fs.writeFileSync(outputPath, [...header, ...table, ''].join('\n'))
console.log(`Route inventory written: ${outputPath}`)
