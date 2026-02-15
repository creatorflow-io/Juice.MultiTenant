namespace Juice.MultiTenant.Domain.Commands.Tenants
{
    public record ResetTenantOwnerCommand : MessageBase, IRequest<IOperationResult>, ITenantCommand, IIdempotentRequest
    {
        public string IdempotencyKey => _idempotencyKey;
        private readonly string _idempotencyKey;

        public string Id { get; private set; }
        public string OwnerUser { get; private set; }
        
        public ResetTenantOwnerCommand(string id, string ownerUser, string idempotencyKey)
        {
            Id = id;
            OwnerUser = ownerUser;
            _idempotencyKey = idempotencyKey;
        }
    }
}
