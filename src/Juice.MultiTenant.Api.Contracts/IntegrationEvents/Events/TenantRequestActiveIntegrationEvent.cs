using Juice.EventBus;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant Request Active Integration Event
    /// </summary>
    /// <param name="TenantId"></param>
    /// <param name="TenantIdentifier"></param>
    public record TenantRequestActiveIntegrationEvent(string TenantId, string TenantIdentifier) : IntegrationEvent, IMultiTenantIntegrationEvent;
}
