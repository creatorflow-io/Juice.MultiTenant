using Juice.EventBus;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant Deactivated Integration Event
    /// </summary>
    /// <param name="TenantIdentifier"></param>
    public record TenantDeactivatedIntegrationEvent(string TenantIdentifier) : IntegrationEvent;
}
