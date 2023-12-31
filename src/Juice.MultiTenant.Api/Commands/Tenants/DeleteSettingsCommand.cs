﻿namespace Juice.MultiTenant.Api.Commands.Tenants
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
