using Juice.MultiTenant.Domain.AggregatesModel.SettingsAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Juice.MultiTenant.EF.Extensions.Configuration
{
    internal class EntityConfigurationProvider : ConfigurationProvider
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public EntityConfigurationProvider(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }


        public override void Load()
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<ITenantSettingsRepository>();

            Data =
                repo.GetAllAsync(default).GetAwaiter().GetResult()
                .ToDictionary<TenantSettings, string, string?>(c => c.Key, c => c.Value, StringComparer.OrdinalIgnoreCase);
        }

    }
}
