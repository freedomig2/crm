#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$ROOT_DIR"

echo "=== Backend build ==="
dotnet build backend/backend.csproj

echo "=== Backend tests ==="
dotnet test tests/api/CRM.Api.Tests/CRM.Api.Tests.csproj --configuration Debug

echo "=== Frontend install ==="
npm install --prefix frontend

echo "=== Frontend lint ==="
npm --prefix frontend run lint

echo "=== Frontend unit tests ==="
npm --prefix frontend run test

echo "=== Frontend build ==="
npm --prefix frontend run build

echo "=== Playwright browser install ==="
pushd frontend >/dev/null
npx playwright install
popd >/dev/null

echo "=== Playwright e2e ==="
pushd frontend >/dev/null
npx playwright test --config ../tests/e2e/playwright.config.js
popd >/dev/null

echo "All test suites completed successfully."
