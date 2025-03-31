using Juice.Extensions.Options.Stores;
using Juice.MultiTenant.Domain.AggregatesModel.SettingsAggregate;
using Microsoft.Extensions.DependencyInjection;

namespace Juice.MultiTenant.Extensions.Options.Stores
{
    internal class TenantSettingsOptionsMutableStore : IOptionsMutableStore
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public TenantSettingsOptionsMutableStore(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        public async Task UpdateAsync(string section, object? options)
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ITenantSettingsRepository>();
            await repository.UpdateSectionAsync(section, options);
        }
    }
}
