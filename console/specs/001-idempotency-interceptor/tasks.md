---

description: "Task list for Idempotency-Safe Tenant Operations"
---

# Tasks: Idempotency-Safe Tenant Operations

**Input**: Design documents from `/specs/001-idempotency-interceptor/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/tenants-public-api.md, quickstart.md

**Tests**: INCLUDED. The spec defines per-story independent tests and research R5 mandates a
Karma/Jasmine + `HttpClientTestingModule` strategy; the constitution makes test discipline
NON-NEGOTIABLE.

**Organization**: Tasks grouped by user story (US1 → US2 → US3) for independent implementation
and verification. A single central interceptor (wired in Phase 2) serves all stories; each
story phase then verifies its own slice of behavior.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: US1, US2, US3
- All paths are repo-relative to `D:\Workspaces\Juice\juice-tenants`

## Path Conventions

Multi-project Angular workspace. Library under `projects/juice-js/tenants/`; demo host under
`projects/app/`. New tests are co-located `.spec.ts` files (Karma/Jasmine).

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Bring in and verify the upgraded `@juice-js/core` dependency.

- [X] T001 Bump `@juice-js/core` to `1.2.0` (exact) in the workspace root `dependencies` in `package.json`
- [X] T002 [P] Add `"@juice-js/core": "^1.2.0"` to `peerDependencies` in `projects/juice-js/tenants/package.json`
- [X] T003 Ran `npm install @juice-js/core@1.2.0`; inspected `node_modules/@juice-js/core/types/juice-js-core.d.ts`. **API confirmed (differs from research R1 assumption)**: the package exports `IdempotencyModule` (registers the interceptor), `IdempotencyInterceptor`, `withIdempotency(operationId, maxAgeMs?, context?)` (builds an opt-in `HttpContext`), `IDEMPOTENCY_OPERATION` token, and `IdempotencyKeyService`. Idempotency is **opt-in per request via `HttpContext`**, not auto verb/URL tagging — so no `idempotency.provider.ts` predicate file is needed. See Implementation Notes below.

**Checkpoint**: ✅ Dependency resolved (`1.2.0`); real interceptor API confirmed. Peer warning: `@juice-js/layout@1.0.0-alpha.10` still expects core `1.0.0-alpha.10` (non-fatal; layout is unaffected by this feature).

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Central interceptor wiring + configuration that ALL user stories depend on.

**⚠️ CRITICAL**: No user story verification can pass until this phase is complete.

- [X] T004 [P] Added optional `enableIdempotency?: boolean` to both `TenantConfiguration` and `TenantConfigurationParams` in `projects/juice-js/tenants/src/lib/shared/tenant-configuration.ts` (default: enabled unless explicitly `false`; additive/non-breaking)
- [X] T005 **(revised for real API)** Instead of a URL/verb predicate provider, opt-in happens per state-changing method in `projects/juice-js/tenants/src/lib/shared/services/tenant-admin.service.ts` via a private `idempotent(operationId)` helper returning `{ context: withIdempotency(operationId) }` (or `{}` when disabled). This inherently scopes tagging to tenant requests (research R2 concern resolved — non-tenant traffic never opts in). No `idempotency.provider.ts` created.
- [X] T006 Registered the interceptor by importing `IdempotencyModule` from `@juice-js/core` in `TenantsModule.imports` (`projects/juice-js/tenants/src/lib/tenants.module.ts`). The interceptor is inert for non-opted-in requests, so opt-out is enforced at the service via `enableIdempotency`.
- [X] T007 [P] No new public type introduced beyond the additive `TenantConfigurationParams` field (already exported); `public-api.ts` unchanged.

**Checkpoint**: ✅ Interceptor registered; mutating tenant requests opt in and carry `Idempotency-Key`, reads do not.

---

## Phase 3: User Story 1 - Safe retries of tenant creation (Priority: P1) 🎯 MVP

**Goal**: Retried tenant-creation requests carry a stable idempotency marker so only one tenant is created.

**Independent Test**: `createTenant` issues a request with a marker; a retried create carries the same marker (verified via `HttpTestingController`).

### Tests for User Story 1

> Write these FIRST and ensure they FAIL before/while implementing Phase 2 wiring.

- [X] T008 [P] [US1] Created `projects/juice-js/tenants/src/lib/shared/idempotency/idempotency-create.spec.ts` asserting `createTenant` issues a POST carrying an `Idempotency-Key` header and a payload-scoped operation id `create-tenant:<identifier>` (via `HttpClientTestingModule` + `HttpTestingController` + live `IdempotencyModule`)
- [X] T009 [P] [US1] Same spec asserts retry-stability (same operation id → same key via `IdempotencyKeyService.getKey`) and distinct creates → distinct keys (FR-003/FR-004)

### Implementation for User Story 1

- [X] T010 [US1] `createTenant` opts in with `create-tenant:${tenant.identifier}` — a payload-derived, retry-stable operation id (research R3). No `HttpContext` fallback needed: core's `IdempotencyKeyService` keeps the key stable per operation id. Tests green.

**Checkpoint**: US1 passes independently — creation is idempotent and retry-stable (SC-001, SC-002).

---

## Phase 4: User Story 2 - Safe retries of tenant state changes (Priority: P2)

**Goal**: All non-create mutating tenant operations carry a unique, retry-stable marker.

**Independent Test**: Each of update/delete/activate/deactivate/reactivate/suspend/approve/reject/abandon/settings/properties/root-settings issues a request with a marker; distinct actions get distinct markers.

### Tests for User Story 2

- [X] T011 [P] [US2] Created `projects/juice-js/tenants/src/lib/shared/idempotency/idempotency-mutations.spec.ts` asserting all 12 mutating methods carry an `Idempotency-Key` header and the expected `<action>-tenant:<id>` operation id: `updateTenant`, `deleteTenant`, `activateTenant`, `deactivateTenant`, `reactivateTenant`, `suspendTenant`, `approveTenant`, `rejectTenant`, `abandonTenant`, `updateTenantSettings`, `updateTenantProperties`, `updateRootSettings`
- [X] T012 [P] [US2] Same spec asserts two distinct targets (`suspend acme` vs `suspend globex`) produce different operation ids (FR-004)

### Implementation for User Story 2

- [X] T013 [US2] All 12 mutating methods in `tenant-admin.service.ts` opt in via the `idempotent(...)` helper with semantic, target-scoped operation ids. Verified by T011/T012 (all green).

**Checkpoint**: US1 AND US2 pass independently — all mutating operations are idempotent (SC-001, SC-003).

---

## Phase 5: User Story 3 - Read operations remain unaffected (Priority: P3)

**Goal**: Read (GET) requests and non-tenant traffic are never tagged.

**Independent Test**: `getTenants`/`getTenant`/`getSummary`/`getTenantSettings`/`getRootSettings` issue requests with no marker; a mutating request to a non-tenant URL is not tagged.

### Tests for User Story 3

- [X] T014 [P] [US3] Created `projects/juice-js/tenants/src/lib/shared/idempotency/idempotency-reads.spec.ts` asserting each read method (`getTenants`, `getTenant`, `getSummary`, `getTenantSettings`, `getRootSettings`) issues a GET with **no** `Idempotency-Key` header and a null `IDEMPOTENCY_OPERATION` context (FR-005)
- [X] T015 [P] [US3] Same spec asserts a non-opted-in request to a foreign URL (`https://other.test/things`) is **not** tagged — proving the interceptor only affects opt-in traffic (scoping, research R2 resolved)

