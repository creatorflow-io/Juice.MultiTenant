# Research: Per-Tenant Native OpenAPI Support

**Feature**: 001-per-tenant-openapi
**Date**: 2026-05-14

---

## 1. Microsoft.AspNetCore.OpenApi — Native Stack Overview

**Decision**: Use `Microsoft.AspNetCore.OpenApi` as the native document-generation
engine, not Swashbuckle.

**Rationale**: The spec explicitly targets the native ASP.NET Core OpenAPI package
introduced in .NET 8 and substantially enhanced in .NET 9 (async transformers, YAML
output). It is the Microsoft-supported replacement for Swashbuckle in the long term
and is available on all three target frameworks (net8.0, net9.0, net10.0).

**Alternatives considered**:
- *Swashbuckle.AspNetCore*: Already covered by the existing `AddPerTenantSwaggerGen`
  integration; adding it here would be duplication, not "beside".
- *NSwag*: Third-party, not the stated intent.

---

## 2. Per-Request Tenant Scoping for Document Generation

**Decision**: Implement `IConfigureNamedOptions<OpenApiOptions>` backed by a scoped
service factory (`IServiceScopeFactory`) to resolve the current tenant at document-
generation time.

**Rationale**: `OpenApiOptions` configuration runs at startup (singleton scope), but
tenant resolution is inherently request-scoped (via `IMultiTenantContextAccessor`).
The same problem exists in the existing `ConfigureSwaggerGeneratorOptions` for
Swashbuckle, which solves it by capturing a service provider reference and creating
a scope inside `Configure()`. We apply the identical pattern.

**Alternatives considered**:
- *Static per-tenant configuration at startup*: Would require enumerating all tenants
  at startup — not viable for dynamic tenant stores.
- *Middleware-based document override*: Technically feasible but would bypass the
  standard `IConfigureNamedOptions` pipeline and complicate coexistence with the native
  `/openapi/{documentName}.json` endpoint routing.

---

## 3. Framework Version Differences (net8 vs net9 vs net10)

**Decision**: Use conditional compilation (`#if NET9_0_OR_GREATER`) only where the API
surface differs; keep shared code unconditional.

**Rationale**:
- net8.0: `OpenApiOptions` supports synchronous document transformers only
  (`AddDocumentTransformer`, `AddOperationTransformer`).
- net9.0+: Adds async `IOpenApiDocumentTransformer`, schema transformers, and YAML
  support. The `UseOpenApi()` middleware call also differs slightly.
- net10.0: Expected to be API-compatible with net9.0 for the transformer surface;
  no breaking changes anticipated based on current previews.

The `IConfigureNamedOptions<OpenApiOptions>` interface itself is stable across all
three versions, so the configurator class needs no version guards. Guards are only
needed inside the options builder if transformer registration APIs differ.

**Alternatives considered**:
- *Separate class per TFM via MSBuild Condition*: Overkill for one or two method
  call differences; `#if` blocks are cleaner and already the established pattern in
  `DocumentProvider.cs`.

---

## 4. Coexistence with AddPerTenantSwaggerGen

**Decision**: The two registrations are independent DI registrations with no shared
singleton types. `OpenApiOptions` (native) and `SwaggerGenOptions` (Swashbuckle) are
unrelated types, so no collision occurs.

**Rationale**: Examined existing DI registrations in `SwaggerGenServiceCollectionExtensions.cs`.
It registers `ISwaggerProvider`, `ISchemaGenerator`, `SwaggerGeneratorOptions`, and
`SchemaGeneratorOptions` — none of which overlap with the native stack's
`OpenApiOptions` / `OpenApiDocumentService`. Both can be registered simultaneously.

**Risk**: If a host app also calls the standard `builder.Services.AddOpenApi()` (the
non-per-tenant native registration), there may be duplicate `OpenApiOptions` named
instances. Mitigated by: our extension must not call `AddOpenApi()` internally;
instead it should register `OpenApiOptions` directly with a named instance per
document, so the host can still call `AddOpenApi()` independently if desired.

---

## 5. Document Endpoint Routing

**Decision**: Rely on the standard `app.MapOpenApi()` call in the host for endpoint
routing. The extension method does NOT call `MapOpenApi()`.

**Rationale**: Endpoint routing is an application-level concern; the library registers
*options*, not routes. This is consistent with how `AddPerTenantSwaggerGen` works —
the host calls `app.UseSwagger()` / `app.UseSwaggerUI()` separately.

---

## 6. Package Reference Strategy

**Decision**: Add `Microsoft.AspNetCore.OpenApi` as a framework-conditional package
reference in `Juice.Extensions.MultiTenant.AspNetCore.csproj` under
`NET8_0_OR_GREATER` condition (or use `$(OpenApiVersion)` variable via
`Directory.Build.props` consistent with how `$(SwashbuckleVersion)` is managed).

**Rationale**: The project already uses `$(SwashbuckleVersion)` from `Directory.Build.props`
for version management. Adding `$(OpenApiVersion)` per-TFM follows the same pattern
and keeps version pins in one place.

**Version pins** (as of 2026-05-14):
- net8.0 → `Microsoft.AspNetCore.OpenApi` 8.x
- net9.0 → `Microsoft.AspNetCore.OpenApi` 9.x
- net10.0 → `Microsoft.AspNetCore.OpenApi` 10.x

---

## 7. `IDocumentProvider` for Tooling (dotnet-getdocument)

**Decision**: Do NOT implement `IDocumentProvider` for the native stack in this feature.

**Rationale**: `IDocumentProvider` (from `Microsoft.Extensions.ApiDescription.Server`)
is a Swashbuckle-specific integration point used by the `dotnet-getdocument` tool for
design-time document export. The native `Microsoft.AspNetCore.OpenApi` package has its
own separate tooling path (via `dotnet openapi` and endpoint-based export). Adding a
parallel `IDocumentProvider` for the native stack is out of scope for this feature and
would add complexity without clear current need (YAGNI).
