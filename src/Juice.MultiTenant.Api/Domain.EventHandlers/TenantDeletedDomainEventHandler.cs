using Juice.Messaging.Outbox;
using Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events;
using Juice.MultiTenant.Domain.Events;

namespace Juice.MultiTenant.Api.Domain.EventHandlers
{
    internal class TenantDeletedDomainEventHandler : INotificationHandler<TenantDeletedDomainEvent>
    {
        private IOutboxService _outbox;
        private readonly ILoggerFactory _logger;
        public TenantDeletedDomainEventHandler(ILoggerFactory logger, IOutboxService<TenantStoreDbContext> outbox)
        {
            _logger = logger;
            _outbox = outbox;
        }
        public async ValueTask Handle(TenantDeletedDomainEvent notification, CancellationToken cancellationToken)
        {
            _logger.CreateLogger<TenantDeletedDomainEventHandler>()
            .LogTrace("Tenant with Identifier: {Identifier} has been successfully deleted",
                notification.TenantIdentifier);

            var integrationEvent = new TenantDeletedIntegrationEvent(notification.TenantId, notification.TenantIdentifier, notification.TenantName);
            await _outbox.AddEventAsync(integrationEvent);
        }
    }
}
