# CRM Administration and Authentication Module (.NET 10)

This workspace contains a single deployable .NET 10 app with a React + TypeScript admin frontend.

When you run a backend build/publish, the frontend is built automatically and emitted to `backend/wwwroot`.

## Projects

- `backend/`: ASP.NET Core Web API (.NET 10, EF Core, Identity, JWT)
- `frontend/`: React + TypeScript app (Vite)
- `erp.slnx`: Solution file containing the backend project

## Prerequisites

- .NET SDK (10.x)
- Node.js (18+ recommended)

## Run In Development

The API seeds data on startup and applies migrations automatically.

Default admin credentials:

- Email: `admin@crm.local`
- Password: `Admin@12345`

### Backend

```powershell
dotnet run --project backend/backend.csproj
```

### Frontend

```powershell
cd frontend
npm run dev
```

## Included API Areas

- Authentication: register, login, logout, refresh token, forgot/reset/change password
- Users: CRUD, role assignment, team assignment, department assignment
- Roles and Permissions: role CRUD, permission listing, assign permissions
- Teams and Departments: CRUD with ownership/hierarchy support
- System Settings: listing and authorized updates with audit logging
- Audit Logs: filterable endpoint by user/entity/action/date
- Reference Data: lookup category/value CRUD

## Security

- Password hashing via ASP.NET Core Identity
- Lockout after 5 failed login attempts
- JWT access tokens include roles and permission claims
- Refresh tokens are hashed in database and rotated
- Role and permission-based authorization (for example `HasPermission("Users.Create")`)
- Global exception middleware and DTO validation

## Build

### Single .NET Build (includes frontend)

```powershell
dotnet build backend/backend.csproj
```

### Single .NET Publish (deployable output)

```powershell
dotnet publish backend/backend.csproj -c Release -o out
```

The publish output in `out/` is ready to deploy as one ASP.NET Core application.
