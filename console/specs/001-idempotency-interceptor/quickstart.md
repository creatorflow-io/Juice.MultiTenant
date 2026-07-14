# Quickstart: Idempotency-Safe Tenant Operations

**Feature**: `001-idempotency-interceptor` | **Date**: 2026-07-12

This guide shows how the integration is validated locally and how consumers adopt it.

## Prerequisites

- Node + Angular CLI 16 workspace (this repo).
- `@juice-js/core@1.2.0` resolvable (root `dependencies` updated by this feature).

## 1. Upgrade the dependency

Root `package.json` and `projects/juice-js/tenants/package.json` reference
`@juice-js/core@1.2.0` (peer in the library, dependency at the workspace root).

```bash
npm install
```

Then confirm the interceptor is actually exported by the upgraded package before wiring:

```bash
# inspect the real API that ships in 1.2.0
cat node_modules/@juice-js/core/public-api.d.ts
```

Wire against the form it exposes (see research R1): a `forRoot()` / provider factory if
present, otherwise register the exported interceptor class via `HTTP_INTERCEPTORS`.

## 2. Consumer usage (no code change required)

Existing consumers keep their current call — idempotency is ON by default:

```ts
imports: [
  TenantsModule.forRoot(tenantOptions),  // idempotency enabled automatically
]
```

To opt out explicitly:

```ts
TenantsModule.forRoot({ ...tenantOptions, enableIdempotency: false })
```

## 3. Validate behavior (tests)

```bash
ng test
```

The suite must show:

- Every mutating `TenantAdminService` call (create/update/delete/activate/deactivate/
  reactivate/suspend/approve/reject/abandon/settings/properties/root-settings) issues a
  request **with** an idempotency marker.
- Every read call (getTenants/getTenant/getSummary/getTenantSettings/getRootSettings)
  issues a request **without** a marker.
- A retried mutating request carries the **same** marker.
- Two distinct actions carry **different** markers.
- A mutating request to a non-tenant URL is **not** tagged.
- `TenantsModule.forRoot()` registers the interceptor when enabled and skips it when
  `enableIdempotency: false`.

## 4. Manual end-to-end check (demo app)

```bash
npm start
```

1. Open the tenants admin UI, create a tenant, and trigger a retry (slow network / double
   submit). Confirm exactly one tenant is created (SC-001, SC-002).
2. Perform a state change (e.g., suspend) and retry — confirm the state is applied once.
3. Browse/search tenants — confirm read behavior is unchanged (SC-005).

## Success signals

| Signal | Maps to |
|--------|---------|
| One tenant created despite retried create | SC-001, SC-002 |
| 100% mutating ops tagged, 0% reads tagged | SC-003 |
| Existing consumers build with no code change | SC-004 |
| Read interactions unchanged | SC-005 |
| `ng test` and `ng build` green | Principle III + workflow gate |

## Rollback

Set `enableIdempotency: false` at the consumer, or revert the dependency bump. Because the
config field is optional and defaults preserve prior behavior, rollback is non-breaking.
