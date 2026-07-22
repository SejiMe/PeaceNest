# Environment Variables

This file is the launch checklist for PeaceNest environments. Configure these values before running the app in development or production.

Do not commit real secrets. The repository intentionally ignores `.env`, `.env.*`, nested `.env` files, and `src/PeaceNest.Api/appsettings.Development.json`.

## Frontend

Store local frontend values in `src/PeaceNest.App/.env`. Start from:

```powershell
Copy-Item src/PeaceNest.App/.env.example src/PeaceNest.App/.env
```

Required for development and production:

| Variable | Required | Example | Notes |
| --- | --- | --- | --- |
| `EXPO_PUBLIC_API_BASE_URL` | Yes | `http://localhost:5000` | Use the deployed backend URL in production. Android emulators map local backend URLs automatically; physical devices and Expo tunnels need a reachable backend LAN, ngrok, or dev tunnel URL. Do not use the Expo `exp.direct` app URL for the backend API. |
| `EXPO_PUBLIC_SUPABASE_URL` | Yes | `https://<project-ref>.supabase.co` | Supabase Project URL. |
| `EXPO_PUBLIC_SUPABASE_PUBLISHABLE_KEY` | Yes | `sb_publishable_<key>` | Frontend-safe Supabase publishable key only. |
| `EXPO_PUBLIC_ENABLE_DEV_AUTH_TOKEN` | Dev only | `true` | Set to `false` or omit for production builds. |

Frontend variables prefixed with `EXPO_PUBLIC_` are bundled into the app. Never put backend secrets, service-role keys, database passwords, refresh tokens, or private family content in frontend variables.

## Backend Development

Prefer .NET user secrets for local backend development:

```powershell
dotnet user-secrets set "ConnectionStrings:PeaceNest" "<runtime-connection-string>" --project src/PeaceNest.Api/PeaceNest.Api.csproj
dotnet user-secrets set "ConnectionStrings:PeaceNestMigration" "<migration-connection-string>" --project src/PeaceNest.Api/PeaceNest.Api.csproj
dotnet user-secrets set "Authentication:Supabase:ProjectUrl" "https://<project-ref>.supabase.co" --project src/PeaceNest.Api/PeaceNest.Api.csproj
dotnet user-secrets set "Authentication:Supabase:Audience" "authenticated" --project src/PeaceNest.Api/PeaceNest.Api.csproj
```

Equivalent environment variable names:

| Variable | Required | Example | Notes |
| --- | --- | --- | --- |
| `ConnectionStrings__PeaceNest` | Recommended | `Host=...;Database=postgres;Username=...;Password=...;SSL Mode=Require;...` | Runtime connection string. If omitted outside production, the API falls back to local Postgres defaults. |
| `ConnectionStrings__PeaceNestMigration` | Required for migrations against Supabase | `Host=...;Database=postgres;Username=...;Password=...;SSL Mode=Require;...` | Used by EF design-time factory before the runtime string. |
| `Authentication__Supabase__ProjectUrl` | Yes | `https://<project-ref>.supabase.co` | Required for Supabase JWT validation. |
| `Authentication__Supabase__Audience` | Yes | `authenticated` | Defaults to `authenticated` in config, but set it explicitly per environment. |
| `Database__EnableSensitiveDataLogging` | Optional dev only | `true` | Do not enable in production. |
| `JoinCodes__LifetimeMinutes` | Optional | `15` | Backend-owned temporary code lifetime. |
| `JoinCodes__RequestLifetimeDays` | Optional | `7` | Backend-owned pending request review window. |
| `JoinCodes__MaxRequestsPerCode` | Optional | `10` | Maximum distinct requests created by one code. |
| `FamilyRecovery__LifetimeDays` | Optional | `30` | Sole-creator recovery window before permanent deletion. |
| `FamilyRecovery__SweepIntervalMinutes` | Optional | `60` | Delay between expired-workspace purge sweeps. |
| `FamilyRecovery__ClaimLeaseMinutes` | Optional | `10` | Time before a failed purge claim may be retried. |
| `FamilyRecovery__BatchSize` | Optional | `20` | Maximum expired workspaces claimed per sweep. |
| `FamilyRecovery__WorkerEnabled` | Optional | `true` | Enables the backend purge worker. Keep enabled in production after applying the migration. |
| `ASPNETCORE_ENVIRONMENT` | Optional dev | `Development` | Controls dev-only behavior such as Scalar availability. |

For local Docker runs, copy `src/PeaceNest.Api/.env.example` to `src/PeaceNest.Api/.env` and fill the same double-underscore variables.

```powershell
Copy-Item src/PeaceNest.Api/.env.example src/PeaceNest.Api/.env
```

When using Docker `--env-file`, prefer unquoted `KEY=value` lines if your tooling preserves quote characters.

## Backend Production

Required for Railway or any production backend host:

| Variable | Required | Example | Notes |
| --- | --- | --- | --- |
| `ConnectionStrings__PeaceNest` | Yes | Supabase session pooler connection string | Required at startup in production. |
| `Authentication__Supabase__ProjectUrl` | Yes | `https://<project-ref>.supabase.co` | Must match the project issuing frontend access tokens. |
| `Authentication__Supabase__Audience` | Yes | `authenticated` | Must match Supabase JWT audience. |
| `ASPNETCORE_ENVIRONMENT` | Yes | `Production` | Enables production behavior. |
| `PORT` | Host-provided | `8080` | Railway provides this. Dockerfile falls back to `8080`. |

The family recovery worker is enabled by default. Apply the corresponding database migration before deploying code that starts the worker. Disable it temporarily with `FamilyRecovery__WorkerEnabled=false` only during a coordinated migration rollout.

Production must not use automatic startup migrations. Apply migrations deliberately before deployment:

```powershell
npm run api:migrate
```

Use a trusted machine or release environment with `ConnectionStrings__PeaceNestMigration` configured.

## Supabase Values

Common Supabase dashboard locations:

- Project URL and publishable key: Project Settings, API.
- Database connection strings: Project database connection settings.
- Google OAuth configuration: Authentication, Providers, Google.

Use:

- Publishable key in frontend only.
- Runtime/session-pooler connection string for the deployed backend.
- Direct or migration-safe connection string only for deliberate EF migration commands.

Do not use:

- Supabase service-role key in frontend code.
- Database passwords in frontend variables.
- Supabase refresh tokens in backend storage or logs.
- Private family notes, comments, or recap content in logs or notification previews.

## Minimum Launch Checklist

Development:

- [ ] `src/PeaceNest.App/.env` exists and has frontend Supabase/API values.
- [ ] Backend user secrets or env vars include Supabase Auth settings.
- [ ] Backend has a valid runtime database connection.
- [ ] Migration connection is configured before running `npm run api:migrate`.
- [ ] Google OAuth callback/deep link settings are configured in Supabase.

Production:

- [ ] Backend host has `ASPNETCORE_ENVIRONMENT=Production`.
- [ ] Backend host has `ConnectionStrings__PeaceNest`.
- [ ] Backend host has Supabase Auth project URL and audience.
- [ ] Migrations have been applied manually before deploy.
- [ ] Frontend production build has `EXPO_PUBLIC_ENABLE_DEV_AUTH_TOKEN=false`.
- [ ] Frontend points at the production backend URL.
- [ ] No service-role key, database password, or real token is present in frontend env.
