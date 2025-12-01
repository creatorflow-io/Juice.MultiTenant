
using Juice.Domain;
using Juice.MultiTenant.Shared.Enums;

namespace Juice.MultiTenant.Api.CommandHandlers.Tenants
{
    public class InitializationProcessCommandHandler
        : IRequestHandler<InitializationProcessCommand, IOperationResult>
    {
        private readonly IUnitOfWork<Tenant> _unitOfWork;
        private readonly ILogger _logger;

        public InitializationProcessCommandHandler(IUnitOfWork<Tenant> unitOfWork
            , ILogger<InitializationProcessCommandHandler> logger
            )
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async ValueTask<IOperationResult> Handle(InitializationProcessCommand request, CancellationToken cancellationToken)
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
                    case TenantStatus.Initializing:
                        tenant.Initializing();
                        break;
                    case TenantStatus.Initialized:
                        tenant.Initialized();
                        break;
                    default:
                        return OperationResult.Failed($"Invalid state {request.Status}");
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return OperationResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to change initialization state {id}. {message}", request.Id, ex.Message);
                _logger.LogTrace(ex, "Failed to change initialization state {id}.", request.Id);
                return OperationResult.Failed(ex, $"Failed to change initialization state {request.Id}. {ex.Message}");
            }
        }
    }


    public class InitializationProcessIdentifiedCommandHandler
        : IdentifiedCommandHandler<InitializationProcessCommand, IOperationResult>
    {
        public InitializationProcessIdentifiedCommandHandler(IMediator mediator,
            IRequestManager requestManager, ILogger<InitializationProcessIdentifiedCommandHandler> logger)
          : base(mediator, requestManager, logger)
        {
        }

        protected override ValueTask<IOperationResult> CreateResultForDuplicatedRequestAsync(InitializationProcessCommand mesage)
            => ValueTask.FromResult(OperationResult.Success);
        protected override (string IdProperty, string CommandId) ExtractDebugInfo(InitializationProcessCommand command)
            => (nameof(command.Id), command.Id);
    }
}
