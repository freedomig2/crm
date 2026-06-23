# CRM Test Framework

This folder contains the automated CRM testing and self-healing orchestration framework.

## Structure
- `agents/`: agent playbooks, including `CRM Test Orchestrator`.
- `api/`: backend API and authorization test project.
- `e2e/`: Playwright end-to-end tests.
- `fixtures/`: deterministic users/modules fixtures.
- `reports/`: generated final reports.
- `scripts/`: full-run and orchestrator scripts.

## Main Commands
- Full run: `pwsh ./tests/scripts/run-all-tests.ps1`
- Self-healing orchestrator: `pwsh ./tests/scripts/run-orchestrator.ps1`
- Frontend aggregate command: `npm --prefix frontend run test:all`

## Notes
- The orchestrator retries up to 10 times.
- Automated healing is conservative (`dotnet format`, `eslint --fix`, dependency refresh).
- Do not disable failing tests to force green.
