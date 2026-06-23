# Module CRUD Test Agent

## Scope
Validate route-based CRUD operations across implemented CRM entities.

## Test Checklist
- Create, read/details, update, and soft-delete flows succeed.
- Deleted records do not appear in default lists.
- Lookup fields use IDs in requests and labels in UI.
- Number sequence fields are generated and read-only in UI where required.

## Commands To Run
- `dotnet test tests/api/CRM.Api.Tests/CRM.Api.Tests.csproj --filter "Category=Crud|Category=Sequence|Category=Lookup"`
- `npx playwright test --config tests/e2e/playwright.config.ts --grep @crud|@lookup|@sequence`

## Failure Detection Rules
- Any CRUD lifecycle step fails.
- List/detail mismatch after update/delete.
- Lookup/sequence contract breaks.

## Fix Instructions
- Fix API contract or UI mapping with minimum surface change.
- Never expose raw GUIDs in grid/detail labels when lookup label exists.

## Retest Instructions
- Re-run failing entity suite.
- Re-run full CRUD suite.

## Done Criteria
- CRUD, lookup, and sequence tests pass for covered entities.
