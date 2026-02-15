
namespace Juice.MultiTenant.Domain.Events
{
    public record TenantCreatedDomainEvent : MessageBase, INotification
    {
        public new string TenantId { get; init; }
        public string TenantIdentifier { get; init; }
        public string? CreateAdminUser { get; init; }
        public string? CreateAdminPassword { get; init; }
        public string? CreateAdminEmail { get; init; }

        public TenantCreatedDomainEvent(string tenantId,
            string tenantIdentifier,
            string? createAdminUser,
            string? createAdminPassword,
            string? createAdminEmail)
        {
            TenantId = tenantId;
            TenantIdentifier = tenantIdentifier;
            CreateAdminUser = createAdminUser;
            CreateAdminPassword = createAdminPassword;
            CreateAdminEmail = createAdminEmail;
        }
    }
}
