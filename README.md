# EWS

Enterprise Workflow System for position-based approval routing.

## Stack

- Backend: ASP.NET Core 8 Web API
- Application architecture: Domain / Application / Infrastructure / API
- Frontend: React 18 + Vite + Ant Design
- Database: SQL Server via Entity Framework Core

## Project Structure

```text
src/
  EWS.Domain/          Core entities and enums
  EWS.Application/     Use cases, commands, queries, validators
  EWS.Infrastructure/  EF Core, persistence, workflow services
  EWS.API/             HTTP API, middleware, Swagger
  EWS.Web/             Admin web app
docs/                  API and frontend notes
scripts/               Seed/import utilities
```

## Prerequisites

- .NET SDK 8
- Node.js 20.19+ or 22.12+ and npm 10+ for the Vite 8 frontend
- SQL Server

## Local Configuration

Copy the API config template and fill in your local database settings:

```powershell
Copy-Item src\EWS.API\appsettings.example.json src\EWS.API\appsettings.json
```

Important:

- `appsettings.json` is ignored by Git on purpose
- do not commit real connection strings or passwords

## Run the Project

### Option 1: Start both services with the helper script

```powershell
.\start-dev.bat
```

### Option 2: Start manually

Backend:

```powershell
dotnet run --project src\EWS.API\EWS.API.csproj
```

Frontend:

```powershell
cd src\EWS.Web
npm install
npm run dev
```

## Default Local URLs

- API: `http://localhost:5271`
- Frontend: `http://localhost:3000`
- Swagger: `http://localhost:5271/`

The frontend dev server proxies `/api` to the backend.

## Build

Backend solution:

```powershell
dotnet build EWS.sln
```

If MSBuild spawns too many `dotnet.exe` processes on Windows, run a single-node
build:

```powershell
dotnet build EWS.sln -m:1 -nr:false
```

If Windows reports `MSB3101: Could not write state file ... AssemblyReference.cache`,
repair the local .NET build cache and start again:

```powershell
.\repair-dotnet-cache.bat
.\start-dev.bat
```

## Test

Run all unit and integration tests:

```powershell
dotnet test EWS.sln
```

SQL Server integration tests create and drop a temporary database named
`EWS_IntegrationTests_{guid}`. By default, tests read the local API connection
string from `src\EWS.API\appsettings.json`. For CI or safer local runs, set a
dedicated SQL Server test connection:

```powershell
$env:EWS_TEST_CONNECTION_STRING="Server=localhost,1433;Database=master;User Id=sa;Password=YOUR_TEST_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
dotnet test EWS.sln
```

Run a quick API smoke test against a running local API:

```powershell
.\scripts\api-smoke.ps1
```

The smoke test verifies Swagger and read-only DB-backed endpoints. If your API
uses a custom URL, pass it explicitly:

```powershell
.\scripts\api-smoke.ps1 -BaseUrl http://127.0.0.1:5271
```

Frontend:

```powershell
cd src\EWS.Web
npm run build
```

## Key Documents

- [API Playbook](docs/EWS_API_Playbook.md)
- [Frontend Patterns](docs/EWS_Frontend_Patterns.md)

## Notes

- The repo currently excludes local runtime outputs such as `node_modules`, `bin`, `obj`, logs, and local app settings.
- Seed scripts are available in `scripts/` for setting up workflow-related data.
