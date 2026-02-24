# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

This is a .NET multi-target solution (`net6.0;net8.0;net9.0`) that provides multi-tenant management for the Juice/Creatorflow platform. It manages tenant lifecycle, per-tenant settings, and exposes both REST API and gRPC interfaces. Built on top of [Finbuckle.MultiTenant](https://www.finbuckle.com/MultiTenant) and the internal Juice framework packages.

## Commands

### Build
```bash
dotnet build Multitenant.sln
```

### Run Tests
```bash
# All tests
dotnet test test/Juice.MultiTenant.Tests/Juice.MultiTenant.Tests.csproj

# Single test by name
dotnet test test/Juice.MultiTenant.Tests/Juice.MultiTenant.Tests.csproj --filter "DisplayName~GrpcTest"

# With framework target
dotnet test test/Juice.MultiTenant.Tests/Juice.MultiTenant.Tests.csproj -f net9.0
```

Tests marked `[IgnoreOnCIFact]` require live external services (SQL Server, PostgreSQL, Redis, gRPC host). Connection strings are configured in `test/Juice.MultiTenant.Tests/appsettings.Development.json`.

### Run Test Host
```bash
dotnet run --project test/Juice.MultiTenant.Host
```

### EF Migrations (run from solution root)
```bash
# Add migration for PostgreSQL TenantStore
dotnet ef migrations add <MigrationName> \
  --project src/Juice.MultiTenant.EF.PostgreSQL \
  --startup-project test/Juice.MultiTenant.Tests \
  --context TenantStoreDbContext \
  --output-dir Migrations/TenantStore

# Add migration for SQL Server TenantStore
dotnet ef migrations add <MigrationName> \
  --project src/Juice.MultiTenant.EF.SqlServer \
  --startup-project test/Juice.MultiTenant.Tests \
  --context TenantStoreDbContext \
  --output-dir Migrations/TenantStore
```

## Architecture

### Project Layout

| Project | Purpose |
|---|---|
| `Juice.MultiTenant` | Core domain: `Tenant` aggregate, domain commands, domain events, command handlers |
| `Juice.MultiTenant.Shared` | Shared enums (`TenantStatus`) and authorization policy names |
| `Juice.MultiTenant.Api.Contracts` | Protobuf definitions (`tenant.proto`, `tenantsettings.proto`) for gRPC |
| `Juice.MultiTenant.Api` | REST controllers, gRPC service implementations, MediatR behaviors, DI extensions |
| `Juice.MultiTenant.EF` | EF Core DbContexts, migrations extension, EF-backed stores and repositories |
| `Juice.MultiTenant.EF.PostgreSQL` | PostgreSQL-specific migration files |
| `Juice.MultiTenant.EF.SqlServer` | SQL Server-specific migration files |
| `Juice.MultiTenant.Grpc` | gRPC client: Finbuckle store backed by gRPC, tenant configuration/options via gRPC |
| `Juice.Extensions.MultiTenant.AspNetCore` | ASP.NET Core helpers: Swagger per-tenant, auth extensions |

Test projects live under `test/`:
- `Juice.MultiTenant.Tests` — xUnit tests (needs live DB/services)
- `Juice.MultiTenant.Host` — Full ASP.NET Core test host (REST + gRPC + messaging)
- `Juice.MultiTenant.SelfHost` — Minimal host using direct EF store (no gRPC)

### Domain Model

`Tenant` (in `Juice.MultiTenant`) is a DDD aggregate root implementing both `ITenant` (internal) and `ITenantInfo` (Finbuckle). It has a rich lifecycle enforced via methods:

```
New → PendingApproval → Approved/Rejected
New/Approved → Initializing → Initialized → PendingToActive → Active ↔ Inactive
Active/Inactive → Suspended → (delete allowed)
Any → Abandoned
```

Per-tenant key/value settings are stored in `TenantSettings` aggregate (in `Juice.MultiTenant`), persisted via `TenantSettingsDbContext`.

### Two EF DbContexts

**`TenantStoreDbContext`** (`[Domain("Tenants")]`):
- Stores the `Tenant` aggregate and Outbox events
- Used by the tenant host service (the microservice that owns tenants)
- Migrations assembly is provider-specific (`Juice.MultiTenant.EF.PostgreSQL` or `Juice.MultiTenant.EF.SqlServer`)

**`TenantSettingsDbContext`** (implements `IMultiTenantDbContext`):
- Multi-tenant aware — data is scoped per tenant via Finbuckle's EF multi-tenant enforcement
- Stores `TenantSettings` key/value pairs + Outbox events
- Uses schema (default: `"App"`) and cross-tenant indexing (`Key + TenantId` unique)

### Supported Database Providers

Both DbContexts support `"PostgreSQL"` and `"SqlServer"`. The provider is selected via `DbOptions.DatabaseProvider`. Connection string names default to:
- `PostgreConnection` for PostgreSQL
- `SqlServerConnection` for SQL Server

### CQRS / MediatR Pattern

- Commands and domain events are defined in `Juice.MultiTenant` under `Domain.Commands/` and `Domain.Events/`
- Command handlers for `Tenant` are in `Juice.MultiTenant/Domain.CommandHandlers/`
- Command handlers for `TenantSettings` are in `Juice.MultiTenant.Api/CommandHandlers/`
- Transaction behaviors wrapping commands: `TenantTransactionBehavior`, `TenantSettingsTransactionBehavior`
- Integration events are published via the Outbox pattern → RabbitMQ

### gRPC Services

Defined in `Juice.MultiTenant.Api.Contracts` (proto files), implemented in `Juice.MultiTenant.Api/Grpc.Services/`:
- `TenantStoreService` — CRUD for tenants
- `TenantSettingsStoreService` — read/write per-tenant settings

gRPC clients are in `Juice.MultiTenant.Grpc`:
- `MultiTenantGrpcStore<TTenantInfo>` — Finbuckle `IMultiTenantStore` implementation backed by gRPC (with memory caching)
- `TenantSettingsOptionsMutableGrpcStore` — `IOptionsMutableStore` implementation backed by gRPC
- `GrpcConfigurationProvider` — ASP.NET Core `IConfigurationProvider` backed by gRPC

### DI Extension Entrypoints

For the **tenant microservice host** (owns the DB):
```csharp
builder.Services.AddMultiTenant()
    .ConfigureTenantHost(configuration, options => {
        options.DatabaseProvider = "PostgreSQL"; // or "SqlServer"
        options.ConnectionName = "PostgreConnection";
        options.Schema = "App";
    });
```

For **other microservices** using gRPC:
```csharp
builder.Services.AddMultiTenant()
    .WithGprcStore(/* options */);
services.AddTenantGrpcConfiguration(/* options */);
services.AddTenantOptionsMutableGrpcStore(/* options */);
```

For **microservices** with direct EF access to tenant DB:
```csharp
builder.Services.AddMultiTenant()
    .ConfigureTenantEFDirectly(configuration, options => { ... }, environment);
```

### Tenant Resolution Strategies (Finbuckle)

The host uses multiple strategies chained: `WithBasePathStrategy` → `WithHeaderStrategy` → `WithRouteStrategy` → `WithDistributedCacheStore`. The `__tenant__` header key is used for gRPC calls.

### Messaging / Outbox

Integration events flow: domain event → MediatR handler → Outbox table → RabbitMQ delivery. Exchange for tenant events: `x.tenants.integration`. Idempotency for gRPC write operations is handled via the `idempotency-key` metadata header (a new GUID per call).

### Versioning

Library version is set in `Directory.Build.props` as `9.0.0`. Framework-specific package versions (EF, Npgsql, Finbuckle) are also defined there and selected by `TargetFramework`.
