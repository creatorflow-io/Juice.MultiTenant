# Upgrade Plan: Angular 16 → 22 Workspace Migration

**Status**: ✅ COMPLETE (2026-07-14) | **Date**: 2026-07-14
**Blocks**: `specs/001-idempotency-interceptor` (idempotency requires `@juice-js/core@1.2.0`,
which — like the whole `@juice-js/*@1.2.0` suite — is compiled for Angular 22). **Now unblocked.**

> **Outcome**: Migrated 16 → 17 → 18 → 19 → 20 → 21 → 22, one major per commit on branch
> `002-angular-22-upgrade`. Final state: `@angular/* 22.0.6`, `material/cdk 22.0.4`,
> TypeScript 6.0.3, `@juice-js/*@1.2.0`. **Full suite green**: app builds; tenants 32/32,
> dict-builder 4/4, app 2/2 tests. The `001` idempotency feature now builds and runs in the
> full app, not just in library isolation.
>
> Key work beyond version bumps: (R4) `@ngx-translate/core` 14 → 18 API migration
> (`TranslateModule` → standalone `TranslatePipe`/`TranslateDirective` + `provideTranslateService()`,
> `setDefaultLang` → `setFallbackLang`); (R1) `ngx-mat-multi-sort` held at 21.0.3 under Material 22
> and **validated at runtime** — no fork needed; TS 6.0 `ignoreDeprecations`; app bundle budget
> raised. Recurring gotcha handled at every step: `legacy-peer-deps` let the `@angular/*` graph
> drift to mixed majors, so each step re-pinned the whole family to the exact target version.

## Why this exists

The idempotency feature depends on `@juice-js/core@1.2.0`. Investigation during `/speckit.implement`
established:

- Every `@juice-js/*@1.2.0` package ships Angular **22** partial-Ivy declarations (`ngDeclare "22.0.6"`).
- `@juice-js/auth@1.2.0` (12×) and `@juice-js/layout@1.2.0` (18×) use `ChangeDetectionStrategy.Eager`,
  an Angular 22 feature absent in Angular 16 → **`ng build app` fails** under the current toolchain.
- The suite is **lockstep-versioned** (`layout` peers `core` *exactly*), so partial adoption is
  impossible: using `core@1.2.0` forces `layout@1.2.0`/`auth@1.2.0`/`localize@1.2.0`.
- **No Angular-16-compatible `@juice-js/core` carries the idempotency API** — it was introduced in
  the `1.2.0` line, which is entirely Angular 22.

Therefore the app cannot build with the feature until the workspace is on Angular 22.

## Current vs. target

| Concern | Current (Angular 16) | Target (Angular 22) | Notes |
|---------|----------------------|---------------------|-------|
| `@angular/*` | 16.2.x | 22.0.6 | core, common, forms, router, animations, platform-browser, cdk |
| `@angular/material` | 16.2.2 | 22.0.4 | peer `^22 \|\| ^23` |
| `@angular/cli` + `build-angular` | 16.2.12 | 22.0.6 | build system change (webpack → esbuild/`@angular/build`) |
| TypeScript | ~4.9.4 | **~6.0** (`>=6.0 <6.1`) | large jump; strictness/API changes |
| zone.js | ~0.13 | ~0.15 or ~0.16 | |
| RxJS | ~7.8 | ~7.8 (`^7.4` ok) | no change needed |
| Node | — | **^22.22.3 \|\| ^24.15.0 \|\| >=26** | ✅ this env is v24.18.0 — already satisfies |
| ng-packagr | 16.x | 22.0.1 | rebuilds the `@juice-js/tenants` + `dict-builder` libs |
| `@ngx-translate/core` | 14.0.0 | 18.0.0 (`>=18`) | ⚠️ breaking API changes across v15–18 |
| `angular-oauth2-oidc` | 15.0.1 | 22.0.2 (`>=22`) | ⚠️ major jump |
| `ngx-mat-multi-sort` | 16.0.2 | **21.0.3 (max)** | ✅ R1 resolved — carry 21.0.3 (Angular-21 partial-Ivy, linker-compatible) behind a peer override; fork fallback |
| Test runner | Karma + Jasmine | Vitest / web-test-runner (`@angular/build`) | ⚠️ Karma builder retired in recent Angular — see Risk R2 |

## Strategy: incremental, one major at a time

Angular's official guidance is **do not skip majors**. Run `ng update` for each step, commit, and
verify build+test between steps. Use https://angular.dev/update-guide for the per-version checklist.

### Phase 0 — Preconditions
- Confirm Node ≥ 22.22.3 (env is v24.18.0 ✅) and a clean git tree.
- Create a dedicated branch (`002-angular-22-upgrade`).
- Snapshot current green state: `ng build` + `ng test` all projects.
- Freeze feature work on `001` until this lands (or develop `001` on top of this branch).

### Phase 1 — Step through majors 16 → 22
For each `N` in 17, 18, 19, 20, 21, 22:
1. `npx ng update @angular/core@N @angular/cli@N @angular/material@N`
2. Apply the migrations the schematic runs (standalone/control-flow migrations are optional but
   offered at 17+; the `application` builder migration is offered at 17).
3. Bump TypeScript to each version's supported range as `ng update` dictates (ends at ~6.0).
4. Bump zone.js when prompted (→ ~0.15/0.16).
5. `ng build` + `ng test` (or the migrated test runner) → must be green before the next step.
6. Commit per major (`chore(angular): update to vN`).

