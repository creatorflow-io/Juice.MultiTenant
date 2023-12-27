using Juice.Integrations.EventBus;
using Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events;
using Juice.MultiTenant.Domain.Events;

namespace Juice.MultiTenant.Api.Domain.EventHandlers
{
    internal class TenantClassChangedDomainEventHandler : INotificationHandler<TenantClassChangedDomainEvent>
    {
        private IIntegrationEventService<TenantStoreDbContext> _integrationService;
        private readonly ILoggerFactory _logger;
        public TenantClassChangedDomainEventHandler(ILoggerFactory logger, IIntegrationEventService<TenantStoreDbContext> integrationService)
        {
            _logger = logger;
            _integrationService = integrationService;
        }
        public async Task Handle(TenantClassChangedDomainEvent notification, CancellationToken cancellationToken)
        {
            _logger.CreateLogger<TenantClassChangedDomainEventHandler>()
            .LogTrace("Tenant with Identifier: {Identifier} has been successfully activated",
                notification.TenantIdentifier);

            var integrationEvent = new TenantClassChangedIntegrationEvent(notification.TenantIdentifier, notification.TenantClass);
            await _integrationService.AddAndSaveEventAsync(integrationEvent);

        }
    }
}
