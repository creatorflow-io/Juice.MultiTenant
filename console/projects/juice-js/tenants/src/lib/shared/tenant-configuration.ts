// import { InjectionToken } from '@angular/core';

// export const TENANT_OPTIONS_TOKEN = new InjectionToken<TenantConfiguration>('tenant.options');


export class TenantConfiguration {
    apiEndpoint: string = "";
    apiVersion: string = "2";
    dialogWidth: string = "800px";
    dialogMaxHeight: string = "600px";
}

export interface TenantConfigurationParams {
    apiEndpoint?: string;
    apiVersion?: string;
    dialogWidth?: string;
    dialogMaxHeight?: string;
}