using Juice.Messaging;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant Settings Changed Integration Event
    /// </summary>
    public record TenantSettingsChangedIntegrationEvent: IntegrationEvent
    {
        public override string EventName => TenantEventNameConstants.TenantSettingsChanged;
        public string TenantIdentifier { get; init; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="tenantIdentifier"></param>
        public TenantSettingsChangedIntegrationEvent(string tenantId, string tenantIdentifier)
        {
            TenantId = tenantId;
            TenantIdentifier = tenantIdentifier;
        }
    }
}
