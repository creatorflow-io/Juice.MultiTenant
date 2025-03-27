using Juice.EventBus;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant Deleted Integration Event
    /// </summary>
    /// <param name="TenantId"></param>
    /// <param name="TenantIdentifier"></param>
    /// <param name="TenantName"></param>
    public record TenantDeletedIntegrationEvent(string TenantId, string TenantIdentifier, string? TenantName) : IntegrationEvent, IMultiTenantIntegrationEvent;
}
