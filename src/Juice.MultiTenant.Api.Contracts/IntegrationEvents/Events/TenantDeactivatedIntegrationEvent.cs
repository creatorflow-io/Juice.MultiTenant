using Juice.EventBus;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant Deactivated Integration Event
    /// </summary>
    /// <param name="TenantId"></param>
    /// <param name="TenantIdentifier"></param>
    public record TenantDeactivatedIntegrationEvent(string TenantId, string TenantIdentifier) : IntegrationEvent, IMultiTenantIntegrationEvent;
}
