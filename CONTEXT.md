# PeaceNest Context

PeaceNest is a family peace and planning system for shared priorities, milestones, progress, comments, notifications, and reflection. This context keeps domain language precise so implementation does not drift into budgeting, banking, or generic task management.

## Language

**Family Workspace**:
A shared planning space owned by one family.
_Avoid_: Account, organization, tenant

**Preferred Currency**:
The confirmed three-letter currency used consistently for estimated costs within one family workspace.
_Avoid_: Per-user display currency, inferred Google profile currency, transaction currency

**Family Member**:
A user who belongs to a family workspace with a role and membership status.
_Avoid_: Collaborator, staff, teammate

**Family Role**:
The permission level a family member has within a family workspace.
_Avoid_: UI label only, badge, preference

**Backend User Mirror**:
A local PeaceNest user record created from a validated Supabase Google OAuth user.
_Avoid_: Account, credential, login

**Display Name**:
A user's global PeaceNest name, initially suggested from Google identity data and confirmed during onboarding.
_Avoid_: Email prefix, family-specific nickname

**Family Nickname**:
An optional name for one member within one family workspace, overriding their display name only in that family context.
_Avoid_: Global profile name, Google identity

**Backend Authorization**:
The .NET-enforced permission boundary for authenticated users, family membership, family roles, and family-scoped data access.
_Avoid_: Client-side authorization, database-only authorization

**Migration Workflow**:
The deliberate process for applying EF Core schema changes to Supabase Postgres.
_Avoid_: Production startup auto-migration, accidental schema mutation

**Deployment Readiness**:
The Docker, Railway, health check, configuration, logging, Scalar guard, and migration-release preparation required before frontend screens depend on the backend.
_Avoid_: Deployment afterthought, startup migration surprise

**Feature Endpoint**:
A REST-like backend endpoint named after PeaceNest product language and owned by a vertical feature slice.
_Avoid_: Generic CRUD route, table endpoint

**Backend Target Framework**:
The concrete .NET target chosen by the locally installed latest LTS SDK when the backend project is scaffolded.
_Avoid_: Invented framework version, unsupported SDK target

**Source Control Baseline**:
The Git repository state established before source scaffolding begins.
_Avoid_: Untracked implementation drift, scaffold without review point

**Backend Test Strategy**:
The staged verification approach for domain rules and FastEndpoints behavior.
_Avoid_: Untested feature slice, production-only verification

**Application Version 7 GUID**:
A backend-generated UUID v7 used for append-heavy transactional, reflection, notification, memory, and log records while keeping Postgres storage as `uuid`.
_Avoid_: Database-generated primary key, ULID string, random ID for append-heavy records

**Frontend Foundation**:
The Expo app shell, navigation, auth session handling, API client, and Warm Nest UI tokens built before full product screens.
_Avoid_: Fake finished screens, UI-only prototype

**Profile Onboarding**:
The required post-Google step where a user confirms their display name and country or region before creating or joining a family workspace.
_Avoid_: Password signup, optional profile polish

**Frontend App Project**:
The Expo and React Native workspace under `src/PeaceNest.App`, kept beside the backend API project and runnable through root npm wrapper scripts.
_Avoid_: Root-scattered frontend project, backend-coupled UI project

**Server State**:
Backend-owned PeaceNest data fetched, cached, invalidated, and refreshed by the frontend.
_Avoid_: Global source of truth, duplicated business state

**Auth State**:
The current Supabase session, authenticated user identity, access token expiry, and sign-in/sign-out state.
_Avoid_: Family permissions, business rules

**Dev Auth Token Page**:
A non-production-only frontend route that lets developers inspect and copy the current Supabase access token for backend testing.
_Avoid_: Production token viewer, token log

**Warm Nest UI**:
The PeaceNest visual system using warm ivory, white surfaces, rose actions, gold wins, sage progress, and charcoal text with functional contrast.
_Avoid_: Washed-out palette, decorative accent flooding, finance dashboard styling

**Family Plan**:
The core planning record that represents something a family wants to prioritize, progress, discuss, complete, archive, and possibly include in recaps.
_Avoid_: Task, budget item, transaction

**Want or Need**:
A family plan about buying, saving, spending, or prioritizing resources, tagged as a need or a want.
_Avoid_: Expense, purchase, budget line

**Plan Progress**:
A lightweight indication of how far a family plan has moved toward completion.
_Avoid_: Savings ledger, payment tracking, transaction history