### Implementation for User Story 3

- [X] T016 [US3] Reads simply never call `withIdempotency`, so no context/header is attached; the interceptor's own `isUnsafe` check further guards non-mutating methods. Verified by T014/T015 (green).

**Checkpoint**: All three stories pass independently — reads unchanged (SC-005), scoping correct.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Config opt-out coverage, build/release gates, and validation.

- [X] T017 [P] Opt-out test added in `idempotency-reads.spec.ts` (revised location): with `enableIdempotency: false`, `suspendTenant` issues a request with **no** `Idempotency-Key` header and null context (contract §1, adapted — opt-out is enforced at the service since the interceptor is inert without opt-in)
- [X] T018 Ran `ng test @juice-js/tenants --watch=false --browsers=ChromeHeadless` → **32 of 32 SUCCESS** (24 new idempotency specs + 8 pre-existing) (Principle III gate)
- [X] T019 Ran `ng build @juice-js/tenants` → built successfully with the new peer dependency; no public-API break (FR-008 / SC-004)
- [ ] T020 [P] **Pending manual verification** — the quickstart e2e (`npm start`) needs local SSL certs and a live backend at `https://localhost:44314`, which are not available in this environment. Unit tests cover the behavior at the HTTP layer; the live double-submit/retry demo remains for the developer to run.
- [ ] T021 **Not done (awaiting user)** — no commit made; the change is staged in the working tree only. Suggested Conventional Commit: `feat(tenants): add @juice-js/core 1.2.0 idempotency for state-changing tenant requests`.

