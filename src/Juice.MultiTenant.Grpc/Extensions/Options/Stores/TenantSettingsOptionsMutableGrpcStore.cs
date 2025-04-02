using Grpc.Core;
using Juice.Extensions.Options.Stores;
using Juice.MultiTenant.Settings.Grpc;
using Juice.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Juice.MultiTenant.Grpc.Extensions.Options.Stores
{
    internal class TenantSettingsOptionsMutableGrpcStore : IOptionsMutableStore
    {
        private readonly TenantSettingsStore.TenantSettingsStoreClient _client;

        private ITenantAccessor _tenantAccessor;

        private ILogger _logger;

        public TenantSettingsOptionsMutableGrpcStore(
            TenantSettingsStore.TenantSettingsStoreClient client,
            ITenantAccessor tenantAccessor,
            ILogger<TenantSettingsOptionsMutableGrpcStore> logger)
        {
            _client = client;
            _tenantAccessor = tenantAccessor;
            _logger = logger;
        }
        public async Task UpdateAsync(string section, object? options)
        {
            var request = new UpdateSectionParams
            {
                Section = section
            };
            request.Settings.Add(JsonConfigurationParser.Parse(options));
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Update section {section} for tenant {tenant}. Data: {json}",
                    section, _tenantAccessor.Tenant?.Identifier, JsonConvert.SerializeObject(options));
            }
            var result = await _client.UpdateSectionAsync(request,
                new Metadata { new Metadata.Entry("__tenant__", _tenantAccessor.Tenant?.Identifier ?? "") });
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(result.Message);
            }
        }
    }
}
