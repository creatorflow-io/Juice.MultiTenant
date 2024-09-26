using System.Text.Json;
using Juice.MultiTenant;
using Juice.MultiTenant.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SwaggerGenServiceCollectionExtensions
    {
        public static IServiceCollection AddPerTenantSwaggerGen<TTenant>(
            this IServiceCollection services,
            Action<SwaggerGenOptions, TTenant?> setupAction)
            where TTenant : ITenant
        {
            // Add Mvc convention to ensure ApiExplorer is enabled for all actions
            services.Configure<MvcOptions>(c =>
                c.Conventions.Add(new SwaggerApplicationConvention()));

            // Register custom configurators that takes values from SwaggerGenOptions (i.e. high level config)
            // and applies them to SwaggerGeneratorOptions and SchemaGeneratorOptoins (i.e. lower-level config)

            services.AddTransient<IConfigureOptions<SwaggerGeneratorOptions>, ConfigureSwaggerGeneratorOptions>();
            services.AddTransient<IConfigureOptions<SchemaGeneratorOptions>, ConfigureSchemaGeneratorOptions>();

            // Register generator and it's dependencies
            services.TryAddTransient<ISwaggerProvider, SwaggerGenerator>();
            services.TryAddTransient<IAsyncSwaggerProvider, SwaggerGenerator>();
            services.TryAddTransient<ISchemaGenerator, SchemaGenerator>();
            
            services.TryAddTransient<ISerializerDataContractResolver>(s =>
            {
                var serializerOptions = s.GetService<IOptions<JsonOptions>>()?.Value?.JsonSerializerOptions
                    ?? new JsonSerializerOptions();

                return new JsonSerializerDataContractResolver(serializerOptions);
            });
            services.TryAddTransient(s => s.GetRequiredService<IOptionsSnapshot<SwaggerGeneratorOptions>>().Value);
            services.TryAddTransient(s => s.GetRequiredService<IOptionsSnapshot<SchemaGeneratorOptions>>().Value);
            // Used by the <c>dotnet-getdocument</c> tool from the Microsoft.Extensions.ApiDescription.Server package.
            services.TryAddScoped<IDocumentProvider, DocumentProvider>();

            services.AddOptions<SwaggerGenOptions>()
                .Configure<IServiceScopeFactory>((c, scf) =>
                {
                    using var scope = scf.CreateScope();
                    var tc = scope.ServiceProvider.GetService<TTenant>();

                    setupAction(c, tc);
                });

            return services;
        }

    }
}
