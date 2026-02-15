using Juice.MultiTenant.Shared.Enums;

namespace Juice.MultiTenant.Domain.Events
{
    public record TenantStatusChangedDomainEvent : MessageBase, INotification
    {
        public string Id { get; init; }
        public string TenantIdentifier { get; private set; }
        public TenantStatus PreviousStatus { get; private set; }
        public TenantStatus TenantStatus { get; private set; }
        public TenantStatusChangedDomainEvent(string tenantId, string tenantIdentifier, TenantStatus previousStatus, TenantStatus tenantStatus)
        {
            Id = tenantId;
            TenantId = tenantId;
            TenantIdentifier = tenantIdentifier;
            PreviousStatus = previousStatus;
            TenantStatus = tenantStatus;
        }
    }
}
