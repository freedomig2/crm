$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$root = Resolve-Path (Join-Path $PSScriptRoot '..\..')
Set-Location $root

$maxAttempts = 10
$attempt = 1
$allPassed = $false
$failures = @()
$fixesApplied = @()

function Invoke-SafeCommand {
    param(
        [string]$Name,
        [string]$Command
    )

    Write-Host "`n[$Name] $Command" -ForegroundColor Cyan
    try {
        Invoke-Expression $Command
        return $true
    }
    catch {
        Write-Warning "$Name failed: $($_.Exception.Message)"
        return $false
    }
}

while ($attempt -le $maxAttempts -and -not $allPassed) {
    Write-Host "`n===== CRM Test Orchestrator Attempt $attempt/$maxAttempts =====" -ForegroundColor Yellow

    $runPassed = Invoke-SafeCommand -Name 'Run all tests' -Command 'pwsh ./tests/scripts/run-all-tests.ps1'

    if ($runPassed) {
        $allPassed = $true
        break
    }

    $failures += "Attempt $attempt failed"

    # Self-healing actions with minimal risk.
    if (Invoke-SafeCommand -Name 'dotnet format' -Command 'dotnet format backend/backend.csproj') {
        $fixesApplied += "Attempt $attempt: dotnet format applied"
    }

    if (Invoke-SafeCommand -Name 'eslint autofix' -Command 'npm --prefix frontend run lint -- --fix') {
        $fixesApplied += "Attempt $attempt: eslint --fix applied"
    }

    if (Invoke-SafeCommand -Name 'frontend reinstall' -Command 'Push-Location frontend; npm install; Pop-Location') {
        $fixesApplied += "Attempt $attempt: frontend dependencies refreshed"
    }

    $attempt += 1
}

$reportPath = Join-Path $root 'tests/reports/final-test-report.md'
$timestamp = (Get-Date).ToString('yyyy-MM-dd HH:mm:ss K')
$status = if ($allPassed) { 'PASS' } elseif ($failures.Count -gt 0 -and $attempt -gt $maxAttempts) { 'FAIL' } else { 'PASS WITH WARNINGS' }

$modules = Get-Content -Raw 'tests/fixtures/modules.json' | ConvertFrom-Json
$moduleList = ($modules.modules -join ', ')

$passedSummary = if ($allPassed) {
    'Backend build/tests, frontend lint/unit/build, Playwright e2e'
}
else {
    'Partial, see failed tests section'
}

$failedSummary = if ($failures.Count -eq 0) { 'None' } else { ($failures -join '; ') }
$fixesSummary = if ($fixesApplied.Count -eq 0) { 'No automated fixes applied.' } else { ($fixesApplied -join '; ') }

@"
# CRM Final Test Report

- Test run date: $timestamp
- Modules tested: $moduleList
- Passed tests: $passedSummary
- Failed tests: $failedSummary
- Fixes applied: $fixesSummary
- Remaining known issues: $(if ($allPassed) { 'None detected in executed suites.' } else { 'Review latest orchestrator failure output.' })
- Security/permission results: $(if ($allPassed) { 'Auth and permission guard tests passed in executed suites.' } else { 'Not fully green; permission failures may remain.' })
- Final status: $status
"@ | Set-Content -Path $reportPath -Encoding UTF8

if (-not $allPassed) {
    throw "CRM Test Orchestrator did not reach green status within $maxAttempts attempts."
}

Write-Host "`nCRM Test Orchestrator completed with status: $status" -ForegroundColor Green
Write-Host "Final report: tests/reports/final-test-report.md" -ForegroundColor Green
