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
