import { NgModule, ModuleWithProviders } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';  
import { TranslateModule } from '@ngx-translate/core'
import { TenantsComponent } from './tenants.component';
import { TenantDetailComponent } from './tenant-detail/tenant-detail.component';
import { TenantService } from './shared/services/tenant.service';
import { ApiConfiguration, ApiConfigurationParams } from './shared/api-configuration';
import { MaterialModule } from './shared/material.module';
import { TenantSummaryComponent } from './tenant-summary/tenant-summary.component';
import { TenantCreateComponent } from './tenant-create/tenant-create.component';
import { DictBuilderComponent } from './dict-builder/dict-builder.component';
import { TenantUpdateComponent } from './tenant-update/tenant-update.component';
import { ClipboardModule } from '@angular/cdk/clipboard';
import { MatMultiSortModule } from 'ngx-mat-multi-sort';
import { ConfirmDialogComponent } from './confirm-dialog/confirm-dialog.component';


@NgModule({
  declarations: [
    TenantsComponent,
    TenantDetailComponent,
    TenantSummaryComponent,
    TenantCreateComponent,
    DictBuilderComponent,
    TenantUpdateComponent,
    ConfirmDialogComponent
  ],
  imports: [
    TranslateModule,
    FormsModule,
    ReactiveFormsModule,
    CommonModule,
    MaterialModule,
    ClipboardModule,
    MatMultiSortModule
  ],
  exports: [
    TenantsComponent,
    TenantDetailComponent,
    TenantSummaryComponent,
    TenantCreateComponent,
    TenantUpdateComponent,
    DictBuilderComponent,
    TranslateModule
  ]
})
export class TenantsModule { 

  public static forRoot(environment: ApiConfigurationParams): ModuleWithProviders<TenantsModule> {

    return {
        ngModule: TenantsModule,
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
