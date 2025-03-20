using Juice.EventBus;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant created integration event
    /// </summary>
    /// <param name="TenantIdentifier"></param>
    /// <param name="CreateAdminUser"></param>
    /// <param name="CreateAdminPassword"></param>
    /// <param name="CreateAdminEmail"></param>
    public record TenantCreatedIntegrationEvent(string TenantIdentifier,
        string? CreateAdminUser,
        string? CreateAdminPassword,
        string? CreateAdminEmail
        ) : IntegrationEvent(Guid.NewGuid(), DateTime.UtcNow), IMultiTenantIntegrationEvent
    {
        /// <inheritdoc/>
        public string? TenantId => TenantIdentifier;
    }
}
