using Juice.MultiTenant.Shared.Enums;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant Deactivated Integration Event
    /// </summary>
    public record TenantDeactivatedIntegrationEvent : TenantStatusChangedIntegrationEvent
    {
        public TenantDeactivatedIntegrationEvent(string tenantId, string tenantIdentifier, TenantStatus previousStatus)
            : base(tenantId, tenantIdentifier, previousStatus, TenantStatus.Inactive)
        {
        }
    }
}
