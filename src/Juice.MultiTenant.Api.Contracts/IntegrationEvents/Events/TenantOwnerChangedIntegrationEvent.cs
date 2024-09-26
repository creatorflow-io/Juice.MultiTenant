using Juice.EventBus;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant Owner Changed Integration Event
    /// </summary>
    /// <param name="TenantIdentifier"></param>
    /// <param name="OrignalOwnerId"></param>
    /// <param name="CurrentOwnerId"></param>
    public record TenantOwnerChangedIntegrationEvent(string TenantIdentifier, string? OrignalOwnerId, string? CurrentOwnerId) : IntegrationEvent;
}
