using Juice.EventBus;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    public record TenantClassChangedIntegrationEvent(string TenantIdentifier, string TenantClass)
        : IntegrationEvent;

}
