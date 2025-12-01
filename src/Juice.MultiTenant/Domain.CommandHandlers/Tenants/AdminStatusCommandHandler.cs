
using Juice.Domain;
using Juice.MultiTenant.Shared.Enums;

namespace Juice.MultiTenant.Domain.CommandHandlers.Tenants
{
    /// <summary>
    /// Handle tenant status for the admin.
    /// </summary>
    public class AdminStatusCommandHandler
        : IRequestHandler<AdminStatusCommand, IOperationResult>
    {
        private readonly IUnitOfWork<Tenant> _unitOfWork;
        private readonly ILogger _logger;

        public AdminStatusCommandHandler(IUnitOfWork<Tenant> unitOfWork
            , ILogger<AdminStatusCommandHandler> logger
            )
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async ValueTask<IOperationResult> Handle(AdminStatusCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var tenant = await _unitOfWork.FindAsync(t => t.Id == request.Id, cancellationToken);
                if (tenant == null)
                {
                    return OperationResult.Failed("Tenant not found");
                }

                switch (request.Status)
                {
                    case TenantStatus.PendingToActive:
                        tenant.RequestActivate();
                        break;
                    case TenantStatus.Active:
                        tenant.Activate();
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
                _logger.LogError("Failed to change the activating status {id}. {message}", request.Id, ex.Message);
                _logger.LogTrace(ex, "Failed to change the activating status {id}.", request.Id);
                return OperationResult.Failed(ex, $"Failed to change the activating status {request.Id}. {ex.Message}");
            }
        }
    }


    public class ActivateProcessIdentifiedCommandHandler
        : IdentifiedCommandHandler<AdminStatusCommand, IOperationResult>
    {
        public ActivateProcessIdentifiedCommandHandler(IMediator mediator,
            IRequestManager requestManager, ILogger<ActivateProcessIdentifiedCommandHandler> logger)
          : base(mediator, requestManager, logger)
        {
        }

        protected override ValueTask<IOperationResult> CreateResultForDuplicatedRequestAsync(AdminStatusCommand message) 
            => ValueTask.FromResult(OperationResult.Success);
        protected override (string IdProperty, string CommandId) ExtractDebugInfo(AdminStatusCommand command)
            => (nameof(command.Id), command.Id);
    }
}
