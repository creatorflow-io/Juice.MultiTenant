namespace Juice.MultiTenant.Domain.Commands.Tenants
{
    public class CreateTenantCommand : IRequest<IOperationResult>, ITenantCommand
    {
        public string Id { get; private set; }
        public string Identifier { get; private set; }
        public string Name { get; private set; }
        public Dictionary<string, string?> Properties { get; private set; }
        public CreateTenantCommand(string id, string identifier,
            string name, 
            Dictionary<string, string?>? properties)
        {
            Id = id;
            Identifier = identifier;
            Name = name;
            Properties = properties ?? new Dictionary<string, string?>();
        }
    }
}
