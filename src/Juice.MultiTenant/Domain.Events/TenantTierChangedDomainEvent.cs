
namespace Juice.MultiTenant.Domain.Events
{
    public record TenantTierChangedDomainEvent : MessageBase, INotification
    {
        public string Id { get; init; }
        public string TenantIdentifier { get; private set; }
        public string TenantTier { get; private set; }
        public TenantTierChangedDomainEvent(string tenantId, string tenantIdentifier, string tenantTier)
        {
            Id = tenantId;
            TenantId = tenantId;
            TenantIdentifier = tenantIdentifier;
            TenantTier = tenantTier;
        }
    }
}
