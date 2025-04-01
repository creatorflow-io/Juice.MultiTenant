using Juice.MultiTenant.Grpc.Extensions.Configuration;
using Juice.MultiTenant.Settings.Grpc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Juice.MultiTenant.Grpc
{
    public static class JuiceTenantGrpcConfigurationServiceCollectionExtensions
    {
        /// <summary>
        /// Read tenant settings from tenant grpc service
        /// <para>Leave <c>configureGrpcClient</c> null to bypass add grpc client</para>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configureGrpcClient"></param>
        /// <returns></returns>
        public static IServiceCollection AddTenantGrpcConfiguration(this IServiceCollection services, Action<IHttpClientBuilder>? configureGrpcClient)
        {
            if (configureGrpcClient != null)
            {
                var grpcBuilder = services.AddGrpcClient<TenantSettingsStore.TenantSettingsStoreClient>();
                configureGrpcClient(grpcBuilder);
            }
            return services.AddSingleton<IConfigurationSource>(sp =>
            {
                var tenantAccessor = sp.GetRequiredService<ITenantAccessor>();
                var client = sp.GetRequiredService<TenantSettingsStore.TenantSettingsStoreClient>();
                var logger = sp.GetService<ILoggerFactory>();

                return new GrpcConfigurationSource(client, tenantAccessor, logger);
            });
        }

    }
}
