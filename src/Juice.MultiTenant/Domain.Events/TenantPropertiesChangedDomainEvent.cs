
namespace Juice.MultiTenant.Domain.Events
{
    public record TenantPropertiesChangedDomainEvent : MessageBase, INotification
    {
        public string TenantIdentifier { get; private set; }
        public TenantPropertiesChangedDomainEvent(string tenantId, string tenantIdentifier)
        {
            TenantId = tenantId;
            TenantIdentifier = tenantIdentifier;
        }
    }
}
