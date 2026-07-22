# PeaceNest

## Project Overview

**PeaceNest** is a peaceful family planning space for families who love, plan, and grow together.

The app helps families organize their needs, wants, goals, milestones, and memories in one shared place. It is not meant to feel like a strict budgeting app. Instead, it should feel like a calm family nest where everyone can see what matters, understand priorities, and move forward with less stress.

PeaceNest helps families answer questions like:

- What do we need as a family?
- What do we want someday?
- What should we prioritize first?
- What have we already achieved?
- What milestones are we working toward?
- How can we plan together with more peace?

The heart of the product is **family alignment**: making plans visible, helping everyone feel heard, and turning family planning into something warm, fun, and meaningful.

---

## Project Context

Many families make decisions reactively. Needs, wants, expenses, dreams, and milestones are often scattered across conversations, notes, chats, and memory.

This creates problems such as:

- Misaligned family priorities
- Forgotten goals
- Unclear needs vs wants
- Stressful money conversations
- Lack of shared direction
- No central record of family achievements
- Difficulty reflecting on monthly, quarterly, or yearly progress

PeaceNest exists to solve this by becoming a **family peace and planning system**.

The product is built around two major planning categories:

### 1. Wants & Needs

This section is for plans related to buying, saving, spending, or prioritizing resources.

Examples:

- New refrigerator
- Tuition
- Emergency fund
- House repair
- Family vacation
- School supplies
- Medical needs

These items can be ranked by urgency, cost, importance, emotional value, and family votes.

### 2. Family Milestones

This section is for meaningful family plans that are not only about buying things.

Examples:

- Sunday family dinner habit
- Visit grandparents monthly
- Child graduation
- Family reunion
- Health improvement goal
- Less screen time
- Spiritual or personal growth goals
- Yearly family reflection

This makes PeaceNest feel broader than a finance tool. It becomes a home for family direction, growth, and memories.

---

## Project Roadmap

### Phase 1: Foundation

Build the core planning experience.

- Create family workspace
- Invite family members
- Add Wants & Needs
- Add Family Milestones
- Categorize plans
- Rank priorities
- Track basic progress
- Add comments or notes

### Phase 2: Engagement & Fun

Make the app feel alive and enjoyable.

- Family Quests
- Progress bars
- Reactions
- Peace Points
- Milestone celebrations
- Gentle reminders
- Inactivity nudges

### Phase 3: Recaps & Reflection

Turn family progress into meaningful reflection.

- Monthly recaps
- Quarterly recaps
- Yearly recaps
- Achievement history
- Family timeline
- Completed goals archive

### Phase 4: Memory Layer

Add lightweight memory features.

- Attach photos to goals or milestones
- Optional image upload
- Future Google Photos integration
- Future OneDrive integration
- Memory timeline

### Phase 5: Smart Planning

Add intelligence after validating the main workflow.

- Priority suggestions
- Goal risk alerts
- Timeline warnings
- Tradeoff insights
- AI family planning assistant

---

## Project MVP

The MVP should focus on validating the core idea: families planning together for peace and shared direction.

### MVP Features

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

### MVP Should Avoid

- Bank integrations
- Complex AI advisor
- Heavy analytics
- Complex media storage
- Google Photos integration
- OneDrive integration
- Overbuilt gamification

The MVP should prove that families want a shared planning space before adding complex infrastructure.

### MVP Success Signals

- Families add multiple plans
- Families return weekly or monthly
- Members vote or comment on priorities
- Users complete milestones
- Users read or share recaps
- The app reduces confusion in family planning

---

## Project Technical Features
### Technical Direction

PeaceNest will be built with a mobile-first architecture focused on Android and Web. The system will use Expo for the frontend, a .NET backend for business rules, Supabase for Google OAuth and Postgres, and Docker-based deployment to Railway.

The technical direction is intentionally practical: move fast for MVP, keep business rules in the backend, and avoid overbuilding infrastructure before the family planning workflow is validated.

