# Integration Test Agent

## Scope
Validate integrations import/export behavior, execution logs, and security boundaries around secrets.

## Test Checklist
- Import validation rejects invalid rows.
- Export returns a downloadable file.
- Integration run logs are created for success/failure.
- Connector errors are handled without exposing secrets.
- AI/provider abstractions remain interface-driven.

## Commands To Run
- `dotnet test tests/api/CRM.Api.Tests/CRM.Api.Tests.csproj --filter "Category=Integration|Category=AI"`
- `npx playwright test --config tests/e2e/playwright.config.ts --grep @integration|@ai`

## Failure Detection Rules
- Missing integration log records.
- Secret-bearing fields leaked in response payloads/logs.

## Fix Instructions
- Apply minimal service/controller masking and validation fixes.
- Keep provider abstraction interfaces intact.

## Retest Instructions
- Re-run failed integration/AI tests.
- Re-run full regression.

## Done Criteria
- Integration and AI scoped tests pass with no secret leaks.
