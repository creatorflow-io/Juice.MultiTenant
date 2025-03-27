using Juice.EventBus;
using Juice.MultiTenant.Shared.Enums;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant Initialization Changed Integration Event
    /// </summary>
    public record TenantInitializationChangedIntegrationEvent : IntegrationEvent, IMultiTenantIntegrationEvent
    {
        /// <summary>
        /// Tenant Id
        /// </summary>
        public string TenantId { get; init; }
        /// <summary>
        /// Tenant Identifier
        /// </summary>
        public string TenantIdentifier { get; init; }
        /// <summary>
        /// Tenant Status
        /// </summary>
        public TenantStatus Status { get; init; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="tenantIdentifier"></param>
        /// <param name="status"></param>
        /// <exception cref="ArgumentException"></exception>
        public TenantInitializationChangedIntegrationEvent(string tenantId, string tenantIdentifier, TenantStatus status)
        {
            ArgumentNullException.ThrowIfNull(tenantId);
            ArgumentNullException.ThrowIfNull(tenantIdentifier);
            if (status != TenantStatus.Initializing && status != TenantStatus.Initialized)
            {
                throw new ArgumentException("Invalid status", nameof(status));
            }
            TenantId = tenantId;
            TenantIdentifier = tenantIdentifier;
            Status = status;
        }
    }
}
