namespace Juice.MultiTenant.Domain.Commands.Tenants
{
    public record CreateTenantCommand : MessageBase, IRequest<IOperationResult>, ITenantCommand, IIdempotentRequest
    {
        public string Id { get; private set; }
        public string Identifier { get; private set; }
        public string Name { get; private set; }
        public Dictionary<string, string?> Properties { get; private set; }

        public string IdempotencyKey => _idempotencyKey;
        private readonly string _idempotencyKey;

        public CreateTenantCommand(string id, string identifier,
            string name,
            Dictionary<string, string?>? properties,
            string idempotencyKey)
        {
            Id = id;
            Identifier = identifier;
            Name = name;
            Properties = properties ?? new Dictionary<string, string?>();
            _idempotencyKey = idempotencyKey;
        }
    }
}
