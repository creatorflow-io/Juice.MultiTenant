# Data Model: Per-Tenant Native OpenAPI Support

**Feature**: 001-per-tenant-openapi
**Date**: 2026-05-14

> No persistent entities are introduced. This feature is a DI/configuration layer only.
> The "entities" below are the in-memory configuration objects that flow through the
> options pipeline.

---

## PerTenantOpenApiOptions\<TTenant\>

**Purpose**: Holds the caller-supplied per-tenant setup action and is registered as a
singleton in DI. Consumed by `ConfigurePerTenantOpenApiOptions` at document-generation
time.

| Field | Type | Description |
|-------|------|-------------|
| `SetupAction` | `Action<OpenApiOptions, TTenant?>` | Invoked once per document-generation request with the resolved tenant (null if no tenant). |
| `ServiceScopeFactory` | `IServiceScopeFactory` | Captured at registration time to enable scoped tenant resolution during singleton configure calls. |

**Constraints**:
- `TTenant` MUST implement `ITenantInfo` (Finbuckle contract).
- `SetupAction` MUST be idempotent — it may be called multiple times (once per document
  name per request).

---

## ConfigurePerTenantOpenApiOptions\<TTenant\>

**Purpose**: Implements `IConfigureNamedOptions<OpenApiOptions>`. Called by the ASP.NET
Core options infrastructure once per named document (e.g., `"v1"`, `"v2"`) when the
`/openapi/{name}.json` endpoint is hit.

**State transitions / flow**:

```
HTTP GET /openapi/v1.json
  └─> ASP.NET Core resolves IConfigureNamedOptions<OpenApiOptions>("v1")
        └─> ConfigurePerTenantOpenApiOptions.Configure("v1", options)
              ├─ Opens IServiceScope
              ├─ Resolves IMultiTenantContextAccessor<TTenant>
              ├─ Extracts current TTenant? from context
              └─ Calls SetupAction(options, tenant)
```

**Invariants**:
- The configure call MUST NOT throw when no tenant is resolved — tenant is passed as
  `null` and the setup action is still called (allowing a default document).
- Scope MUST be disposed after each configure call to avoid resource leaks.

---

## Relationship to Existing Types

```
AddPerTenantSwaggerGen<TTenant>()          AddPerTenantOpenApi<TTenant>()
        │                                           │
        ▼                                           ▼
ConfigureSwaggerGeneratorOptions<T>    ConfigurePerTenantOpenApiOptions<T>
  (IConfigureOptions<SwaggerGen...>)     (IConfigureNamedOptions<OpenApiOptions>)
        │                                           │
        └──── both resolve TTenant via ─────────────┘
              IMultiTenantContextAccessor<TTenant>
```

Both configurator types are independent — they do not share base classes or state.
