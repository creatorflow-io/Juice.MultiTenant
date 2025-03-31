using Grpc.Core;
using Juice.Extensions.Options.Stores;
using Juice.MultiTenant.Settings.Grpc;
using Juice.Utils;

namespace Juice.MultiTenant.Grpc.Extensions.Options.Stores
{
    internal class TenantSettingsOptionsMutableGrpcStore : IOptionsMutableStore
    {
        private readonly TenantSettingsStore.TenantSettingsStoreClient _client;

        private ITenantAccessor _tenantAccessor;

        public TenantSettingsOptionsMutableGrpcStore(TenantSettingsStore.TenantSettingsStoreClient client,
            ITenantAccessor tenantAccessor)
        {
            _client = client;
            _tenantAccessor = tenantAccessor;
        }
        public async Task UpdateAsync(string section, object? options)
        {
            var request = new UpdateSectionParams
            {
                Section = section
            };
            request.Settings.Add(JsonConfigurationParser.Parse(options));
            var result = await _client.UpdateSectionAsync(request,
                new Metadata { new Metadata.Entry("__tenant__", _tenantAccessor.Tenant?.Identifier ?? "") });
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(result.Message);
            }
        }
    }
}