---

# Frontend Technical Features

## Core Frontend Stack

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

## Styling & Component System

PeaceNest should use a styling approach that feels close to **Tailwind CSS + shadcn/ui**, but uses tools that work naturally with Expo and React Native.

### Recommended Stack

```txt
NativeWind + React Native Reusables + Lucide React Native
```

### Why This Stack

- **NativeWind** gives Tailwind-style utility classes for React Native.
- **React Native Reusables** gives a shadcn-inspired component pattern for React Native.
- **Lucide React Native** gives clean, calm, modern icons.
- **Expo Vector Icons** can be used when Lucide does not have the needed icon.

## Styling Rules

- Use NativeWind for layout, spacing, colors, typography, and responsive styles.
- Use React Native Reusables for reusable UI components.
- Use Lucide React Native as the default icon set.
- Use Expo Vector Icons only as fallback.
- Avoid mixing too many UI libraries.
- Create PeaceNest design tokens early.

## Design Direction

PeaceNest should feel:

- Calm
- Warm
- Family-friendly
- Soft
- Trustworthy
- Peaceful
- Light and simple

## Suggested Design Tokens

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

## Core UI Components

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

## Frontend Navigation

Use Expo Router.

### Suggested Route Structure

```txt
src/
  PeaceNest.App/
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

## Frontend Authentication

PeaceNest will use **Google OAuth only** through Supabase Auth.

### Features

- Continue with Google button
- Supabase OAuth session handling
- Secure session persistence
- Logout
- Session refresh
- Protected routes
- Redirect unauthenticated users to sign-in
- Redirect authenticated users to dashboard
- Dev-only auth token page

### Not Supported

- Email/password login
- Magic links
- Phone login
- Anonymous login

## Development Auth Token Page

### Route

```txt
/dev/auth-token
```

### Purpose

Allow developers to sign in with Google OAuth and copy the Supabase access token for backend testing.

### Features

- Google OAuth sign-in
- Display current user
- Display access token
- Copy token button
- Show token expiration
- Refresh session
- Sign out

### Production Rule

This page must be disabled in production.

## Frontend API Client

The frontend should communicate with the .NET backend through a centralized API client.

### Features

- Base API client
- Automatically attach Supabase access token
- Handle 401 unauthorized responses
- Handle 403 forbidden responses
- Handle validation errors
- Handle rate limit errors
- Handle offline or network errors
- Standard API response handling

### Request Header

```txt
Authorization: Bearer <supabase_access_token>
```

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
- Attach memory

### Notifications

- In-app notification list
- Mark notification as read
- Notification detail
- Empty notification state

### Recaps

- Monthly recap
- Quarterly recap
- Yearly recap
- Peace wins
- Completed plans
- Delayed plans
- Family memory highlights

## Frontend Realtime Preparation

Prepare the frontend for future SignalR support.

### Features

- SignalR client setup later
- Family-based realtime updates
- Notification refresh
- Activity feed refresh
- Voting update refresh
- Comment update refresh

### MVP Rule

Use normal API refresh first. Add realtime only where it improves the family experience.

## Frontend State Handling

### Local UI State

Examples:

- Modal open/close
- Form step
- Selected tab
- Temporary filters

### Server State

Examples:

- Family data
- Wants & Needs list
- Milestones
- Votes
- Comments
- Notifications
- Recaps

### Auth State

Examples:

- Current Supabase session
- Current user
- Token expiry

### Rule

Keep business truth in the backend. The frontend should not become the source of truth.

## Frontend Error Handling

### Features

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

### Error Types to Handle

- Validation error
- Unauthorized
- Forbidden
- Not found
- Conflict
- Rate limited
- Server error
- Network error

---

# Backend Technical Features

## Core Backend Stack

- .NET Latest LTS
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

## Backend Architecture

PeaceNest backend will use **FastEndpoints with Vertical Slice Architecture**.

Each feature will own its endpoint, request model, response model, validator, and feature logic.

### Example Backend Structure

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

### Backend Slice Example

```txt
Features/
  WantsAndNeeds/
    CreateWantOrNeed/
      Endpoint.cs
      Request.cs
      Response.cs
      Validator.cs
