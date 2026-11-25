# Phase 0 Research: Idempotency-Safe Tenant Operations

**Feature**: `001-idempotency-interceptor` | **Date**: 2026-07-12

This document resolves the open questions from the Technical Context. Where the exact
`@juice-js/core@1.2.0` API cannot be inspected ahead of the upgrade, the decision
records the assumed shape (based on the package's existing `Module.forRoot()` conventions
and Angular interceptor idioms) and the fallback if the assumption is wrong.

> **Post-implementation update (confirmed API)**: The installed `1.2.0` package uses an
> **opt-in-per-request** model — `IdempotencyModule` registers the interceptor and
> `withIdempotency(operationId)` opts a request in via `HttpContext`. This supersedes the
> auto verb/URL-tagging assumption in R1/R2 below (R2 scoping is now inherent; R3 stability
> comes from stable operation ids). See `tasks.md` → "Implementation Notes / Deviations" for
> the authoritative record of what shipped.

---

## R1. Public API shape of the `@juice-js/core` idempotency interceptor

**Unknown**: The currently installed `@juice-js/core@1.0.0-alpha.10` exposes no idempotency
interceptor (`public-api` surfaces only tenant/menu/routing/extensions/dialog). The target
`1.2.0` is expected to add it, but its exact export is not yet known.

**Decision**: Integrate against whichever of these the upgraded package exposes, in order of
preference:
1. A DI-friendly provider factory (e.g. `provideIdempotency(options)` or an
   `IdempotencyModule.forRoot(options)`) — preferred, wired inside `TenantsModule.forRoot()`.
2. An exported `IdempotencyInterceptor` class implementing `HttpInterceptor` — registered by
   the tenants library via an `HTTP_INTERCEPTORS` multi-provider.

**Rationale**: The `@juice-js/*` packages consistently ship an NgModule with a static
`forRoot()` that registers providers (`TenantsModule.forRoot`, `AuthModule.forRoot`,
`LayoutModule.forRoot`, `TenantModule.forRoot`). An idempotency feature most likely follows
the same idiom. Registering through the tenants library's own `forRoot()` keeps consumers
unchanged (Principle VIII of the spec: no breaking API change) and satisfies FR-006 (central,
opt-in-free wiring).

**Alternatives considered**:
- Requiring the host app to register core's interceptor directly in `AppModule` — rejected:
  pushes wiring responsibility onto every consumer and risks the tenant library silently
  losing idempotency if a consumer forgets.
- Manually attaching the header inside each `TenantAdminService` method — rejected: violates
  FR-006 (new operations would not be covered by default) and duplicates logic.

**Action for planning**: The first implementation task inspects the real `1.2.0`
type definitions (`node_modules/@juice-js/core/public-api.d.ts`) after the version bump and
confirms which registration form is available before wiring. This is a small, contained
branch point, not a scope risk.

---

## R2. Scoping the interceptor to tenant traffic only

**Unknown**: Angular `HTTP_INTERCEPTORS` are injector-global — once provided in the root
injector they see every `HttpClient` request in the host application, including non-tenant
requests. FR-005 requires read requests to be unaffected and the spec constrains the change
so it must not attach markers to non-tenant traffic.

**Decision**: Two-layer scoping.
1. **Method filter**: only requests using state-changing HTTP methods
   (`POST`, `PUT`, `PATCH`, `DELETE`) receive a marker; `GET`/`HEAD`/`OPTIONS` pass through
   untouched. This directly satisfies FR-005 (all tenant reads are GET).
2. **URL/endpoint filter**: only requests whose URL targets the configured tenant
   `apiEndpoint` receive a marker, so unrelated application POST/PUTs are not tagged.

If core's interceptor exposes a predicate/URL-allowlist option, configure it. If it does not,
the tenants library wires a thin adapter provider that applies the `apiEndpoint` predicate
before delegating to core's interceptor.

**Rationale**: Matches FR-002 (all mutating tenant ops), FR-005 (reads untouched), and the
constraint against tagging non-tenant traffic. The `apiEndpoint` is already available on
`TenantConfiguration`, so the URL filter needs no new required config.

**Alternatives considered**:
- Global tagging of all mutating requests app-wide — rejected: over-reaches beyond the
  tenant capability and could interfere with other backends' idempotency semantics.
