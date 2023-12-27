using Juice.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Juice.MultiTenant.Tests.Infrastructure
{
    public static class SeviceCollectionExtensions
    {
        public static IServiceCollection AddTenantContentDbContext(this IServiceCollection services,
            IConfiguration configuration,
            string provider,
            string schema)
        {
            var connectionName =
                provider switch
                {
                    "PostgreSQL" => "PostgreConnection",
                    "SqlServer" => "SqlServerConnection",
                    _ => throw new NotSupportedException($"Unsupported provider: {provider}")
                }
                ;
            var connectionString = configuration.GetConnectionString(connectionName);

            switch (provider)
            {
                case "PostgreSQL":
                    services.AddScoped(sp => new Juice.EF.DbOptions<TenantContentPostgreDbContext> { Schema = schema, DatabaseProvider = provider });

                    services.AddDbContext<TenantContentPostgreDbContext>(
                       options =>
                       {
                           ConfigurationHelper.ConfigurePostgreSQL(options, connectionString, schema);
                       });

                    services.AddScoped<TenantContentDbContext>(sp => sp.GetRequiredService<TenantContentPostgreDbContext>());
                    break;
                case "SqlServer":
                    services.AddScoped(sp => new Juice.EF.DbOptions<TenantContentSqlServerDbContext> { Schema = schema, DatabaseProvider = provider });

                    services.AddDbContext<TenantContentSqlServerDbContext>(
                       options =>
                       {
                           ConfigurationHelper.ConfigureSqlServer(options, connectionString, schema);
                       });

                    services.AddScoped<TenantContentDbContext>(sp => sp.GetRequiredService<TenantContentSqlServerDbContext>());
                    break;
                default: throw new NotSupportedException($"Unsupported provider: {provider}");
            }
            return services;
        }

        public static IServiceCollection AddHybridTenantContentDbContext(this IServiceCollection services, string schema)
        {
            services.AddScoped(sp =>
            {
                var configuration = sp.GetRequiredService<ITenantsConfiguration>();
                var options =
                new Juice.EF.DbOptions<HybridTenantContentDbContext> { Schema = schema };
                configuration.GetSection("Contents").Bind(options);
                return options;
            });
            services.AddDbContext<HybridTenantContentDbContext>();
            return services.AddScoped<TenantContentDbContext>(sp => sp.GetRequiredService<HybridTenantContentDbContext>());
        }
    }
}
