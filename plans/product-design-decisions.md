# Product Design Decisions

This document records confirmed product decisions, open questions, and deferred
implementation work discussed during product design. It should be updated as
decisions are made so that unresolved details are not lost.

## Confirmed decisions

### Co-parenting groups and membership

- The service will support multiple independent co-parenting groups.
- A group has two decision-making parent memberships rather than one global
  calendar shared by the entire installation.
- A user may eventually belong to more than one group.
- A group may also include read-only observers, such as either parent's new
  partner.
- Both parents must approve granting an observer access.
- Both parents can see everyone who has access to their group.
- Observer invitation proposals, approvals, rejections, acceptance, expiry,
  and removal must be included in the group's history.

### Schedule approval and history

- Whether schedule changes require approval is configurable per co-parenting
  group.
- Schedule changes must always be traceable, regardless of whether approval is
  enabled.
- History is visible in full to both parents.
- History includes direct and approved changes, rejected requests, initial
  schedule creation, imports, and relevant system actions.
- An applied change records the affected date, previous and new values, actor,
  timestamp, change mechanism, approval requirement at that time, approval
  details when applicable, associated request, and optional reason.

### Notifications

- Approval-related activity generates notifications.
- Notifications are visible inside the application after login.
- The service also supports browser notifications after obtaining explicit
  browser permission from the user.
- Browser notifications must be delivered even when Selmaschema is not open.
  This requires web push subscriptions, a service worker, and server-side push
  delivery rather than relying only on foreground browser notifications.
- Browser and lock-screen notification previews use generic text and do not
  expose names, dates, comments, decisions, or other family details. The user
  must open the authenticated application to see those details.
- Notification events and delivery state must respect group membership and
  must not expose one group's information to another group.

## Current role model

- `Parent`: one of the two equal decision-making participants in a group.
- `Observer`: a mutually approved, read-only group member. Observers cannot
  change the schedule, participate in approvals, modify settings, or manage
  membership.
- Platform operation is separate from family decision-making. A platform
  administrator, if one exists, does not approve or override family schedule
  decisions.

## Open questions

### Observer access

- Can either parent revoke an observer immediately, or does removal require
  mutual approval?
- Can observers read parent comments, or only calendar assignments, statuses,
  and history?

### Signup and group activation

- Can the creating parent use the group before the second parent accepts?
- Does the initial schedule remain a draft until both parents confirm it?
- How are existing single-installation users and data migrated into a group?

### Schedule permissions

- Which schedule fields may parents edit directly when assignment changes
  require approval?
- Should VAB and special-status updates be limited to the parent assigned to
  that day?

### Notifications

- Which events beyond approval activity generate notifications?
- Can users configure notification channels and event preferences?
- Should notifications be grouped or summarized to avoid excessive alerts?
- What are the read, unread, dismissed, expiry, and retention rules?

## Deferred implementation work

- Introduce group, membership, invitation, and approval entities.
- Scope all calendar, configuration, comments, history, and requests to a
  group.
- Add immutable, append-only audit events.
- Implement observer proposal and invitation workflows.
- Add an in-app notification inbox with unread counts.
- Add browser notification permission UX and delivery infrastructure.
- Revisit API authorization and ownership rules after the group role model is
  finalized.
- Add signup, account verification, invitation acceptance, and account recovery.
- Replace installation-specific personal seed names with neutral defaults.
- Align EF Core and database initialization documentation.

## Incremental delivery roadmap

Each step should be delivered as a separate pull request. A step advances only
after its checks and GitHub review complete and actionable feedback is resolved.

1. Record the agreed product decisions and delivery roadmap.
2. Add co-parenting groups and parent memberships, including migration of the
   existing single-household data.
3. Scope calendars, configuration, comments, and change requests to groups and
   enforce group isolation.
4. Add an immutable audit history and make the complete history visible to both
   parents.
5. Add per-group schedule-approval settings and enforce the corrected request,
   review, and direct-edit authorization rules.
6. Add observer proposals, mutual parental approval, invitations, acceptance,
   and read-only access.
7. Add account signup, group onboarding, and initial-schedule confirmation.
8. Add the persistent in-app notification inbox for approval activity.
9. Add optional, privacy-safe background web push notifications.
10. Harden session restoration and logout, neutralize default data, and align
    database and development documentation.
