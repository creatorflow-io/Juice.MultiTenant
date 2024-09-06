using Juice.Domain;

namespace Juice.MultiTenant.Api.CommandHandlers.Tenants
{
    public class UpdateTenantPropertiesCommandHandler
        : IRequestHandler<UpdateTenantPropertiesCommand, IOperationResult>
    {
        private readonly IUnitOfWork<Tenant> _unitOfWork;
        private readonly ILogger _logger;

        public UpdateTenantPropertiesCommandHandler(IUnitOfWork<Tenant> unitOfWork
            , ILogger<UpdateTenantPropertiesCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<IOperationResult> Handle(UpdateTenantPropertiesCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var tenant = await _unitOfWork.FindAsync(ti => ti.Id == request.Id, cancellationToken);
                if (tenant == null)
                {
                    return OperationResult.Failed("Tenant not found");
                }
                tenant.UpdateProperties(request.Properties);
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

    public class UpdateTenantPropertiesIdentifiedCommandHandler
        : IdentifiedCommandHandler<UpdateTenantPropertiesCommand, IOperationResult>
    {
        public UpdateTenantPropertiesIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager, ILogger<UpdateTenantPropertiesIdentifiedCommandHandler> logger)
            : base(mediator, requestManager, logger)
        {
        }

        protected override Task<IOperationResult> CreateResultForDuplicatedRequestAsync(UpdateTenantPropertiesCommand mesage)
            => Task.FromResult(OperationResult.Success);

        protected override (string IdProperty, string CommandId) ExtractDebugInfo(UpdateTenantPropertiesCommand command) => (nameof(command.Id), command.Id);
    }
}
