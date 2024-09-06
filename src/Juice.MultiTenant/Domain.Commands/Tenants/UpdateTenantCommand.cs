namespace Juice.MultiTenant.Domain.Commands.Tenants
{
    public class UpdateTenantCommand : IRequest<IOperationResult>, ITenantCommand
    {
        public string Id { get; private set; }
        public string Identifier { get; private set; }
        public string Name { get; private set; }

        public UpdateTenantCommand(string id, string identifier, string name)
        {
            Id = id;
            Identifier = identifier;
            Name = name;
        }
    }
}
