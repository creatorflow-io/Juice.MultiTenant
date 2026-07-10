# Feature Specification: Per-Tenant Native OpenAPI Support

**Feature Branch**: `001-per-tenant-openapi`
**Created**: 2026-05-14
**Status**: Draft
**Input**: User description: "Support per-tenant openapi natively beside swaggergen"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Register Native OpenAPI Per-Tenant (Priority: P1)

A developer building a multi-tenant ASP.NET Core application wants to use the built-in
ASP.NET Core OpenAPI document generation (Microsoft.AspNetCore.OpenApi) with per-tenant
customization, instead of or alongside the existing Swashbuckle/SwaggerGen integration.
They call a new extension method (e.g., `AddPerTenantOpenApi`) during service registration
and get tenant-aware OpenAPI document endpoints without pulling in Swashbuckle.

**Why this priority**: This is the core deliverable. Without it, the feature does not exist.
It unblocks .NET 9+ users who prefer the native stack over Swashbuckle.

**Independent Test**: A test host registers `AddPerTenantOpenApi`, resolves a tenant,
requests `/openapi/v1.json`, and receives a document whose `info.title` (or other field)
reflects the resolved tenant — verifiable without Swashbuckle present.

**Acceptance Scenarios**:

1. **Given** a multi-tenant app with `AddPerTenantOpenApi` registered and tenant "alpha"
   resolved, **When** a client requests the OpenAPI document endpoint, **Then** the
   returned document contains tenant-specific metadata (e.g., title, description, or
   security scheme) set via the per-tenant setup action.

2. **Given** no tenant is resolved (anonymous / fallback context), **When** a client
   requests the OpenAPI document endpoint, **Then** the document is generated using the
   default (non-tenant) configuration without error.

3. **Given** two tenants "alpha" and "beta" with different configured titles, **When**
   each tenant requests the document, **Then** each receives their respective document
   content.

---

### User Story 2 - Coexist with Existing SwaggerGen Integration (Priority: P2)

A developer who already uses `AddPerTenantSwaggerGen` wants to also enable the native
OpenAPI endpoint (e.g., for .NET tooling or Scalar UI) in the same application without
the two integrations interfering with each other.

**Why this priority**: The feature description explicitly says "beside swaggergen" —
coexistence is a stated requirement, not an afterthought.

**Independent Test**: A test host registers both `AddPerTenantSwaggerGen` and
`AddPerTenantOpenApi`. Both document endpoints respond correctly for a resolved tenant,
and neither throws at startup or document-generation time.

**Acceptance Scenarios**:

1. **Given** both integrations registered, **When** the application starts, **Then**
   no conflict or startup error is thrown.

2. **Given** both integrations registered and tenant "alpha" resolved, **When** each
   respective document endpoint is requested, **Then** both return valid, tenant-scoped
   OpenAPI documents independently.

---

### User Story 3 - Multi-Version Document Support (Priority: P3)

A developer wants to expose multiple API versions (e.g., `v1`, `v2`) as separate
per-tenant OpenAPI documents using the native integration, mirroring the multi-document
capability already available in `AddPerTenantSwaggerGen`.

**Why this priority**: Multi-version document support is expected by API consumers and
is already an established pattern in the existing Swashbuckle integration.

**Independent Test**: A test host registers two OpenAPI documents (`v1`, `v2`) via
`AddPerTenantOpenApi`. Both document endpoints respond with tenant-scoped content for
a resolved tenant.

**Acceptance Scenarios**:

1. **Given** `v1` and `v2` documents registered and tenant "alpha" resolved, **When**
   the `v1` document endpoint is requested, **Then** only v1 operations are present.

2. **Given** the same setup, **When** the `v2` document endpoint is requested, **Then**
   only v2 operations are present.

---

### Edge Cases

- What happens when the tenant store is unavailable during document generation? The
  document should fall back to the default (non-tenant) configuration without throwing.
- What if the per-tenant setup action throws? The exception must surface clearly so
  developers can diagnose misconfiguration — it must not be silently swallowed.
- What if `AddPerTenantOpenApi` is called on net6.0 where native OpenAPI is not
  available? The application must fail at startup with a clear, actionable error message.
- What if a future net10.0+ API changes the `OpenApiOptions` surface? The integration
  must compile cleanly across net8.0, net9.0, and net10.0 without conditional workarounds
  visible to callers.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a new extension method that registers per-tenant native
  OpenAPI document generation, accepting a per-tenant setup action consistent with the
  existing `AddPerTenantSwaggerGen` pattern.
- **FR-002**: The OpenAPI document endpoint MUST return tenant-specific content when a
  tenant is resolved in the current request context.
- **FR-003**: System MUST support registering multiple named OpenAPI documents (e.g.,
  `v1`, `v2`) per application.
- **FR-004**: The native OpenAPI integration MUST coexist with `AddPerTenantSwaggerGen`
  in the same application without conflicts.
- **FR-005**: When no tenant is resolved, the document endpoint MUST return the default
  OpenAPI document without error.
- **FR-006**: The feature MUST be scoped to net8.0, net9.0, and net10.0 targets only;
  net6.0 is explicitly out of scope.
- **FR-007**: All new public types and extension methods MUST follow the naming
  conventions and project layout established in `Juice.Extensions.MultiTenant.AspNetCore`.

### Key Entities

- **PerTenantOpenApiOptions**: Holds the per-tenant setup action and baseline
  configuration; consumed by the options configurator at document-generation time.
- **ConfigurePerTenantOpenApiOptions**: An `IConfigureNamedOptions<OpenApiOptions>`
  implementation that applies tenant-specific configuration when a document is requested.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A developer can enable per-tenant native OpenAPI support with a single
  extension method call — no more steps than the equivalent `AddPerTenantSwaggerGen` setup.
- **SC-002**: Both `AddPerTenantSwaggerGen` and `AddPerTenantOpenApi` can be active
  simultaneously in the same process with zero startup errors on net8.0, net9.0, and net10.0.
- **SC-003**: Per-tenant OpenAPI document endpoints respond correctly for all registered
  tenants and for the no-tenant fallback case.
- **SC-004**: The test host (`Juice.MultiTenant.Host`) demonstrates a working end-to-end
  configuration with both integrations enabled side by side.

## Assumptions

- `Microsoft.AspNetCore.OpenApi` (the native package, not Swashbuckle) is the target
  library — introduced in .NET 8 and enhanced in .NET 9.
- net6.0 support is out of scope; new code will be guarded by `NET8_0_OR_GREATER`
  preprocessor symbols or project-level multi-targeting exclusion. Targets are net8.0,
  net9.0, and net10.0.
- The per-tenant setup action will receive the resolved tenant (nullable) alongside
  `OpenApiOptions`, consistent with the existing `AddPerTenantSwaggerGen<TTenant>` pattern.
- Swagger UI hosting (Swashbuckle middleware) is out of scope; consumers who want a UI
  can pair native OpenAPI endpoints with Scalar or another UI independently.
- `Juice.Extensions.MultiTenant.AspNetCore` is the correct home for the new code,
  placed in a new `OpenApi/` subfolder parallel to the existing `SwaggerGen/` subfolder.
