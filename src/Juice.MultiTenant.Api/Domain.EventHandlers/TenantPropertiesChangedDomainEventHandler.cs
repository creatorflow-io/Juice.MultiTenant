using Juice.Messaging.Outbox;
using Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events;
using Juice.MultiTenant.Domain.Events;

namespace Juice.MultiTenant.Api.Domain.EventHandlers
{
    internal class TenantPropertiesChangedDomainEventHandler :
        INotificationHandler<TenantPropertiesChangedDomainEvent>
    {
        private IOutboxService<TenantStoreDbContext> _outbox;
        private readonly ILoggerFactory _logger;
        public TenantPropertiesChangedDomainEventHandler(ILoggerFactory logger, IOutboxService<TenantStoreDbContext> outbox)
        {
            _logger = logger;
            _outbox = outbox;
        }
        public async ValueTask Handle(TenantPropertiesChangedDomainEvent notification, CancellationToken cancellationToken)
        {
            _logger.CreateLogger<TenantPropertiesChangedDomainEventHandler>()
            .LogTrace("Tenant with Identifier: {Identifier} has been successfully updated properties",
                notification.TenantIdentifier);

            var integrationEvent = new TenantPropertiesChangedIntegrationEvent(notification.TenantIdentifier);
            await _outbox.AddEventAsync(integrationEvent);
        }
    }
}
