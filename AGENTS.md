# AGENTS.md

## Purpose

This file is the operating contract for AI coding agents working on **PeaceNest**.

PeaceNest is a peaceful family planning space for families who love, plan, and grow together. It is not a strict budgeting app. It is a calm family nest for shared planning, visible priorities, family milestones, memories, and gentle alignment.

Agents must implement the project exactly within the product and technical direction defined here. Do not silently change the stack, architecture, scope, authentication model, deployment target, or MVP boundaries.

---

## Highest Priority Instructions

1. Build PeaceNest as a **family peace and planning system**, not as a generic budget tracker.
2. MVP comes first. Validate the shared family planning workflow before adding advanced infrastructure.
3. Keep business truth and authorization in the .NET backend.
4. Use **Expo + React Native + TypeScript** for frontend.
5. Use **ASP.NET Core Web API + FastEndpoints + Vertical Slice Architecture** for backend.
6. Use **Supabase Auth with Google OAuth only** for identity.
7. Use **Supabase Postgres + EF Core Code First** for persistence.
8. Deploy backend with **Docker to Railway**.
9. Use **xUnit** for backend tests.
10. Avoid feature creep. Do not implement parked features unless explicitly requested.
11. Use the required agent skills in this file. The `grill-me`, `grill-with-docs`, and `planning-and-task-breakdown` skills are non-negotiable.

When instructions conflict, follow this priority order:

1. Security and privacy rules in this file
2. MVP scope rules in this file
3. Architecture rules in this file
4. Existing repository conventions
5. Project reference materials in `.agents/references/`
6. Framework official documentation
7. Agent preference or convenience

---

## Required Agent Skill Usage

Skills are part of the PeaceNest operating contract. Agents must use available skills deliberately, not treat them as optional background advice.

Before starting work:

- Identify which available skills match the request.
- Read each relevant `SKILL.md` before applying it.
- Follow the skill workflow unless it conflicts with the higher-priority security, MVP, architecture, or stack rules in this file.
- If a required skill is unavailable, say so clearly and continue with the closest safe workflow.

### Non-Negotiable Skills

Use `planning-and-task-breakdown` before implementing any non-trivial work, including:

- Backend feature slices
- Frontend screens or flows
- Database schema or migration work
- Authentication, authorization, privacy, or family-scoping work
- Architecture, deployment, or dependency changes
- Work that spans more than one file
- Work that needs sequencing, estimation, or parallelization

The planning output must include ordered tasks, acceptance criteria, verification steps, dependencies, and checkpoints. For tiny single-file changes, briefly note why a full task breakdown is unnecessary before proceeding.

Use `grill-with-docs` before committing to product, domain, terminology, or architecture decisions when the decision should be checked against existing project language or documentation.

Required `grill-with-docs` behavior:

- Explore the codebase and existing docs before asking questions that can be answered locally.
- Challenge unclear or conflicting terminology against `CONTEXT.md`, `CONTEXT-MAP.md`, and ADRs when they exist.
- Ask one question at a time when user input is needed.
- Provide a recommended answer with each question.
- Update `CONTEXT.md` inline when domain terms are resolved.
- Create ADRs only for decisions that are hard to reverse, surprising without context, and the result of a real trade-off.

Use `grill-me` whenever the user asks to be grilled, asks to stress-test a plan or design, or when a plan has unresolved decisions that cannot be answered from the codebase or docs.

Required `grill-me` behavior:

- Interview the user one question at a time.
- Walk the decision tree until dependencies between decisions are resolved.
- Provide the recommended answer for each question.
- Explore the codebase instead of asking whenever local context can answer the question.

When both grill skills could apply, prefer `grill-with-docs` if project documentation or established domain language exists. Use `grill-me` for general plan stress-testing or when no useful docs exist yet.

### Skill Workflow Gate

For meaningful implementation work, follow this gate:

1. Read relevant skills.
2. Explore relevant code and documentation.
3. Use the grill skills to resolve unclear product, domain, or architecture decisions.
4. Produce a task breakdown with acceptance criteria and verification.
5. Implement the smallest approved vertical slice.
6. Verify with the repository's validation commands.
7. Report what changed, what was verified, and what remains.

### Other Skills

Use other available skills whenever their trigger conditions match the work. This includes, but is not limited to:

- Supabase skills for Supabase Auth, Supabase Postgres, RLS, JWTs, schema work, and Postgres best practices.
- GitHub skills for pull requests, review comments, publishing changes, and CI failures.
- Browser skills for local frontend verification and screenshots.
- Documents, presentations, and spreadsheets skills for those artifact types.
- Skill creation or installation skills when the user asks to create, update, find, or install skills.

