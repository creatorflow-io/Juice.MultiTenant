
export enum TenantStatus {
    New, PendingApproval, Approved, Rejected, Initializing, Initialized, PendingToActive, Inactive, Active, Suspended, Abandoned
}

export class TenantStatusHelper{
    static getColor(status: TenantStatus){
        var s = (typeof status == "string" ? TenantStatus[status] : status)  as TenantStatus;
        switch(s){
            case TenantStatus.New:
            return "accent-color";
            case TenantStatus.Initialized:
            return "success-color";
            case TenantStatus.Approved:
            return "success-color";
            case TenantStatus.PendingApproval:
            return "accent-color";
            case TenantStatus.PendingToActive:
            return "accent-color";
            case TenantStatus.Active:
            return "success-color";
            case TenantStatus.Inactive:
            return "warn-color";
            case TenantStatus.Suspended:
            return "warn-color";
            case TenantStatus.Abandoned:
            return "warn-color";
            default:
                return "";
        }
    }
}