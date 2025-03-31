using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Juice.MultiTenant.EF.Extensions.Configuration
{
    internal class EntityConfigurationSource : IConfigurationSource
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public EntityConfigurationSource(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new EntityConfigurationProvider(_scopeFactory);
        }
    }
}