Do not bypass a relevant skill because a task looks familiar. PeaceNest depends on consistent workflows more than agent memory.

---

## Project Reference Materials

The `.agents/references/` folder contains required project source material. Agents must consult these references before making decisions in the matching area.

References are not a dumping ground. Treat them as project memory that supports this operating contract.

Use these files as follows:

- `.agents/references/project_peacenest_context.md`: Product context, roadmap, MVP scope, frontend direction, backend direction, security posture, and module expectations.
- `.agents/references/peacenest_design_guide.md`: Warm Nest UI, color tokens, typography, layout, components, screen direction, copy tone, accessibility notes, and frontend implementation guidance.
- `.agents/references/peacenest_color_palette_design_guide.png`: Visual confirmation of the PeaceNest palette. Use when implementing or reviewing brand colors, UI mockups, design tokens, or visual polish.
- `.agents/references/peacenest_erd.mmd`: Detailed database model, entities, fields, and relationships. Use before creating or changing EF Core entities, migrations, database constraints, family-scoped queries, or persistence-related API behavior.
- `.agents/references/summarized_database_design.mmd`: High-level database relationship map. Use for quick orientation before diving into the detailed ERD.

Reference usage rules:

- Read the relevant reference before implementing the related feature or changing project direction.
- Mention which reference files were consulted in plans, task breakdowns, reviews, and final implementation summaries.
- If a reference conflicts with this `AGENTS.md`, follow `AGENTS.md` and call out the conflict.
- If references conflict with each other, prefer the more specific reference for that area and call out the conflict.
- If a reference appears stale compared with implemented code, inspect the code, state the mismatch, and use `grill-me` or `grill-with-docs` to resolve the decision before changing behavior.
- Do not copy reference content blindly. Apply it through the MVP, security, architecture, privacy, and stack constraints in this file.
- Do not store secrets, credentials, tokens, `.env` values, or private family content in reference files.

### Discussing References With `grill-me`

Use `grill-me` when discussing, interpreting, or challenging what is referenced in `.agents/references/`.

Required behavior:

- Start by identifying which reference file or diagram is being discussed.
- Summarize the relevant referenced guidance in plain language.
- Ask one question at a time only when the decision cannot be answered from the references, existing docs, or code.
- Provide the recommended answer with each question.
- Walk the decision tree until the reference interpretation is clear enough to implement.
- Record any resolved domain terminology in `CONTEXT.md` when appropriate.
- Consider an ADR only when the resolved decision is hard to reverse, surprising without context, and based on a real trade-off.

For frontend design discussions, grill against `peacenest_design_guide.md` and the palette image.

For database or backend persistence discussions, grill against `summarized_database_design.mmd` first, then `peacenest_erd.mmd`.

For product scope and roadmap discussions, grill against `project_peacenest_context.md` and the MVP scope rules in this file.

---

## Product Identity

PeaceNest helps families answer:

- What do we need as a family?
- What do we want someday?
- What should we prioritize first?
- What have we already achieved?
- What milestones are we working toward?
- How can we plan together with more peace?

The heart of the product is **family alignment**:

- Make plans visible.
- Help everyone feel heard.
- Reduce stressful planning conversations.
- Turn family goals into warm, meaningful progress.
- Preserve completed goals and memories.

Every product and code decision must support this emotional direction:

- Calm
- Warm
- Family-friendly
- Soft
- Trustworthy
- Peaceful
- Light and simple

Do not make PeaceNest feel like banking software, enterprise project management, or a gamified chore machine.

---

## Core Planning Domains

### Wants & Needs

Use this domain for plans related to buying, saving, spending, or prioritizing resources.

Examples:

- New refrigerator
- Tuition
- Emergency fund
- House repair
- Family vacation
- School supplies
- Medical needs

Supported ranking factors:

- Urgency
- Cost
- Importance
- Emotional value
- Family votes

### Family Milestones

Use this domain for meaningful family plans that are not only about buying things.

Examples:

- Sunday family dinner habit
- Visit grandparents monthly
- Child graduation
- Family reunion
- Health improvement goal
- Less screen time
- Spiritual or personal growth goals
- Yearly family reflection

This domain keeps PeaceNest broader than a finance app.

---

## Roadmap Discipline

### Phase 1: Foundation

Build first:

