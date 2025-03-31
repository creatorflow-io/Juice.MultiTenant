using Juice.MultiTenant;
using Juice.MultiTenant.Domain.AggregatesModel.SettingsAggregate;
using Juice.MultiTenant.EF;
using Juice.MultiTenant.EF.Extensions.Configuration;
using Juice.MultiTenant.EF.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ConfigurationBuilderExtensions
    {
        /// <summary>
        /// Read tenant settings from TenantDbContext
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddTenantEFConfiguration(this IServiceCollection services, IConfiguration configuration, Action<Juice.EF.DbOptions> configureOptions)
        {
            services.AddTenantSettingsDbContext(configuration, configureOptions);

            services.TryAddScoped<ITenantSettingsRepository, TenantSettingsRepository>();

            return services.AddSingleton<IConfigurationSource, EntityConfigurationSource>();
        }

        /// <summary>
        /// Register an <see cref="ITenantsOptionsMutableStore"/> to update tenant settings into DB
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddTenantOptionsMutableEF(this IServiceCollection services)
        {
            services.UseTenantOptionsMutableEFStore();

            services.TryAddScoped<ITenantSettingsRepository, TenantSettingsRepository>();
            return services;
        }

    }
}
