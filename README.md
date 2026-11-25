Manage tenants and their settings in your application.
The project Juice.Extensions.MultiTenant.AspNetCore was here to easily maintainance only.

## Tenant Ids

A tenant's Id is a GUID in its standard form (`0193f0aa-0000-7000-8000-0000000000a1`), the key every Juice service stores and
passes. Its Identifier (`acme`) is for URLs and display only. This follows the tenant pattern decision record in juice-docs.

- **New tenants** get a generated Id. One given by the caller must be a GUID; anything else is refused.
- **Before 10.4**, the REST API issued Ids in a 26-character base32 form. That form is the same GUID, so run the rewrite once,
  after the migrations, while nothing else writes tenants. Wait until the outbox has delivered first, because a waiting event
  carries the old Id.

  ```csharp
  var rewrites = await app.Services.StandardizeTenantIdsAsync();   // each (From, To)
  ```

  - **What changes:** the tenant's Id and its settings' `TenantId`. Each change is logged as a warning.
  - **What it leaves alone:** an Id that is neither form, which it can't rewrite, and one whose GUID another tenant already has.
  - **When there's nothing to do:** it does nothing if every Id is standard.
  - **Other copies of the Id:** whatever else holds an old Id is updated by its owner, using the returned list or the logged pairs.
    That means services' tenant columns, and the `platform_tenant` attribute in Keycloak.
- **Lifecycle events** carry the Id (`TenantId`) and the Identifier. Services key on the Id. These are tenant created, activated,
  deactivated, suspended, abandoned, deleted, settings changed and properties changed.
- **The gRPC store** sends each tenant's status, so a service can refuse a tenant that isn't active.

## Console

`console/` is the Angular workspace for the tenant admin UI (formerly the `juice-tenants` repository, moved here with
its history):

| Project | Package | What |
|---|---|---|
| `projects/juice-js/tenants` | `@juice-js/tenants` | tenant admin pages (list, edit, settings) |
| `projects/juice-js/dict-builder` | `@juice-js/dict-builder` | the key/value dictionary editor the tenant pages use |
| `projects/app` | (not published) | a host app that runs the pages against a local Juice.MultiTenant API |

```bash
cd console
npm ci
npm run build:lib    # dict-builder, then tenants (the app and tenants read them from dist/)
npm run test:ci      # Karma, ChromeHeadless
npm run build        # the app
npm start            # the app on https://localhost:4200 with the ASP.NET dev certificate
```

## Building and releasing

`build.yml` builds and tests both sides on every push to `master`, `release/**` and `dev/**`. On a `release/*` branch,
once build and test pass, it publishes alphas of each side the push changed:

| Package | Alpha | Version from |
|---|---|---|
| `Juice.MultiTenant.*` | `<VersionPrefix>-alpha.<run>` on GitHub Packages | `VersionPrefix` in `Directory.Build.props` |
| `@juice-js/tenants`, `@juice-js/dict-builder` | `<version>-alpha.<run>` on npm, dist-tag `alpha` | `version` in `console/projects/juice-js/*/package.json` (keep the two equal) |

Run Build by hand on a `release/*` branch to publish both. Releases come from a GitHub release on `master` or a
`release/*` branch (`release.yml`). The tag names the package and its version, without a "v":

| Tag | Publishes |
|---|---|
| `api-10.4.0` | `Juice.MultiTenant.*` to nuget.org (trusted publishing) |
| `console-1.3.0` | `@juice-js/dict-builder` and `@juice-js/tenants` to npm at that version. Dist-tag `latest` from `master`, `release-10` from `release/10` unless it is the newest |

A pre-release version (`api-10.4.0-beta.1`, `console-1.3.0-rc.1`) is published as a pre-release, with its label as the
npm dist-tag. A NuGet beta or rc references the Juice packages of the same stage. Each next version is the one to
release: raise it after a release so alphas sort above it.
