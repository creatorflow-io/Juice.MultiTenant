namespace Juice.MultiTenant.Domain.Commands.Tenants
{
    public record UpdateTierCommand : MessageBase, IRequest<IOperationResult>, ITenantCommand, IIdempotentRequest
    {
        public string Id { get; private set; }
        public string Tier { get; private set; }

        public string IdempotencyKey => _idempotencyKey;
        private readonly string _idempotencyKey;

        public UpdateTierCommand(string id, string tier, string idempotencyKey)
        {
            Id = id;
            Tier = tier;
            _idempotencyKey = idempotencyKey;
        }

    }
}
