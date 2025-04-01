using Grpc.Net.ClientFactory;
using Juice.MultiTenant.Grpc;
using Juice.MultiTenant.Settings.Grpc;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class GrpcClientServiceCollectionExtensions
    {
        public static IHttpClientBuilder AddTenantGrpcClient(this IServiceCollection services, Action<GrpcClientFactoryOptions> configure)
        {
            return services.AddGrpcClient<TenantStore.TenantStoreClient>(configure);
        }

        public static IHttpClientBuilder AddTenantConfigurationGrpcClient(this IServiceCollection services, Action<GrpcClientFactoryOptions> configure)
        {
            return services.AddGrpcClient<TenantSettingsStore.TenantSettingsStoreClient>(configure);
        }
    }
}
