using Juice.MultiTenant.Shared.Enums;

namespace Juice.MultiTenant.Domain.Commands.Tenants
{
    public record ApprovalProcessCommand : MessageBase, IRequest<IOperationResult>, ITenantCommand, IIdempotentRequest
    {
        public string Id { get; private set; }
        public TenantStatus Status { get; private set; }

        public string IdempotencyKey => _idempotencyKey;
        private readonly string _idempotencyKey;

        public ApprovalProcessCommand(string id, TenantStatus status, string idempotencyKey)
        {
            Id = id;
            Status = status;
            _idempotencyKey = idempotencyKey;
        }
    }
}
