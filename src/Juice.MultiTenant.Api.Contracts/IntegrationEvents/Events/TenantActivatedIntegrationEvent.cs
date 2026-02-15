using Juice.MultiTenant.Shared.Enums;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant activated integration event
    /// </summary>
    public record TenantActivatedIntegrationEvent : TenantStatusChangedIntegrationEvent
    {
        public TenantActivatedIntegrationEvent(string tenantId, string tenantIdentifier, TenantStatus previousStatus)
            : base(tenantId, tenantIdentifier, previousStatus, TenantStatus.Active)
        {
        }
    }
}
