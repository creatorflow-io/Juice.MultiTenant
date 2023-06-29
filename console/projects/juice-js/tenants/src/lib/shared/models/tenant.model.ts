import { TenantStatus } from "./tenant.status";

export interface TenantBasic {
    id: string;
    name: string;
    identifier: string;
    status: TenantStatus;
}

export interface Tenant extends TenantBasic {
    connectionString: string;
    ownerUser: string;
    ownerName: string;
    serializedProperties: string;
}