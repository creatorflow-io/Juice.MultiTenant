using Juice.EventBus;
using Juice.MediatR;
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
            var command = new InitializationProcessCommand(@event.TenantIdentifier, @event.Status);

            var identifiedCommand = new IdentifiedCommand<InitializationProcessCommand, IOperationResult>(command, @event.Id);
            var rs = await _mediator.Send(identifiedCommand);
            if (!rs.Succeeded)
            {
                _logger.LogError("Failed to change initialization state {id}. {message}", @event.TenantIdentifier, rs.Message);
            }
        }
    }
}
