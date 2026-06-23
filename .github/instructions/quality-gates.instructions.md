---
applyTo: "backend/**,frontend/**"
description: "Use when making code changes and deciding completion status. Enforce hard quality gates so tasks are not marked complete until required checks run and results are reported."
---

# Hard Completion Gates

Do not consider coding tasks complete until required verification commands have been executed for touched areas and results are reported.

## Required Checks By Scope
- If frontend files changed: run `npm --prefix frontend run lint` and `npm --prefix frontend run build`.
- If backend files changed: run `dotnet build backend/backend.csproj`.
- If both changed: run all relevant commands above.

## Tests Policy
- If tests are configured for the touched area, run them before completion.
- If tests are not configured, explicitly state that no runnable tests are configured for that area.

## Failure Policy
- If a required check fails, do not mark the task complete.
- Either fix the issue and re-run checks, or report the blocker with exact failing command and key output.

## Reporting Policy
- Summarize verification status with pass/fail per command.
- Report non-blocking warnings separately from failures.
