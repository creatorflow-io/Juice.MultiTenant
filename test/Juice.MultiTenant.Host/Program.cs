using Asp.Versioning;
using Finbuckle.MultiTenant;
using Juice.AspNetCore.Mvc.Formatters;
using Juice.Extensions.Swagger;
using Juice.MultiTenant;
using Juice.MultiTenant.Api;
using Juice.MultiTenant.Api.Grpc.Services;
using Juice.MultiTenant.Domain.AggregatesModel.TenantAggregate;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;

var builder = WebApplication.CreateBuilder(args);

ConfigureMultiTenant(builder);

ConfigureGRPC(builder.Services);

ConfigureEvents(builder);

ConfigureDistributedCache(builder.Services, builder.Configuration);

ConfigureSecurity(builder);

ConfigureSwagger(builder);

ConfigureOrigins(builder);

// For unit test
builder.Services.AddScoped<TenantStoreService>();

builder.Services.AddControllers(options =>
{
    options.InputFormatters.Add(new TextSingleValueFormatter());
}).AddNewtonsoftJson(options =>
{
    options.SerializerSettings.Converters.Add(new StringEnumConverter());
});

var app = builder.Build();

app.UseCors("AllowAllOrigins");
app.UseMultiTenant();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapTenantGrpcServices();
app.RegisterTenantIntegrationEventSelfHandlersAsync();

UseTenantSwagger(app);

app.MapControllers();

app.MapGet("/", () => "Support gRPC only!");

// For unit test
app.MapGet("/tenant", async (context) =>
{
    var a = context.RequestServices.GetService<ITenantAccessor>();
    var s = context.RequestServices.GetService<ITenant>();
    if (a == null || s == null)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }
    await context.Response.WriteAsJsonAsync(s);
});

app.Run();

static void ConfigureMultiTenant(WebApplicationBuilder builder)
{
    builder.Services
    .AddMultiTenant()
    .ConfigureTenantHost(builder.Configuration, options =>
    {
        options.DatabaseProvider = "PostgreSQL";
        options.ConnectionName = "PostgreConnection";
        options.Schema = "App";
    }).WithBasePathStrategy(options => options.RebaseAspNetCorePathBase = true)
    .WithHeaderStrategy()
    .WithRouteStrategy()
    .WithDistributedCacheStore().ShouldUpdateCacheStore()
    ;

    builder.Services.ConfigureAllPerTenant<JwtBearerOptions, Juice.Extensions.MultiTenant.TenantInfo>((options, tc) =>
    {
        var authority = builder.Configuration.GetSection("OpenIdConnect:Authority").Get<string>();
        if(authority == null)
        {
            throw new InvalidOperationException("OpenIdConnect:Authority is required in appsettings.json");
        }
        options.Authority = GetAuthority(authority, tc);
    });

    builder.Services.AddTenantIntegrationEventSelfHandlers<Tenant>();

    builder.Services.AddTenantOwnerResolverDefault();

    builder.Services.AddEFMediatorRequestManager(builder.Configuration, options =>
    {
        options.DatabaseProvider = "PostgreSQL";
        options.ConnectionName = "PostgreConnection";
        options.Schema = "App";
    });

}

static void ConfigureGRPC(IServiceCollection services)
{
    // Add services to the container.
    services.AddGrpc(o => o.EnableDetailedErrors = true);
}

static void ConfigureEvents(WebApplicationBuilder builder)
{

    builder.Services.RegisterRabbitMQEventBus(builder.Configuration.GetSection("RabbitMQ"),
       options =>
       {
           options.BrokerName = "topic.juice_bus";
           options.SubscriptionClientName = "juice_multitenant_test_host_events";
           options.ExchangeType = "topic";
       });

}

static void ConfigureDistributedCache(IServiceCollection services, IConfiguration configuration)
{
    services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = configuration.GetConnectionString("Redis");
        options.InstanceName = "SampleInstance";
    });
}

static void ConfigureSecurity(WebApplicationBuilder builder)
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
        {
            options.Authority = GetAuthority(builder.Configuration.GetSection("OpenIdConnect:Authority").Get<string>(), default);
            options.Audience = builder.Configuration.GetSection("OpenIdConnect:Audience").Get<string?>();
            options.RequireHttpsMetadata = false;
        });

    //builder.Services.AddTenantAuthorizationDefault();
    builder.Services.AddTenantAuthorizationTest();

}

static void ConfigureOrigins(WebApplicationBuilder builder)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAllOrigins",
                       builder =>
                       {
                           builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                       });
    });
}

static void ConfigureSwagger(WebApplicationBuilder builder)
{

    builder.Services.AddApiVersioning(setup =>
    {
        setup.DefaultApiVersion = new ApiVersion(2, 0);
        setup.AssumeDefaultVersionWhenUnspecified = true;
        setup.ReportApiVersions = true;
    }).AddMvc()
    .AddApiExplorer(options =>
    {
        // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
        // note: the specified format code will format the version as "'v'major[.minor][-status]"
        options.GroupNameFormat = "'v'VVV";

        // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
        // can also be used to control the format of the API version in route templates
        options.SubstituteApiVersionInUrl = true;
    });

    builder.Services.AddPerTenantSwaggerGen<ITenant>((c, tc) =>
    {
        var authority = GetAuthority(builder.Configuration.GetSection("OpenIdConnect:Authority").Get<string>(), tc);
        c.IgnoreObsoleteActions();

        c.IgnoreObsoleteProperties();

        c.SchemaFilter<SwaggerIgnoreFilter>();

        c.UseInlineDefinitionsForEnums();

        c.DescribeAllParametersInCamelCase();

        c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri(authority + "/connect/authorize"),
                    TokenUrl = new Uri(authority + "/connect/token"),
                    Scopes = new Dictionary<string, string>
                    {
                        { "openid", "OpenId" },
                        { "profile", "Profile" },
                        { "tenants-api", "Tenants API" }
                    }
                }
            },
            Scheme = "Bearer"
        });

        c.OperationFilter<AuthorizeCheckOperationFilter>();
        c.OperationFilter<ReApplyOptionalRouteParameterOperationFilter>();
        c.DocumentFilter<TenantDocsFilter>();

        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "Tenants API V1",
            Description = "Provide Tenants Management Web API"
        });

        c.SwaggerDoc("v2", new OpenApiInfo
        {
            Version = "v2",
            Title = "Tenants API V2",
            Description = "Provide Tenants Management Web API"
        });

        c.IncludeReferencedXmlComments();
    });

    builder.Services.AddSwaggerGenNewtonsoftSupport();

}

static void UseTenantSwagger(WebApplication app)
{
    app.UseSwagger(options => options.RouteTemplate = "tenants/swagger/{documentName}/swagger.json");
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("v1/swagger.json", "Tenants API V1");
        c.SwaggerEndpoint("v2/swagger.json", "Tenants API V2");
        c.RoutePrefix = "tenants/swagger";

        c.OAuthClientId("tenants_api_swaggerui");
        c.OAuthAppName("Tenants API Swagger UI");
        c.OAuthUsePkce();
    });
}
static string GetAuthority(string? configuredAuthority, ITenant? tenant)
{
    if (string.IsNullOrEmpty(configuredAuthority))
    {
        throw new InvalidOperationException("OpenIdConnect:Authority is required in appsettings.json");
    }
    return configuredAuthority
        .Replace('/' + Juice.MultiTenant.Constants.TenantToken, !string.IsNullOrEmpty(tenant?.Identifier) ? '/' + tenant.Identifier : "");
}



