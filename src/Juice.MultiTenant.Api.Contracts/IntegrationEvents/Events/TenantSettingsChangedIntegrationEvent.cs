using Juice.EventBus;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant Settings Changed Integration Event
    /// </summary>
    /// <param name="TenantIdentifier"></param>
    public record TenantSettingsChangedIntegrationEvent(string TenantIdentifier) : IntegrationEvent;
}
