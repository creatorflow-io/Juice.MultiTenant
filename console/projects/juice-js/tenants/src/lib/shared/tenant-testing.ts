import { NgModule, ModuleWithProviders } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';  
import { TenantService } from './services/tenant.service';
import { ApiConfiguration, ApiConfigurationParams } from './api-configuration';
import { MaterialModule } from './material.module';
import { HttpClientModule } from '@angular/common/http';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';


@NgModule({
  imports: [
    FormsModule,
    ReactiveFormsModule,
    CommonModule,
    MaterialModule,
    HttpClientModule,
    BrowserModule,
    BrowserAnimationsModule
  ],
  exports: [
    MaterialModule,
    FormsModule,
    ReactiveFormsModule,
    CommonModule
  ]
})
export class TenantsTestingModule { 

  public static forTest(environment: ApiConfigurationParams): ModuleWithProviders<TenantsTestingModule> {

    return {
        ngModule: TenantsTestingModule,
        providers: [
            TenantService,
            {
                provide: ApiConfiguration,
                useValue: environment
            }
        ]
    };
  }
}
