# Workflow Test Agent

## Scope
Validate end-to-end business process transitions:
- Lead to Opportunity
- Opportunity to Quote
- Quote to Order
- Order to Invoice
- Case lifecycle
- Document versioning and linkage
- Notification lifecycle

## Test Checklist
- Each workflow completes with correct state transitions.
- Linked records are created and related correctly.
- Audit-trail related endpoints return expected entries.

## Commands To Run
- `dotnet test tests/api/CRM.Api.Tests/CRM.Api.Tests.csproj --filter "Category=Workflow"`
- `npx playwright test --config tests/e2e/playwright.config.ts --grep @workflow`

## Failure Detection Rules
- Missing linked record after conversion.
- Invalid status transition accepted or valid transition rejected.

## Fix Instructions
- Fix business rule/service/controller layer first.
- Keep permission and audit behaviors intact.

## Retest Instructions
- Re-run failing workflow only.
- Re-run workflow suite and then regression.

## Done Criteria
- Covered workflow journeys pass fully.
