# Reporting Test Agent

## Scope
Validate dashboard/report data retrieval and permission-aware access.

## Test Checklist
- Dashboard endpoints return real data payloads.
- Reporting endpoints return expected structures.
- Unauthorized/forbidden access is blocked.
- Export endpoints return downloadable payloads when allowed.

## Commands To Run
- `dotnet test tests/api/CRM.Api.Tests/CRM.Api.Tests.csproj --filter "Category=Reporting|Category=Dashboard"`
- `npx playwright test --config tests/e2e/playwright.config.ts --grep @reporting`

## Failure Detection Rules
- Empty/invalid report payload where data should exist.
- Report endpoint ignores permission restrictions.

## Fix Instructions
- Fix reporting query/service/controller with minimal edits.
- Preserve existing lookup/permission framework usage.

## Retest Instructions
- Re-run report-specific tests.
- Re-run full regression.

## Done Criteria
- Reporting and dashboard tests pass.
