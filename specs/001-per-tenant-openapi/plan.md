# Implementation Plan: Per-Tenant Native OpenAPI Support

**Branch**: `001-per-tenant-openapi` | **Date**: 2026-05-14 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-per-tenant-openapi/spec.md`

## Summary

Add a `AddPerTenantOpenApi` extension method to `Juice.Extensions.MultiTenant.AspNetCore`
that wires up `Microsoft.AspNetCore.OpenApi` (the native ASP.NET Core stack) with
per-tenant document customization — mirroring the existing `AddPerTenantSwaggerGen`
pattern but targeting the native stack available from net8.0 onwards. The two
integrations MUST coexist without DI conflicts. All new code lives in a new `OpenApi/`
subfolder inside the existing project; no new project is added to the solution.

## Technical Context

**Language/Version**: C# 12 / .NET 8, 9, 10 (net8.0, net9.0, net10.0)
**Primary Dependencies**: Microsoft.AspNetCore.OpenApi (native, per-framework version),
Finbuckle.MultiTenant.AspNetCore, internal Juice packages
**Storage**: N/A — document generation only, no persistence
**Testing**: xUnit v3 via `Juice.MultiTenant.Tests`; integration demonstrated in
`Juice.MultiTenant.Host`
**Target Platform**: ASP.NET Core server (library — consumed by host applications)
**Project Type**: Library extension (DI extension methods + IConfigureOptions wrappers)
**Performance Goals**: Document generation latency must be in the same order of magnitude
as the equivalent Swashbuckle endpoint under identical load
**Constraints**: Must not break existing net8.0/net9.0 builds; net6.0 is explicitly
excluded. New code guarded by `NET8_0_OR_GREATER`.
**Scale/Scope**: Small — approximately 3–5 new C# files inside one existing project

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Domain-Driven Tenant Lifecycle | ✅ Pass | Feature is infrastructure/DI only; no tenant aggregate mutations |
| II. Dual-Interface Contract (REST + gRPC) | ✅ Pass | OpenAPI doc endpoint is not a tenant management operation; no gRPC equivalent required |
| III. Multi-Provider Database Portability | ✅ Pass | No DB involvement |
| IV. Outbox-Backed Integration Events | ✅ Pass | No integration events emitted |
| V. Simplicity & YAGNI | ✅ Pass | No new project; new folder inside existing project; mirrors established pattern |
| Tech Constraint: Target Frameworks | ✅ Pass | Scoped to net8.0/net9.0/net10.0; guarded by `NET8_0_OR_GREATER` |

**Gate result**: All principles pass. Phase 0 research approved.

## Project Structure

### Documentation (this feature)

```text
specs/001-per-tenant-openapi/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── extension-api.md
└── tasks.md             # Phase 2 output (/speckit-tasks command)
```

### Source Code (repository root)

```text
src/Juice.Extensions.MultiTenant.AspNetCore/
├── SwaggerGen/                          # EXISTING — unchanged
│   ├── ConfigureSchemaGeneratorOptions.cs
│   ├── ConfigureSwaggerGeneratorOptions.cs
│   ├── DocumentProvider.cs
│   └── SwaggerGenServiceCollectionExtensions.cs
└── OpenApi/                             # NEW
    ├── OpenApiServiceCollectionExtensions.cs   # AddPerTenantOpenApi<TTenant>()
    ├── PerTenantOpenApiOptions.cs              # Options holder
    └── ConfigurePerTenantOpenApiOptions.cs     # IConfigureNamedOptions<OpenApiOptions>

test/Juice.MultiTenant.Host/
└── Program.cs                           # MODIFIED — add AddPerTenantOpenApi demo
```

**Structure Decision**: Single-project extension (no new .csproj). The `OpenApi/` folder
is a direct parallel to `SwaggerGen/` — same project, same DI pattern, same tenant-aware
configurator approach.

## Complexity Tracking

> No constitution violations — table not required.
