using Juice.EventBus;
using Juice.Integrations.EventBus;
using Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events;
using Juice.MultiTenant.Domain.Events;
using Juice.MultiTenant.Shared.Enums;

namespace Juice.MultiTenant.Api.Domain.EventHandlers
{
    internal class TenantStatusChangedDomainEventHandler : INotificationHandler<TenantStatusChangedDomainEvent>
    {
        private IIntegrationEventService<TenantStoreDbContext> _integrationService;
        private readonly ILoggerFactory _logger;
        public TenantStatusChangedDomainEventHandler(ILoggerFactory logger, IIntegrationEventService<TenantStoreDbContext> integrationService)
        {
            _logger = logger;
            _integrationService = integrationService;
        }
        public async ValueTask Handle(TenantStatusChangedDomainEvent notification, CancellationToken cancellationToken)
        {
            _logger.CreateLogger<TenantStatusChangedDomainEventHandler>()
            .LogTrace("Tenant with Identifier: {Identifier} has been successfully updated status",
                notification.TenantIdentifier);

            IntegrationEvent? integrationEvent =
                notification.TenantStatus switch
                {
                    TenantStatus.Initializing => new TenantInitializationChangedIntegrationEvent(notification.TenantId, notification.TenantIdentifier, notification.TenantStatus),
                    TenantStatus.Initialized => new TenantInitializationChangedIntegrationEvent(notification.TenantId, notification.TenantIdentifier, notification.TenantStatus),
                    TenantStatus.Approved => new TenantApprovalChangedIntegrationEvent(notification.TenantId, notification.TenantIdentifier, notification.TenantStatus),
                    TenantStatus.PendingApproval => new TenantApprovalChangedIntegrationEvent(notification.TenantId, notification.TenantIdentifier, notification.TenantStatus),
                    TenantStatus.Rejected => new TenantApprovalChangedIntegrationEvent(notification.TenantId, notification.TenantIdentifier, notification.TenantStatus),
                    TenantStatus.Active => new TenantActivatedIntegrationEvent(notification.TenantId, notification.TenantIdentifier),
                    TenantStatus.Inactive => new TenantDeactivatedIntegrationEvent(notification.TenantId, notification.TenantIdentifier),
                    TenantStatus.PendingToActive => new TenantRequestActiveIntegrationEvent(notification.TenantId, notification.TenantIdentifier),
                    TenantStatus.Suspended => new TenantSuspendedIntegrationEvent(notification.TenantId, notification.TenantIdentifier),
                    TenantStatus.Abandoned => new TenantAbandonedIntegrationEvent(notification.TenantId, notification.TenantIdentifier),
                    _ => default
                };
            if (integrationEvent != null)
            {
                await _integrationService.AddAndSaveEventAsync(integrationEvent);
            }
        }
    }
}
