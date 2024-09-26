using Finbuckle.MultiTenant.Abstractions;
using Juice.Domain;

namespace Juice.MultiTenant
{
	public class TenantInfo : DynamicEntity, ITenant, ITenantInfo
	{
        public TenantInfo() { }
		public TenantInfo(
			string id,
			string? identifier,
			string name,
			string? serializedProperties,
			string? ownerUser,
			string? tenantClass)
		{
            Id = id;
            Name = name;
            Identifier = identifier;
			SerializedProperties = serializedProperties ?? "{}";
			OwnerUser = ownerUser;
			TenantClass = tenantClass;
		}

		public string? Identifier { get; set; }

		public string? OwnerUser { get; private set; }
		public string? TenantClass { get; private set; }

        public string? Id { get; set; }

        public string? Name { get; set; }
    }
}
