using Juice.EventBus;
using Juice.MultiTenant.Shared.Enums;

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events
{
    /// <summary>
    /// Tenant approval changed integration event
    /// </summary>
    /// <param name="TenantIdentifier"></param>
    /// <param name="Status"></param>
    public record TenantApprovalChangedIntegrationEvent(string TenantIdentifier, TenantStatus Status) : IntegrationEvent;
}
