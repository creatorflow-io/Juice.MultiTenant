// import { InjectionToken } from '@angular/core';

// export const TENANT_OPTIONS_TOKEN = new InjectionToken<TenantConfiguration>('tenant.options');


export class TenantConfiguration {
    apiEndpoint: string = "";
    apiVersion: string = "2";
    dialogWidth: string = "800px";
    dialogMaxHeight: string = "600px";
    // When not explicitly false, state-changing tenant requests opt into the
    // @juice-js/core idempotency interceptor (retry-safe Idempotency-Key).
    enableIdempotency?: boolean;
}

export interface TenantConfigurationParams {
    apiEndpoint?: string;
    apiVersion?: string;
    dialogWidth?: string;
    dialogMaxHeight?: string;
    enableIdempotency?: boolean;
}