```

### Architecture Rule

Avoid creating generic service layers too early.

Start with:

- Endpoint
- Request
- Response
- Validator
- DbContext
- Domain entity

Extract shared services only when duplication becomes painful.

## Backend Authentication

Supabase handles identity. The .NET backend handles authorization and business rules.

### Auth Rules

- Google OAuth only
- Validate Supabase JWT in .NET
- Mirror Supabase user into backend Users table
- Require authenticated user for protected endpoints
- No direct client writes to core database tables

## Authorization & Family Permissions

### Features

- Family membership validation
- Role-based access control
- Owner/admin permissions
- Child-safe permissions
- Backend-enforced authorization
- Endpoint-level authorization policies
- Family-scoped data access

### Suggested Roles

- Owner
- Parent/Admin
- Adult Member
- Child Member
- Viewer

## Global Exception Handling

Use centralized exception handling with `IExceptionHandler`.

### Features

- Global exception handler
- ProblemDetails response format
- Exception-to-status-code mapping
- Request trace ID
- Error codes
- Safe production errors
- Detailed development logs
- No sensitive error exposure to clients

### Error Variants

- Validation Error: 400
- Authentication Error: 401
- Authorization Error: 403
- Not Found Error: 404
- Conflict Error: 409
- Domain Rule Error: 422
- Rate Limit Error: 429
- External Provider Error: 502
- Unexpected Server Error: 500

## API Documentation

Use Scalar for API documentation.

### Features

- OpenAPI document generation
- Scalar API reference UI
- JWT bearer authentication support
- Development and staging API testing
- Endpoint grouping by feature
- Request/response schema visibility

### Route

```txt
/scalar
```

### Rule

Enable Scalar in development and staging. Disable or protect it in production.

## Rate Limiting

### Purpose

Protect backend resources and prevent abuse.

### Features

- Global rate limit policy
- Auth endpoint rate limit policy
- Write action rate limit policy
- Invite endpoint rate limit policy
- Recap generation rate limit policy
- Custom 429 response
- Retry-After header
- Per-user or per-IP partitioning

## SignalR Realtime Preparation

Use SignalR for future realtime family collaboration.

### Suggested Hub

```txt
/family-hub
```

### Grouping Rule

```txt
family:{familyId}
```

### Realtime Events

- Family plan created
- Want or Need updated
- Milestone completed
- Vote cast
- Comment added
- Notification created
- Monthly recap ready

### MVP Rule

Prepare infrastructure lightly. Use it first for notifications only.

## Caching Strategy

PeaceNest will prepare for HybridCache but start with in-memory caching.

### MVP Cache

- In-memory cache
- Single backend instance assumption
- Short-lived cache entries
- Cache mostly read-heavy data

### Cache Candidates

- Current user profile
- Family membership lookup
- Family role permissions
- Family settings
- Dashboard summaries
- Recap summaries
- Priority ranking snapshots

### Cache Rule

Correctness first, speed second. Do not cache security-sensitive tokens or stale permission changes carelessly.

## Database

PeaceNest will use Supabase Postgres with EF Core Code First.

### Features

- EF Core entities
- EF Core migrations
- Code First schema management
- PostgreSQL provider
- Audit columns
- Soft deletes
- Created/updated timestamps
- Family-scoped data isolation
- Optimistic concurrency for important records
- Development seed data

### Core Tables

- Users
- Families
- FamilyMembers
- FamilyInvitations
- FamilyJoinCodes
- FamilyJoinRequests
- FamilyRecoveryCodes
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

## Core Backend Product Modules

### Families

- Create family
- Update family
- Invite member
- Join family
- Remove member
- Manage roles
- Leave family with owner guardrails
- Recover a sole-creator workspace within 30 days
- Permanently purge expired inactive family workspaces

### Wants & Needs

- Create item
- Edit item
- Archive item
- Complete item
- Add estimated cost
- Add urgency
- Add emotional value
- Add priority score
- Attach memory
- Vote
- Comment

### Family Milestones

- Create milestone
- Edit milestone
- Complete milestone
- Add participants
- Add checklist
- Attach memory
- Include in recap

### Voting & Reactions

- Cast vote
- Update vote
- Add reaction
- Track participation
- Show agreement level

### Comments

- Add comment
- List comments
- Delete comment
- Notify family members

### Notifications

- In-app notifications
- Push notification preparation
- Mark as read
- Notification preferences
- Quiet hours later
- Weekly digest later

### Recaps

- Monthly recap
- Quarterly recap
- Yearly recap
- Completed plans
- Peace wins
- Delayed plans
- Family activity summary
- Memory highlights

## Backend Testing Environment

PeaceNest backend testing will use **xUnit**.

### Test Projects

```txt
tests/
  PeaceNest.Api.Tests.Unit/
  PeaceNest.Api.Tests.Integration/
