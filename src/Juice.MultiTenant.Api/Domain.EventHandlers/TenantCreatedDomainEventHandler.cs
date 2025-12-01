using Juice.Integrations.EventBus;
using Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events;
using Juice.MultiTenant.Domain.Events;

namespace Juice.MultiTenant.Api.Domain.EventHandlers
{
    internal class TenantCreatedDomainEventHandler : INotificationHandler<TenantCreatedDomainEvent>
    {
        private IIntegrationEventService<TenantStoreDbContext> _integrationService;
        private readonly ILoggerFactory _logger;
        public TenantCreatedDomainEventHandler(ILoggerFactory logger, IIntegrationEventService<TenantStoreDbContext> integrationService)
        {
            _logger = logger;
            _integrationService = integrationService;
        }
        public async ValueTask Handle(TenantCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            _logger.CreateLogger<TenantCreatedDomainEventHandler>()
                .LogTrace("Tenant with Identifier: {Identifier} has been successfully created",
                notification.TenantIdentifier);

            var integrationEvent = new TenantCreatedIntegrationEvent(notification.TenantId, notification.TenantIdentifier,
                notification.CreateAdminUser, notification.CreateAdminPassword, notification.CreateAdminEmail);
            await _integrationService.AddAndSaveEventAsync(integrationEvent);
        }
    }
}
