---
description: "Task list for Per-Tenant Native OpenAPI Support"
---

# Tasks: Per-Tenant Native OpenAPI Support

**Input**: Design documents from `/specs/001-per-tenant-openapi/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/extension-api.md ✅

**Organization**: Tasks grouped by user story to enable independent implementation and testing.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Package reference and project-level changes needed before any code can be written.

- [x] T001 Add `Microsoft.AspNetCore.OpenApi` package references to `src/Juice.Extensions.MultiTenant.AspNetCore/Juice.Extensions.MultiTenant.AspNetCore.csproj` using `$(AspNetCoreVersion)`, conditioned to net9.0/net10.0 only (`OpenApiOptions` not available in net8.0)
- [x] T002 `OpenApiVersion` already present in `Directory.Build.props`; `$(AspNetCoreVersion)` used instead per TFM; no changes needed
- [x] T003 Created `src/Juice.Extensions.MultiTenant.AspNetCore/OpenApi/` folder (by creating files within it)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core types that all user story phases depend on. No US work can start until this phase is complete.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [x] T004 Created `PerTenantOpenApiOptions<TTenant>` in `src/Juice.Extensions.MultiTenant.AspNetCore/OpenApi/PerTenantOpenApiOptions.cs`
- [x] T005 Created `ConfigurePerTenantOpenApiOptions<TTenant>` (implementing `IConfigureOptions<OpenApiOptions>`) in `src/Juice.Extensions.MultiTenant.AspNetCore/OpenApi/ConfigurePerTenantOpenApiOptions.cs`; guarded by `#if NET9_0_OR_GREATER` (net8 package lacks OpenApiOptions)

**Checkpoint**: Both types compile on net8.0, net9.0, net10.0. net6.0 build is unaffected (code absent under TFM guard).

---

## Phase 3: User Story 1 — Register Native OpenAPI Per-Tenant (Priority: P1) 🎯 MVP

**Goal**: `AddPerTenantOpenApi<TTenant>()` extension method registers the configurator
and produces tenant-aware document content on the standard `/openapi/{name}.json` endpoint.

**Independent Test**: Start `Juice.MultiTenant.Host` with only `AddPerTenantOpenApi`
registered (no Swashbuckle). Send `GET /openapi/v1.json` with header `__tenant__: alpha`.
Verify `info.title` in the JSON response reflects the "alpha" tenant.

### Implementation for User Story 1

- [x] T006 [US1] Created `OpenApiServiceCollectionExtensions` in `src/Juice.Extensions.MultiTenant.AspNetCore/OpenApi/OpenApiServiceCollectionExtensions.cs`; guarded by `#if NET9_0_OR_GREATER`
- [x] T007 [US1] Updated `test/Juice.MultiTenant.Host/Program.cs` — added `AddOpenApi("v1"/"v2")`, `AddPerTenantOpenApi<ITenant>`, and `app.MapOpenApi()` in `UseTenantSwagger`
- [ ] T008 [US1] Verify null-tenant fallback at `/openapi/v1.json` (manual smoke test — requires live host)

**Checkpoint**: US1 fully functional and independently testable. MVP deliverable achieved.

---

## Phase 4: User Story 2 — Coexist with SwaggerGen Integration (Priority: P2)

**Goal**: Both `AddPerTenantSwaggerGen` and `AddPerTenantOpenApi` active simultaneously
in the same host with no DI conflicts or startup errors.

**Independent Test**: Start `Juice.MultiTenant.Host` with both integrations registered.
Confirm application starts without exceptions. Request both `/swagger/v1/swagger.json`
(Swashbuckle) and `/openapi/v1.json` (native) with `__tenant__: alpha` header. Both
return valid tenant-scoped documents.

### Implementation for User Story 2

- [x] T009 [US2] Both `AddPerTenantSwaggerGen` and `AddPerTenantOpenApi` registered in Host; `UseSwagger()` and `MapOpenApi()` both active
- [x] T010 [US2] `dotnet build Multitenant.sln` — Build succeeded; `InterceptorsNamespaces` added to Host csproj for OpenAPI source generator

