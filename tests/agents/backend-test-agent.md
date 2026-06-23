# Backend Test Agent

## Scope
Validate API correctness, authorization boundaries, validation, soft-delete behavior, and error handling for backend controllers.

## Test Checklist
- Unauthorized requests return `401`.
- Authenticated without permission returns `403`.
- Authorized requests return success responses.
- Validation returns `400`.
- Missing entities return `404`.
- CRUD, list, search, filter, sort, and pagination are validated.
- Soft delete hides deleted records from normal list endpoints.
- Internal errors are not exposed in response payloads.

## Commands To Run
- `dotnet build backend/backend.csproj`
- `dotnet test tests/api/CRM.Api.Tests/CRM.Api.Tests.csproj --configuration Debug`

## Failure Detection Rules
- Any non-zero dotnet command exit code.
- Test output contains failed assertions.
- API startup failure in test host.

## Fix Instructions
- Fix API/DTO/validation/permission code in backend only.
- Keep existing architecture (auth, permissions, lookups, base entities).
- Apply minimum change and preserve soft-delete/audit patterns.

## Retest Instructions
- Re-run failing test class first.
- Re-run full backend suite.

## Done Criteria
- Backend build passes.
- All backend tests pass.
