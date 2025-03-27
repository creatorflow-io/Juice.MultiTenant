using Juice.EventBus;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant abandoned event
    /// </summary>
    /// <param name="TenantId"></param>
    /// <param name="TenantIdentifier"></param>
    public record TenantAbandonedIntegrationEvent(string TenantId, string TenantIdentifier) : IntegrationEvent, IMultiTenantIntegrationEvent;
}
