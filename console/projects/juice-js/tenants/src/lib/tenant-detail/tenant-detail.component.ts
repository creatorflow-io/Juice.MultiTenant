import { Component, OnInit, Input, OnChanges } from '@angular/core';
import { TenantAdminService } from '../shared/services/tenant-admin.service';
import { Tenant } from '../shared/models/tenant.model';
import { TenantStatusHelper } from '../shared/models/tenant.status';

@Component({
  selector: 'juice-tenant-detail',
  templateUrl: './tenant-detail.component.html',
  styleUrls: ['./tenant-detail.component.scss']
})
export class TenantDetailComponent implements OnChanges {

  @Input()  tenantIdentifier: string = "";

  tenant!: Tenant;
  properties: any;
  statusHelper = TenantStatusHelper;

  constructor(private tenantService: TenantAdminService) { }

  public loadTenant(identifier: string){
    this.tenantService.getTenant(identifier).subscribe(tenant => {
      this.tenant = tenant;
      this.properties = JSON.parse(tenant.serializedProperties);
    });
  }

  ngOnChanges(): void {
    this.loadTenant(this.tenantIdentifier);
  }
}
