using Juice.Domain;
using Juice.MultiTenant.Shared.Enums;

namespace Juice.MultiTenant.Api.CommandHandlers.Tenants
{
    public class OperationStatusCommandHandler
        : IRequestHandler<OperationStatusCommand, IOperationResult>
    {
        private readonly IUnitOfWork<Tenant> _unitOfWork;
        private readonly ILogger _logger;

        public OperationStatusCommandHandler(IUnitOfWork<Tenant> unitOfWork
            , ILogger<OperationStatusCommandHandler> logger
            )
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async ValueTask<IOperationResult> Handle(OperationStatusCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var tenant = await _unitOfWork.FindAsync(ti => ti.Id == request.Id, cancellationToken);
                if (tenant == null)
                {
                    return OperationResult.Failed("Tenant not found");
                }

                switch (request.Status)
                {
                    case TenantStatus.Active:
                        tenant.Reactivate();
                        break;
                    case TenantStatus.Inactive:
                        tenant.Deactivate();
                        break;
                    case TenantStatus.Suspended:
                        tenant.Suspend();
                        break;
                    default:
                        throw new InvalidOperationException($"Request status {request.Status} is not valid.");
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return OperationResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to change the operation status {id}. {message}", request.Id, ex.Message);
                _logger.LogTrace(ex, "Failed to change the operation status {id}.", request.Id);
                return OperationResult.Failed(ex, $"Failed to change the operation status {request.Id}. {ex.Message}");
            }
        }
    }


    public class OperationStatusIdentifiedCommandHandler
        : IdentifiedCommandHandler<OperationStatusCommand, IOperationResult>
    {
        public OperationStatusIdentifiedCommandHandler(IMediator mediator,
            IRequestManager requestManager, ILogger<OperationStatusIdentifiedCommandHandler> logger)
          : base(mediator, requestManager, logger)
        {
        }

        protected override ValueTask<IOperationResult> CreateResultForDuplicatedRequestAsync(OperationStatusCommand mesage)
            => ValueTask.FromResult(OperationResult.Success);
        protected override (string IdProperty, string CommandId) ExtractDebugInfo(OperationStatusCommand command)
            => (nameof(command.Id), command.Id);
    }
}
