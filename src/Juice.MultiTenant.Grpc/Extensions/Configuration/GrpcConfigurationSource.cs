using Juice.Extensions.Configuration;
using Juice.MultiTenant.Settings.Grpc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Juice.MultiTenant.Grpc.Extensions.Configuration
{
    internal class GrpcConfigurationSource : IConfigurationSource
    {
        private readonly TenantSettingsStore.TenantSettingsStoreClient _client;
        private ITenantAccessor _tenantAccessor;
        private ILoggerFactory? _logger;

        public GrpcConfigurationSource(TenantSettingsStore.TenantSettingsStoreClient client,
            ITenantAccessor tenantAccessor, ILoggerFactory? logger)
        {
            _tenantAccessor = tenantAccessor;
            _client = client;
            _logger = logger;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new GrpcConfigurationProvider(_client, _tenantAccessor, _logger?.CreateLogger<GrpcConfigurationProvider>());
        }
    }
}
