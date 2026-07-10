# Quickstart: Per-Tenant Native OpenAPI

**Feature**: 001-per-tenant-openapi
**Date**: 2026-05-14

This guide shows how to add per-tenant native OpenAPI document generation to an
ASP.NET Core multi-tenant application.

---

## Prerequisites

- ASP.NET Core application targeting **net9.0 or net10.0** (`OpenApiOptions` was not
  available in `Microsoft.AspNetCore.OpenApi` 8.x)
- `Finbuckle.MultiTenant` configured with at least one resolution strategy
- `Juice.Extensions.MultiTenant.AspNetCore` package referenced
- `<InterceptorsNamespaces>$(InterceptorsNamespaces);Microsoft.AspNetCore.OpenApi.Generated</InterceptorsNamespaces>`
  added to your host project's `.csproj` (required by the OpenAPI source generator)

---

## Minimal Setup

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. Configure multi-tenancy (existing setup)
builder.Services.AddMultiTenant<MyTenantInfo>()
    .WithHeaderStrategy("__tenant__")
    .WithInMemoryStore(/* ... */);

// 2. Register one or more native OpenAPI documents
builder.Services.AddOpenApi("v1");

// 3. Register per-tenant customization
//    The setup action runs when OpenApiOptions are configured.
//    Use a document transformer + IServiceScopeFactory for per-request tenant data.
builder.Services.AddPerTenantOpenApi<MyTenantInfo>((options, _) =>
{
    options.AddDocumentTransformer((doc, ctx, ct) =>
    {
        // IHttpContextAccessor (AsyncLocal) captures the current request, so
        // creating a scope here resolves the current request's tenant correctly.
        var scopeFactory = ctx.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        var tenant = scope.ServiceProvider.GetService<MyTenantInfo>();
        doc.Info.Title = tenant?.Name ?? "My API";
        return Task.CompletedTask;
    });
});

var app = builder.Build();

// 4. Map the OpenAPI endpoint (standard call — unchanged)
app.MapOpenApi();   // serves /openapi/v1.json

app.Run();
```

---

## Side-by-Side with Swashbuckle

```csharp
// Swashbuckle (existing)
builder.Services.AddPerTenantSwaggerGen<MyTenantInfo>((swaggerOptions, tenant) =>
{
    swaggerOptions.SwaggerDoc("v1", new OpenApiInfo
    {
        Title   = tenant?.Name ?? "My API",
        Version = "v1"
    });
});

// Native OpenAPI (new)
builder.Services.AddOpenApi("v1");
builder.Services.AddPerTenantOpenApi<MyTenantInfo>((options, tenant) =>
{
    options.AddDocumentTransformer((doc, ctx, ct) =>
    {
        doc.Info.Title = tenant?.Name ?? "My API";
        return Task.CompletedTask;
    });
});

// Middleware
app.UseSwagger();       // /swagger/v1/swagger.json  (Swashbuckle)
app.MapOpenApi();       // /openapi/v1.json           (native)
```

---

## Multi-Version Example

```csharp
builder.Services.AddOpenApi("v1");
builder.Services.AddOpenApi("v2");

builder.Services.AddPerTenantOpenApi<MyTenantInfo>((options, tenant) =>
{
    options.AddDocumentTransformer((doc, ctx, ct) =>
    {
        doc.Info.Title = $"{tenant?.Name ?? "My API"} — {doc.Info.Version}";
        return Task.CompletedTask;
    });
});

// Both /openapi/v1.json and /openapi/v2.json will carry tenant-specific titles.
app.MapOpenApi();
```

---

## Validation

After starting the application, verify per-tenant behaviour:

```bash
# Resolve tenant "alpha" via header
curl -H "__tenant__: alpha" http://localhost:5000/openapi/v1.json | jq '.info.title'
# Expected: "alpha API"  (or whatever the tenant's name is)

# No tenant header — fallback document
curl http://localhost:5000/openapi/v1.json | jq '.info.title'
# Expected: "My API"  (the null-tenant default)
```

---

## Troubleshooting

| Symptom | Likely cause | Fix |
|---------|-------------|-----|
| Document title never changes per tenant | Tenant not being resolved | Check that the `__tenant__` header (or other strategy) is reaching the app; verify `IMultiTenantContextAccessor` resolves non-null |
| `InvalidOperationException` at startup | `AddOpenApi()` not called before `AddPerTenantOpenApi()` | Call `services.AddOpenApi(documentName)` first |
| Compile error on net6.0 | Target framework not supported | The feature requires net8.0 or later |