Key inflection points:
- **v17**: default build system becomes esbuild/`application` builder; new control-flow syntax
  available (migration optional). `angular.json` builder targets change.
- **v19/20**: Karma builder deprecated/removed → migrate tests (Risk R2).
- **v22**: TypeScript ~6.0 required; final alignment with `@juice-js/*@1.2.0`.

### Phase 2 — Third-party library alignment
Do these as their Angular peer allows, ideally folded into the matching major step:
- `@ngx-translate/core` 14 → 18: update usage (v15+ moved to provider-based APIs, `TranslateModule`
  changes, `MissingTranslationHandler` wiring in `app.module.ts`). Budget real refactor time.
- `angular-oauth2-oidc` 15 → 22: verify `OAuthModule.forRoot()` config and the `@juice-js/auth`
  integration still align.
- `ngx-mat-multi-sort` → resolve Risk R1 before completing v22.

### Phase 3 — Library rebuild & publish
- Rebuild `@juice-js/tenants` and `@juice-js/dict-builder` with ng-packagr 22 (Angular 22 partial-Ivy).
- Re-point their peerDependencies to Angular 22 ranges.
- The `001` idempotency integration (already implemented) then compiles in the full app, not just
  in library isolation.

### Phase 4 — Verify the whole app
- `ng build app` green (the currently-failing step).
- `ng test` green across all projects on the migrated runner.
- Manual e2e (quickstart from `001`): retried create → single tenant; reads unchanged.

## Risks

- **R1 — `ngx-mat-multi-sort` has no Angular 22 release (max 21.0.3, peers `^21.1.0`). ✅ RESOLVED (2026-07-14).**
  **Decision: carry `21.0.3` into Angular 22 behind a peer override; validate at the Material-22
  step; fork into the tenants library only if it breaks.**

  Evidence supporting this (from tarball inspection of `21.0.3`):
  - Built as **Angular 21 partial-Ivy** (`ngDeclare "21.1.2"`) — the linker-supported direction
    (a v22 app can consume v21 libs; only the reverse fails).
  - Imports **only stable** Material/CDK APIs (`@angular/material/sort|menu|dialog|chips|…`, cdk
    `drag-drop|overlay|a11y|collections|portal`); **0** `MatLegacy`/`ChangeDetectionStrategy.Eager`
    references; sole runtime dep is `tslib`.
  - `TableData<T>` (the component's core state object) is a plain class — Angular-version-agnostic.
  - Usage is confined to **one** component (`projects/juice-js/tenants/src/lib/tenants.component.ts`
    + `.html` + `tenants.module.ts`).

  **The one bounded risk**: `MatMultiSort extends MatSort` and `MatMultiSortHeaderComponent extends
  MatSortHeader`. If Material 22 moved `MatSort`/`MatSortHeader` to signal inputs or sealed members,
  those subclasses can break at runtime — verifiable only once on Material 22.

  **Mechanism to apply at the Material-22 step** (relax the too-strict peer so `npm install`
  resolves): set project-wide `legacy-peer-deps=true` in a root `.npmrc` (the whole `@juice-js`
  suite already needs loose peer handling), or install with `--legacy-peer-deps`. Optionally pin via
  root `package.json` `overrides` to feed it the workspace Angular version.

  **Validation checkpoint (gates v22 completion)**: after bumping Material to 22, `ng build` +
  `ng test` the tenants library, then exercise the tenants table UI — column sort (single + multi),
  pagination, and `matSortChange` wiring — since the `MatSort` subclassing is the only failure mode.

  **Fallback if it breaks — fork into the tenants library** (bounded, one-time): vendor the MIT
  source (5 self-contained pieces: `MatMultiSort`, `MatMultiSortHeaderComponent`,
  `MatMultiSortTableDataSource`, `TableData`, `MatMultiSortModule`; only `tslib`) under
  `projects/juice-js/tenants/src/lib/shared/multi-sort/`, adjust the `MatSort`/`MatSortHeader`
  subclassing to Material 22, rebuild via ng-packagr 22, and drop the external dependency. This
  removes the peer constraint entirely at the cost of owning ~5 small classes.
- **R2 — Test-runner migration.** Karma/Jasmine setup must move to Vitest or web-test-runner via
  `@angular/build`. The `001` idempotency specs (`HttpClientTestingModule`-based) port directly, but
  the runner config and `angular.json` test targets change.
- **R3 — TypeScript 4.9 → ~6.0** may surface new type errors (stricter inference, lib changes).
  Fix incrementally per major rather than all at v22.
- **R4 — `@ngx-translate` and `angular-oauth2-oidc` breaking APIs** require app-code changes in
  `app.module.ts` and auth wiring, not just version bumps.

## Rollback

Each major is a separate commit; revert to the last green major if a step blocks. The working tree
currently has all `@juice-js/*` at `1.2.0` (internally consistent, app build red on Angular 16) — the
migration turns it green rather than requiring a dependency change.

## Definition of done

- All projects build and test green on Angular 22 with `typescript ~6.0`.
- `@juice-js/*@1.2.0` resolve without peer conflicts.
- `001` idempotency feature builds in the full app and its specs pass on the new runner.
- R1 (`ngx-mat-multi-sort`) ✅ resolved: carry `21.0.3` behind a peer override, validated at the
  Material-22 step, with a fork-into-tenants-lib fallback documented.