**Family Milestone**:
A family plan about a meaningful family goal, habit, event, achievement, or shared growth that is not primarily a purchase.
_Avoid_: Project, chore, sprint goal

**Goal Step**:
A checklist step that breaks a family plan into visible progress.
_Avoid_: Subtask, ticket

**Milestone Participant**:
A family member involved in a family milestone.
_Avoid_: Assignee, owner, resource

**Priority**:
The family-visible ordering of plans, influenced by urgency, importance, emotional value, cost, and votes.
_Avoid_: Severity, rank score only

**Priority Score**:
A backend-calculated signal for a family plan based on urgency, importance, emotional value, estimated cost sensitivity, and votes.
_Avoid_: Final priority, manual rank

**Priority Rank**:
The family-visible ordering position for a family plan that can be manually adjusted.
_Avoid_: Calculated score, severity

**Urgency**:
How soon a want or need requires family attention, presented as `Someday`, `Soon`, or `Now`.
_Avoid_: Emotional value, importance

**Emotional Value**:
How emotionally meaningful a want or need is to the family, scored independently from urgency and importance.
_Avoid_: Desired timing, urgency

**Vote**:
A family member's participation signal for a family plan.
_Avoid_: Approval, estimate

**Comment**:
A family member's note or discussion entry on a family plan.
_Avoid_: Chat message, log entry

**Plan Note**:
The MVP form of a comment: a simple plan-level note without visible threaded replies.
_Avoid_: Thread, chat room

**Notification**:
A gentle in-app alert about relevant family activity.
_Avoid_: Alarm, escalation

**Notification Preview**:
Minimal contextual notification text that points to authorized family content without copying private content into the alert body.
_Avoid_: Full comment body, private note excerpt, recap summary

**Family Invitation**:
An email-bound invitation for a person to join a family workspace with a predefined family role.
_Avoid_: Open join link, public invite

**Temporary Family Join Code**:
A short-lived reusable credential shared outside PeaceNest that lets authenticated Google users request entry to one family workspace.
_Avoid_: Invitation Code, permanent family password

**Family Join Request**:
A pending request from an authenticated user who redeemed a valid Temporary Family Join Code but has not received family access.
_Avoid_: Membership, accepted invitation

**Family Recovery Code**:
A creator-bound, single-use credential issued only when the sole remaining Family Member is the workspace creator and leaves the family workspace. It can restore that creator's Owner membership during the recovery window.
_Avoid_: Temporary Family Join Code, ownership invitation, reusable family password

**Family Recovery Window**:
The 30-day period that begins automatically when the sole creator leaves a family workspace. During this period the workspace is inactive and recoverable; after the deadline the workspace and its family-owned data are permanently deleted.
_Avoid_: Manually scheduled deletion, indefinite archive, reversible deletion after expiry

**Monthly Recap**:
A reflection summary for a family workspace covering completed plans, peace wins, delayed plans, and recent activity for one month.
_Avoid_: Report, analytics dashboard

**Recap Period**:
A deterministic calendar month used to group family progress for a monthly recap.
_Avoid_: Rolling window, arbitrary date range

**Memory**:
A preserved family moment attached to a plan or recap, introduced after the planning workflow is stable.
_Avoid_: File attachment, media asset

## Relationships

- A **Family Workspace** has one or more **Family Members**.
- A **Family Workspace** has exactly one **Preferred Currency**.
- A **Family Workspace** can have many **Family Invitations**.
- A **Temporary Family Join Code** may produce many **Family Join Requests** during its active window.
- A **Family Join Request** does not become a **Family Member** until an authorized member approves it.
- A **Family Recovery Code** belongs to one inactive **Family Workspace** and may only restore the authenticated workspace creator during its **Family Recovery Window**.
- A **Family Member** is backed by one authenticated **Backend User Mirror**.
- A **Backend User Mirror** has one confirmed **Display Name**.
- A **Family Member** may have one **Family Nickname**.
- A **Family Member** has exactly one **Family Role**.
- A **Family Member** is authorized through **Backend Authorization** before accessing family-scoped data.
- A **Family Plan** belongs to exactly one **Family Workspace**.
- A family-scoped **Feature Endpoint** includes the family workspace identity in its route or resolves it through an explicitly authorized context.
- The **Frontend App Project** communicates with backend **Feature Endpoints** through the centralized API client.
- **Profile Onboarding** is completed before a **Backend User Mirror** may create or join a **Family Workspace**.
- A **Family Plan** has **Plan Progress**.
- A **Want or Need** is a specialized **Family Plan**.
- A **Family Milestone** is a specialized **Family Plan**.
- A **Family Plan** can have many **Goal Steps**, **Votes**, **Comments**, **Notifications**, and **Activity Logs**.
- Append-heavy child records such as **Family Invitations**, **Goal Steps**, **Votes**, **Comments**, **Notifications**, **Recaps**, **Memories**, and **Activity Logs** use **Application Version 7 GUIDs**.
- A **Comment** is exposed as a simple **Plan Note** in the MVP.
- A **Family Milestone** can have many **Milestone Participants**.
- A **Family Plan** has a **Priority Score** and may have a manually adjusted **Priority Rank**.
- A **Monthly Recap** belongs to exactly one **Family Workspace** and may feature many **Family Plans**.
- A **Monthly Recap** covers exactly one **Recap Period**.
- A **Memory** belongs to a **Family Workspace** and may attach to a **Family Plan** or **Monthly Recap** later.

