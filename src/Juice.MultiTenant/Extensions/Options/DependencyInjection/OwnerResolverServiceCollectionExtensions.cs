using Microsoft.Extensions.DependencyInjection;

namespace Juice.MultiTenant
{
    public static class OwnerResolverServiceCollectionExtensions
    {
        public static IServiceCollection AddTenantOwnerResolverDefault(this IServiceCollection services)
        {
            services.AddScoped<IOwnerResolver, DefaultOwnerResolver>();
            return services;
        }
    }
}
