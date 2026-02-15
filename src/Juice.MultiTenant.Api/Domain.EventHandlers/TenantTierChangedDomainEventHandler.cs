using Juice.Messaging.Outbox;
using Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events;
using Juice.MultiTenant.Domain.Events;

namespace Juice.MultiTenant.Api.Domain.EventHandlers
{
    internal class TenantTierChangedDomainEventHandler : INotificationHandler<TenantTierChangedDomainEvent>
    {
        private IOutboxService<TenantStoreDbContext> _outbox;
        private readonly ILoggerFactory _logger;
        public TenantTierChangedDomainEventHandler(ILoggerFactory logger, IOutboxService<TenantStoreDbContext> outbox)
        {
            _logger = logger;
            _outbox = outbox;
        }
        public async ValueTask Handle(TenantTierChangedDomainEvent notification, CancellationToken cancellationToken)
        {
            _logger.CreateLogger<TenantTierChangedDomainEventHandler>()
            .LogTrace("Tenant with Identifier: {Identifier} has been successfully changed class",
                notification.TenantIdentifier);

            var integrationEvent = new TenantTierChangedIntegrationEvent(notification.Id, notification.TenantIdentifier, notification.TenantTier);
            await _outbox.AddEventAsync(integrationEvent);

        }
    }
}
