using Juice.Messaging.Outbox;
using Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events;
using Juice.MultiTenant.Domain.Events;

namespace Juice.MultiTenant.Api.Domain.EventHandlers
{
    internal class TenantSettingsChangedDomainEventHandler : INotificationHandler<TenantSettingsChangedDomainEvent>
    {
        private IOutboxService<TenantSettingsDbContext> _outbox;
        private readonly ILoggerFactory _logger;

        public TenantSettingsChangedDomainEventHandler(IOutboxService<TenantSettingsDbContext> outbox, ILoggerFactory logger)
        {
            _outbox = outbox;
            _logger = logger;
        }

        public async ValueTask Handle(TenantSettingsChangedDomainEvent notification, CancellationToken cancellationToken)
        {
            _logger.CreateLogger<TenantSettingsChangedDomainEventHandler>()
            .LogTrace("Tenant settings with Identifier: {Identifier} has been successfully updated.",
                notification.TenantIdentifier);

            var integrationEvent = new TenantSettingsChangedIntegrationEvent(notification.TenantId, notification.TenantIdentifier);
            await _outbox.AddEventAsync(integrationEvent);
        }
    }
}
