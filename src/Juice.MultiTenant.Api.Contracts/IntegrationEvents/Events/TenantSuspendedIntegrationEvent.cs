using Juice.MultiTenant.Shared.Enums;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant Suspended Integration Event
    /// </summary>
    public record TenantSuspendedIntegrationEvent : TenantStatusChangedIntegrationEvent
    {
        public TenantSuspendedIntegrationEvent(string tenantId, string tenantIdentifier, TenantStatus previousStatus)
            : base(tenantId, tenantIdentifier, previousStatus, TenantStatus.Suspended)
        {
        }
    }
}
