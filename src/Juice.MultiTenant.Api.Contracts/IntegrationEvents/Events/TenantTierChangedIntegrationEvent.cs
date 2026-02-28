using Juice.Messaging;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant class changed integration event
    /// </summary>
    public record TenantTierChangedIntegrationEvent
        : IntegrationEvent
    {
        public override string EventName => TenantEventNameConstants.TenantTierChanged;
        /// <summary>
        /// Tenant identifier
        /// </summary>
        public string TenantIdentifier { get; init; }
        /// <summary>
        /// Tenant tier/class
        /// </summary>
        public string TenantTier { get; init; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="tenantIdentifier"></param>
        /// <param name="tenantTier"></param>
        public TenantTierChangedIntegrationEvent(string tenantId, string tenantIdentifier, string tenantTier)
        {
            TenantId = tenantId;
            TenantIdentifier = tenantIdentifier;
            TenantTier = tenantTier;
        }
    }

}
