using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Juice.MultiTenant.Domain.AggregatesModel.TenantAggregate;
using Juice.MultiTenant.EF;
using Juice.MultiTenant.EF.Migrations;
using Juice.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using DomainTenant = Juice.MultiTenant.Domain.AggregatesModel.TenantAggregate.Tenant;

namespace Juice.MultiTenant.Tests
{
    /// <summary>Tenant Ids are GUIDs in their standard form (the tenant pattern decision record).</summary>
    public sealed class TenantIdStandardizationTest : IAsyncLifetime
    {
        private readonly SqliteConnection _connection = new("Data Source=:memory:");
        private ServiceProvider _services = default!;

        public async ValueTask InitializeAsync()
        {
            await _connection.OpenAsync();
            _services = new ServiceCollection().AddLogging().BuildServiceProvider();
            await using var context = NewContext();
            await context.Database.EnsureCreatedAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await _services.DisposeAsync();
            await _connection.DisposeAsync();
        }

        [Fact(DisplayName = "The old base32 form reads as the same GUID")]
        public void Base32ReadsAsTheSameGuid()
        {
            for (var i = 0; i < 100; i++)
            {
                var guid = Guid.NewGuid();
                TenantIdForm.Standard(new StringIdGenerator().GenerateUniqueId(guid)).Should().Be(guid.ToString("D"));
            }
            TenantIdForm.Standard("0193F0AA-0000-7000-8000-0000000000A1").Should().Be("0193f0aa-0000-7000-8000-0000000000a1");
            TenantIdForm.Standard("acme").Should().BeNull();
            TenantIdForm.Standard(Guid.Empty.ToString()).Should().BeNull();
            Guid.TryParse(TenantIdForm.New(), out _).Should().BeTrue();
        }

        [Fact(DisplayName = "Base32 Ids are rewritten to the standard form, others left, and a second run does nothing")]
        public async Task RewritesBase32IdsAsync()
        {
            var guid = Guid.NewGuid();
            var old = new StringIdGenerator().GenerateUniqueId(guid);
            var standard = Guid.NewGuid().ToString("D");
            await using (var context = NewContext())
            {
                context.TenantInfo.Add(new DomainTenant(old, "old", "Old", null, null, null));
                context.TenantInfo.Add(new DomainTenant(standard, "new", "New", null, null, null));
                context.TenantInfo.Add(new DomainTenant("acme", "acme", "Named", null, null, null));
                await context.SaveChangesAsync();
            }

            await using var settingsConnection = new SqliteConnection("Data Source=:memory:");
            await settingsConnection.OpenAsync();
            await using (var settings = NewSettings(settingsConnection))
            {
                await settings.Database.EnsureCreatedAsync();
            }
            await using (var context = NewContext())
            {
                // Without the settings context nothing is changed: the settings would be left under Ids no tenant has.
                var withoutSettings = () => TenantIdStandardization.RunAsync(context, null, NullLogger.Instance);
                await withoutSettings.Should().ThrowAsync<InvalidOperationException>();
                (await context.TenantInfo.AnyAsync(t => t.Id == old)).Should().BeTrue();
            }
            await using (var context = NewContext())
            await using (var settings = NewSettings(settingsConnection))
            {
                var rewrites = await TenantIdStandardization.RunAsync(context, settings, NullLogger.Instance);
                rewrites.Should().ContainSingle().Which.Should().Be(new TenantIdStandardization.Rewrite(old, guid.ToString("D")));
            }

            await using (var context = NewContext())
            {
                (await context.TenantInfo.Select(t => t.Id).ToListAsync())
                    .Should().BeEquivalentTo([guid.ToString("D"), standard, "acme"]);
                (await TenantIdStandardization.RunAsync(context, null, NullLogger.Instance)).Should().BeEmpty();
            }
        }


        [Fact(DisplayName = "Settings move with their tenant; a tenant whose standard Id is taken is left, with its settings")]
        public async Task SettingsMoveWithTheirTenantAsync()
        {
            await using var settingsConnection = new SqliteConnection("Data Source=:memory:");
            await settingsConnection.OpenAsync();
            var guid = Guid.NewGuid();
            var old = new StringIdGenerator().GenerateUniqueId(guid);
            var taken = Guid.NewGuid();
            var clash = new StringIdGenerator().GenerateUniqueId(taken);
            await using (var context = NewContext())
            {
                context.TenantInfo.Add(new DomainTenant(old, "old", "Old", null, null, null));
                // Two tenants for one GUID: the standard one and a base32 copy; the copy can't take the Id.
                context.TenantInfo.Add(new DomainTenant(taken.ToString("D"), "taken", "Taken", null, null, null));
                context.TenantInfo.Add(new DomainTenant(clash, "clash", "Clash", null, null, null));
                await context.SaveChangesAsync();
            }
            await using (var settings = NewSettings(settingsConnection))
            {
                await settings.Database.EnsureCreatedAsync();
                var table = settings.Model.FindEntityType(typeof(Juice.MultiTenant.Domain.AggregatesModel.SettingsAggregate.TenantSettings))!.GetTableName();
                foreach (var (tenant, key) in new[] { (old, "a"), (old, "b"), (clash, "a") })
                {
                    await settings.Database.ExecuteSqlRawAsync(
                        $"INSERT INTO \"{table}\" (\"Id\", \"Key\", \"Value\", \"TenantId\") VALUES ({{0}}, {{1}}, 'v', {{2}})", Guid.NewGuid(), key, tenant);
                }
            }

            await using (var context = NewContext())
            await using (var settings = NewSettings(settingsConnection))
            {
                var rewrites = await TenantIdStandardization.RunAsync(context, settings, NullLogger.Instance);
                rewrites.Should().ContainSingle().Which.To.Should().Be(guid.ToString("D"));
            }
            await using (var settings = NewSettings(settingsConnection))
            {
                var tenants = await settings.TenantSettings.IgnoreQueryFilters()
                    .Select(s => Microsoft.EntityFrameworkCore.EF.Property<string>(s, "TenantId")).ToListAsync();
                tenants.Should().BeEquivalentTo([guid.ToString("D"), guid.ToString("D"), clash]);
            }
        }

        private static TenantSettingsDbContext NewSettings(SqliteConnection connection)
            => new(new Juice.EF.DbOptions<TenantSettingsDbContext>(),
                new DbContextOptionsBuilder<TenantSettingsDbContext>().UseSqlite(connection).Options, new NoTenant());

        private sealed class NoTenant : Finbuckle.MultiTenant.Abstractions.IMultiTenantContextAccessor
        {
            public Finbuckle.MultiTenant.Abstractions.IMultiTenantContext MultiTenantContext { get; }
                = new Finbuckle.MultiTenant.MultiTenantContext<Juice.Extensions.MultiTenant.TenantInfo>();
        }

        private TenantStoreDbContext NewContext()
            => new(_services, new DbContextOptionsBuilder<TenantStoreDbContext>().UseSqlite(_connection).Options);
    }
}
