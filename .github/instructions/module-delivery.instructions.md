---
applyTo: "backend/**,frontend/**"
description: "Use when implementing remaining CRM modules. Enforce module-by-module vertical slice delivery and do not start next module until current module passes acceptance checks and quality gates."
---

# Module Delivery Workflow

Deliver remaining modules in strict order and complete one module end-to-end before moving to the next.

## Delivery Sequence
1. Module 8: Quote Management
2. Module 9: Order Management
3. Module 10: Invoice Module
4. Module 11: Case / Service Management
5. Module 12: Activity Management
6. Module 13: Document Management
7. Module 14: Workflow / Business Process Module
8. Module 15: Dashboard & Reporting Module
9. Module 16: Notification Module
10. Module 17: Security Module Extension
11. Module 18: Integration Module
12. Module 19: Configuration Module Extension
13. Module 20: AI / Copilot Module

## Vertical Slice Definition (per module)
Each module implementation must include:
- Entities and inheritance correctness
- DTOs (create/update/list/detail)
- Validators
- Services
- API endpoints and permissions
- EF Fluent API configurations
- Migrations
- Seed data and lookup values
- Frontend pages and routes
- Sidebar menu updates
- Relevant dashboard/report updates where required

## Completion Gate Per Module
Do not start the next module until current module has:
- Required build/lint gates passed
- Business rules implemented
- Menu and routing updated consistently
- Acceptance checklist reported with pass/fail status

## Frontend/UX Constraints
- No modal CRUD forms; use routed pages
- No inline filter bars on list pages; use popup filter controls
- Do not display raw GUIDs in UI
- Use existing shared list/page components and design system

## Data/Safety Constraints
- Soft delete only for business entities
- No cascade delete for CRM business records
- Number sequence usage where required by module scope
- Reuse existing framework infrastructure; do not recreate auth/permissions/lookups
