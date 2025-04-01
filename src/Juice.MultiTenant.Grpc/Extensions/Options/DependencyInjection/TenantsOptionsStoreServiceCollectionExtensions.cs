using Juice.Extensions.Options.Stores;
using Juice.MultiTenant.Grpc.Extensions.Options.Stores;
using Juice.MultiTenant.Settings.Grpc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Juice.MultiTenant.Grpc
{
    public static class TenantsOptionsStoreServiceCollectionExtensions
    {

        /// <summary>
        /// Save tenant settings to TenantSettings grpc service
        /// <para>Leave <c>configureGrpcClient</c> null to bypass add grpc client</para>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configureGrpcClient"></param>
        /// <returns></returns>
        public static IServiceCollection AddTenantOptionsMutableGrpcStore(this IServiceCollection services, Action<IHttpClientBuilder>? configureGrpcClient)
        {
            if (configureGrpcClient != null)
            {
                var grpcBuilder = services.AddGrpcClient<TenantSettingsStore.TenantSettingsStoreClient>();
                configureGrpcClient(grpcBuilder);
            }
            services.TryAddSingleton<IOptionsMutableStore, TenantSettingsOptionsMutableGrpcStore>();
            return services;
        }
    }
}
