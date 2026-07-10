# Contract: Public Extension API

**Feature**: 001-per-tenant-openapi
**Date**: 2026-05-14

This document defines the public surface area of the new `OpenApi/` integration.
All signatures below are the binding contract; implementation details may vary.

---

## IServiceCollection Extension

```csharp
// Namespace: Microsoft.Extensions.DependencyInjection
// File: OpenApi/OpenApiServiceCollectionExtensions.cs
// Availability: NET8_0_OR_GREATER

public static class OpenApiServiceCollectionExtensions
{
    /// <summary>
    /// Registers per-tenant native OpenAPI document generation.
    /// </summary>
    /// <typeparam name="TTenant">
    ///   The tenant type; must implement ITenantInfo.
    /// </typeparam>
    /// <param name="services">The DI container.</param>
    /// <param name="setupAction">
    ///   Called at document-generation time with the resolved OpenApiOptions
    ///   and the current tenant (null when no tenant is resolved).
    /// </param>
    /// <returns>The same IServiceCollection for chaining.</returns>
    public static IServiceCollection AddPerTenantOpenApi<TTenant>(
        this IServiceCollection services,
        Action<OpenApiOptions, TTenant?> setupAction)
        where TTenant : class, ITenantInfo;
}
```

### Behaviour contract

- Calling `AddPerTenantOpenApi` MUST NOT call `services.AddOpenApi()` internally —
  the host is responsible for calling `AddOpenApi(documentName)` for each document it
  wants to expose. This prevents duplicate registrations when both integrations coexist.
- Calling `AddPerTenantOpenApi` multiple times with different `TTenant` generic
  arguments is undefined; the last registration wins for the configurator.
- The `setupAction` MUST be invoked every time a document is generated (not cached
  across requests), because tenant context changes per request.

---

## Host Usage Pattern

```csharp
// In Program.cs / Startup.cs — binding contract (not implementation detail)

// Step 1: Register native OpenAPI documents (one per version)
builder.Services.AddOpenApi("v1");
builder.Services.AddOpenApi("v2");

// Step 2: Register per-tenant customization
builder.Services.AddPerTenantOpenApi<ITenant>((options, tenant) =>
{
    if (tenant is not null)
    {
        options.AddDocumentTransformer((doc, ctx, ct) =>
        {
            doc.Info.Title = $"{tenant.Name} API";
            return Task.CompletedTask;
        });
    }
});

// Step 3: Map endpoints (unchanged from standard usage)
app.MapOpenApi();
```

---

## Coexistence Pattern

```csharp
// Both integrations active simultaneously — supported contract

builder.Services.AddPerTenantSwaggerGen<ITenant>((swaggerOptions, tenant) =>
{
    swaggerOptions.SwaggerDoc("v1", new OpenApiInfo { Title = tenant?.Name ?? "API" });
});

builder.Services.AddOpenApi("v1");
builder.Services.AddPerTenantOpenApi<ITenant>((openApiOptions, tenant) =>
{
    // Native stack customization here
});

// Middleware / endpoints:
app.UseSwagger();          // Swashbuckle endpoint: /swagger/v1/swagger.json
app.MapOpenApi();          // Native endpoint:      /openapi/v1.json
```

---

## Error Cases

| Scenario | Expected behaviour |
|----------|--------------------|
| `setupAction` throws | Exception propagates to the document-generation request; HTTP 500 returned to caller |
| No tenant resolved | `setupAction` called with `null` tenant; document generated with default config |
| Called on net6.0 | Compile-time error (code excluded via `#if NET8_0_OR_GREATER`) |
| `setupAction` is null | `ArgumentNullException` thrown at registration time |
