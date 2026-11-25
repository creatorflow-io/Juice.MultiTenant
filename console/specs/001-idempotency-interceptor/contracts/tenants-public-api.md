# Contract: `@juice-js/tenants` Public API (post-change)

**Feature**: `001-idempotency-interceptor` | **Date**: 2026-07-12

The tenants library's external contract is its published Angular module surface. This feature
MUST NOT introduce breaking changes (FR-008). The only additions are **optional**.

---

## 1. `TenantsModule.forRoot(params)` — unchanged signature, additive input

```ts
// public-api.ts (unchanged exports; TenantConfigurationParams gains an optional field)
TenantsModule.forRoot(params: TenantConfigurationParams): ModuleWithProviders<TenantsModule>
```

**Behavioral contract (new)**:
- When `params.enableIdempotency` is `undefined` or `true`, `forRoot()` registers the
  idempotency interceptor provider (sourced from `@juice-js/core`) in the module providers.
- When `params.enableIdempotency` is `false`, no idempotency provider is registered and
  behavior is identical to the previous release.
- Registration MUST NOT alter existing providers (`TenantAdminService`,
  `TenantConfiguration`) or the module's declarations/exports.

**Compatibility**: Existing callers `TenantsModule.forRoot(tenantOptions)` continue to compile
and run unchanged; idempotency defaults ON (SC-004).

---

## 2. `TenantConfigurationParams` — additive optional field

```ts
export interface TenantConfigurationParams {
  apiEndpoint?: string;
  apiVersion?: string;
  dialogWidth?: string;
  dialogMaxHeight?: string;
  enableIdempotency?: boolean; // NEW — optional, defaults to true
}
```

- Adding an optional property is backward compatible; no existing consumer object becomes
  invalid.

---

## 3. `TenantAdminService` — unchanged public surface

- No method signatures change. No new required parameters.
- Mutating methods gain the idempotency marker via the interceptor, transparently.
- Return types (`Observable<...>`) are unchanged.

**Contract test obligations** (verified under `HttpClientTestingModule`):

| Assertion | Requirement |
|-----------|-------------|
| Each mutating method's outgoing request carries an idempotency marker | FR-002 / SC-003 |
| Each read method's outgoing request carries NO marker | FR-005 / SC-003 |
| A retried mutating request carries the same marker | FR-003 / SC-001 |
| Two distinct mutating actions carry different markers | FR-004 |
| A mutating request to a non-tenant URL is not tagged | R2 scoping |
| `forRoot()` registers the interceptor when enabled; not when `enableIdempotency:false` | Contract §1 |

---

## 4. Dependency contract

- `@juice-js/tenants` declares `@juice-js/core@1.2.0` as a **peerDependency**.
- Consumers MUST provide a compatible `@juice-js/core` in their app (documented in quickstart).
- No change to the existing `@juice-js/dict-builder` peer.

---

## Non-goals (explicitly out of contract)

- The wire header name / format of the idempotency marker (owned by `@juice-js/core`).
- Server-side de-duplication semantics (backend responsibility).
- Idempotency for any package other than `@juice-js/tenants`.