---

## Implementation Notes / Deviations from the original plan

The real `@juice-js/core@1.2.0` API (confirmed in T003) differs from the research R1/R2/R3
assumptions, which were written before the package could be inspected:

- **Opt-in, not auto-tagging**: Idempotency is opt-in per request via
  `withIdempotency(operationId)` on the request's `HttpContext`. There is no verb/URL
  predicate interceptor. Consequently **no `idempotency.provider.ts` was created**; the
  opt-in lives in `TenantAdminService` (one `idempotent()` helper + a call per mutating
  method).
- **Scoping (R2) is inherent**: because only tenant service calls opt in, non-tenant traffic
  is never tagged — the `apiEndpoint` URL filter is unnecessary.
- **Retry-stability (R3)**: achieved via stable, semantic operation ids
  (`create-tenant:<identifier>`, `<action>-tenant:<id>`) rather than an `HttpContext` fallback.
  `IdempotencyKeyService` returns the same key per operation id until it settles.
- **FR-006 nuance**: wiring is central (single `IdempotencyModule` import + one helper), but a
  *future* mutating method must remember to call `this.idempotent(...)`. This is the intended
  usage of the package and the only option it offers; noted as a maintenance convention.
- **Opt-out (contract §1)**: enforced at the service (`enableIdempotency: false` ⇒ no context
  attached; interceptor stays registered but inert), rather than by conditionally registering
  the interceptor.
- **Bonus behavior gained for free**: core's interceptor also handles `409` in-progress
  retries, `422` conflict / `400` required as typed errors, and double-submit coalescing.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately. T001 → T003 sequential (install after bump); T002 [P] alongside T001.
- **Foundational (Phase 2)**: Depends on Phase 1 (needs the confirmed interceptor API). BLOCKS all user stories. T004/T007 [P]; T005 before T006.
- **User Stories (Phase 3–5)**: All depend on Phase 2. Because wiring is central, once Phase 2 is done the stories are largely verification slices and can be validated in parallel.
- **Polish (Phase 6)**: Depends on all targeted stories complete.

### User Story Dependencies

- **US1 (P1)**: After Phase 2. Independently testable. MVP.
- **US2 (P2)**: After Phase 2. Independent of US1 (different methods/spec file).
- **US3 (P3)**: After Phase 2. Independent of US1/US2 (reads + scoping, separate spec file).

### Within Each User Story

- Tests written first and expected to fail until Phase 2 wiring lands.
- Implementation tasks only refine the shared predicate/fallback if a test fails.

### Parallel Opportunities

- T001 + T002 (Phase 1).
- T004 + T007 (Phase 2).
- Test authoring across stories: T008/T009 (US1), T011/T012 (US2), T014/T015 (US3) are in
  separate files → fully parallelizable once Phase 2 lands.
- T017 + T020 (Phase 6).

---

## Parallel Example: User Story test authoring (post-Phase 2)

```bash
# Separate spec files → run/author in parallel:
Task: "US1 create-idempotency tests in .../idempotency/idempotency-create.spec.ts"
Task: "US2 mutation-idempotency tests in .../idempotency/idempotency-mutations.spec.ts"
Task: "US3 read/scoping tests in .../idempotency/idempotency-reads.spec.ts"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Phase 1: Setup (bump + verify core API).
2. Phase 2: Foundational (central interceptor wiring + config).
3. Phase 3: US1 — creation idempotent and retry-stable.
4. **STOP and VALIDATE**: run US1 specs; demo single-create-on-retry.

### Incremental Delivery

1. Setup + Foundational → interceptor live.
2. US1 → validate → MVP (SC-001, SC-002).
3. US2 → validate → all mutating ops covered (SC-003).
4. US3 → validate → reads/scoping confirmed (SC-005).
5. Polish → opt-out test, build/release gates, quickstart, Conventional Commit.

---

## Notes

- The interceptor is central (Phase 2), so most story-phase "implementation" tasks are
  confirmations that refine the shared predicate/fallback only if a test fails — this is the
  nature of a cross-cutting interceptor and keeps stories independently *verifiable*.
- All additions are optional/additive to preserve the public API (FR-008).
- Do not tag GET requests or non-`apiEndpoint` traffic.
- Commit after each phase; keep the final commit Conventional-Commit compliant for
  semantic-release.
