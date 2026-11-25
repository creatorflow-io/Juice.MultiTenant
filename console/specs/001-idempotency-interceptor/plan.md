# Implementation Plan: Idempotency-Safe Tenant Operations

**Branch**: `001-idempotency-interceptor` | **Date**: 2026-07-12 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-idempotency-interceptor/spec.md`

> **✅ Unblocked (2026-07-14)** — the workspace was migrated to Angular 22
> (see [../002-angular-22-upgrade/plan.md](../002-angular-22-upgrade/plan.md)), so `@juice-js/core@1.2.0`
> and the idempotency integration now build and run in the **full app**, not just in library
> isolation. All tests green across the workspace (tenants 32/32, dict-builder 4/4, app 2/2).

## Summary

Upgrade the `@juice-js/core` dependency to `1.2.0` and integrate its idempotency
HTTP interceptor into the `@juice-js/tenants` library so that every state-changing tenant
request (create/update/delete and all status transitions, plus settings/properties updates)
carries a unique, retry-stable idempotency marker. Wiring happens centrally via
`TenantsModule.forRoot()` so all current and future mutating operations on
`TenantAdminService` are covered without per-call changes, while read (GET) requests are
left untouched. The interceptor is scoped so it does not attach markers to non-tenant
application traffic. Released through the existing semantic-release / Conventional
Commits pipeline.

## Technical Context

**Language/Version**: TypeScript ~4.9.4 (Angular 16.2 workspace)
**Primary Dependencies**: Angular 16.2 (`@angular/common/http`, DI), Angular Material 16.2,
RxJS ~7.8, `@juice-js/core@1.2.0` (new/upgraded — provides the idempotency
interceptor), `@juice-js/dict-builder` (existing peer)
**Storage**: N/A (client-side library; idempotency keys are per-request, in-memory)
**Testing**: Karma + Jasmine (`ng test`), `HttpClientTestingModule` for interceptor/service tests
**Target Platform**: Modern evergreen browsers (Angular 16 web application libraries)
**Project Type**: Multi-project Angular CLI workspace publishing `@juice-js/*` libraries;
this change is scoped to the `projects/juice-js/tenants` library, exercised by `projects/app`
**Performance Goals**: No measurable regression on request latency; interceptor overhead
negligible (single header addition + key generation per mutating request)
**Constraints**: No breaking change to the `@juice-js/tenants` public API; read (GET)
requests must be byte-for-byte unchanged; interceptor must not attach markers to
non-tenant HTTP traffic in the host application
**Scale/Scope**: One library, one service (`TenantAdminService`, 13 mutating operations),
one module wiring point (`TenantsModule.forRoot`), plus one new configuration option

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Evaluated against Juice Tenants Constitution v1.0.0:

- **I. Library-First & Publishable Modules**: PASS. All changes live in
  `projects/juice-js/tenants` and are surfaced through `public-api.ts` /
  `TenantsModule.forRoot()`. `@juice-js/core` is consumed as a published sibling package
  (declared as a peerDependency, mirroring the existing `@juice-js/dict-builder` peer),
  not by reaching into another library's internals or the `app` project.
- **II. Conventional Commits & Automated Release**: PASS. Delivered as a `feat:` change;
  the dependency bump ships through semantic-release. No manual version edits.
- **III. Test Discipline (NON-NEGOTIABLE)**: PASS (planned). New/updated co-located
  `.spec.ts` cover: marker presence on all mutating operations, absence on GET requests,
  scoping to tenant URLs, and `forRoot` provider registration. `ng test` must be green.
- **IV. Typed Contracts & API Models**: PASS. The new idempotency configuration is a typed
  addition to `TenantConfigurationParams`; no `any` at boundaries.
- **V. Angular & Material Conventions**: PASS. Uses standard `HTTP_INTERCEPTORS` DI
  multi-provider and module `forRoot` provider registration; RxJS subscription semantics
  unchanged; no hard-coded user-facing strings introduced.

**Result**: PASS — no violations. Complexity Tracking table intentionally left empty.

## Project Structure

### Documentation (this feature)

```text
specs/001-idempotency-interceptor/
├── plan.md              # This file (/speckit.plan command output)
├── spec.md              # Feature specification
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   └── tenants-public-api.md
├── checklists/
│   └── requirements.md  # Spec quality checklist (/speckit.specify)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created here)
```

### Source Code (repository root)

```text
projects/juice-js/tenants/                    # The library changed by this feature
├── package.json                              # ADD @juice-js/core@1.2.0 peerDependency
├── src/
│   ├── public-api.ts                         # Re-export new idempotency config type if needed
│   └── lib/
│       ├── tenants.module.ts                 # forRoot(): register idempotency interceptor provider
│       └── shared/
│           ├── tenant-configuration.ts       # ADD optional idempotency config fields
│           ├── services/
│           │   ├── tenant-admin.service.ts   # No per-call change (central interceptor)
│           │   └── tenant.service.spec.ts     # Existing service spec (extend as needed)
│           └── idempotency/                   # NEW: wiring + tests for the interceptor integration
│               └── idempotency.provider.spec.ts

projects/app/                                  # Consumer/demo host used to exercise the change
└── src/app/app.module.ts                      # TenantsModule.forRoot(tenantOptions) — verify wiring
```

**Structure Decision**: Multi-project Angular workspace. This feature is confined to the
`projects/juice-js/tenants` library (its module, configuration, and tests), with
`projects/app` used only to validate end-to-end behavior. No new top-level projects are
introduced, consistent with Principle I (Library-First).

## Complexity Tracking

> No constitution violations. Table intentionally empty.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| —         | —          | —                                   |
