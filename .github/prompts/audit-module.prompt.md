---
description: "Audit a completed module against architecture rules, UX constraints, permissions, and hard acceptance criteria. Input: module number."
---

# Audit Module

Validate a completed module against repository and remaining-modules acceptance rules.

## Audit Checklist
- Correct entity inheritance and no duplicated audit fields
- Required routes exist and use routed pages (no modal CRUD)
- List pages use popup filter pattern (no inline filters)
- No raw GUIDs displayed in frontend
- Number sequence integration where required
- Permissions seeded and enforced
- Lookup categories/values present and used
- Soft-delete behavior and no cascade business deletes
- Backend/frontend calls use real APIs (no mock data)
- Quality gate commands passed

## Required Output
- Findings ordered by severity
- File references for each finding
- Gate status summary
- Clear pass/fail decision for module readiness
