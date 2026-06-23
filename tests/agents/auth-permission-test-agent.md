# Auth Permission Test Agent

## Scope
Validate authentication flows, role-based visibility, and backend-enforced permissions.

## Test Checklist
- Login success for admin test user.
- Unauthenticated API calls return `401`.
- Forbidden actions return `403` for under-privileged users.
- Role-specific menu visibility is enforced.
- Direct API calls cannot bypass hidden UI actions.

## Commands To Run
- `dotnet test tests/api/CRM.Api.Tests/CRM.Api.Tests.csproj --filter "Category=Auth|Category=Permission"`
- `npx playwright test --config tests/e2e/playwright.config.ts --grep @auth|@permission`

## Failure Detection Rules
- Wrong status code for auth/permission assertions.
- Privileged action succeeds without required permission.

## Fix Instructions
- Fix permission attributes/policies on backend first.
- Then align frontend visibility to backend permissions.

## Retest Instructions
- Re-run auth+permission subsets.
- Re-run full regression suite.

## Done Criteria
- All auth and permission tests pass with backend enforcement confirmed.
