using Juice.MultiTenant.Shared.Enums;

namespace Juice.MultiTenant.Domain.Commands.Tenants
{
    public record InitializationProcessCommand : MessageBase, IRequest<IOperationResult>, ITenantCommand, IIdempotentRequest
    {
        public string Id { get; private set; }
        public TenantStatus Status { get; private set; }

        public string IdempotencyKey => _idempotencyKey;
        private readonly string _idempotencyKey;

        public InitializationProcessCommand(string id, TenantStatus status, string idempotencyKey)
        {
            ArgumentNullException.ThrowIfNull(id);
            if (status != TenantStatus.Initializing && status != TenantStatus.Initialized)
            {
                throw new ArgumentException("Invalid status", nameof(status));
            }
            Id = id;
            Status = status;
            _idempotencyKey = idempotencyKey;
        }
    }
}
