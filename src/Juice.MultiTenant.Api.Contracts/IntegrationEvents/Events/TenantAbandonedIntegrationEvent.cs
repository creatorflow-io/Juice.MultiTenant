using Juice.MultiTenant.Shared.Enums;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant abandoned event
    /// </summary>
    public record TenantAbandonedIntegrationEvent : TenantStatusChangedIntegrationEvent
    {
        public TenantAbandonedIntegrationEvent(string tenantId, string tenantIdentifier, TenantStatus previousStatus)
            : base(tenantId, tenantIdentifier, previousStatus, TenantStatus.Abandoned)
        {
        }
    }
}