```

### Unit Testing

Use unit tests for:

- Domain rules
- Priority score calculation
- Permission checks
- Error mapping
- Validators
- Small pure functions
- Recap summary logic

### Integration Testing

Use integration tests for:

- FastEndpoints endpoints
- Authentication behavior
- Authorization behavior
- Validation responses
- Global exception handling
- Rate limiting behavior
- Database write/read flows
- Family-scoped data access

### Recommended Testing Packages

- xUnit
- FastEndpoints.Testing
- WebApplicationFactory
- Shouldly or FluentAssertions
- Testcontainers for PostgreSQL later

### MVP Testing Rule

Start with xUnit and FastEndpoints integration tests. Add Testcontainers later when database behavior needs to match production PostgreSQL more closely.

## Backend Deployment

### Features

- Dockerized .NET API
- Railway deployment from Docker image
- Environment-based configuration
- Health check endpoint
- Production logging
- Migration workflow

## Backend Observability

### Features

- Structured logging
- Request trace IDs
- Global error logging
- Health check endpoint
- Railway logs
- Database migration logs

## Backend Security

### Features

- HTTPS only in production
- JWT validation
- Google OAuth only
- No password storage
- No refresh token exposure
- Role-based authorization
- Family-scoped data access
- Rate limiting
- File upload validation
- Soft deletion
- Account deletion flow
- No secrets in source control

---

# MVP Technical Scope

## Frontend Must Have

- Expo Android/Web app
- Expo Router
- NativeWind
- React Native Reusables
- Lucide React Native
- Google OAuth through Supabase
- Protected routes
- API client with bearer token
- Family setup UI
- Wants & Needs UI
- Family Milestones UI
- Voting UI
- Comments UI
- Notifications UI
- Monthly recap UI
- Dev-only auth token page

## Backend Must Have

- .NET API
- FastEndpoints
- Vertical Slice Architecture
- EF Core Code First
- Supabase Postgres
- Supabase JWT validation
- Family authorization
- Global exception handling
- ProblemDetails error format
- Rate limiting
- Scalar API docs
- xUnit testing setup
- Docker backend
- Railway deployment
- Family workspace module
- Wants & Needs module
- Family Milestones module
- Basic voting
- Basic comments
- In-app notifications
- Monthly recap

## Should Have

- SignalR foundation
- Activity feed
- In-memory caching
- Image upload
- Push notification preparation
- Invite links

## Later

- HybridCache with distributed backing store
- Redis or distributed cache
- Advanced SignalR collaboration
- AI recap writer
- AI family advisor
- Google Photos integration
- OneDrive integration
- Bank or wallet integrations
- Exportable yearly memory book
