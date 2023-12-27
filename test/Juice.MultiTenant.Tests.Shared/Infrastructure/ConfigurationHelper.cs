using Juice.EF.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Juice.MultiTenant.Tests.Infrastructure
{
    internal static class ConfigurationHelper
    {
        public static void ConfigureSqlServer(DbContextOptionsBuilder optionsBuilder,
            string? connectionString, string? schema)
        {
            optionsBuilder.UseSqlServer(
                                connectionString,
                                x =>
                                {
                                    x.MigrationsHistoryTable("__EFTenantContentMigrationsHistory", schema);
                                });

            optionsBuilder
                .ReplaceService<IMigrationsAssembly, DbSchemaAwareMigrationAssembly>()
            ;
        }

        public static void ConfigurePostgreSQL(DbContextOptionsBuilder optionsBuilder,
                       string? connectionString, string? schema)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            optionsBuilder.UseNpgsql(
                                connectionString,
                                x =>
                                {
                                    x.MigrationsHistoryTable("__EFTenantContentMigrationsHistory", schema);
                                });
            optionsBuilder
                .ReplaceService<IMigrationsAssembly, DbSchemaAwareMigrationAssembly>()
            ;
        }
    }
}
