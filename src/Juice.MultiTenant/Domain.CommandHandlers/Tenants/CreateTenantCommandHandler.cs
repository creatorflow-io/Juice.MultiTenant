using Juice.Domain;
using Juice.MultiTenant.Domain.Events;
using Microsoft.AspNetCore.Http;

namespace Juice.MultiTenant.Domain.CommandHandlers.Tenants
{
    public class CreateTenantCommandHandler
        : IRequestHandler<CreateTenantCommand, IOperationResult>
    {
        private readonly IUnitOfWork<Tenant> _unitOfWork;
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOwnerResolver _ownerResolver;
        public CreateTenantCommandHandler(IUnitOfWork<Tenant> unitOfWork
            , ILogger<CreateTenantCommandHandler> logger
            , IHttpContextAccessor httpContextAccessor
            , IOwnerResolver ownerResolver
            )
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _ownerResolver = ownerResolver;
        }
        public async ValueTask<IOperationResult> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.Identifier))
                {
                    return OperationResult.Failed("Identifier is required");
                }
                var tenant = new Tenant
                {
                    Id = request.Id ?? request.Identifier,
                    Identifier = request.Identifier,
                    Name = request.Name
                };

                var adminUser = request.Properties.GetValueOrDefault("AdminUser");
                var adminEmail = request.Properties.GetValueOrDefault("AdminEmail");
                var adminPassword = request.Properties.GetValueOrDefault("AdminPassword");

                request.Properties.Remove("AdminUser");
                request.Properties.Remove("AdminEmail");
                request.Properties.Remove("AdminPassword");

                tenant.UpdateProperties(request.Properties);

                tenant.AddDomainEvent(new TenantCreatedDomainEvent(tenant.Id, tenant.Identifier, adminUser, adminPassword, adminEmail));

                // try to set the owner info from current user
                if (string.IsNullOrEmpty(adminUser)
                    && (_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false))
                {
                    var owner = await _ownerResolver.GetOwnerAsync(_httpContextAccessor.HttpContext.User);
                    if (!string.IsNullOrEmpty(owner))
                    {
                        tenant.SetOwner(owner);
                    }
                }
                else if (!string.IsNullOrEmpty(adminUser))
                {
                    var owner = await _ownerResolver.GetOwnerAsync(adminUser);
                    if (!string.IsNullOrEmpty(owner))
                    {
                        tenant.SetOwner(owner);
                    }
                }

                return await _unitOfWork.AddAndSaveAsync(tenant, cancellationToken);
            }
            catch (Exception ex)
            {
                // Exception may not raise here if TransactionBehavior was resgiterd in mediator pipeline
                _logger.LogError("Failed to create tenant {identifier}. {message}", request.Identifier, ex.Message);
                _logger.LogTrace(ex, "Failed to create tenant {identifier}.", request.Identifier);
                return OperationResult.Failed(ex, $"Failed to create tenant {request.Identifier}. {ex.Message}");
            }
        }
    }

    // Use for Idempotency in Command process
    public class CreateTenantIdentifiedCommandHandler
        : IdentifiedCommandHandler<CreateTenantCommand, IOperationResult>
    {
        public CreateTenantIdentifiedCommandHandler(IMediator mediator, IRequestManager requestManager, ILogger<CreateTenantIdentifiedCommandHandler> logger)
            : base(mediator, requestManager, logger)
        {
        }

        protected override ValueTask<IOperationResult> CreateResultForDuplicatedRequestAsync(CreateTenantCommand mesage)
            => ValueTask.FromResult(OperationResult.Success);
        protected override (string IdProperty, string CommandId) ExtractDebugInfo(CreateTenantCommand command)
            => (nameof(command.Identifier), command.Identifier);
    }
}
