# ADR 0001: Application Version 7 GUIDs For Append-Heavy Records

## Status

Accepted

## Context

PeaceNest stores primary keys as PostgreSQL `uuid` values through EF Core Code First. Random UUIDs are simple, but append-heavy tables such as invitations, votes, comments, notifications, recaps, memories, and activity logs benefit from IDs that carry creation-time locality while remaining UUID-compatible.

The backend is the source of truth for business behavior and persistence writes. Core tables are not written directly by the frontend.

## Decision

PeaceNest generates UUID version 7 values in the .NET backend for append-heavy transactional, reflection, memory, notification, and log-style entities.

The database column type remains `uuid`. The database does not generate these IDs for the MVP. EF Core assigns a UUID v7 in the change-tracker convention when a marked entity is added with `Guid.Empty`.

Stable root identity and workspace records keep their existing explicit ID behavior:

- Users
- Families
- Family members
- Plan categories
- Family plans
- Type-detail records such as want/need details and milestone details

## Consequences

Append-heavy records get time-ordered UUIDs without introducing ULID string columns, provider-specific database functions, or a Supabase/Postgres extension dependency.

There is no database migration for this decision because the physical schema remains `uuid`.

Future append-heavy entities should opt into the same application-side convention instead of hand-calling `Guid.NewGuid()`.
