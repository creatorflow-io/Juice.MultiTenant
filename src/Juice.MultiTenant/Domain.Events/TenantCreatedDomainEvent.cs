using MediatR;

namespace Juice.MultiTenant.Domain.Events
{
    public record TenantCreatedDomainEvent(string TenantId,
        string TenantIdentifier,
        string? CreateAdminUser,
        string? CreateAdminPassword,
        string? CreateAdminEmail
        ) : INotification;
}
