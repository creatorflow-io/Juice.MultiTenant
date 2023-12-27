namespace Juice.MultiTenant.Domain.Commands.Tenants
{
    public class UpdateClassCommand : IRequest<IOperationResult>, ITenantCommand
    {
        public string Id { get; private set; }
        public string TenantClass { get; private set; }


        public UpdateClassCommand(string id, string tenantClass)
        {
            Id = id;
            TenantClass = tenantClass;
        }

    }
}
