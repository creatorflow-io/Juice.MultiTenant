<!--
SYNC IMPACT REPORT
==================
Version change: [unversioned] → 1.0.0
Modified principles: N/A (initial population)
Added sections:
  - Core Principles (I–V)
  - Technology Constraints
  - Development Workflow
  - Governance
Removed sections: N/A
Templates requiring updates:
  - .specify/templates/plan-template.md ✅ aligned (Constitution Check section present)
  - .specify/templates/spec-template.md ✅ aligned (no constitution-specific sections required)
  - .specify/templates/tasks-template.md ✅ aligned (phase/story structure compatible)
Follow-up TODOs:
  - RATIFICATION_DATE set to today (2026-05-14) as initial adoption date — update if a
    prior governance decision predates this session.
-->

# Juice MultiTenant Constitution

## Core Principles

### I. Domain-Driven Tenant Lifecycle

The `Tenant` aggregate MUST own all lifecycle transitions (New → PendingApproval →
Approved/Rejected → Initializing → Initialized → Active ↔ Inactive → Suspended →
Abandoned). No external service MAY mutate tenant state directly; all mutations MUST
flow through domain commands dispatched via MediatR. This ensures the aggregate is
the single source of truth for lifecycle invariants.

### II. Dual-Interface Contract (REST + gRPC)

Every tenant management operation MUST be reachable via both REST controllers and gRPC
service implementations. Proto definitions in `Juice.MultiTenant.Api.Contracts` are the
canonical contract; REST and gRPC implementations MUST remain consistent with those
contracts. Breaking changes to proto files require a version bump in the package.

### III. Multi-Provider Database Portability

All EF Core DbContexts MUST support both `"PostgreSQL"` and `"SqlServer"` without
code changes — provider selection is configuration-driven via `DbOptions.DatabaseProvider`.
Migration assemblies MUST be kept provider-specific (`EF.PostgreSQL`, `EF.SqlServer`).
No provider-specific SQL MAY be embedded in domain or application layer code.

### IV. Outbox-Backed Integration Events (NON-NEGOTIABLE)

Integration events MUST be published via the Outbox pattern only. Direct in-process
publishing to RabbitMQ is prohibited. The exchange `x.tenants.integration` is the sole
delivery target. Idempotency for gRPC write operations MUST be enforced via the
`idempotency-key` metadata header (a new GUID per call).

### V. Simplicity & YAGNI

New abstractions MUST be justified by an existing, concrete need — not anticipated future
requirements. The number of projects in the solution MUST NOT grow unless a genuine
boundary (separate deployment unit, separate migration assembly, separate client contract)
warrants it. Three similar lines of code are preferable to a premature abstraction.

## Technology Constraints

- **Target Frameworks**: `net6.0`, `net8.0`, `net9.0` (multi-target). All public APIs
  MUST compile and pass tests on all three targets.
- **Multi-Tenancy Library**: Finbuckle.MultiTenant is the mandated abstraction for tenant
  resolution and store contracts. Custom resolution strategies MUST implement Finbuckle
  interfaces.
- **CQRS**: MediatR is the mandated mediator. Commands, domain events, and behaviors MUST
  follow the existing `Domain.Commands / Domain.Events / Domain.CommandHandlers` layout.
- **gRPC Client Caching**: `MultiTenantGrpcStore` MUST use in-memory caching to avoid
  redundant remote calls on every request.
- **Tenant Resolution Order**: `BasePathStrategy → HeaderStrategy → RouteStrategy →
  DistributedCacheStore`. Changes to this chain MUST be documented and reviewed.

## Development Workflow

- Tests marked `[IgnoreOnCIFact]` require live external services and MUST NOT be run in
  CI pipelines without those services available. Connection strings live in
  `test/Juice.MultiTenant.Tests/appsettings.Development.json` (never committed with real
  credentials).
- EF migrations MUST be generated via the commands in CLAUDE.md using the correct
  `--startup-project` and `--context` flags. Ad-hoc schema edits to migration files are
  prohibited.
- All PRs MUST pass the multi-target build (`dotnet build Multitenant.sln`) and the xUnit
  test suite (non-live tests) before merge.
- Complexity violations (extra projects, non-Outbox event publishing, provider-specific
  SQL) MUST be justified in the plan's Complexity Tracking table before implementation.

## Governance

This constitution supersedes all other written practices for this repository. Amendments
require:

1. A documented rationale explaining what changed and why.
2. A version bump following semantic versioning:
   - **MAJOR**: Principle removal, redefinition, or backward-incompatible governance change.
   - **MINOR**: New principle or section added; material expansion of guidance.
   - **PATCH**: Clarification, wording, or typo fix.
3. Update of `LAST_AMENDED_DATE` to the amendment date.
4. Propagation check across `.specify/templates/` files (plan, spec, tasks, commands).

All implementation plans MUST include a Constitution Check gate before Phase 0 research
and re-verify after Phase 1 design. Complexity justifications in plans take precedence
over this constitution only when explicitly approved by the project owner.

**Version**: 1.0.0 | **Ratified**: 2026-05-14 | **Last Amended**: 2026-05-14
