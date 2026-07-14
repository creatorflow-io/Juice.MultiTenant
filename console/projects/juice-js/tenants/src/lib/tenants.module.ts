import { NgModule, ModuleWithProviders } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';  
import { TranslatePipe, TranslateDirective } from '@ngx-translate/core'
import { TenantsComponent } from './tenants.component';
import { TenantDetailComponent } from './tenant-detail/tenant-detail.component';
import { TenantAdminService } from './shared/services/tenant-admin.service';
import { TenantConfiguration, TenantConfigurationParams } from './shared/tenant-configuration';
import { MaterialModule } from './shared/material.module';
import { TenantSummaryComponent } from './tenant-summary/tenant-summary.component';
import { TenantCreateComponent } from './tenant-create/tenant-create.component';
import { TenantUpdateComponent } from './tenant-update/tenant-update.component';
import { ClipboardModule } from '@angular/cdk/clipboard';
import { MatMultiSortModule } from 'ngx-mat-multi-sort';
import { ConfirmDialogComponent } from './confirm-dialog/confirm-dialog.component';
import { TenantSettingsComponent } from './tenant-settings/tenant-settings.component';
import { DictBuilderModule } from '@juice-js/dict-builder';
import { IdempotencyModule } from '@juice-js/core';


@NgModule({
  declarations: [
    TenantsComponent,
    TenantDetailComponent,
    TenantSummaryComponent,
    TenantCreateComponent,
    TenantUpdateComponent,
    ConfirmDialogComponent,
    TenantSettingsComponent
  ],
  imports: [
    TranslatePipe,
    TranslateDirective,
    FormsModule,
    ReactiveFormsModule,
    CommonModule,
    MaterialModule,
    ClipboardModule,
    MatMultiSortModule,
    DictBuilderModule,
    IdempotencyModule
  ],
  exports: [
    TenantsComponent,
    TenantDetailComponent,
    TenantSummaryComponent,
    TenantCreateComponent,
    TenantUpdateComponent,
    TranslatePipe,
    TranslateDirective
  ]
})
export class TenantsModule { 

  public static forRoot(environment: TenantConfigurationParams): ModuleWithProviders<TenantsModule> {

    return {
        ngModule: TenantsModule,
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
