
using Juice.Domain;
using Juice.MultiTenant.Shared.Enums;

namespace Juice.MultiTenant.Domain.CommandHandlers.Tenants
{
    public class ApprovalProcessCommandHandler
        : IRequestHandler<ApprovalProcessCommand, IOperationResult>
    {
        private readonly IUnitOfWork<Tenant> _unitOfWork;
        private readonly ILogger _logger;

        public ApprovalProcessCommandHandler(IUnitOfWork<Tenant> unitOfWork
            , ILogger<AdminStatusCommandHandler> logger
            )
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<IOperationResult> Handle(ApprovalProcessCommand request, CancellationToken cancellationToken)
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
                    case TenantStatus.Approved:
                        tenant.Approved();
                        break;
                    case TenantStatus.Rejected:
                        tenant.Rejected();
                        break;
                    case TenantStatus.PendingApproval:
                        tenant.RequestApproval();
                        break;
                    default:
                        return OperationResult.Failed($"Invalid status {request.Status}");
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return OperationResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to change the approval status {id}. {message}", request.Id, ex.Message);
                _logger.LogTrace(ex, "Failed to change the approval status {id}.", request.Id);
                return OperationResult.Failed(ex, $"Failed to change the approval status {request.Id}. {ex.Message}");
            }
        }
    }


    public class ApprovalProcessIdentifiedCommandHandler
        : IdentifiedCommandHandler<ApprovalProcessCommand>
    {
        public ApprovalProcessIdentifiedCommandHandler(IMediator mediator,
            IRequestManager requestManager, ILogger<ApprovalProcessIdentifiedCommandHandler> logger)
          : base(mediator, requestManager, logger)
        {
        }

        protected override Task<IOperationResult> CreateResultForDuplicateRequestAsync(IdentifiedCommand<ApprovalProcessCommand> mesage)
            => Task.FromResult((IOperationResult)OperationResult.Success);
        protected override (string IdProperty, string CommandId) ExtractInfo(ApprovalProcessCommand command)
            => (nameof(command.Id), command.Id);
    }
}
