# Agent Operating Policy

This repository uses architecture-first coding rules and hard quality gates.

## Source of Truth
- `.github/copilot-instructions.md` is the canonical architecture baseline.
- `.github/instructions/*.instructions.md` provides scoped implementation rules.
- `.github/agents/implementation-guard.agent.md` is the preferred coding agent profile.

## Mandatory Completion Gates
- Frontend changes: run
  - `npm --prefix frontend run lint`
  - `npm --prefix frontend run build`
- Backend changes: run
  - `dotnet build backend/backend.csproj`
- Tests:
  - Run relevant tests when configured.
  - If no tests are configured for touched areas, explicitly report that.

## Remaining Modules Sequence
Implement remaining CRM modules in strict order and finish one module completely before the next:
1. Module 8: Quotes
2. Module 9: Orders
3. Module 10: Invoices
4. Module 11: Cases / Service
5. Module 12: Activities
6. Module 13: Documents
7. Module 14: Workflow / Business Process
8. Module 15: Dashboard & Reporting
9. Module 16: Notifications
10. Module 17: Security Extension
11. Module 18: Integrations
12. Module 19: Configuration Extension
13. Module 20: AI / Copilot

Per-module completion requires full vertical slice delivery (backend, data, permissions, lookups, frontend routes/pages) and passed quality gates.

## Safety Rules
- Do not run destructive commands unless explicitly requested.
- Preserve unrelated local edits.
- Avoid recreating existing infrastructure.

## Architecture Requirements
- Reuse existing auth/authorization, permissions, teams/departments, lookups, middleware, validation, and base entity hierarchy.
- Follow soft-delete, audit, and lookup architecture conventions.
- Keep existing CRM UI patterns (dense layout, route-based CRUD, Fluent UI v9, no modal-form architecture).
- Keep popup filter UX on list pages (no inline filter bars).
- Do not display raw GUIDs in frontend UI.
- Keep only implemented modules visible unless feature flags explicitly enable future modules.
