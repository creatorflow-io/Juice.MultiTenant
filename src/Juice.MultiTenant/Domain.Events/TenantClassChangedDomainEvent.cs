using MediatR;

namespace Juice.MultiTenant.Domain.Events
{
    public class TenantClassChangedDomainEvent : INotification
    {
        public string TenantId { get; private set; }
        public string TenantIdentifier { get; private set; }
        public string TenantClass { get; private set; }
        public TenantClassChangedDomainEvent(string tenantId, string tenantIdentifier, string tenantClass)
        {
            TenantId = tenantId;
            TenantIdentifier = tenantIdentifier;
            TenantClass = tenantClass;
        }
    }
}
