---
applyTo: "backend/**,frontend/**"
description: "Use when implementing Modules 8-20 scope. Enforce entity fields, routes, permissions, lookup categories, business rules, and final acceptance requirements from the remaining CRM modules prompt."
---

# Modules 8-20 Scope Contract

Treat the remaining modules prompt as the functional contract for implementation.

## Required Scope Coverage
For each module, enforce:
- Entity list and base class inheritance
- Field definitions excluding inherited base fields
- Lookup categories and values
- Routes
- Permission set
- Business rules (calculations, conversions, status transitions)

## Number Sequence Rules
Use existing Number Sequence module for required entities:
- QUOTE, ORDER, INVOICE, CASE, DOCUMENT (and any others explicitly required)

## Cross-Module Rules
- Quote -> Order conversion required where defined
- Order -> Invoice generation required where defined
- AI provider must be abstracted behind interface (no hardcoded provider)
- Security extension must enforce field/record access server-side, not only frontend

## Mandatory UI/System Rules
- Route-based CRUD pages only
- Popup filter UX only
- Reuse existing lookup and permission frameworks
- Reuse existing notification framework if available
- Hide future modules unless implemented or enabled by feature flags

## Final Acceptance Alignment
Do not report completion until all global acceptance criteria from the remaining-modules prompt are satisfied, including:
- inheritance correctness
- no cascade deletes
- backend-driven lookups
- no mock data
- frontend and backend build success
