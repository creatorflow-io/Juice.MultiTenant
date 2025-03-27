using Juice.EventBus;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant activated integration event
    /// </summary>
    /// <param name="TenantId"></param>
    /// <param name="TenantIdentifier"></param>
    public record TenantActivatedIntegrationEvent(string TenantId, string TenantIdentifier) : IntegrationEvent, IMultiTenantIntegrationEvent;
}