- Family workspace
- Invite family members
- Add Wants & Needs
- Add Family Milestones
- Categorize plans
- Rank priorities
- Track basic progress
- Add comments or notes

### Phase 2: Engagement & Fun

Build after the foundation is stable:

- Family Quests
- Progress bars
- Reactions
- Peace Points
- Milestone celebrations
- Gentle reminders
- Inactivity nudges

### Phase 3: Recaps & Reflection

Build after meaningful user activity exists:

- Monthly recaps
- Quarterly recaps
- Yearly recaps
- Achievement history
- Family timeline
- Completed goals archive

### Phase 4: Memory Layer

Build after core planning and recaps are useful:

- Attach photos to goals or milestones
- Optional image upload
- Future Google Photos integration
- Future OneDrive integration
- Memory timeline

### Phase 5: Smart Planning

Build only after validating the main workflow:

- Priority suggestions
- Goal risk alerts
- Timeline warnings
- Tradeoff insights
- AI family planning assistant

---

## MVP Scope

The MVP validates whether families want a shared planning space for peace and shared direction.

### MVP Must Include

- Family workspace
- Family member invitation
- Wants & Needs board
- Family Milestones board
- Need vs Want tagging
- Priority ranking
- Goal status tracking
- Milestone checklist
- Comments or notes
- Simple notifications
- Monthly recap

### MVP Must Avoid

Do not build these in the MVP:

- Bank integrations
- Complex AI advisor
- Heavy analytics
- Complex media storage
- Google Photos integration
- OneDrive integration
- Overbuilt gamification
- Bank or wallet integrations
- Exportable yearly memory book
- Distributed cache requirement
- Advanced SignalR collaboration

### MVP Success Signals

Favor features and instrumentation that can prove:

- Families add multiple plans.
- Families return weekly or monthly.
- Members vote or comment on priorities.
- Users complete milestones.
- Users read or share recaps.
- The app reduces confusion in family planning.

---

## Technical Direction

PeaceNest is mobile-first, Android-first, and web-second.

System shape:

- Frontend: Expo, React Native, TypeScript
- Backend: .NET latest LTS, ASP.NET Core Web API, FastEndpoints
- Architecture: Vertical Slice Architecture with Clean Architecture principles
- Identity: Supabase Auth with Google OAuth only
- Database: Supabase Postgres through EF Core Code First
- Deployment: Docker image deployed to Railway
- Tests: xUnit for backend unit and integration tests

Move fast for MVP, but do not put business rules in the wrong place. The backend owns business rules, authorization, validation, family scoping, and persistence.

---

## Documentation and Versioning Rules

Agents must consult `.agents/references/` before using external documentation when the question is about PeaceNest product direction, UX, data model, or domain language.

Agents must use current official documentation when implementing framework-specific behavior, especially for:

- .NET latest LTS
- ASP.NET Core authentication and authorization
- ASP.NET Core rate limiting
- `IExceptionHandler`
- ProblemDetails
- Entity Framework Core
- FastEndpoints
- Supabase Auth JWT validation
- Expo
- Expo Router
- NativeWind
- React Native Reusables
- Railway Docker deployment

Do not copy outdated API patterns when newer official framework guidance exists.

Do not hardcode a .NET version in code or documentation unless the repository has already chosen one. Prefer `latest LTS` until a concrete version is selected in project files.

---

## Repository Layout

Use this repository shape unless the existing repository already has a compatible structure.

```txt
src/
  PeaceNest.Api/
    Common/
      Auth/
      Database/
      Errors/
      Caching/
      Realtime/
      RateLimiting/
      OpenApi/
      Middleware/
    Features/
      Auth/
      Families/
      FamilyMembers/
      Invitations/
      WantsAndNeeds/
      FamilyMilestones/
      Voting/
      Comments/
      Notifications/
      Recaps/
      Memories/

tests/
  PeaceNest.Api.Tests.Unit/
  PeaceNest.Api.Tests.Integration/
```

Frontend shape:

```txt
app/
  index.tsx
  auth/
    sign-in.tsx
  dev/
    auth-token.tsx
  family/
    setup.tsx
    invite.tsx
  tabs/
    home.tsx
    wants-needs.tsx
    milestones.tsx
    recaps.tsx
    notifications.tsx
  wants-needs/
    [id].tsx
    create.tsx
  milestones/
    [id].tsx
    create.tsx
  settings/
    family.tsx
    profile.tsx
```

Backend feature slice shape:

