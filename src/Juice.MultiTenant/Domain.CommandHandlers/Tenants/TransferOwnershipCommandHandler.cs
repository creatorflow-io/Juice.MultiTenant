using Juice.Domain;
using Microsoft.AspNetCore.Http;

namespace Juice.MultiTenant.Api.CommandHandlers.Tenants
{
    public class TransferOwnershipCommandHandler
        : IRequestHandler<TransferOwnershipCommand, IOperationResult>
    {
        private readonly IUnitOfWork<Tenant> _unitOfWork;
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOwnerResolver _ownerResolver;

        public TransferOwnershipCommandHandler(IUnitOfWork<Tenant> unitOfWork
            , ILogger<TransferOwnershipCommandHandler> logger
            , IHttpContextAccessor httpContextAccessor
            , IOwnerResolver ownerResolver
            )
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _ownerResolver = ownerResolver;
        }
        public async Task<IOperationResult> Handle(TransferOwnershipCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var tenant = await _unitOfWork.FindAsync(ti => ti.Id == request.Id, cancellationToken);
                if (tenant == null)
                {
                    return OperationResult.Failed("Tenant not found");
                }

                var principal = _httpContextAccessor.HttpContext?.User;

                var currentOwner = principal != null ?
                    await _ownerResolver.GetOwnerAsync(principal)
                    : default(string);

                var owner = await _ownerResolver.GetOwnerAsync(request.OwnerUser);

                if (owner == null)
                {
                    return OperationResult.Failed("Owner not found");
                }

                tenant.TransferOwner(currentOwner, owner);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return OperationResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to transfer tenant {id}'s owner. {message}", request.Id, ex.Message);
                _logger.LogTrace(ex, "Failed to transfer tenant {id}'s owner.", request.Id);
                return OperationResult.Failed(ex, $"Failed to transfer tenant's owner {request.Id}. {ex.Message}");
            }
        }
    }


    public class TransferOwnershipIdentifiedCommandHandler
        : IdentifiedCommandHandler<TransferOwnershipCommand, IOperationResult>
    {
        public TransferOwnershipIdentifiedCommandHandler(IMediator mediator,
            IRequestManager requestManager, ILogger<TransferOwnershipIdentifiedCommandHandler> logger)
          : base(mediator, requestManager, logger)
        {
        }

        protected override Task<IOperationResult> CreateResultForDuplicatedRequestAsync(TransferOwnershipCommand mesage)
            => Task.FromResult(OperationResult.Success);
        protected override (string IdProperty, string CommandId) ExtractDebugInfo(TransferOwnershipCommand command)
            => (nameof(command.Id), command.Id);
    }
}
