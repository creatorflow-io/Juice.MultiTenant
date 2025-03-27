using Juice.EventBus;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant Properties Changed Integration Event
    /// </summary>
    /// <param name="TenantIdentifier"></param>
    public record TenantPropertiesChangedIntegrationEvent(string TenantIdentifier) : IntegrationEvent;
}
