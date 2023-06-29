import { Component, Input, OnChanges } from '@angular/core';
import { TenantBasic } from '../shared/models/tenant.model';
import { TenantService } from '../shared/services/tenant.service';

@Component({
  selector: 'juice-tenant-summary',
  templateUrl: './tenant-summary.component.html',
  styleUrls: ['./tenant-summary.component.css']
})
export class TenantSummaryComponent implements OnChanges{

  @Input()
  tenantIdentifier: string = "";

  tenant!: TenantBasic;

  constructor(private tenantService: TenantService) { }

  public loadTenant(identifier: string){
    this.tenantService.getSummary(identifier).subscribe(tenant => {
      this.tenant = tenant;
    });
  }

  ngOnChanges(): void {
    this.loadTenant(this.tenantIdentifier);
  }
}
