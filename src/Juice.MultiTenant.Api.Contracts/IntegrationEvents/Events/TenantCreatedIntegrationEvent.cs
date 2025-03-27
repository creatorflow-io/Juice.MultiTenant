using Juice.EventBus;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant created integration event
    /// </summary>
    /// <param name="TenantId"></param>
    /// <param name="TenantIdentifier"></param>
    /// <param name="CreateAdminUser"></param>
    /// <param name="CreateAdminPassword"></param>
    /// <param name="CreateAdminEmail"></param>
    public record TenantCreatedIntegrationEvent(string TenantId, string TenantIdentifier,
        string? CreateAdminUser,
        string? CreateAdminPassword,
        string? CreateAdminEmail
        ) : IntegrationEvent(Guid.NewGuid(), DateTime.UtcNow), IMultiTenantIntegrationEvent;
}
