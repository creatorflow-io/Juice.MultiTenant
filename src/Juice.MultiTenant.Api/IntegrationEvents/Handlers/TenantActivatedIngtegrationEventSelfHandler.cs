using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Stores.DistributedCacheStore;
using Juice.Messaging;
using Juice.Extensions.MultiTenant;
using Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events;
using Juice.MultiTenant.Shared.Enums;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Juice.MultiTenant.Tests")]
namespace Juice.MultiTenant.Api.IntegrationEvents.Handlers
{
    /// <summary>
    /// Self handle TenantActivatedIngtegrationEvent to update distributed cache store if exists.
    /// </summary>
    internal class TenantActivatedIngtegrationEventSelfHandler<TTenantInfo> : IIntegrationEventHandler<TenantActivatedIntegrationEvent>
        where TTenantInfo : class, ITenantInfo, new()
    {
        private readonly TenantStoreDbContext _context;
        private readonly DistributedCacheStore<TTenantInfo>? _cacheStore;
        private readonly ILogger _logger;

        public TenantActivatedIngtegrationEventSelfHandler(
            TenantStoreDbContext dbContext,
            ILogger<TenantActivatedIngtegrationEventSelfHandler<TTenantInfo>> logger,
            DistributedCacheStore<TTenantInfo>? cacheStore = null)
        {
            _cacheStore = cacheStore;
            _context = dbContext;
            _logger = logger;
        }

        public async Task HandleAsync(TenantActivatedIntegrationEvent @event)
        {
            if (_cacheStore != null)
            {
                // This event can arrive after a later deactivation or suspension, so the row decides, not the event.
                var tenant = await FindActiveAsync(@event.TenantIdentifier);
                if (tenant == null)
                {
                    _logger.LogInformation("Tenant {identifier} is not active, it was not added to DistributedCacheStore", @event.TenantIdentifier);
                }
                else if (tenant is TTenantInfo tenantInfo)
                {
                    var added = await _cacheStore.TryAddAsync(tenantInfo);
                    if (added)
                    {
                        _logger.LogInformation("Tenant {identifier} info was updated to DistributedCacheStore", @event.TenantIdentifier);
                    }
                    else
                    {
                        _logger.LogInformation("Tenant {identifier} info was not updated to DistributedCacheStore", @event.TenantIdentifier);
                    }

                    // A deactivation committed since the read may have removed the entry before it was added.
                    if (await FindActiveAsync(@event.TenantIdentifier) == null)
                    {
                        await _cacheStore.TryRemoveAsync(@event.TenantIdentifier);
                        _logger.LogInformation("Tenant {identifier} stopped being active, it was removed from DistributedCacheStore", @event.TenantIdentifier);
                    }
                }
                else
                {
                    _logger.LogWarning("Tenant {identifier} info is not a {type}", @event.TenantIdentifier, typeof(TTenantInfo).Name);
                }
            }
            else
            {
                _logger.LogInformation("DistributedCacheStore was not registerd in DI");
            }
        }

        private Task<TenantInfo?> FindActiveAsync(string identifier)
            => _context.TenantInfo.AsNoTracking()
                .Where(t => t.Identifier == identifier && t.Status == TenantStatus.Active)
                .Select(ti => new TenantInfo(ti.Id, ti.Identifier, ti.Name, ti.Properties, ti.OwnerUser, ti.Tier, ti.Region))
                .FirstOrDefaultAsync();
    }
}