## Example Dialogue

> **Dev:** "Should the Wants & Needs board store separate records from the Milestones board?"
> **Domain expert:** "They should feel separate in the product, but both are family plans underneath. Wants & Needs need resource details; Milestones need checklist, participants, and reflection details."

## Flagged Ambiguities

- "Foundation" means backend-owned business truth first: authentication, user mirroring, family membership, family-scoped data access, ProblemDetails errors, rate limiting, EF Core persistence, tests, Docker, and health checks before UI polish.
- "Painting the UI" means frontend visual refinement after core Expo routes, auth state, API client, and working MVP screens exist.
- Resolved: **Want or Need** and **Family Milestone** use one unified **Family Plan** backend model with type-specific details, but remain separate feature slices and frontend screens.
- Resolved: the **Backend User Mirror** is created lazily on the first authenticated backend request through `GET /auth/me`.
- Resolved: the backend models all five **Family Roles** from day one: Owner, Parent/Admin, Adult Member, Child Member, and Viewer. The MVP UI may expose a simpler role selection, but backend permission checks must support the full role set.
- Resolved: **Family Invitations** are email-bound plus token-based. The database stores invited email, invited role, token hash, expiry, and status; accept flow checks token hash, expiry, status, authenticated user, and matching authenticated email.
- Resolved: PeaceNest uses hybrid priority. The backend calculates **Priority Score**, while families may manually adjust **Priority Rank** for visible ordering. `Now`, `Soon`, and `Someday` describe **Urgency**; **Importance** and **Emotional Value** use their own level labels.
- Resolved: after auth, user mirroring, family workspace, and invitations, the first planning slice is **Want or Need**, implemented through the shared **Family Plan** model so **Family Milestones** can follow immediately.
- Resolved: MVP **Plan Progress** for **Want or Need** records is lightweight and manual, not transaction-based savings tracking. Estimated cost is informational; PeaceNest must not introduce deposits, payments, bank sync, wallets, or transaction history in the MVP.
- Resolved: **Goal Steps** for **Family Milestones** are shared progress markers in the MVP. A step may record who completed it, but there is no heavy assignment workflow, chore engine, per-step reminders, or workload tracking.
- Resolved: MVP comments are simple plan-level **Plan Notes**. The schema may keep nullable `parent_comment_id` for future replies, but the first UI does not show nested threads.
- Resolved: MVP **Notification Previews** use minimal contextual text and do not include full private content such as comment bodies, funding notes, sensitive plan descriptions, or recap summaries. Notifications may reference related IDs for authorized deep-linking.
- Resolved: MVP **Monthly Recaps** are generated on demand by an authorized family member, with deterministic monthly **Recap Period** boundaries. No scheduler, AI-written summaries, quarterly/yearly recaps, or memory highlights are required in the MVP.
- Resolved: **Backend Authorization** is the source of truth for app permissions, and Supabase Postgres is configured defensively. Core tables are not for direct frontend writes; service-role keys never go to clients; exposed schemas should use RLS or equivalent defense-in-depth without replacing .NET family membership and role checks.
- Resolved: the **Migration Workflow** uses explicit EF Core migration execution. Production startup must not auto-apply migrations; development seeding stays development-only unless explicitly promoted.
- Resolved: **Deployment Readiness** is added after core backend MVP APIs are working and before full frontend screen implementation begins. Health checks come early; Docker/Railway polish should not block the first backend slices.
- Resolved: backend API routes use REST-like **Feature Endpoints** grouped by product language, not generic CRUD/table names. Family-scoped routes should make family context explicit, commonly with `familyId` in the route.
- Resolved: the **Backend Target Framework** is established by the installed latest LTS .NET SDK available locally during scaffolding. Do not invent or hardcode an unsupported framework version before checking local SDKs.
- Resolved: initialize the **Source Control Baseline** before scaffolding source code so planning/docs and implementation changes can be reviewed separately.
- Resolved: create an initial planning/docs commit containing `.agents/`, `.gitignore`, `AGENTS.md`, and `CONTEXT.md` before backend scaffolding. Backend source scaffolding belongs in the next commit.
- Resolved: the **Backend Test Strategy** starts with xUnit unit tests plus FastEndpoints integration tests using a replaceable test database strategy. Add Testcontainers when Postgres-specific behavior matters, such as `citext`, migrations, concurrency behavior, or realistic SQL constraints.
- Resolved: full frontend screens wait until the backend has family workspace and the first **Want or Need** API slice working. **Frontend Foundation** may be scaffolded earlier when useful, but data-moving screens should be built against working backend endpoints.
- Resolved: the **Frontend App Project** lives under `src/PeaceNest.App` beside `src/PeaceNest.Api`. Root npm scripts remain the stable developer entry point and delegate frontend commands into the app workspace.
- Resolved: frontend **Server State** uses TanStack Query through the centralized API client. Supabase **Auth State** stays in a dedicated auth provider/context, and temporary local UI state stays near the component.
- Resolved: the **Dev Auth Token Page** is compiled or rendered only in non-production environments and fails closed in production. It may display and copy the current Supabase access token for local testing, but must never log tokens or send them to telemetry.
- Resolved: UI polish uses **Warm Nest UI** tokens exactly, with functional contrast and restrained accent usage as acceptance criteria. Rose is for primary actions, gold for wins, sage for progress/completion, and charcoal for readable text.
- Resolved: append-heavy transactional and log-style records use **Application Version 7 GUIDs** generated by the .NET backend. Stable root identity records such as **Backend User Mirrors**, **Family Workspaces**, **Family Members**, **Family Plans**, and plan type-detail records keep their existing identity behavior. Supabase Postgres continues to store all IDs as `uuid`; this does not require a schema migration.
- Resolved: **Preferred Currency** belongs to the **Family Workspace** so shared estimated costs display consistently for every member. Device locale may preselect the user's country or region during **Profile Onboarding**; family setup then suggests a currency from that confirmed profile country and requires confirmation. Google profile data is not treated as a reliable country source.
- Resolved: existing **Family Workspaces** are backfilled with `PHP` as their **Preferred Currency** because the MVP launches for Filipino families. The currency remains an ISO-style three-letter workspace setting so future Singapore and wider Asian expansion does not require changing the planning model.
- Resolved: authorized family members may fully edit an active **Family Plan**. Completed and archived plans remain read-only so recaps and family history stay trustworthy; permitted members may still add **Plan Notes**. Reopening a plan is outside the current scope.
- Resolved: changing a **Family Workspace**'s **Preferred Currency** does not convert or relabel existing estimated costs. Existing estimates retain the amount and currency originally entered; the new preference applies to new or subsequently edited estimates. Automatic forex conversion is postponed until after the MVP and must not be frontend-owned business truth.
- Resolved: editing an active **Family Milestone** includes adding, renaming, reordering, and removing **Goal Steps**. Existing step identities and completion history are preserved, new steps receive new identifiers, and removed steps are soft-deleted.
- Resolved: the MVP **Preferred Currency** picker offers `PHP`, `SGD`, and `USD`. Family setup suggests the supported currency matching the creator's confirmed profile country and otherwise falls back to `PHP`; the backend stores an uppercase three-letter currency code so the supported Asian market list can expand without a schema redesign.
- Resolved: onboarding asks for a global **Display Name**, prefilled from Google identity data when available. The existing **Family Nickname** remains an optional later override for family-specific names such as `Mama` or `Ate Mia`.
- Resolved: MVP manual joining uses a reusable, expiring **Temporary Family Join Code** shared through an external messaging platform or the device share sheet. Redeeming the code creates a **Family Join Request** and never grants immediate family access; an authorized family member must approve the request.
- Resolved: a **Temporary Family Join Code** carries no family role. During approval, an owner or parent/admin selects `Adult Member`, `Child Member`, or `Viewer`; only the family owner may approve a request as `Parent/Admin`.
- Resolved: Parent/Admin, Adult Member, Child Member, and Viewer may leave their own family membership without approval. Their membership becomes removed while historical family attribution remains intact.
- Resolved: an Owner cannot leave while other active family members remain; ownership must be transferred first. When the Owner is the sole active member and is also the workspace creator, leaving atomically removes the membership, deactivates the workspace, issues a creator-bound **Family Recovery Code**, and begins the 30-day **Family Recovery Window**. There is no separate deletion-scheduling action.
- Resolved: a **Family Recovery Code** is separate from a **Temporary Family Join Code**, is stored only as a cryptographic hash, is revealed once, and cannot grant access to anyone except the same authenticated workspace creator. Successful recovery restores the creator's Owner membership and invalidates the code.
- Resolved: a backend background service periodically claims expired recovery windows and permanently deletes the family workspace and family-owned data transactionally. Recovery also checks the authoritative deadline so an overdue workspace cannot be restored before a delayed purge runs.
- Resolved: permanent family purge removes family memberships, plans, progress, notes, votes, recaps, notifications, access credentials, and other family-owned records. Global **Backend User Mirrors** remain because one user may belong to other family workspaces.
- Resolved: when the sole creator leaves, all active **Temporary Family Join Codes** are revoked and all pending **Family Join Requests** are cancelled. Reactivation starts a fresh family access cycle with no inherited pending requests.
- Resolved: leaving requires explicit confirmation. A sole creator sees the immediate loss of access and 30-day permanent-deletion warning, then receives a dedicated one-time recovery-code view with the authoritative deadline. Other members see a simpler warning that their historical family contributions remain.
- Resolved: the backend owns the **Temporary Family Join Code** lifetime, initially configured as 15 minutes. Code creation requests cannot choose or extend the duration; responses provide authoritative creation and expiry timestamps for frontend display.
- Resolved: a **Family Workspace** has at most one active **Temporary Family Join Code**. Owners and parent/admins may rotate or revoke it; rotation revokes the previous code immediately without invalidating **Family Join Requests** already created from it.
- Resolved: a **Family Join Request** has a backend-controlled seven-day review window independent of its source code's expiry. The requester may withdraw a pending request; expired, rejected, withdrawn, or approved requests do not prevent a later active code from creating a fresh request.
- Resolved: family join reviewers see the requester's confirmed **Display Name**, masked verified Google email, optional profile avatar, and request timestamps. Country or region and unrelated profile data remain private.
- Resolved: redeeming a **Temporary Family Join Code** is idempotent while the authenticated user has a pending **Family Join Request** for that family. The backend returns the original request and timestamps without creating a duplicate or extending its review window.
- Resolved: a user with an approved **Family Join Request** resolves to their existing active **Family Membership** and cannot create another request for the same family. Expired, rejected, or withdrawn requests allow a fresh request; a removed member must redeem a currently active code and receive approval again before rejoining.
- Resolved: a **Temporary Family Join Code** is revealed only in the creation response and stored only as a cryptographic hash. Later reads expose only whether a code is active and its timestamps; losing the plaintext requires rotation, which revokes the old code and begins a new 15-minute window.
- Resolved: a **Temporary Family Join Code** may create at most ten distinct successful **Family Join Requests**, controlled by backend configuration. Idempotent retries do not consume additional capacity; redemption remains rate-limited by authenticated user and IP.
- Resolved: the MVP creates minimal in-app notifications for family owners and parent/admins when a **Family Join Request** arrives and for the requester when it is approved or rejected. Pending state remains visible through normal API refresh; SignalR and push delivery are deferred.
- Resolved: invitation email delivery is postponed until after the manual join-code flow is validated. Email remains a possible future delivery channel rather than the source of invitation truth.
- Resolved: after Google authentication, **Profile Onboarding** requires the user to confirm their **Display Name** and country or region. The next workspace choice offers creating a family or joining with a **Temporary Family Join Code**.
- Resolved: users mirrored before **Profile Onboarding** existed must confirm their prefilled current display name and select a country or region on their next authenticated session. Completing this profile migration does not change or remove existing family memberships.
- Resolved: **Profile Onboarding** offers every country or region, but the MVP **Preferred Currency** mapping remains market-bound to `PHP`, `SGD`, and `USD`. Other countries fall back to `PHP`, which the user must confirm during family setup; additional currencies are introduced market by market.
