namespace Juice.MultiTenant.Domain.Commands.Tenants
{
    public record UpdateTenantCommand : MessageBase, IRequest<IOperationResult>, ITenantCommand, IIdempotentRequest
    {
        public string Id { get; private set; }
        public string Identifier { get; private set; }
        public string Name { get; private set; }

        public string IdempotencyKey => _idempotencyKey;
        private readonly string _idempotencyKey;

        public UpdateTenantCommand(string id, string identifier, string name, string idempotencyKey)
        {
            Id = id;
            Identifier = identifier;
            Name = name;
            _idempotencyKey = idempotencyKey;
        }
    }
}
