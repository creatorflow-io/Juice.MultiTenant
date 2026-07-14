<!--
Sync Impact Report
==================
Version change: (template, unversioned) → 1.0.0
Bump rationale: Initial ratification — first concrete constitution replacing the
  unfilled template. MAJOR baseline established.

Principles (all newly defined):
  I.   Library-First & Publishable Modules
  II.  Conventional Commits & Automated Release
  III. Test Discipline (NON-NEGOTIABLE)
  IV.  Typed Contracts & API Models
  V.   Angular & Material Conventions

Added sections:
  - Technology Constraints
  - Development Workflow
  - Governance

Removed sections: none (all template placeholders replaced)

Templates requiring updates:
  ✅ .specify/templates/plan-template.md   — Constitution Check reads gates
       generically from this file; no hardcoded principles, remains compatible.
  ✅ .specify/templates/spec-template.md    — no principle references; compatible.
  ✅ .specify/templates/tasks-template.md   — no principle references; compatible.
  ✅ .specify/templates/commands/*          — directory holds config/scripts, no
       agent-specific principle references requiring change.

Follow-up TODOs: none. RATIFICATION_DATE set to initial adoption date (today),
  as no earlier adoption record exists in the repository.
-->

# Juice Tenants Constitution

## Core Principles

### I. Library-First & Publishable Modules

Every shippable feature MUST live in a self-contained Angular library under
`projects/juice-js/*` (e.g. `tenants`, `dict-builder`), each exposing its public
surface exclusively through its `public-api.ts`. The `projects/app` project is a
consumer and demonstration host only; it MUST NOT contain shippable domain logic.
Libraries MUST be independently buildable with `ng build <lib>` (ng-packagr) and
MUST NOT import from the `app` project or from another library's internals.

Rationale: These packages are published to npm as `@juice-js/*` and consumed by
other applications. Clean public boundaries keep each package independently
versionable, testable, and reusable.

### II. Conventional Commits & Automated Release

All commits MUST follow Conventional Commits (`feat:`, `fix:`, `docs:`, `chore:`,
etc.). Versioning and publishing are owned by semantic-release driven by the
commit-analyzer; contributors MUST NOT hand-edit package versions or changelog
entries. Breaking changes MUST be declared with `BREAKING CHANGE:` in the commit
body or a `!` marker so the release tooling produces the correct MAJOR bump.

Rationale: Release automation depends entirely on commit metadata. Consistent
commit messages are the single source of truth for what version ships and why.

### III. Test Discipline (NON-NEGOTIABLE)

Every component, service, pipe, and guard MUST ship with a co-located `.spec.ts`
covering its public behavior. `ng test` (Karma + Jasmine) MUST pass before any
merge or publish. New behavior and bug fixes MUST be accompanied by tests that
would fail without the change. Broken or skipped tests MUST NOT be merged.

Rationale: These libraries are consumed downstream; regressions propagate to
every consuming application. Tests are the contract that protects consumers.

### IV. Typed Contracts & API Models

All backend interactions MUST be modeled with explicit TypeScript interfaces in
the library's `shared/models` directory, kept consistent with the source
contracts (`swagger-admin.json`, `swagger-operation.json`). `any` MUST NOT be
used to bypass typing at API boundaries. Data flowing through services MUST be
strongly typed end to end.

Rationale: Strong typing at the API boundary catches contract drift at compile
time rather than in production, and keeps the published surface predictable.

### V. Angular & Material Conventions

Code MUST follow the Angular style guide: reactive forms for user input, RxJS
for async flows (with proper subscription teardown), NgModule/component
boundaries respected, and Angular Material for UI primitives via each library's
`material.module`. User-facing strings MUST be localizable via `@ngx-translate`
rather than hard-coded. Authentication MUST go through `@juice-js/auth`
(OAuth2/OIDC) rather than ad-hoc token handling.

Rationale: A shared, idiomatic convention set keeps the libraries consistent,
accessible, and interoperable across the Juice ecosystem.

## Technology Constraints

- **Framework**: Angular 16 (CLI workspace, multi-project). Do not introduce
  major-version upgrades without a coordinated migration plan.
- **Language**: TypeScript ~4.9 with the workspace `tsconfig.json` strictness in
  force; do not relax compiler options per-library to silence errors.
- **UI**: Angular Material 16 + `ngx-mat-multi-sort`; RxJS ~7.8.
- **Packaging**: ng-packagr for libraries; artifacts build to `dist/`.
- **Distribution**: Published as `@juice-js/*` scoped npm packages, including
  alpha prereleases, via semantic-release.
- New runtime dependencies MUST be justified against existing capabilities before
  being added to a published library's dependency set.

## Development Workflow

- Work happens on feature branches; `master` is the release branch and `alpha`
  carries prerelease work. Direct pushes to release branches are discouraged.
- Every change is delivered by pull request. Reviewers MUST verify: Conventional
  Commit compliance (Principle II), passing `ng test` and `ng build` (Principle
  III), typed API boundaries (Principle IV), and public-api hygiene (Principle I).
- `ng build` and `ng test` MUST both succeed before a PR is mergeable.
- Public-API changes to a library MUST be called out in the PR description so the
  correct semantic-release bump is triggered.

## Governance

This constitution supersedes ad-hoc practices for the Juice Tenants workspace.
Amendments MUST be proposed via pull request, documenting the change, its
rationale, and any migration impact on dependent templates or consuming projects.

Versioning policy for this document follows semantic versioning:
- **MAJOR**: Backward-incompatible governance change or removal/redefinition of a
  principle.
- **MINOR**: A new principle or section, or materially expanded guidance.
- **PATCH**: Clarifications, wording, and non-semantic refinements.

All PRs and reviews MUST verify compliance with the principles above. Deviations
MUST be justified in the PR and, where they represent added complexity, recorded
in the plan's Complexity Tracking table. The Spec Kit templates under
`.specify/templates/` are the runtime guidance artifacts and MUST stay aligned
with this constitution.

**Version**: 1.0.0 | **Ratified**: 2026-07-12 | **Last Amended**: 2026-07-12
