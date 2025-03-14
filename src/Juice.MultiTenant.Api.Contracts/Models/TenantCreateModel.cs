using System.ComponentModel.DataAnnotations;

namespace Juice.MultiTenant.Api.Contracts.Models
{
    /// <summary>
    /// Tenant create model
    /// </summary>
    public class TenantCreateModel
    {
        /// <summary>
        /// Tenant identifier (unique), E.g. "acme"
        /// </summary>
        [Required]
        public string Identifier { get; set; }

        /// <summary>
        /// Display name of the tenant, E.g. "Acme Corporation"
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Optional admin user name, E.g. "admin"
        /// </summary>
        public string? AdminUser { get; set; }

        /// <summary>
        /// Optional admin password, E.g. "admin@domain.com"
        /// </summary>

        [DataType(DataType.EmailAddress)]
        public string? AdminEmail { get; set; }

        public Dictionary<string, string?>? Properties { get; set; }
    }
}
