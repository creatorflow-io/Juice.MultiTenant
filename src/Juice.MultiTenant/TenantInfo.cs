﻿using Finbuckle.MultiTenant;
using Juice.Domain;

namespace Juice.MultiTenant
{
    public class TenantInfo : DynamicEntity<string>, ITenant, ITenantInfo
    {
        public TenantInfo() { }
        public TenantInfo(
            string id,
            string? identifier,
            string name,
            string? serializedProperties,
            string? connectionString,
            string? ownerUser,
            string? tenantClass)
        {
            Id = id;
            Identifier = identifier;
            Name = name;
            ConnectionString = connectionString;
            SerializedProperties = serializedProperties ?? "{}";
            OwnerUser = ownerUser;
            TenantClass = tenantClass;
        }
        public string? Identifier { get; set; }
        public string? ConnectionString { get; set; }

        public string? OwnerUser { get; private set; }
        public string? TenantClass { get; private set; }

    }
}
