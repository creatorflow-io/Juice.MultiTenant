using Juice.EventBus;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant Deleted Integration Event
    /// </summary>
    /// <param name="TenantIdentifier"></param>
    /// <param name="TenantName"></param>
    public record TenantDeletedIntegrationEvent(string TenantIdentifier, string? TenantName) : IntegrationEvent;
}
