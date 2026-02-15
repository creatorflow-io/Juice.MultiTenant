using Juice.MultiTenant.Shared.Enums;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant Request Active Integration Event
    /// </summary>
    public record TenantRequestActiveIntegrationEvent : TenantStatusChangedIntegrationEvent
    {
        public TenantRequestActiveIntegrationEvent(string tenantId, string tenantIdentifier, TenantStatus previousStatus)
            : base(tenantId, tenantIdentifier, previousStatus, TenantStatus.PendingToActive)
        {
        }
    }
}
