using Juice.EventBus;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant Request Active Integration Event
    /// </summary>
    /// <param name="TenantIdentifier"></param>
    public record TenantRequestActiveIntegrationEvent(string TenantIdentifier) : IntegrationEvent;
}
