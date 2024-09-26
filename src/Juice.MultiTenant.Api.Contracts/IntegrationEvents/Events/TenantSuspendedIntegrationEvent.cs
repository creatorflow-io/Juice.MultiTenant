using Juice.EventBus;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant Suspended Integration Event
    /// </summary>
    /// <param name="TenantIdentifier"></param>
    public record TenantSuspendedIntegrationEvent(string TenantIdentifier) : IntegrationEvent;

}