```txt
Features/
  WantsAndNeeds/
    CreateWantOrNeed/
      Endpoint.cs
      Request.cs
      Response.cs
      Validator.cs
```

---

## Architecture Rules

### Vertical Slice First

Each backend feature owns its:

- Endpoint
- Request model
- Response model
- Validator
- Feature logic
- Relevant data access

Do not create generic service layers too early.

Start with:

- Endpoint
- Request
- Response
- Validator
- DbContext
- Domain entity

Extract shared services only when duplication becomes painful and real.

### Clean Architecture Principles

Use Clean Architecture principles without turning the MVP into a cathedral of folders.

Required boundaries:

- Frontend does not own business truth.
- Backend owns business rules.
- Domain rules must be testable.
- Infrastructure details must not leak into UI code.
- Authorization must be enforced server-side.
- Family-scoped data access must be enforced server-side.
- Persistence models must not become uncontrolled public API contracts.

### Forbidden Architecture Drift

Do not introduce:

- Controller-based API instead of FastEndpoints
- Generic repository pattern by default
- Large generic service layer by default
- Direct frontend database writes to core tables
- Business rules in React components
- Bank integration abstractions during MVP
- Distributed systems patterns before they are needed
- Complex event sourcing or CQRS infrastructure during MVP

---

## Backend Stack

Use:

- .NET latest LTS
- ASP.NET Core Web API
- FastEndpoints
- Vertical Slice Architecture
- Entity Framework Core
- Code First database approach
- Supabase Postgres
- Supabase Auth JWT validation
- Google OAuth only
- Docker
- Railway deployment using Docker image
- Scalar for API documentation
- SignalR preparation
- In-memory cache first
- HybridCache-ready design
- Rate limiting
- xUnit testing environment

Do not replace these choices without explicit approval.

---

## Backend Authentication Rules

Supabase handles identity. The .NET backend handles authorization and business rules.

Required behavior:

- Google OAuth only.
- Validate Supabase JWT in .NET.
- Mirror Supabase user into backend `Users` table.
- Require authenticated user for protected endpoints.
- Reject unsupported auth methods.
- Do not store passwords.
- Do not expose refresh tokens.
- Do not allow direct client writes to core database tables.

Every protected request from the frontend must send:

```txt
Authorization: Bearer <supabase_access_token>
```

---

## Backend Authorization and Family Permissions

Every family-scoped operation must verify membership before reading or writing data.

Required features:

- Family membership validation
- Role-based access control
- Owner/admin permissions
- Child-safe permissions
- Backend-enforced authorization
- Endpoint-level authorization policies
- Family-scoped data access

Suggested roles:

- Owner
- Parent/Admin
- Adult Member
- Child Member
- Viewer

Do not trust family IDs supplied by the client without checking membership.

Do not return data from a family unless the authenticated user belongs to that family.

---

## Backend Error Handling

Use centralized exception handling with `IExceptionHandler`.

Responses must use ProblemDetails style responses with:

- HTTP status code
- Error code
- Human-safe message
- Request trace ID
- Validation details when applicable

Required mappings:

| Error Type | HTTP Status |
| --- | --- |
| Validation Error | 400 |
| Authentication Error | 401 |
| Authorization Error | 403 |
| Not Found Error | 404 |
| Conflict Error | 409 |
| Domain Rule Error | 422 |
| Rate Limit Error | 429 |
| External Provider Error | 502 |
| Unexpected Server Error | 500 |

Production errors must not expose sensitive internals.

Development logs may include more detail, but secrets must never be logged.

---

## Backend API Documentation

Use Scalar for API documentation.

Required behavior:

- Generate OpenAPI documentation.
- Expose Scalar API reference UI at `/scalar`.
- Support JWT bearer authentication in API docs.
- Group endpoints by feature.
- Show request and response schemas.
- Enable Scalar in development and staging.
- Disable or protect Scalar in production.

---

## Backend Rate Limiting

Use ASP.NET Core rate limiting.

Required policies:

- Global rate limit policy
- Auth endpoint rate limit policy
- Write action rate limit policy
- Invite endpoint rate limit policy
- Recap generation rate limit policy

Required behavior:

- Return custom 429 responses.
- Include `Retry-After` when appropriate.
- Partition by authenticated user when available.
- Partition by IP when user identity is unavailable.

---

## Backend SignalR Preparation

Prepare SignalR lightly. Do not overbuild realtime collaboration in the MVP.

Suggested hub:

```txt
/family-hub
```

Group naming rule:

