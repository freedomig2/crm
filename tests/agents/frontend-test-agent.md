# Frontend Test Agent

## Scope
Validate routed CRM UI, layout behaviors, list/grid UX, and component correctness.

## Test Checklist
- Sidebar collapsed by default.
- Menu groups collapsed by default and expandable.
- Navigation routes resolve to real pages.
- No visible item routes to blank/dead page.
- List pages use popup filter UX (not inline filter bars).
- Filter apply, clear, and cancel behaviors work.
- Search, sort, and pagination remain functional with filters.

## Commands To Run
- `npm --prefix frontend install`
- `npm --prefix frontend run lint`
- `npm --prefix frontend run build`
- `npm --prefix frontend run test`
- `npx playwright test --config tests/e2e/playwright.config.ts`

## Failure Detection Rules
- Any non-zero npm or Playwright command exit code.
- Browser-console errors on critical routes.

## Fix Instructions
- Fix smallest affected UI file.
- Preserve dense CRM layout and routed page patterns.
- Do not bypass permission checks via frontend-only hacks.

## Retest Instructions
- Re-run affected unit test or Playwright spec.
- Re-run full frontend test suite.

## Done Criteria
- Frontend lint/build pass.
- Frontend unit tests pass.
- E2E smoke tests pass.
