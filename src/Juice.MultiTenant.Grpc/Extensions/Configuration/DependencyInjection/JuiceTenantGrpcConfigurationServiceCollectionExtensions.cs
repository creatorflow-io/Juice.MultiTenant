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
        /// Read tenant settings from TenantDbContext
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddTenantGrpcConfiguration(this IServiceCollection services, string endpoint)
        {
            services.AddGrpcClient<TenantSettingsStore.TenantSettingsStoreClient>(o =>
            {
                o.Address = new Uri(endpoint);
            });
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
