using Juice.Domain;
using Juice.MultiTenant.Domain.Events;

namespace Juice.MultiTenant.Api.CommandHandlers.Tenants
{
    public class DeleteTenantCommandHandler
        : IRequestHandler<DeleteTenantCommand, IOperationResult>
    {
        private readonly IUnitOfWork<Tenant> _unitOfWork;
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public DeleteTenantCommandHandler(IUnitOfWork<Tenant> unitOfWork
            , ILogger<DeleteTenantCommandHandler> logger
            , IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mediator = mediator;
        }
        public async Task<IOperationResult> Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var tenant = await _unitOfWork.FindAsync(ti => ti.Id == request.Id, cancellationToken);
                if (tenant == null)
                {
                    return OperationResult.Result(request.Id, "Tenant not found");
                }

                await _unitOfWork.DeleteAsync(tenant, cancellationToken);

                var evt = new TenantDeletedDomainEvent(tenant.Id, tenant.Identifier, tenant.Name);
                await _mediator.Publish(evt);

                return OperationResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to remove tenant {id}. {message}", request.Id, ex.Message);
                _logger.LogTrace(ex, "Failed to remove tenant {id}.", request.Id);
                return OperationResult.Failed(ex, $"Failed to remove tenant {request.Id}. {ex.Message}");
            }
        }
    }

    // Use for Idempotency in Command process
    public class DeleteTenantIdentifiedCommandHandler
        : IdentifiedCommandHandler<DeleteTenantCommand, IOperationResult>
    {
        public DeleteTenantIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager, ILogger<DeleteTenantIdentifiedCommandHandler> logger)
            : base(mediator, requestManager, logger)
        {
        }

        protected override Task<IOperationResult> CreateResultForDuplicatedRequestAsync(DeleteTenantCommand mesage)
            => Task.FromResult(OperationResult.Success);
        protected override (string IdProperty, string CommandId) ExtractDebugInfo(DeleteTenantCommand command)
            => (nameof(command.Id), command.Id);
    }
}
