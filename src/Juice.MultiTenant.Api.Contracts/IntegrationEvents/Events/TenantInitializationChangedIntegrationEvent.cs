using Juice.EventBus;
using Juice.MultiTenant.Shared.Enums;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant Initialization Changed Integration Event
    /// </summary>
    /// <param name="TenantIdentifier"></param>
    /// <param name="Status"></param>
    public record TenantInitializationChangedIntegrationEvent(string TenantIdentifier, TenantStatus Status) : IntegrationEvent;
}
