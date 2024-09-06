using Finbuckle.MultiTenant;
using Juice.EF.Extensions;
using Juice.Extensions.Configuration;
using Juice.Extensions.DependencyInjection;
using Juice.MultiTenant.Tests.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Juice.MultiTenant.Tests.Infrastructure
{
    public class TenantContentDbContext : EF.MultiTenantDbContext
    {
        public DbSet<TenantContent> TenantContents { get; set; }

        public TenantContentDbContext(IServiceProvider serviceProvider,
           DbContextOptions options) : base(options)
        {
            ConfigureServices(serviceProvider);
        }

        protected override void ConfigureModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TenantContent>(options =>
            {
                options.ToTable(nameof(TenantContent), Schema);
                options.IsMultiTenant();
                options.IsAuditable();
            });
        }
    }

    public class HybridTenantContentDbContext : TenantContentDbContext
    {
        private ITenantsConfiguration _configuration;

        public HybridTenantContentDbContext(IServiceProvider serviceProvider,
                       DbContextOptions<HybridTenantContentDbContext> options)
            : base(serviceProvider, options)
        {
            _configuration = serviceProvider.GetRequiredService<ITenantsConfiguration>();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var provider = _options?.DatabaseProvider;
            var schema = _options?.Schema;
            var connectionName = _options?.ConnectionName ??
                provider switch
                {
                    "PostgreSQL" => "PostgreConnection",
                    "SqlServer" => "SqlServerConnection",
                    _ => throw new NotSupportedException($"Unsupported provider: {provider}")
                }
                ;
            var connectionString =
                _configuration.GetConnectionString(connectionName);
            switch (provider)
            {
                case "PostgreSQL":
                    {
                        ConfigurationHelper.ConfigurePostgreSQL(optionsBuilder, connectionString, schema);
                    }
                    break;
                case "SqlServer":
                    {
                        ConfigurationHelper.ConfigureSqlServer(optionsBuilder, connectionString, schema);
                    }
                    break;
                default: throw new NotSupportedException($"Unsupported provider: {provider}");
            }
        }
    }


    public class TenantContentSqlServerDbContext : TenantContentDbContext
    {
        public TenantContentSqlServerDbContext(IServiceProvider serviceProvider, DbContextOptions<TenantContentSqlServerDbContext> options) : base(serviceProvider, options)
        {
        }
    }
    public class TenantContentPostgreDbContext : TenantContentDbContext
    {
        public TenantContentPostgreDbContext(IServiceProvider serviceProvider, DbContextOptions<TenantContentPostgreDbContext> options) : base(serviceProvider, options)
        {

        }
    }

    public class TenantContentSqlServerDbContextFactory : IDesignTimeDbContextFactory<TenantContentSqlServerDbContext>
    {
        public TenantContentSqlServerDbContext CreateDbContext(string[] args)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            var resolver = new DependencyResolver
            {
                CurrentDirectory = AppContext.BaseDirectory
            };

            resolver.ConfigureServices(services =>
            {

                // Register DbContext class
                var configService = services.BuildServiceProvider().GetRequiredService<IConfigurationService>();

                var configuration = configService.GetConfiguration(args);

                services.AddTenantContentDbContext(configuration, "SqlServer", "Contents");
            });

            return resolver.ServiceProvider.GetRequiredService<TenantContentSqlServerDbContext>();
        }
    }

    public class TenantContentPostgreDbContextFactory : IDesignTimeDbContextFactory<TenantContentPostgreDbContext>
    {
        public TenantContentPostgreDbContext CreateDbContext(string[] args)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            var resolver = new DependencyResolver
            {
                CurrentDirectory = AppContext.BaseDirectory
            };

            resolver.ConfigureServices(services =>
            {

                // Register DbContext class
                var configService = services.BuildServiceProvider().GetRequiredService<IConfigurationService>();

                var configuration = configService.GetConfiguration(args);

                services.AddTenantContentDbContext(configuration, "PostgreSQL", "Contents");
            });

            return resolver.ServiceProvider.GetRequiredService<TenantContentPostgreDbContext>();
        }
    }
}
