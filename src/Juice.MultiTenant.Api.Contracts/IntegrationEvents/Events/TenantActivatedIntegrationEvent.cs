using Juice.EventBus;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant activated integration event
    /// </summary>
    /// <param name="TenantIdentifier"></param>
    public record TenantActivatedIntegrationEvent(string TenantIdentifier) : IntegrationEvent;
}
