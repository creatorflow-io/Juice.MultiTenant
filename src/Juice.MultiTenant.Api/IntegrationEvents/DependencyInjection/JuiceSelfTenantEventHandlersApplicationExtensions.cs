using Finbuckle.MultiTenant.Abstractions;
using Juice.EventBus;
using Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events;
using Juice.MultiTenant.Api.IntegrationEvents.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Juice.MultiTenant.Api
{
    public static class JuiceSelfTenantEventHandlersApplicationExtensions
    {
        public static Task RegisterTenantIntegrationEventSelfHandlersAsync(this WebApplication app)
        {
            return app.Services.RegisterTenantIntegrationEventSelfHandlersAsync<Tenant>();
        }

        public static async Task RegisterTenantIntegrationEventSelfHandlersAsync<TTenantInfo>(this IServiceProvider sp)
            where TTenantInfo : class, ITenantInfo, new()
        {
            var eventBus = sp.GetRequiredService<IEventBus>();

            await eventBus.SubscribeAsync<TenantActivatedIntegrationEvent, TenantActivatedIngtegrationEventSelfHandler<TTenantInfo>>();
            await eventBus.SubscribeAsync<TenantDeactivatedIntegrationEvent, TenantDeactivatedIngtegrationEventSelfHandler<TTenantInfo>>();
            await eventBus.SubscribeAsync<TenantSuspendedIntegrationEvent, TenantSuspendedIngtegrationEventSelfHandler<TTenantInfo>>();
            await eventBus.SubscribeAsync<TenantInitializationChangedIntegrationEvent, TenantInitializingIntegrationEventSelfHandler>();
        }
    }
}
