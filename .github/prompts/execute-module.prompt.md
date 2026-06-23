---
description: "Execute one CRM module end-to-end using repository architecture and hard quality gates. Input: module number and optional scope notes."
---

# Execute Module

Implement one remaining CRM module as a complete vertical slice.

## Inputs
- Module number (8-20)
- Optional scope clarifications

## Execution Rules
- Follow `.github/copilot-instructions.md` and all matching `.github/instructions/*.instructions.md`
- Reuse existing architecture and shared patterns
- Do not move to next module until this one passes completion checks

## Required Output
1. Implementation plan for the selected module
2. Files to be changed
3. Business rules implemented
4. Verification commands executed and results
5. Acceptance checklist with pass/fail per item
6. Remaining risks/blockers
7. Recommended next module
