using System;
using System.Threading;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Stores.DistributedCacheStore;
using FluentAssertions;
using Juice.Extensions.MultiTenant;
using Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events;
using Juice.MultiTenant.Api.IntegrationEvents.Handlers;
using Juice.MultiTenant.EF;
using Juice.MultiTenant.Shared.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;
using DomainTenant = Juice.MultiTenant.Domain.AggregatesModel.TenantAggregate.Tenant;

namespace Juice.MultiTenant.Tests
{
    /// <summary>
    /// Delivery can overtake: TenantActivated may arrive after the deactivation that followed it.
    /// </summary>
    public sealed class TenantCacheOrderingTest : IAsyncLifetime
    {
        private const string Identifier = "acme";

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

        [Fact(DisplayName = "An active tenant is cached")]
        public async Task ActiveTenantIsCachedAsync()
        {
            await AddActiveTenantAsync();
            var cache = new HookedCache();

            await HandleActivatedAsync(cache);

            (await Store(cache).TryGetByIdentifierAsync(Identifier)).Should().NotBeNull();
        }

        [Fact(DisplayName = "A late TenantActivated doesn't cache a deactivated tenant")]
        public async Task LateActivatedAfterDeactivationAsync()
        {
            await AddActiveTenantAsync();
            await ChangeAsync(t => t.Deactivate());
            var cache = new HookedCache();

            await HandleActivatedAsync(cache);

            (await Store(cache).TryGetByIdentifierAsync(Identifier)).Should().BeNull();
        }

        [Fact(DisplayName = "A late TenantActivated doesn't cache a suspended tenant")]
        public async Task LateActivatedAfterSuspensionAsync()
        {
            await AddActiveTenantAsync();
            await ChangeAsync(t => t.Suspend());
            var cache = new HookedCache();

            await HandleActivatedAsync(cache);

            (await Store(cache).TryGetByIdentifierAsync(Identifier)).Should().BeNull();
        }

        [Fact(DisplayName = "A reactivated tenant is cached, even if the event is handled twice")]
        public async Task ReactivatedTenantIsCachedAsync()
        {
            await AddActiveTenantAsync();
            await ChangeAsync(t => t.Deactivate());
            await ChangeAsync(t => t.Reactivate());
            var cache = new HookedCache();

            await HandleActivatedAsync(cache);
            await HandleActivatedAsync(cache);

            (await Store(cache).TryGetByIdentifierAsync(Identifier)).Should().NotBeNull();
        }

        [Fact(DisplayName = "A deactivation handled while the tenant is being added doesn't leave it cached")]
        public async Task DeactivationDuringTheAddAsync()
        {
            await AddActiveTenantAsync();
            var cache = new HookedCache();
            cache.BeforeFirstSet = async () =>
            {
                // The whole of the deactivation happens between the handler's read and its write.
                await ChangeAsync(t => t.Deactivate());
                await new TenantDeactivatedIngtegrationEventSelfHandler<TenantInfo>(
                    NullLogger<TenantDeactivatedIngtegrationEventSelfHandler<TenantInfo>>.Instance, Store(cache))
                    .HandleAsync(new TenantDeactivatedIntegrationEvent("id", Identifier, TenantStatus.Active));
            };

            await HandleActivatedAsync(cache);

            (await Store(cache).TryGetByIdentifierAsync(Identifier)).Should().BeNull();
        }

        private async Task HandleActivatedAsync(HookedCache cache)
        {
            var handler = Handler(cache, out var context);
            await using (context)
            {
                await handler.HandleAsync(new TenantActivatedIntegrationEvent("id", Identifier, TenantStatus.Initialized));
            }
        }

        private TenantActivatedIngtegrationEventSelfHandler<TenantInfo> Handler(HookedCache cache, out TenantStoreDbContext context)
        {
            context = NewContext();
            return new TenantActivatedIngtegrationEventSelfHandler<TenantInfo>(
                context, NullLogger<TenantActivatedIngtegrationEventSelfHandler<TenantInfo>>.Instance, Store(cache));
        }

        private static DistributedCacheStore<TenantInfo> Store(HookedCache cache)
            => new(cache, "test-", null);

        private TenantStoreDbContext NewContext()
            => new(_services, new DbContextOptionsBuilder<TenantStoreDbContext>().UseSqlite(_connection).Options);

        private async Task AddActiveTenantAsync()
        {
            await using var context = NewContext();
            var tenant = new DomainTenant("id", Identifier, "Acme", null, null, null);
            tenant.Activate();
            context.TenantInfo.Add(tenant);
            await context.SaveChangesAsync();
        }

        private async Task ChangeAsync(Action<DomainTenant> change)
        {
            await using var context = NewContext();
            var tenant = await context.TenantInfo.SingleAsync(t => t.Identifier == Identifier);
            change(tenant);
            await context.SaveChangesAsync();
        }

        /// <summary>A real in-memory cache that can run something just before the first write reaches it.</summary>
        private sealed class HookedCache : IDistributedCache
        {
            private readonly MemoryDistributedCache _inner = new(Options.Create(new MemoryDistributedCacheOptions()));

            public Func<Task>? BeforeFirstSet { get; set; }

            public byte[]? Get(string key) => _inner.Get(key);
            public Task<byte[]?> GetAsync(string key, CancellationToken token = default) => _inner.GetAsync(key, token);
            public void Refresh(string key) => _inner.Refresh(key);
            public Task RefreshAsync(string key, CancellationToken token = default) => _inner.RefreshAsync(key, token);
            public void Remove(string key) => _inner.Remove(key);
            public Task RemoveAsync(string key, CancellationToken token = default) => _inner.RemoveAsync(key, token);
            public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => _inner.Set(key, value, options);

            public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
            {
                var hook = BeforeFirstSet;
                BeforeFirstSet = null;
                if (hook != null)
                {
                    await hook();
                }
                await _inner.SetAsync(key, value, options, token);
            }
        }
    }
}
