using Juice.Domain;

namespace Juice.MultiTenant.Api.CommandHandlers.Tenants
{
    public class UpdateTenantCommandHandler
        : IRequestHandler<UpdateTenantCommand, IOperationResult>
    {
        private readonly IUnitOfWork<Tenant> _unitOfWork;
        private readonly ILogger _logger;

        public UpdateTenantCommandHandler(IUnitOfWork<Tenant> unitOfWork
            , ILogger<UpdateTenantCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<IOperationResult> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var tenant = await _unitOfWork.FindAsync(ti => ti.Id == request.Id, cancellationToken);
                if (tenant == null)
                {
                    return OperationResult.Failed("Tenant not found");
                }
                tenant.Update(request.Name, request.Identifier);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return OperationResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to update tenant {id}. {message}", request.Id, ex.Message);
                _logger.LogTrace(ex, "Failed to update tenant {id}.", request.Id);
                return OperationResult.Failed(ex, $"Failed to update tenant {request.Id}. {ex.Message}");
            }
        }
    }

    public class UpdateTenantIdentifiedCommandHandler
        : IdentifiedCommandHandler<UpdateTenantCommand, IOperationResult>
    {
        public UpdateTenantIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager, ILogger<UpdateTenantIdentifiedCommandHandler> logger)
            : base(mediator, requestManager, logger)
        {
        }

        protected override Task<IOperationResult> CreateResultForDuplicatedRequestAsync(UpdateTenantCommand mesage)
            => Task.FromResult(OperationResult.Success);

        protected override (string IdProperty, string CommandId) ExtractDebugInfo(UpdateTenantCommand command) => (nameof(command.Id), command.Id);
    }
}
