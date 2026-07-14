import { NgModule, ModuleWithProviders } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';  
import { TenantAdminService } from './services/tenant-admin.service';
import { TenantConfiguration, TenantConfigurationParams } from './tenant-configuration';
import { MaterialModule } from './material.module';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';


@NgModule({ exports: [
        MaterialModule,
        FormsModule,
        ReactiveFormsModule,
        CommonModule
    ], imports: [FormsModule,
        ReactiveFormsModule,
        CommonModule,
        MaterialModule,
        BrowserModule,
        BrowserAnimationsModule], providers: [provideHttpClient(withInterceptorsFromDi())] })
export class TenantsTestingModule { 

  public static forTest(environment: TenantConfigurationParams): ModuleWithProviders<TenantsTestingModule> {

    return {
        ngModule: TenantsTestingModule,
        providers: [
            TenantAdminService,
            {
                provide: TenantConfiguration,
                useValue: environment
            }
        ]
    };
  }
}
