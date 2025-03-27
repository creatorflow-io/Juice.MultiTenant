using Juice.EventBus;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant Suspended Integration Event
    /// </summary>
    /// <param name="TenantId"></param>
    /// <param name="TenantIdentifier"></param>
    public record TenantSuspendedIntegrationEvent(string TenantId, string TenantIdentifier) : IntegrationEvent, IMultiTenantIntegrationEvent;

}
