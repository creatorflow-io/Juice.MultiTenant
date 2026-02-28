using Juice.Messaging;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant Deleted Integration Event
    /// </summary>
    public record TenantDeletedIntegrationEvent : IntegrationEvent
    {
        public override string EventName => TenantEventNameConstants.TenantDeleted;
        public string DeletedTenantId { get; set; }
        public string TenantIdentifier { get; init; }
        public string? TenantName { get; init; }
        /// <summary>
        /// Initializes a new instance of the <see cref="TenantDeletedIntegrationEvent"/> class.
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="tenantIdentifier"></param>
        /// <param name="tenantName"></param>
        public TenantDeletedIntegrationEvent(string tenantId, string tenantIdentifier, string? tenantName)
        {
            DeletedTenantId = tenantId;
            TenantIdentifier = tenantIdentifier;
            TenantName = tenantName;
        }
    }
}
