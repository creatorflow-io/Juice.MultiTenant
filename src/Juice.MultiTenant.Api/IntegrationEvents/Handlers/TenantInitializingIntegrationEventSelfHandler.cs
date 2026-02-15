using Juice.EventBus;
using Juice.MultiTenant.Api.Contracts.IntegrationEvents.Events;

namespace Juice.MultiTenant.Api.IntegrationEvents.Handlers
{
    internal class TenantInitializingIntegrationEventSelfHandler : IIntegrationEventHandler<TenantInitializationChangedIntegrationEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public TenantInitializingIntegrationEventSelfHandler(IMediator mediator, ILogger<TenantInitializingIntegrationEventSelfHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task HandleAsync(TenantInitializationChangedIntegrationEvent @event)
        {
            if(@event.TenantId == null)
            {
                _logger.LogError("TenantId is null or empty for tenant initialization changed event. TenantIdentifier: {tenantIdentifier}", @event.TenantIdentifier);
                return;
            }
            var command = new InitializationProcessCommand(@event.TenantId, @event.Status, @event.MessageId.ToString());

            var rs = await _mediator.Send(command);
            if (!rs.Succeeded)
            {
                _logger.LogError("Failed to change initialization state {id}. {message}", @event.TenantIdentifier, rs.Message);
            }
        }
    }
}
