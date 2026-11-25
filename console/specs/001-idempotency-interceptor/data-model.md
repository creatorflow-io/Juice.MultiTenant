# Phase 1 Data Model: Idempotency-Safe Tenant Operations

**Feature**: `001-idempotency-interceptor` | **Date**: 2026-07-12

This feature is client-side and introduces no persisted storage. The "entities" below are the
in-memory / configuration shapes involved in attaching idempotency markers to tenant requests.

---

## Entity: IdempotencyMarker (Idempotency Key)

Represents the unique value attached to one logical state-changing tenant request so the
backend can de-duplicate repeated deliveries.

| Field | Type | Description | Rules |
|-------|------|-------------|-------|
| value | string | Unique key for one logical operation | Non-empty; globally unique per action; UUID-style |

**Lifecycle / rules**:
- Generated once per user-initiated state-changing request (FR-002).
- **Stable** across automatic transport retries of that same request (FR-003).
- **Distinct** across different user-initiated actions (FR-004).
- Carried on the outgoing HTTP request (owned by core's interceptor; the tenants library does
  not define the wire header name — it is core's contract).
- Never generated for read (GET/HEAD/OPTIONS) requests (FR-005).

**Provenance**: Produced by the `@juice-js/core@1.2.0` idempotency interceptor, not by
`TenantAdminService`.

---

## Entity: TenantConfiguration (extended)

Existing runtime configuration for the tenants library, extended with optional idempotency
settings. The additions are **optional** to preserve the public API (FR-008).

Current fields (unchanged): `apiEndpoint`, `apiVersion`, `dialogWidth`, `dialogMaxHeight`.

| New field | Type | Default | Description |
|-----------|------|---------|-------------|
| enableIdempotency | boolean (optional) | `true` | Whether the idempotency interceptor is wired by `forRoot()`. Allows a consumer to opt out. |

Corresponding optional field is added to `TenantConfigurationParams` (the public input type
used by `TenantsModule.forRoot(params)`).

**Validation rules**:
- All new fields optional; omission yields the default behavior (idempotency ON).
- No change to existing required behavior when the field is absent → non-breaking (FR-008).

**Note on scoping input**: The interceptor's URL scope reuses the existing `apiEndpoint`
value (see research R2); no additional required configuration is introduced.

---

## Entity: Tenant Operation (classification)

Not a new type — a classification of existing `TenantAdminService` methods that determines
whether a marker is attached. Drives the acceptance tests.

| Operation | HTTP method | Marker attached? |
|-----------|-------------|------------------|
| getTenants / getTenant / getSummary / getTenantSettings / getRootSettings | GET | No |
| createTenant | POST | Yes |
| updateTenant / abandonTenant / updateTenantSettings / updateTenantProperties / updateRootSettings | PUT | Yes |
| deleteTenant | DELETE | Yes |
| activateTenant / deactivateTenant / reactivateTenant / suspendTenant / approveTenant / rejectTenant | POST | Yes |

**Rule**: Marker attached ⇔ (HTTP method ∈ {POST, PUT, PATCH, DELETE}) ∧ (URL targets the
configured tenant `apiEndpoint`). This mapping is the source of truth for FR-002 and FR-005
and for success criteria SC-003.

---

## Relationships

```text
TenantsModule.forRoot(TenantConfigurationParams)
        │  reads enableIdempotency (default true)
        ▼
registers idempotency interceptor provider (from @juice-js/core)
        │  intercepts HttpClient traffic
        ▼
Tenant Operation (mutating + tenant-scoped URL) ──gets──> IdempotencyMarker
Tenant Operation (read, or non-tenant URL)      ──gets──> (no marker)
```

No state transitions or persistence beyond the request lifetime.