```txt
family:{familyId}
```

Realtime event names should map to product language:

- Family plan created
- Want or Need updated
- Milestone completed
- Vote cast
- Comment added
- Notification created
- Monthly recap ready

MVP usage rule:

- Prepare infrastructure lightly.
- Use it first for notifications only.
- Use normal API refresh before adding realtime behavior elsewhere.

---

## Backend Caching Strategy

Start with in-memory caching.

Design code so HybridCache or a distributed backing store can be added later, but do not require Redis for MVP.

MVP cache assumptions:

- Single backend instance
- Short-lived cache entries
- Read-heavy data only

Cache candidates:

- Current user profile
- Family membership lookup
- Family role permissions
- Family settings
- Dashboard summaries
- Recap summaries
- Priority ranking snapshots

Cache rules:

- Correctness first, speed second.
- Do not cache security-sensitive tokens.
- Do not keep stale permission changes carelessly.
- Invalidate or shorten caches around membership and role changes.

---

## Backend Database Rules

Use Supabase Postgres with EF Core Code First.

Required features:

- EF Core entities
- EF Core migrations
- PostgreSQL provider
- Audit columns
- Soft deletes
- Created and updated timestamps
- Family-scoped data isolation
- Optimistic concurrency for important records
- Development seed data

Core tables:

- Users
- Families
- FamilyMembers
- FamilyInvitations
- WantsAndNeeds
- FamilyMilestones
- GoalSteps
- Votes
- Comments
- Reactions
- Notifications
- Recaps
- Memories
- ActivityLogs

Database rules:

- Use server-generated IDs where appropriate.
- Use UTC timestamps.
- Never expose internal IDs in logs unnecessarily.
- Always filter family-owned rows by authorized family membership.
- Prefer soft deletion for user-facing records.
- Preserve audit information for meaningful family history.

---

## Backend Product Modules

### Families

Required capabilities:

- Create family
- Update family
- Invite member
- Join family
- Remove member
- Manage roles

### Wants & Needs

Required capabilities:

- Create item
- Edit item
- Archive item
- Complete item
- Add estimated cost
- Add urgency
- Add emotional value
- Add priority score
- Attach memory when memory support exists
- Vote
- Comment

### Family Milestones

Required capabilities:

- Create milestone
- Edit milestone
- Complete milestone
- Add participants
- Add checklist
- Attach memory when memory support exists
- Include in recap

### Voting and Reactions

Required capabilities:

- Cast vote
- Update vote
- Add reaction
- Track participation
- Show agreement level

### Comments

Required capabilities:

- Add comment
- List comments
- Delete comment
- Notify family members

### Notifications

Required capabilities:

- In-app notifications
- Push notification preparation
- Mark as read
- Notification preferences
- Quiet hours later
- Weekly digest later

### Recaps

Required capabilities:

- Monthly recap
- Quarterly recap later
- Yearly recap later
- Completed plans
- Peace wins
- Delayed plans
- Family activity summary
- Memory highlights when memory support exists

MVP recap requirement:

- Monthly recap only.

---

## Backend Testing Rules

Use xUnit.

Test projects:

```txt
tests/
  PeaceNest.Api.Tests.Unit/
  PeaceNest.Api.Tests.Integration/
```

Recommended packages:

- xUnit
- FastEndpoints.Testing
- WebApplicationFactory
- Shouldly or FluentAssertions
- Testcontainers for PostgreSQL later

Unit test these:

- Domain rules
- Priority score calculation
- Permission checks
- Error mapping
- Validators
- Small pure functions
- Recap summary logic

Integration test these:

- FastEndpoints endpoints
- Authentication behavior
- Authorization behavior
- Validation responses
- Global exception handling
- Rate limiting behavior
- Database write/read flows
- Family-scoped data access

MVP testing rule:

- Start with xUnit and FastEndpoints integration tests.
- Add Testcontainers later when database behavior needs to match production PostgreSQL more closely.

Do not merge backend feature work without relevant tests unless explicitly instructed.

---

## Backend C# Coding Standards

Use modern C# and .NET best practices.

Required standards:

- Enable nullable reference types.
- Prefer clear domain names over abbreviations.
- Use async APIs for I/O.
- Pass `CancellationToken` through request pipelines where supported.
- Keep endpoints small and focused.
- Validate requests at the boundary.
- Keep domain calculations pure when possible.
- Avoid static mutable state.
- Use dependency injection through ASP.NET Core conventions.
- Return typed responses where practical.
- Use ProblemDetails for API errors.
- Never log secrets, tokens, connection strings, or private family content.

