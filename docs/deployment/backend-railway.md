# PeaceNest Backend Railway Deployment

This runbook covers the ASP.NET Core backend deployment path for Railway.

## Build And Runtime

Railway uses the root `Dockerfile` through `railway.json`.

The container:

- Builds `src/PeaceNest.Api/PeaceNest.Api.csproj` with the .NET 10 SDK image.
- Runs with the .NET 10 ASP.NET runtime image.
- Listens on Railway's `PORT` environment variable, with `8080` as the local fallback.
- Exposes `/health` as the lightweight health check route.
- Does not apply EF Core migrations on application startup.

## Required Railway Variables

Set these on the backend service in Railway:

```txt
ConnectionStrings__PeaceNest=<Supabase runtime/session-pooler connection string>
Authentication__Supabase__ProjectUrl=https://<project-ref>.supabase.co
Authentication__Supabase__Audience=authenticated
ASPNETCORE_ENVIRONMENT=Production
```

Optional join-policy overrides use `JoinCodes__LifetimeMinutes`, `JoinCodes__RequestLifetimeDays`, and `JoinCodes__MaxRequestsPerCode`. Defaults are 15 minutes, 7 days, and 10 distinct requests.

Family recovery defaults to a 30-day recovery window with hourly purge sweeps. `FamilyRecovery__LifetimeDays`, `FamilyRecovery__SweepIntervalMinutes`, `FamilyRecovery__ClaimLeaseMinutes`, `FamilyRecovery__BatchSize`, and `FamilyRecovery__WorkerEnabled` may be overridden. Apply the recovery migration before enabling the worker in a deployed release.

Do not set or commit Supabase service-role keys for this API. The frontend must send Supabase access tokens through:

```txt
Authorization: Bearer <supabase_access_token>
```


## Health Check

Railway is configured to call:

```txt
/health
```

The endpoint intentionally returns only public service health and no database or secret details.

## Migration Workflow

Production migrations are deliberate and manual. Do not add automatic startup migration code.

Recommended flow before deploying a backend version that includes migrations:

1. Review the generated EF Core migration.
2. Apply it from a trusted developer or release environment using the Supabase migration connection string.
3. Deploy the Docker image after the migration succeeds.
4. Verify `/health`.

Local command shape:

```powershell
dotnet ef database update --project src\PeaceNest.Api\PeaceNest.Api.csproj --startup-project src\PeaceNest.Api\PeaceNest.Api.csproj
```

For local EF execution, store the migration connection string in user secrets under:

```txt
ConnectionStrings:PeaceNestMigration
```

The app runtime should use `ConnectionStrings__PeaceNest`.

## Railway Notes

- Keep `railway.json` and `Dockerfile` at the repository root unless the Railway service root directory is changed.
- If `AllowedHosts` is tightened later, include `healthcheck.railway.app` so Railway health checks are accepted.
- Do not put migration commands in `preDeployCommand` until the team explicitly chooses that release workflow.
