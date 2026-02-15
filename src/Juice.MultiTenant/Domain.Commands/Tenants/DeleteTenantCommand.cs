namespace Juice.MultiTenant.Domain.Commands.Tenants
{
    public record DeleteTenantCommand : MessageBase, IRequest<IOperationResult>, ITenantCommand, IIdempotentRequest
    {
        public string Id { get; private set; }

        public string IdempotencyKey => _idempotencyKey;
        private readonly string _idempotencyKey;

        public DeleteTenantCommand(string id, string idempotencyKey)
        {
            Id = id;
            _idempotencyKey = idempotencyKey;
        }
    }
}