Avoid:

- Anemic piles of helpers with unclear ownership
- Reflection-heavy code without need
- Hidden global state
- Unbounded queries
- Client-provided ownership or role decisions
- Catch-all exceptions that hide defects

---

## Backend Deployment and DevOps

Required backend DevOps features:

- Dockerized .NET API
- Railway deployment from Docker image
- Environment-based configuration
- Health check endpoint
- Production logging
- Migration workflow
- Structured logging
- Request trace IDs
- Global error logging
- Railway logs
- Database migration logs

Required security behavior:

- HTTPS only in production
- JWT validation
- Google OAuth only
- No password storage
- No refresh token exposure
- Role-based authorization
- Family-scoped data access
- Rate limiting
- File upload validation when uploads exist
- Soft deletion
- Account deletion flow
- No secrets in source control

Configuration rules:

- Use environment variables for secrets and deployment settings.
- Never commit real secrets.
- Provide sample configuration only with safe placeholder values.
- Keep production, staging, and development behavior separated.
- Run migrations through a deliberate workflow, not accidental app startup surprises unless the team explicitly chooses that approach.

Health check rules:

- Provide a lightweight health endpoint.
- Do not expose sensitive dependency details publicly.
- Use deeper diagnostics only in protected environments.

---

## Frontend Stack

Use:

- React Native
- Expo
- Expo Router
- Android first
- Web second
- TypeScript
- Supabase client for Google OAuth session handling
- NativeWind for Tailwind-style styling
- React Native Reusables for shadcn-like reusable components
- Lucide React Native for primary icons
- Expo Vector Icons as fallback icons

Do not add competing UI libraries without explicit approval.

---

## Frontend Styling and Components

Required styling stack:

```txt
NativeWind + React Native Reusables + Lucide React Native
```

Rules:

- Use NativeWind for layout, spacing, colors, typography, and responsive styles.
- Use React Native Reusables for reusable UI components.
- Use Lucide React Native as the default icon set.
- Use Expo Vector Icons only as fallback.
- Avoid mixing too many UI libraries.
- Create PeaceNest design tokens early.

Design tokens:

```txt
Primary: soft green / sage
Secondary: warm beige / cream
Accent: gentle amber
Danger: soft red
Success: green
Background: warm white
Text: charcoal
Muted Text: soft gray
```

Core UI components:

- Button
- IconButton
- Input
- TextArea
- Card
- Badge
- Avatar
- ProgressBar
- Tabs
- Modal
- BottomSheet
- Toast
- Alert
- EmptyState
- LoadingState
- ErrorState
- FamilyMemberChip
- PriorityBadge
- StatusBadge
- PeacePointBadge

UI language should be gentle and useful. Prefer calm product copy over harsh finance or productivity wording.

---

## Frontend Authentication

PeaceNest uses Google OAuth only through Supabase Auth.

Required features:

- Continue with Google button
- Supabase OAuth session handling
- Secure session persistence
- Logout
- Session refresh
- Protected routes
- Redirect unauthenticated users to sign-in
- Redirect authenticated users to dashboard
- Dev-only auth token page

Not supported:

- Email/password login
- Magic links
- Phone login
- Anonymous login

Do not add unsupported auth methods.

---

## Development Auth Token Page

Route:

```txt
/dev/auth-token
```

Purpose:

- Allow developers to sign in with Google OAuth.
- Allow developers to copy the Supabase access token for backend testing.

Required features:

- Google OAuth sign-in
- Display current user
- Display access token
- Copy token button
- Show token expiration
- Refresh session
- Sign out

Production rule:

- This page must be disabled in production.
- It must not leak tokens in production builds.

---

## Frontend API Client

The frontend communicates with the .NET backend through a centralized API client.

Required behavior:

- Base API client
- Automatically attach Supabase access token
- Handle 401 unauthorized responses
- Handle 403 forbidden responses
- Handle validation errors
- Handle rate limit errors
- Handle offline or network errors
- Standard API response handling

Required header:

```txt
Authorization: Bearer <supabase_access_token>
```

Do not scatter raw `fetch` calls throughout screens.

Do not make the frontend the source of truth for family permissions, priority calculations, recap generation, or membership decisions.

---

## Frontend Core Screens

### Authentication

- Sign in with Google
- Auth loading screen
- Auth error screen

### Family Setup

- Create family workspace
- Join family workspace
- Invite family members
- Manage family members

