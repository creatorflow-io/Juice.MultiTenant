using Juice.Domain;

namespace Juice.MultiTenant.Api.CommandHandlers.Tenants
{
    public class UpdateClassCommandHandler
        : IRequestHandler<UpdateClassCommand, IOperationResult>
    {
        private readonly IUnitOfWork<Tenant> _unitOfWork;
        private readonly ILogger _logger;

        public UpdateClassCommandHandler(IUnitOfWork<Tenant> unitOfWork
            , ILogger<UpdateClassCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async ValueTask<IOperationResult> Handle(UpdateClassCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var tenant = await _unitOfWork.FindAsync(ti => ti.Id == request.Id, cancellationToken);
                if (tenant == null)
                {
                    return OperationResult.Failed("Tenant not found");
                }
                tenant.UpdateClass(request.TenantClass);
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

    public class UpdateClassIdentifiedCommandHandler
        : IdentifiedCommandHandler<UpdateClassCommand, IOperationResult>
    {
        public UpdateClassIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager, ILogger<UpdateClassIdentifiedCommandHandler> logger)
            : base(mediator, requestManager, logger)
        {
        }

        protected override ValueTask<IOperationResult> CreateResultForDuplicatedRequestAsync(UpdateClassCommand mesage)
            => ValueTask.FromResult(OperationResult.Success);

        protected override (string IdProperty, string CommandId) ExtractDebugInfo(UpdateClassCommand command) => (nameof(command.Id), command.Id);
    }
}
