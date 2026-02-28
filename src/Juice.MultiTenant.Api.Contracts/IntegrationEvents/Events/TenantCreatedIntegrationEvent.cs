using Juice.Messaging;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant created integration event
    /// </summary>
    public record TenantCreatedIntegrationEvent : IntegrationEvent
    {
        public override string EventName => TenantEventNameConstants.TenantCreated;
        /// <summary>
        /// The created tenant Id
        /// </summary>
        public string Id { get; init; }
        /// <summary>
        /// The tenant identifier
        /// </summary>
        public string TenantIdentifier { get; init; }
        /// <summary>
        /// The admin user to create
        /// </summary>
        public string? CreateAdminUser { get; init; }
        /// <summary>
        /// Initial password for the admin user to create
        /// </summary>
        public string? CreateAdminPassword { get; init; }
        /// <summary>
        /// Set to the email address of the admin user to create
        /// </summary>
        public string? CreateAdminEmail { get; init; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="tenantIdentifier"></param>
        /// <param name="createAdminUser"></param>
        /// <param name="createAdminPassword"></param>
        /// <param name="createAdminEmail"></param>
        public TenantCreatedIntegrationEvent(
            string tenantId,
            string tenantIdentifier,
            string? createAdminUser,
            string? createAdminPassword,
            string? createAdminEmail)
        {
            Id = tenantId;
            TenantId = tenantId;
            TenantIdentifier = tenantIdentifier;
            CreateAdminUser = createAdminUser;
            CreateAdminPassword = createAdminPassword;
            CreateAdminEmail = createAdminEmail;
        }
    }
}
