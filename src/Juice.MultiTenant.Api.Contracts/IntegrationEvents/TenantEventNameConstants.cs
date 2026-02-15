

namespace Juice.MultiTenant.Api.Contracts.IntegrationEvents
{
    public class TenantEventNameConstants
    {
        public const string TenantActivated = "tenant.status.activated";
        public const string TenantAbandoned = "tenant.status.abandoned";
        public const string TenantDeactivated = "tenant.status.deactivated";
        public const string TenantSuspended = "tenant.status.suspended";
        public const string TenantRequestActive = "tenant.status.request_active";

        public const string TenantPendingApproval = "tenant.approval.pending";
        public const string TenantApproved = "tenant.approval.approved";
        public const string TenantRejected = "tenant.approval.rejected";

        public const string TenantCreated = "tenant.created";
        public const string TenantDeleted = "tenant.deleted";
        public const string TenantTierChanged = "tenant.tier.changed";
        public const string TenantOwnerChanged = "tenant.owner.changed";
        public const string TenantSettingsChanged = "tenant.settings.changed";
        public const string TenantPropertiesChanged = "tenant.properties.changed";
        public const string TenantInitializationChanged = "tenant.initialization.changed";
    }
}
