using Juice.EventBus;
using Juice.Extensions;
using Juice.MultiTenant.Shared.Enums;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant approval changed integration event
    /// </summary>
    public record TenantApprovalChangedIntegrationEvent : IntegrationEvent
    {
        public override string EventName => "tenant.approval." + Status.StringValue().ToLower();
        public string TenantIdentifier { get; init; }
        public TenantStatus Status { get; init; }

        /// <summary>
        /// Tenant approval changed integration event constructor
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="tenantIdentifier"></param>
        /// <param name="status"></param>
        public TenantApprovalChangedIntegrationEvent(string tenantId, string tenantIdentifier, TenantStatus status)
        {
            TenantId = tenantId;
            TenantIdentifier = tenantIdentifier;
            Status = status;
        }
    }
}
