using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Juice.Extensions.Configuration;
using Juice.MultiTenant.Grpc.Finbuckle;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Juice.MultiTenant.Grpc
{
    public static class GrpcStoreFinbuckleMultiTenantBuilderExtensions
    {

        /// <summary>
        /// Use grpc client to resolve tenant info
        /// <para>Leave <c>configureGrpcClient</c> null to bypass add grpc client</para>
        /// </summary>
        /// <typeparam name="TTenantInfo"></typeparam>
        /// <param name="builder"></param>
        /// <param name="grpcEndpoint"></param>
        /// <returns></returns>
        public static MultiTenantBuilder<TTenantInfo> WithGprcStore<TTenantInfo>(this MultiTenantBuilder<TTenantInfo> builder, Action<IHttpClientBuilder>? configureGrpcClient)
           where TTenantInfo : class, ITenant, ITenantInfo, new()
        {
            if (configureGrpcClient != null)
            {
                var grpcBuilder = builder.Services.AddGrpcClient<TenantStore.TenantStoreClient>();
                configureGrpcClient(grpcBuilder);
            }

            builder.Services.AddScoped<MultiTenantGprcStore<TTenantInfo>>();
            return builder.WithStore<MultiTenantGprcStore<TTenantInfo>>(ServiceLifetime.Scoped);
        }

        /// <summary>
        /// Configure tenant for multi-tenant microservices working with gRPC
        /// <para></para>JuiceIntegration
        /// <para></para>Use Grpc store and fallback to DistributedCache store after 500 milliseconds
        /// <para></para>TenantConfiguration with grpc
        /// <para></para>Configure MediatR, add Integration event service (NOTE: Required an event bus)
        /// </summary>
        /// <returns></returns>
        public static MultiTenantBuilder<TTenantInfo> ConfigureTenantClient<TTenantInfo>(this MultiTenantBuilder<TTenantInfo> builder, IConfiguration configuration,
            string environment)
            where TTenantInfo : class, ITenant, ITenantInfo, new()
        {
            var tenantGrpcEndpoint = configuration
                .GetSection("Finbuckle:MultiTenant:Stores:Grpc:Endpoint")
                .Get<string>();


            if (string.IsNullOrWhiteSpace(tenantGrpcEndpoint))
            {
                throw new ArgumentNullException("Tenant gRPC endpoint is required");
            }

            builder.AddTenantServices()
                    .WithGprcStore(options =>
                    {
                        options.ConfigureHttpClient(options =>
                        {
                            options.BaseAddress = new Uri(tenantGrpcEndpoint);
                        });
                    })
                    .WithDistributedCacheStore();

            builder.Services
                .AddTenantJsonFile($"appsettings.{environment}.json")
                .AddTenantGrpcConfiguration(options =>
                {
                    options.ConfigureHttpClient(options =>
                    {
                        options.BaseAddress = new Uri(tenantGrpcEndpoint);
                    });
                });

            builder.Services.AddTenantOptionsMutableGrpcStore();

            return builder;
        }

    }
}
