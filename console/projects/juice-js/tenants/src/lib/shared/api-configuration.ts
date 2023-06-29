// import { InjectionToken } from '@angular/core';

// export const TENANT_OPTIONS_TOKEN = new InjectionToken<TenantConfiguration>('tenant.options');


export class ApiConfiguration {
    apiEndpoint: string = "";
    apiVersion: string = "2";
}

export interface ApiConfigurationParams {
    apiEndpoint?: string;
    apiVersion?: string;
}