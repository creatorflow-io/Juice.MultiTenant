using Juice.EventBus;
using Juice.Messaging.Outbox;
using Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events;
using Juice.MultiTenant.Domain.Events;
using Juice.MultiTenant.Shared.Enums;

namespace Juice.MultiTenant.Api.Domain.EventHandlers
{
    internal class TenantStatusChangedDomainEventHandler : INotificationHandler<TenantStatusChangedDomainEvent>
    {
        private IOutboxService<TenantStoreDbContext> _outbox;
        private readonly ILoggerFactory _logger;
        public TenantStatusChangedDomainEventHandler(ILoggerFactory logger, IOutboxService<TenantStoreDbContext> outbox)
        {
            _logger = logger;
            _outbox = outbox;
        }
        public async ValueTask Handle(TenantStatusChangedDomainEvent notification, CancellationToken cancellationToken)
        {
            _logger.CreateLogger<TenantStatusChangedDomainEventHandler>()
            .LogTrace("Tenant with Identifier: {Identifier} has been successfully updated status",
                notification.TenantIdentifier);

            IntegrationEvent? integrationEvent =
                notification.TenantStatus switch
                {
                    TenantStatus.Initializing => new TenantInitializationChangedIntegrationEvent(notification.Id, notification.TenantIdentifier, notification.TenantStatus),
                    TenantStatus.Initialized => new TenantInitializationChangedIntegrationEvent(notification.Id, notification.TenantIdentifier, notification.TenantStatus),
                    TenantStatus.Approved => new TenantApprovalChangedIntegrationEvent(notification.Id, notification.TenantIdentifier, notification.TenantStatus),
                    TenantStatus.PendingApproval => new TenantApprovalChangedIntegrationEvent(notification.Id, notification.TenantIdentifier, notification.TenantStatus),
                    TenantStatus.Rejected => new TenantApprovalChangedIntegrationEvent(notification.Id, notification.TenantIdentifier, notification.TenantStatus),
                    TenantStatus.Active => new TenantActivatedIntegrationEvent(notification.Id, notification.TenantIdentifier,notification.PreviousStatus),
                    TenantStatus.Inactive => new TenantDeactivatedIntegrationEvent(notification.Id, notification.TenantIdentifier, notification.PreviousStatus),
                    TenantStatus.PendingToActive => new TenantRequestActiveIntegrationEvent(notification.Id, notification.TenantIdentifier, notification.PreviousStatus),
                    TenantStatus.Suspended => new TenantSuspendedIntegrationEvent(notification.Id, notification.TenantIdentifier, notification.PreviousStatus),
                    TenantStatus.Abandoned => new TenantAbandonedIntegrationEvent(notification.Id, notification.TenantIdentifier, notification.PreviousStatus),
                    _ => null
                };
            if (integrationEvent != null)
            {
                await _outbox.AddEventAsync(integrationEvent);
            }
        }
    }
}
