using Juice.MultiTenant.Shared.Enums;

namespace Juice.MultiTenant.Domain.Commands.Tenants
{
    public class InitializationProcessCommand : IRequest<IOperationResult>, ITenantCommand
    {
        public string Id { get; private set; }
        public TenantStatus Status { get; private set; }
        public InitializationProcessCommand(string id, TenantStatus status)
        {
            ArgumentNullException.ThrowIfNull(id);
            if (status != TenantStatus.Initializing && status != TenantStatus.Initialized)
            {
                throw new ArgumentException("Invalid status", nameof(status));
            }
            Id = id;
            Status = status;
        }
    }
}
