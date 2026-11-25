import { NgModule, ModuleWithProviders } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';  
import { TenantAdminService } from './services/tenant-admin.service';
import { TenantConfiguration, TenantConfigurationParams } from './tenant-configuration';
import { MaterialModule } from './material.module';
import { provideHttpClient, withInterceptorsFromDi, withXhr } from '@angular/common/http';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { TranslatePipe, TranslateDirective, provideTranslateService } from '@ngx-translate/core';


@NgModule({ exports: [
        MaterialModule,
        FormsModule,
        ReactiveFormsModule,
        CommonModule,
        TranslatePipe,
        TranslateDirective
    ], imports: [FormsModule,
        ReactiveFormsModule,
        CommonModule,
        MaterialModule,
        BrowserModule,
        BrowserAnimationsModule,
        TranslatePipe,
        TranslateDirective], providers: [provideHttpClient(withXhr(), withInterceptorsFromDi()), provideTranslateService()] })
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
