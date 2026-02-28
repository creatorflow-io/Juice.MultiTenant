using Juice.Messaging;
using Juice.Extensions;
using Juice.MultiTenant.Shared.Enums;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    public record TenantStatusChangedIntegrationEvent: IntegrationEvent
    {
        public override string EventName => "tenant.status."+ CurrentStatus.StringValue().ToLower();
        public string TenantIdentifier { get; init; }
        public TenantStatus PreviousStatus { get; init; }
        public TenantStatus CurrentStatus { get; init; }
        public TenantStatusChangedIntegrationEvent(string tenantId, string tenantIdentifier, TenantStatus previousStatus, TenantStatus currentStatus)
        {
            TenantId = tenantId;
            TenantIdentifier = tenantIdentifier;
            PreviousStatus = previousStatus;
            CurrentStatus = currentStatus;
        }
    }
}