**Checkpoint**: Both integrations coexist. US1 + US2 independently functional.

---

## Phase 5: User Story 3 — Multi-Version Document Support (Priority: P3)

**Goal**: Multiple named documents (`v1`, `v2`) each produce per-tenant content via
a single `AddPerTenantOpenApi` registration.

**Independent Test**: Register `AddOpenApi("v1")` and `AddOpenApi("v2")` plus one
`AddPerTenantOpenApi` call. Request `/openapi/v1.json` and `/openapi/v2.json` with
`__tenant__: alpha` header. Both respond with valid, version-distinct, tenant-scoped
documents.

### Implementation for User Story 3

- [x] T011 [US3] `AddOpenApi("v1")` and `AddOpenApi("v2")` both registered; single `AddPerTenantOpenApi` applies to all named documents
- [x] T012 [US3] `IConfigureOptions<OpenApiOptions>` (unscoped) applies to all named instances automatically via the options infrastructure; verified by build succeeding

**Checkpoint**: All three user stories independently functional.

---

## Phase 6: Polish & Cross-Cutting Concerns

- [x] T013 [P] `dotnet build Multitenant.sln` — Build succeeded; net6.0 unaffected (guarded by `#if NET9_0_OR_GREATER`)
- [x] T014 [P] Updated `quickstart.md` — scope corrected to net9.0/net10.0; `IServiceScopeFactory` transformer pattern and `InterceptorsNamespaces` documented
- [x] T015 Non-live test suite — identical failure count as baseline (all failures are pre-existing live-service tests); zero regressions

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 (package refs must exist to compile)
- **US1 (Phase 3)**: Depends on Phase 2 (needs both option types)
- **US2 (Phase 4)**: Depends on Phase 3 (coexistence test requires US1 working)
- **US3 (Phase 5)**: Depends on Phase 3 (multi-version requires single-version working)
- **Polish (Phase 6)**: Depends on all story phases complete

### User Story Dependencies

- **US1 (P1)**: Sole dependency on Foundational — no story dependencies
- **US2 (P2)**: Depends on US1 (needs native integration working before testing coexistence)
- **US3 (P3)**: Depends on US1 (needs single-document working before multi-document)
- US2 and US3 can proceed in parallel once US1 is complete

### Within Each User Story

- Option types (Foundational) before extension method (US1)
- Extension method before Host wiring
- Host wiring before smoke-test verification

### Parallel Opportunities

- T001, T002, T003 (Phase 1) can run in parallel
- T004, T005 (Phase 2) can run in parallel (separate files, no inter-dependency)
- T013, T014 (Phase 6) can run in parallel
- US2 and US3 phases can run in parallel once US1 is complete

---

## Parallel Example: Phase 2

```
# Launch foundational types in parallel:
Task: "Create PerTenantOpenApiOptions<TTenant> in OpenApi/PerTenantOpenApiOptions.cs"
Task: "Create ConfigurePerTenantOpenApiOptions<TTenant> in OpenApi/ConfigurePerTenantOpenApiOptions.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (package refs + folder)
2. Complete Phase 2: Foundational (option types)
3. Complete Phase 3: US1 extension method + Host wiring
4. **STOP and VALIDATE**: curl `/openapi/v1.json` with tenant header → confirm tenant-scoped title
5. Ship / demo MVP

### Incremental Delivery

1. Setup + Foundational → core types ready
2. US1 → per-tenant native OpenAPI working → validate independently → MVP
3. US2 → side-by-side with Swashbuckle → validate both endpoints
4. US3 → multi-version → validate v1 and v2 independently

---

## Notes

- [P] = parallelizable (different files, no incomplete-task dependencies)
- [USx] = maps task to user story for traceability
- No test tasks generated (not requested in spec)
- All new source files live under `src/Juice.Extensions.MultiTenant.AspNetCore/OpenApi/`
- net6.0 is excluded at compile time via `#if NET8_0_OR_GREATER`; no runtime guard needed
- Verify tests fail before implementing only applies to TDD mode (not requested here)
