using Juice.Messaging.Outbox;
using Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events;
using Juice.MultiTenant.Domain.Events;

namespace Juice.MultiTenant.Api.Domain.EventHandlers
{
    internal class TenantOwnerChangedDomainEventHandler : INotificationHandler<TenantOwnerChangedDomainEvent>
    {
        private IOutboxService<TenantStoreDbContext> _outbox;
        private readonly ILoggerFactory _logger;
        public TenantOwnerChangedDomainEventHandler(ILoggerFactory logger, IOutboxService<TenantStoreDbContext> outbox)
        {
            _logger = logger;
            _outbox = outbox;
        }
        public async ValueTask Handle(TenantOwnerChangedDomainEvent notification, CancellationToken cancellationToken)
        {
            _logger.CreateLogger<TenantStatusChangedDomainEventHandler>()
            .LogTrace("Tenant with Identifier: {Identifier} has been changed the owner",
                notification.TenantIdentifier);

            var integrationEvent = new TenantOwnerChangedIntegrationEvent(
                notification.Id,
                notification.TenantIdentifier,
                notification.FromUser, notification.ToUser);

            await _outbox.AddEventAsync(integrationEvent);
        }
    }
}
