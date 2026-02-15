
namespace Juice.MultiTenant.Domain.Events
{
    public record TenantDeletedDomainEvent : MessageBase, INotification
    {
        public string TenantIdentifier { get; init; }
        public string? TenantName { get; init; }

        public TenantDeletedDomainEvent(string tenantId, string tenantIdentifier, string? tenantName)
        {
            TenantId = tenantId;
            TenantIdentifier = tenantIdentifier;
            TenantName = tenantName;
        }
    }
}
