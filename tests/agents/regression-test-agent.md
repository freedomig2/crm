# Regression Test Agent

## Scope
Execute the complete suite after any targeted fix to ensure no cross-module regressions.

## Test Checklist
- Backend build and tests are green.
- Frontend lint, build, and tests are green.
- E2E critical-path tests are green.
- Auth/permission guardrails remain green.

## Commands To Run
- `pwsh ./tests/scripts/run-all-tests.ps1`

## Failure Detection Rules
- Any suite failure after a fix is a regression.

## Fix Instructions
- Trace change set that introduced regression.
- Apply smallest correction and rerun.

## Retest Instructions
- Run impacted suite.
- Run full regression again.

## Done Criteria
- Full pipeline passes in one clean run.
