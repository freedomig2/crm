# CRM Test Orchestrator

## Scope
Coordinate all automated CRM test agents, execute a bounded self-healing loop, and publish the final status report.

## Execution Order
1. Backend Test Agent
2. Frontend Test Agent
3. Auth Permission Test Agent
4. Module CRUD Test Agent
5. Workflow Test Agent
6. Reporting Test Agent
7. Integration Test Agent
8. Regression Test Agent

## Commands To Run
- `pwsh ./tests/scripts/run-orchestrator.ps1`
- Optional direct full run: `pwsh ./tests/scripts/run-all-tests.ps1`

## Loop Logic
```text
for attempt in 1..10
  run all agents in order
  if all pass
    generate final PASS report
    stop
  classify failures
  apply smallest safe fix
  rerun affected suites
  rerun full regression
end
if still failing
  generate FAIL or PASS WITH WARNINGS report
```

## Failure Detection Rules
- Any non-zero exit code is a failure.
- Any test result containing `failed`, `error`, or `timed out` is a failure.
- Build failures are blocking failures.
- Security and permission failures are always high severity.

## Fix Instructions
- Start with the smallest fix that addresses the failing assertion.
- Keep authorization strict; do not bypass permissions.
- Do not delete or disable tests to force green.
- Rebuild backend and frontend after each fix.

## Retest Instructions
- Rerun impacted suite first.
- If impacted suite passes, run full regression (`run-all-tests`).

## Done Criteria
- All required suites pass, or loop reaches 10 attempts with documented blockers.
- `tests/reports/final-test-report.md` is generated.
