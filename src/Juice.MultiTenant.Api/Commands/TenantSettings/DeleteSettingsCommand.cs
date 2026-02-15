namespace Juice.MultiTenant.Api.Commands.TenantSettings
{
    public record DeleteSettingsCommand : MessageBase, IRequest<IOperationResult>, ITenantSettingsCommand
    {
        public string Section { get; private set; }
        public DeleteSettingsCommand(string section)
        {
            Section = section;
        }
    }
}
