$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$root = Resolve-Path (Join-Path $PSScriptRoot '..\..')
Set-Location $root

function Invoke-Step {
    param(
        [string]$Name,
        [string]$Command
    )

    Write-Host "`n=== $Name ===" -ForegroundColor Cyan
    Write-Host $Command -ForegroundColor DarkGray
    Invoke-Expression $Command
}

Invoke-Step -Name 'Backend build' -Command 'dotnet build backend/backend.csproj'
Invoke-Step -Name 'Backend tests' -Command 'dotnet test tests/api/CRM.Api.Tests/CRM.Api.Tests.csproj --configuration Debug'
Invoke-Step -Name 'Frontend install' -Command 'Push-Location frontend; npm install; Pop-Location'
Invoke-Step -Name 'Frontend lint' -Command 'npm --prefix frontend run lint'
Invoke-Step -Name 'Frontend unit tests' -Command 'npm --prefix frontend run test'
Invoke-Step -Name 'Frontend build' -Command 'npm --prefix frontend run build'
Invoke-Step -Name 'Playwright browser install' -Command 'Push-Location frontend; npx playwright install; Pop-Location'
Invoke-Step -Name 'Playwright e2e' -Command 'Push-Location frontend; npx playwright test --config ../tests/e2e/playwright.config.js; Pop-Location'

Write-Host "`nAll test suites completed successfully." -ForegroundColor Green
