using Juice.EventBus;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant class changed integration event
    /// </summary>
    /// <param name="TenantIdentifier"></param>
    /// <param name="TenantClass"></param>
    public record TenantClassChangedIntegrationEvent(string TenantIdentifier, string TenantClass)
        : IntegrationEvent;

}
