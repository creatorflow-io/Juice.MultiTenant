using Grpc.Core;
using Juice.MultiTenant.Settings.Grpc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Juice.MultiTenant.Grpc.Extensions.Configuration
{
    internal class GrpcConfigurationProvider : ConfigurationProvider
    {
        private readonly TenantSettingsStore.TenantSettingsStoreClient _client;
        private ITenantAccessor _tenantAccessor;
        private ILogger? _logger;

        public GrpcConfigurationProvider(TenantSettingsStore.TenantSettingsStoreClient client,
            ITenantAccessor tenantAccessor, ILogger? logger)
        {
            _client = client;
            _tenantAccessor = tenantAccessor;
            _logger = logger;
        }


        public override void Load()
        {
            var metadata = string.IsNullOrEmpty(_tenantAccessor.Tenant?.Identifier)
                ? new Metadata()
                : [new Metadata.Entry("__tenant__", _tenantAccessor.Tenant.Identifier)];
            var start = DateTime.Now;
            var reply = _client.GetAll(
                new TenantSettingQuery(),
                metadata);

            if (reply?.Settings != null)
            {
                Data = reply.Settings.ToDictionary(s => s.Key, s => (string?)s.Value, StringComparer.OrdinalIgnoreCase);
            }
            if(reply?.Succeeded == false)
            {
                _logger?.LogError("Failed to load settings for tenant \"{id}\", message: {message}",
                    _tenantAccessor.Tenant?.Identifier, reply.Message);
            }
            if (_logger?.IsEnabled(LogLevel.Debug) ?? false)
            {
                _logger.LogDebug("Load {count} items take {time} milliseconds, tenant \"{id}\"",
                    reply?.Settings?.Count ?? 0,
                    (DateTime.Now - start).TotalMilliseconds, _tenantAccessor.Tenant?.Identifier);
            }
        }

    }
}
