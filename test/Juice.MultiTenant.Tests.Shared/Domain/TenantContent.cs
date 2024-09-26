using Juice.Domain;

namespace Juice.MultiTenant.Tests.Domain
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public class TenantContent(string code, string name) : DynamicAuditEntity<Guid>(Guid.NewGuid(), name)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        public string Code { get; private set; } = code;
        public string TenantId { get; private set; }
    }
}
