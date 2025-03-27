using Juice.EventBus;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant class changed integration event
    /// </summary>
    /// <param name="TenantId"></param>
    /// <param name="TenantIdentifier"></param>
    /// <param name="TenantClass"></param>
    public record TenantClassChangedIntegrationEvent(string TenantId, string TenantIdentifier, string TenantClass)
        : IntegrationEvent, IMultiTenantIntegrationEvent;

}
