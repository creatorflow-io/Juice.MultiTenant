using Juice.Messaging;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant Properties Changed Integration Event: the tenant's Id (<c>TenantId</c>), which services key on, and its Identifier.
    /// </summary>
    public record TenantPropertiesChangedIntegrationEvent : IntegrationEvent
    {
        public override string EventName => TenantEventNameConstants.TenantPropertiesChanged;

        public string TenantIdentifier { get; init; }

        public TenantPropertiesChangedIntegrationEvent(string tenantId, string tenantIdentifier)
        {
            TenantId = tenantId;
            TenantIdentifier = tenantIdentifier;
        }
    }
}
