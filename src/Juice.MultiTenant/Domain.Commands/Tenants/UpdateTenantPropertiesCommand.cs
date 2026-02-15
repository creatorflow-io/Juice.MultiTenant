namespace Juice.MultiTenant.Domain.Commands.Tenants
{
    public record UpdateTenantPropertiesCommand : MessageBase, IRequest<IOperationResult>, ITenantCommand, IIdempotentRequest
    {
        public string Id { get; private set; }
        public Dictionary<string, string?> Properties { get; private set; } = [];
        
        public string IdempotencyKey => _idempotencyKey;
        private readonly string _idempotencyKey;

        public UpdateTenantPropertiesCommand(string id, Dictionary<string, string?> properties, string idempotencyKey)
        {
            Id = id;
            Properties = properties;
            _idempotencyKey = idempotencyKey;
        }
    }
}
