namespace Juice.MultiTenant.Api.Contracts.Models
{
    /// <summary>
    /// The Created Model
    /// </summary>
    public class CreatedModel
    {
        /// <summary>
        /// Tenant Id
        /// </summary>
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Tenant Identifier
        /// </summary>
        public string Identifier { get; set; } = string.Empty;
    }
}
