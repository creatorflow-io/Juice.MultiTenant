using System.Runtime.CompilerServices;
using Juice.Domain;

namespace Juice.MultiTenant.Domain.CommandHandlers.Tenants
{
    public class AbandonTenantCommandHandler
        : IRequestHandler<AbandonTenantCommand, IOperationResult>
    {
        private readonly IUnitOfWork<Tenant> _unitOfWork;
        private readonly ILogger _logger;

        public AbandonTenantCommandHandler(IUnitOfWork<Tenant> unitOfWork
            , ILogger<AbandonTenantCommandHandler> logger
            )
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async ValueTask<IOperationResult> Handle(AbandonTenantCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var tenant = await _unitOfWork.FindAsync(t => t.Id == request.Id, cancellationToken);
                if (tenant == null)
                {
                    return OperationResult.Failed("Tenant not found");
                }
                tenant.Abandon();
                await _unitOfWork.SaveChangesAsync();

                return OperationResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to abandon tenant {id}. {message}", request.Id, ex.Message);
                _logger.LogTrace(ex, "Failed to abandon tenant {id}.", request.Id);
                return OperationResult.Failed(ex, $"Failed to abandon tenant {request.Id}. {ex.Message}");
            }
        }
    }

    public class AbandonTenantIdentifiedCommandHandler
        : IdentifiedCommandHandler<AbandonTenantCommand, IOperationResult>
    {
        public AbandonTenantIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager, ILogger<AbandonTenantIdentifiedCommandHandler> logger)
          : base(mediator, requestManager, logger)
        {
        }

        protected override ValueTask<IOperationResult> CreateResultForDuplicatedRequestAsync(AbandonTenantCommand message) 
            => ValueTask.FromResult(OperationResult.Success);
        protected override (string? IdProperty, string? CommandId) ExtractDebugInfo(AbandonTenantCommand command) 
            => (nameof(command.Id), command.Id);
    }
}