### Home Dashboard

- Family overview
- Priority plans
- Recent activity
- Upcoming milestones
- Peace recap preview
- Notifications summary

### Wants & Needs

- List wants and needs
- Create want or need
- Edit item
- View item details
- Vote
- Comment
- Track progress
- Mark as completed

### Family Milestones

- List milestones
- Create milestone
- View milestone details
- Add checklist steps
- Add participants
- Mark as achieved
- Attach memory when memory support exists

### Notifications

- In-app notification list
- Mark notification as read
- Notification detail
- Empty notification state

### Recaps

- Monthly recap
- Quarterly recap later
- Yearly recap later
- Peace wins
- Completed plans
- Delayed plans
- Family memory highlights when memory support exists

---

## Frontend State Management Rules

Separate state by responsibility.

Local UI state examples:

- Modal open/close
- Form step
- Selected tab
- Temporary filters

Server state examples:

- Family data
- Wants & Needs list
- Milestones
- Votes
- Comments
- Notifications
- Recaps

Auth state examples:

- Current Supabase session
- Current user
- Token expiry

Core rule:

- Keep business truth in the backend.
- The frontend must not become the source of truth.

---

## Frontend Error Handling

Required features:

- Global error boundary
- API error parser
- Friendly validation messages
- Empty states
- Loading states
- Offline states
- Retry actions
- Toast notifications
- Form-level errors
- Field-level errors

Handle these error types:

- Validation error
- Unauthorized
- Forbidden
- Not found
- Conflict
- Rate limited
- Server error
- Network error

Error copy should be calm and useful. Do not show raw backend exceptions to users.

---

## Frontend Realtime Preparation

Prepare for future SignalR support.

Future features:

- SignalR client setup later
- Family-based realtime updates
- Notification refresh
- Activity feed refresh
- Voting update refresh
- Comment update refresh

MVP rule:

- Use normal API refresh first.
- Add realtime only where it improves the family experience.

---

## Security Rules

Security is not optional.

Required:

- Validate Supabase JWTs server-side.
- Enforce family membership server-side.
- Enforce role permissions server-side.
- Use HTTPS in production.
- Never store passwords.
- Never expose refresh tokens.
- Never log access tokens.
- Never commit secrets.
- Validate file uploads when upload support exists.
- Use rate limiting.
- Use safe production errors.
- Support soft deletion.
- Plan account deletion flow.

Forbidden:

- Client-only authorization
- Trusting client-provided roles
- Trusting client-provided family membership
- Direct client writes to core database tables
- Storing Supabase tokens in backend database without a strong reason
- Logging private family notes, comments, or memories unnecessarily

---

## Privacy and Family Safety

PeaceNest contains sensitive family planning data.

Agents must treat these as private family content:

- Needs
- Wants
- Expenses
- Goals
- Milestones
- Comments
- Notes
- Votes
- Memories
- Notifications
- Recaps

Do not expose private family content across families.

Do not use public logs or analytics events that include private family text.

Child-safe permissions must be supported at the authorization model level.

---

## Feature Implementation Checklist

Before implementing any non-trivial feature, verify:

- Required skills were identified, read, and followed.
- Relevant files in `.agents/references/` were consulted and named in the plan or summary.
- `planning-and-task-breakdown` produced ordered tasks with acceptance criteria, verification steps, dependencies, and checkpoints.
- `grill-with-docs` or `grill-me` resolved unclear product, domain, reference interpretation, terminology, or architecture decisions.
- Existing code and documentation were checked before asking the user questions that local context can answer.
- Any resolved domain language was captured in `CONTEXT.md` when appropriate.
- Any hard-to-reverse, surprising, trade-off-based decision was considered for an ADR.

For every backend feature, verify:

- Endpoint belongs to the correct vertical slice.
- Request model is explicit.
- Response model is explicit.
- Validator exists when input needs validation.
- Auth requirements are declared.
- Family membership is checked for family-scoped data.
- Role permission is checked for protected actions.
- Database queries are scoped correctly.
- Errors map to ProblemDetails.
- Tests cover success, validation failure, unauthorized, forbidden, and not found where relevant.

For every frontend feature, verify:

- Screen uses Expo Router conventions.
- UI uses NativeWind and approved components.
- API calls go through the centralized API client.
- Supabase access token is attached automatically.
- Loading, empty, error, and offline states are handled.
- Business rules are not duplicated as source of truth.
- Copy is calm, family-friendly, and clear.

