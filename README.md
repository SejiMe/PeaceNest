# PeaceNest

PeaceNest is a calm family planning app for shared Wants & Needs, Family Milestones, plan notes, and family alignment.

## Stack

- Frontend: Expo, React Native, TypeScript, Expo Router, NativeWind, TanStack Query, Supabase Auth
- Backend: ASP.NET Core Web API, FastEndpoints, EF Core, Supabase Postgres/Auth
- Deployment target: Dockerized backend on Railway

## Prerequisites

- Node.js LTS and npm
- .NET SDK used by the repo target framework
- Docker Desktop, only for container testing or production-like backend runs
- Supabase project with Google OAuth enabled
- Supabase Postgres database with the committed EF migrations applied

## Install

From the repository root:

```powershell
npm install
dotnet restore PeaceNest.slnx
```

## Environment Setup

Before launching either app, configure the required environment values:

- Frontend: copy `.env.example` to `.env`
- Backend local development: use .NET user secrets, shell environment variables, or `src/PeaceNest.Api/.env`
- Backend Docker/Railway: use environment variables

See [Environment Variables](docs/configuration/environment-variables.md) for the complete dev and production checklist.

## Root Scripts

These scripts can be run from the repository root with `npm run <script>`.

| Script | Purpose |
| --- | --- |
| `start` | Start Expo dev server. |
| `android` | Start Expo and open Android target. |
| `web` | Start Expo for web development. |
| `typecheck` | Run TypeScript type checking. |
| `api` | Run the ASP.NET Core backend locally. |
| `api:build` | Build the backend solution. |
| `api:test` | Run backend unit and integration tests. |
| `api:migrate` | Apply EF Core migrations deliberately. Do not use as production startup behavior. |
| `docker:build` | Build the backend Docker image as `peacenest-api:local`. |
| `docker:run` | Run the backend Docker image with `src/PeaceNest.Api/.env`. |
| `web:export` | Export the Expo web bundle to `artifacts/expo-web-check`. |

## Development Mode

### 1. Configure Supabase

In Supabase:

- Enable Google OAuth as the only supported auth provider.
- Copy the project URL and publishable key for the frontend.
- Copy the database connection string for backend runtime.
- Copy a direct or migration-safe database connection string for EF migrations.

### 2. Configure Frontend Environment

```powershell
Copy-Item .env.example .env
```

Fill in:

```txt
EXPO_PUBLIC_API_BASE_URL=http://localhost:5000
EXPO_PUBLIC_SUPABASE_URL=https://<project-ref>.supabase.co
EXPO_PUBLIC_SUPABASE_PUBLISHABLE_KEY=sb_publishable_<key>
EXPO_PUBLIC_ENABLE_DEV_AUTH_TOKEN=true
```

### 3. Configure Backend User Secrets

From the repository root:

```powershell
dotnet user-secrets set "ConnectionStrings:PeaceNest" "<runtime-connection-string>" --project src/PeaceNest.Api/PeaceNest.Api.csproj
dotnet user-secrets set "ConnectionStrings:PeaceNestMigration" "<migration-connection-string>" --project src/PeaceNest.Api/PeaceNest.Api.csproj
dotnet user-secrets set "Authentication:Supabase:ProjectUrl" "https://<project-ref>.supabase.co" --project src/PeaceNest.Api/PeaceNest.Api.csproj
dotnet user-secrets set "Authentication:Supabase:Audience" "authenticated" --project src/PeaceNest.Api/PeaceNest.Api.csproj
```

### 4. Apply Database Migrations

Apply migrations deliberately:

```powershell
npm run api:migrate
```

Do not add automatic production startup migrations.

### 5. Run Backend

```powershell
npm run api
```

Backend health check:

```powershell
Invoke-RestMethod http://localhost:5000/health
```

API docs are available in development at:

```txt
http://localhost:5000/scalar
```

### 6. Run Frontend

In another terminal:

```powershell
npm run start
```

Useful targets:

```powershell
npm run web
npm run android
```

The dev-only auth token page is:

```txt
/dev/auth-token
```

It must stay disabled/fail-closed in production.

## Production Mode

### Backend Docker Image

Build locally from the repository root:

```powershell
npm run docker:build
```

Run locally with production-style variables in `src/PeaceNest.Api/.env`:

```powershell
npm run docker:run
```

Local container health check:

```powershell
Invoke-RestMethod http://localhost:8080/health
```

### Railway Backend

Railway uses:

- `Dockerfile`
- `railway.json`
- `/health` health check

Required Railway variables:

```txt
ConnectionStrings__PeaceNest=<Supabase runtime/session-pooler connection string>
Authentication__Supabase__ProjectUrl=https://<project-ref>.supabase.co
Authentication__Supabase__Audience=authenticated
ASPNETCORE_ENVIRONMENT=Production
```

`PORT` is supplied by Railway.

See [Backend Railway Deployment](docs/deployment/backend-railway.md).

### Frontend Production

The current repo supports Expo web export verification:

```powershell
npm run web:export
```

For deployed web or mobile builds, configure production frontend values before building:

```txt
EXPO_PUBLIC_API_BASE_URL=https://<backend-host>
EXPO_PUBLIC_SUPABASE_URL=https://<project-ref>.supabase.co
EXPO_PUBLIC_SUPABASE_PUBLISHABLE_KEY=sb_publishable_<key>
EXPO_PUBLIC_ENABLE_DEV_AUTH_TOKEN=false
```

Mobile app-store/EAS build configuration is not yet committed.

## Verification

Run frontend type checking:

```powershell
npm run typecheck
```

Run backend tests:

```powershell
npm run api:test
```

Run Expo web export verification:

```powershell
npm run web:export
```

## Security Notes

- Never commit real `.env` files, database passwords, Supabase access tokens, refresh tokens, or service-role keys.
- The frontend uses only Supabase publishable keys.
- The backend validates Supabase access tokens and owns authorization.
- Core tables are not written to directly by the frontend.
- Production migrations are deliberate and manual.
