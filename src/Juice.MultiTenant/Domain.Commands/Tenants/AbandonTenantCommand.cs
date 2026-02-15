namespace Juice.MultiTenant.Domain.Commands.Tenants
{
    public record AbandonTenantCommand : MessageBase, IRequest<IOperationResult>, ITenantCommand, IIdempotentRequest
    {
        public string Id { get; private set; }

        public string IdempotencyKey => _idempotencyKey;
        private readonly string _idempotencyKey;

        public AbandonTenantCommand(string id, string idempotencyKey)
        {
            Id = id;
            _idempotencyKey = idempotencyKey;
        }
    }
}
