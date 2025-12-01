using Juice.Domain;

namespace Juice.MultiTenant.Api.CommandHandlers.Tenants
{
    public class ResetTenantOwnerCommandHandler
        : IRequestHandler<ResetTenantOwnerCommand, IOperationResult>
    {
        private readonly IUnitOfWork<Tenant> _unitOfWork;
        private readonly ILogger _logger;
        private readonly IOwnerResolver _ownerResolver;

        public ResetTenantOwnerCommandHandler(IUnitOfWork<Tenant> unitOfWork
            , ILogger<ResetTenantOwnerCommandHandler> logger
            , IOwnerResolver ownerResolver
            )
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _ownerResolver = ownerResolver;
        }
        public async ValueTask<IOperationResult> Handle(ResetTenantOwnerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var tenant = await _unitOfWork.FindAsync(ti => ti.Id == request.Id, cancellationToken);
                if (tenant == null)
                {
                    return OperationResult.Failed("Tenant not found");
                }

                var owner = await _ownerResolver.GetOwnerAsync(request.OwnerUser);
                if (owner == null)
                {
                    return OperationResult.Failed("Owner not found");
                }

                tenant.SetOwner(owner);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return OperationResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to reset tenant {id}'s owner. {message}", request.Id, ex.Message);
                _logger.LogTrace(ex, "Failed to reset tenant {id}'s owner.", request.Id);
                return OperationResult.Failed(ex, $"Failed to reset tenant's owner {request.Id}. {ex.Message}");
            }
        }
    }


    public class ResetTenantOwnerIdentifiedCommandHandler
        : IdentifiedCommandHandler<ResetTenantOwnerCommand, IOperationResult>
    {
        public ResetTenantOwnerIdentifiedCommandHandler(IMediator mediator,
            IRequestManager requestManager, ILogger<ResetTenantOwnerIdentifiedCommandHandler> logger)
          : base(mediator, requestManager, logger)
        {
        }

        protected override ValueTask<IOperationResult> CreateResultForDuplicatedRequestAsync(ResetTenantOwnerCommand mesage)
            => ValueTask.FromResult(OperationResult.Success);
        protected override (string IdProperty, string CommandId) ExtractDebugInfo(ResetTenantOwnerCommand command)
            => (nameof(command.Id), command.Id);
    }
}