- Per-request opt-in via a custom HTTP header/context token set in each service method —
  rejected: violates FR-006 (central wiring, future ops covered by default).

---

## R3. Idempotency key generation and retry-stability

**Unknown**: FR-003 requires the same marker across automatic retries of one action; FR-004
requires distinct markers for distinct actions. Angular interceptors re-run on each retry of
the underlying request, which naively would generate a new key per attempt.

**Decision**: Rely on core's interceptor to generate one key per logical request and keep it
stable across transport-level retries (the expected behavior of an idempotency interceptor).
The key is a UUID-style unique value. Where RxJS retry operators re-subscribe and cause the
interceptor to re-run, the intended mechanism is that the key is derived once at request
creation (e.g., cached on the immutable request or via an `HttpContext` token) rather than
per interceptor invocation.

**Rationale**: This is the defining correctness property of an idempotency interceptor and is
the reason to consume core's implementation rather than hand-roll one. Verified by test rather
than assumed (see quickstart): a request replayed/retried must carry an identical marker.

**Alternatives considered**:
- Generating the key in `TenantAdminService` and threading it through each call — rejected:
  reintroduces per-call wiring (violates FR-006) and duplicates core's responsibility.

**Open verification** (resolved in test, not a blocker): confirm core keeps the key stable
under an RxJS `retry()`; if it does not, the tenants library sets the key via `HttpContext`
at the service layer's HTTP factory as a fallback. This is an internal detail with no public
API impact.

---

## R4. How to declare the `@juice-js/core` dependency

**Unknown**: Should `@juice-js/core@1.2.0` be a `dependencies` or `peerDependencies`
entry of the `@juice-js/tenants` library?

**Decision**: Declare it as a **peerDependency** (`"@juice-js/core": "1.2.0"`) in
`projects/juice-js/tenants/package.json`, and add it to the workspace root `package.json`
`dependencies` so the demo `app` and tests resolve it.

**Rationale**: The tenants library already declares its sibling `@juice-js/dict-builder` as a
peerDependency. `@juice-js/core` is a shared singleton-style Angular library (services/
interceptors registered in the root injector); making it a peer avoids duplicate copies in
consumer apps and matches the established `@juice-js/auth → @juice-js/layout` peer pattern.

**Alternatives considered**:
- Bundling core as a direct `dependencies` of the library — rejected: risks multiple Angular
  library instances in the consumer's injector and diverges from the workspace convention.

---

## R5. Testing strategy for interceptor behavior

**Unknown**: How to verify marker presence/absence deterministically under Karma/Jasmine.

**Decision**: Use `HttpClientTestingModule` + `HttpTestingController`. Tests assert:
- every mutating `TenantAdminService` method (create/update/delete/activate/deactivate/
  reactivate/suspend/approve/reject/abandon/settings/properties/root-settings) issues a
  request carrying the idempotency marker;
- every read method (getTenants/getTenant/getSummary/getTenantSettings/getRootSettings)
  issues a request with **no** marker;
- a retried request carries the **same** marker (retry-stability);
- a request to a non-tenant URL is not tagged (scoping);
- `TenantsModule.forRoot()` registers the interceptor provider.

**Rationale**: Satisfies Principle III and gives measurable evidence for SC-001, SC-003, and
SC-005 without a live backend.

**Alternatives considered**: e2e against a real API — rejected as unnecessary for this layer
and outside the library's unit-test discipline.

---

## Summary of resolved unknowns

| # | Unknown | Resolution |
|---|---------|------------|
| R1 | Core interceptor export shape | Prefer `forRoot`/provider factory, fall back to `HTTP_INTERCEPTORS` class; confirm against real `.d.ts` post-upgrade |
| R2 | Interceptor scoping | Method filter (mutating verbs) + `apiEndpoint` URL filter |
| R3 | Key generation & retry stability | Delegate to core; verify stability by test; `HttpContext` fallback |
| R4 | Dependency declaration | peerDependency in the library + root `dependencies` for app/tests |
| R5 | Test approach | `HttpClientTestingModule` assertions on marker presence/absence/stability/scope |

No remaining NEEDS CLARIFICATION items. Ready for Phase 1.
