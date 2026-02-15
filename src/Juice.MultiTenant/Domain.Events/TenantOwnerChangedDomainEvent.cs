
namespace Juice.MultiTenant.Domain.Events
{
    public record TenantOwnerChangedDomainEvent : MessageBase, INotification
    {
        public string Id { get; init; }
        public string TenantIdentifier { get; init; }
        public string? FromUser { get; init; }
        public string? ToUser { get; init; }

        public TenantOwnerChangedDomainEvent(string tenantId, string tenantIdentifier, string? fromUser, string? toUser)
        {
            Id = tenantId;
            TenantId = tenantId;
            TenantIdentifier = tenantIdentifier;
            FromUser = fromUser;
            ToUser = toUser;
        }
    }
}
