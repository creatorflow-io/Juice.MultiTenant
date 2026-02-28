using Juice.Messaging;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant Owner Changed Integration Event
    /// </summary>
    public record TenantOwnerChangedIntegrationEvent
        : IntegrationEvent
    {
        public override string EventName => TenantEventNameConstants.TenantOwnerChanged;
        public string TenantIdentifier { get; init; }
        public string? OriginalOwnerId { get; init; }
        public string? CurrentOwnerId { get; init; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="tenantIdentifier"></param>
        /// <param name="originalOwnerId"></param>
        /// <param name="currentOwnerId"></param>
        public TenantOwnerChangedIntegrationEvent(string tenantId, string tenantIdentifier, string? originalOwnerId, string? currentOwnerId)
        {
            TenantId = tenantId;
            TenantIdentifier = tenantIdentifier;
            OriginalOwnerId = originalOwnerId;
            CurrentOwnerId = currentOwnerId;
        }
    }
}