For every DevOps change, verify:

- Docker still builds.
- Environment variables are documented safely.
- Secrets are not committed.
- Health checks still work.
- Logs are structured and safe.
- Railway deployment assumptions remain valid.

---

## Priority Ranking Rules

Priority ranking belongs in the backend.

The ranking model may use:

- Urgency
- Cost
- Importance
- Emotional value
- Family votes

Keep ranking logic testable as a pure domain calculation where possible.

Do not bury ranking logic inside UI sorting code.

Do not make ranking so complex that families cannot understand why something is prioritized.

---

## Recap Rules

Monthly recap is part of MVP.

Recaps should summarize:

- Completed plans
- Peace wins
- Delayed plans
- Family activity summary
- Memory highlights when available

Quarterly and yearly recaps are later unless explicitly requested.

AI-written recaps are later unless explicitly requested.

---

## Notifications Rules

MVP notifications are simple in-app notifications.

Required:

- Notification list
- Mark notification as read
- Notification detail
- Empty state

Prepare for:

- Push notification support later
- Notification preferences
- Quiet hours later
- Weekly digest later

Do not build a complex notification system during MVP.

---

## Memory Rules

Memory layer is later, except for lightweight attachment preparation when explicitly needed.

Allowed early preparation:

- Data model hooks for future memories
- Attachment references when the MVP feature requires them

Avoid in MVP:

- Complex media storage
- Google Photos integration
- OneDrive integration
- Exportable memory books

---

## AI Feature Rules

AI planning features are later.

Do not build these during MVP unless explicitly requested:

- Complex AI advisor
- AI family planning assistant
- AI recap writer
- Priority suggestions
- Goal risk alerts
- Timeline warnings
- Tradeoff insights

If asked to prepare for AI later, create boundaries and interfaces only after the core workflow is stable.

---

## Dependency Rules

Before adding a dependency, confirm it supports the locked stack and MVP goal.

Allowed by default:

- Expo
- Expo Router
- TypeScript
- NativeWind
- React Native Reusables
- Lucide React Native
- Expo Vector Icons as fallback
- Supabase client for auth session handling
- ASP.NET Core Web API
- FastEndpoints
- Entity Framework Core
- PostgreSQL provider
- Scalar
- xUnit
- FastEndpoints.Testing
- WebApplicationFactory
- Shouldly or FluentAssertions

Require explicit approval:

- Alternative auth providers
- Alternative backend framework
- Alternative ORM
- Alternative database
- Large UI component libraries
- Bank or wallet SDKs
- Heavy analytics platforms
- AI SDKs
- Distributed cache infrastructure
- Complex message queues

---

## Naming Rules

Use product language consistently.

Prefer:

- Family
- Family Workspace
- Wants & Needs
- Family Milestone
- Peace Points
- Peace Wins
- Monthly Recap
- Priority
- Progress
- Memory

Avoid making primary product language sound like:

- Ledger
- Transaction engine
- Portfolio
- Enterprise workspace
- Project management board
- Budget enforcement system

---

## Commit and Pull Request Guidance

Agents should keep changes small and reviewable.

For each change:

- Explain the product slice being touched.
- Explain backend, frontend, and database impact.
- Mention tests added or updated.
- Mention security or authorization impact.
- Mention any migration impact.
- Mention any environment variable changes.

Do not mix unrelated product modules in one change unless needed.

---

## Validation Commands

Use repository-provided scripts when available.

Common backend checks:

```txt
dotnet restore
dotnet build
dotnet test
```

Common frontend checks, when configured:

```txt
npm install
npm run lint
npm run typecheck
npm test
```

Do not invent passing results. If a command is unavailable, missing, or fails, report it truthfully and include the relevant error.

---

## Done Means Done

A feature is not done until:

- Required skills were used appropriately, especially `planning-and-task-breakdown` and the relevant grill skill.
- Relevant `.agents/references/` materials were consulted and any conflicts were surfaced.
- It matches MVP scope or an explicitly requested later scope.
- It follows the locked stack.
- It preserves backend ownership of business rules.
- It enforces authentication and family authorization.
- It handles validation and errors cleanly.
- It includes relevant tests.
- It does not leak secrets or private family content.
- It does not introduce unnecessary infrastructure.
- It keeps the product calm, warm, and family-centered.

---

## Final Guardrail

When uncertain, choose the boring, secure, testable MVP path.

PeaceNest should grow like a nest: small twigs placed carefully, not a skyscraper dropped on a branch.
