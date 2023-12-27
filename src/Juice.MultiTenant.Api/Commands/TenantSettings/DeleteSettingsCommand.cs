namespace Juice.MultiTenant.Api.Commands.TenantSettings
{
    public class DeleteSettingsCommand : IRequest<IOperationResult>, ITenantSettingsCommand
    {
        public string Section { get; private set; }
        public DeleteSettingsCommand(string section)
        {
            Section = section;
        }
    }
}
