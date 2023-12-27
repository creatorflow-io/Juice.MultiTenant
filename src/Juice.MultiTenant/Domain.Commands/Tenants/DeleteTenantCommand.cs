﻿namespace Juice.MultiTenant.Domain.Commands.Tenants
{
    public class DeleteTenantCommand : IRequest<IOperationResult>, ITenantCommand
    {
        public string Id { get; private set; }
        public DeleteTenantCommand(string id)
        {
            Id = id;
        }
    }
}