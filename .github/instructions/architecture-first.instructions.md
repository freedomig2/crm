---
applyTo: "backend/**,frontend/src/**"
description: "Use when implementing or refactoring backend/frontend code in this CRM project. Enforce architecture-first decisions and reuse of existing infrastructure instead of recreating auth, permissions, lookups, base entities, middleware, validation, and UI patterns."
---

# Architecture-First Rules

Always treat `.github/copilot-instructions.md` as the canonical baseline for this repository.

## Reuse, Do Not Recreate
- Reuse existing Authentication, Authorization, Roles, Permissions, Teams, Departments, Audit Logs, LookupCategory, LookupValue.
- Reuse existing base entity hierarchy, DbContext, middleware, permission attributes, global exception handling, and validation framework.
- Reuse established frontend CRM layout and route-based CRUD patterns.

## Backend Entity Constraints
- Follow mandatory inheritance rules from `.github/copilot-instructions.md`.
- Never duplicate inherited base/audit/ownership fields on derived entities.
- Use soft delete for BaseEntity descendants; never physically delete in normal delete endpoints.
- Do not manually set CreatedAt/CreatedById/UpdatedAt/UpdatedById in controllers/services.

## Lookup/Data Constraints
- Do not create duplicate lookup tables for reference data.
- Use LookupValue references for statuses, priorities, types, and other dropdown/reference values.

## Frontend UX Constraints
- Keep dense enterprise layout with compact breadcrumbs and headers.
- Keep title/actions on same row and Fluent UI v9 usage.
- Keep route-based forms/pages and avoid modal-form architecture changes.

## Implementation Behavior
- Before adding new code, inspect existing patterns in the same module.
- Prefer extending existing abstractions over introducing parallel infrastructure.
- If a requested change conflicts with baseline architecture, call it out explicitly and propose the compliant implementation path.
