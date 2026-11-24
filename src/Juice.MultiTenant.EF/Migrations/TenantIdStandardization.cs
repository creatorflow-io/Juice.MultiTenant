using Juice.MultiTenant.Domain.AggregatesModel.SettingsAggregate;
using Juice.MultiTenant.Domain.AggregatesModel.TenantAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Juice.MultiTenant.EF.Migrations
{
    /// <summary>
    /// Rewrites tenant Ids to the standard GUID form (the tenant pattern decision record). Tenants created through the REST API before it have
    /// an Id in the 26-character base32 form, which is the same GUID: their Id, and the <c>TenantId</c> of their settings, become its
    /// standard form. An Id that is neither form (a name) is left as it is and logged, since no GUID is known for it.
    /// <para>Run it once after the migrations, while nothing else writes tenants and once the outbox has delivered (an event still waiting
    /// carries the old Id); it is safe to run again, and does nothing when every Id is standard. An Id whose standard form another tenant
    /// already has is left, with its settings. Anything else holding the old Id (the services' own tenant columns, the Keycloak
    /// <c>platform_tenant</c> attribute) is rewritten by its owner: the result lists each change, and each is logged as a warning.</para>
    /// </summary>
    public static class TenantIdStandardization
    {
        public sealed record Rewrite(string From, string To);

        /// <summary>With the host's contexts: <c>await app.Services.StandardizeTenantIdsAsync();</c></summary>
        public static async Task<IReadOnlyList<Rewrite>> StandardizeTenantIdsAsync(this IServiceProvider services, CancellationToken ct = default)
        {
            using var scope = services.CreateScope();
            var logger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(TenantIdStandardization).FullName!)
                ?? NullLogger.Instance;
            return await RunAsync(scope.ServiceProvider.GetRequiredService<TenantStoreDbContext>(),
                scope.ServiceProvider.GetService<TenantSettingsDbContext>(), logger, ct);
        }

        public static async Task<IReadOnlyList<Rewrite>> RunAsync(TenantStoreDbContext store, TenantSettingsDbContext? settings, ILogger logger,
            CancellationToken ct = default)
        {
            var ids = await store.TenantInfo.IgnoreQueryFilters().AsNoTracking().Select(t => t.Id).ToListAsync(ct);
            var known = ids.ToHashSet(StringComparer.Ordinal);
            var rewrites = new List<Rewrite>();
            foreach (var id in ids)
            {
                var standard = TenantIdForm.Standard(id);
                if (standard is null)
                {
                    logger.LogWarning("Tenant {TenantId} has an Id that isn't a GUID; it is left as it is", id);
                    continue;
                }
                if (standard == id)
                {
                    continue;
                }
                if (known.Contains(standard))
                {
                    logger.LogWarning("Tenant {TenantId} would become {Standard}, which another tenant already has; it is left as it is", id, standard);
                    continue;
                }
                rewrites.Add(new Rewrite(id, standard));
            }

            if (rewrites.Count > 0 && settings is null)
            {
                // Rewriting tenants without their settings would leave the settings under Ids no tenant has, and no later run could find them.
                throw new InvalidOperationException(
                    "Tenant Ids need rewriting, but there is no TenantSettingsDbContext to rewrite their settings with; nothing was changed.");
            }
            foreach (var rewrite in rewrites)
            {
                // Settings first, then the tenant: cut short in between, a second run still finds the tenant under its old Id and does both.
                if (settings is not null)
                {
                    await using var settingsTransaction = await settings.Database.BeginTransactionAsync(ct);
                    await UpdateAsync(settings, typeof(TenantSettings), "TenantId", rewrite, ct);
                    await settingsTransaction.CommitAsync(ct);
                }
                await UpdateAsync(store, typeof(Tenant), nameof(Tenant.Id), rewrite, ct);
                // A warning, so it stays in the logs: owners of other copies of the Id (services, Keycloak) need every pair.
                logger.LogWarning("Tenant Id rewritten: {From} is now {To}", rewrite.From, rewrite.To);
            }
            return rewrites;
        }

        private static Task<int> UpdateAsync(DbContext context, Type entity, string property, Rewrite rewrite, CancellationToken ct)
        {
            var type = context.Model.FindEntityType(entity) ?? throw new InvalidOperationException($"{entity.Name} isn't in {context.GetType().Name}");
            var table = StoreObjectIdentifier.Table(type.GetTableName()!, type.GetSchema());
            var column = type.FindProperty(property)!.GetColumnName(table)!;
            var sql = context.GetService<ISqlGenerationHelper>();
            var command = $"UPDATE {sql.DelimitIdentifier(table.Name, table.Schema)} SET {sql.DelimitIdentifier(column)} = {{0}} "
                + $"WHERE {sql.DelimitIdentifier(column)} = {{1}}";
            return context.Database.ExecuteSqlRawAsync(command, [rewrite.To, rewrite.From], ct);
        }
    }
}
