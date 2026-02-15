using Finbuckle.MultiTenant.Abstractions;
using Juice.EventBus;
using Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events;
using Juice.MultiTenant.Api.IntegrationEvents.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Juice.MultiTenant.Api
{
    public static class JuiceSelfTenantEventHandlersApplicationExtensions
    {
        public static void RegisterTenantIntegrationEventSelfHandlers<TTenantInfo>(this EventBusBuilder eventBus)
            where TTenantInfo : class, ITenantInfo, new()
        {
            eventBus.AddConsumerServices(subs =>
            {
                subs.Subscribe<TenantActivatedIntegrationEvent, TenantActivatedIngtegrationEventSelfHandler<TTenantInfo>>();
                subs.Subscribe<TenantDeactivatedIntegrationEvent, TenantDeactivatedIngtegrationEventSelfHandler<TTenantInfo>>();
                subs.Subscribe<TenantSuspendedIntegrationEvent, TenantSuspendedIngtegrationEventSelfHandler<TTenantInfo>>();
                subs.Subscribe<TenantInitializationChangedIntegrationEvent, TenantInitializingIntegrationEventSelfHandler>();
            });
        }
    }
}
