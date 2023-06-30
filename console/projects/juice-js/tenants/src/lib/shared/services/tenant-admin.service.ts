import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { TenantConfiguration } from '../tenant-configuration';
import { Tenant, TenantBasic } from '../models/tenant.model';
import { TenantCreate, TenantCreated } from '../models/tenant.create.model';
import { TenantUpdate } from '../models/tenant.update.model';
import { TenantStatus } from '../models/tenant.status';
import { TableQueryResult } from '../models/table.query-result';
import { Observable } from 'rxjs';
import { TenantSetting } from '../models/tenant.setting.model';

@Injectable({
  providedIn: 'root'
})
export class TenantAdminService {

  constructor(private http: HttpClient, private options: TenantConfiguration) { 

  }

  public getTenants(filterText: string = "", statuses: TenantStatus[] = [], 
    sortColumns: string[] = [], sortDirections: string[]=[], 
    pageIndex: number =0, pageSize: number = 20): Observable<TableQueryResult<TenantBasic>> {

    var params = new HttpParams()
      .set('q', filterText)
      .set('page', pageIndex+1)
      .set('pageSize', pageSize);

    if (statuses.length > 0) {
      for (var i = 0; i < statuses.length; i++) {
        params = params.append('statuses', statuses[i]);
      }
    }
    if(sortColumns.length > 0) {
      if(sortDirections.length != sortColumns.length) throw new Error("sortColumns and sortDirections must be the same length");
      for (var i = 0; i < sortColumns.length; i++) {
          params = params.set("sorts["+i+"].property", sortColumns[i])
                .set("sorts["+i+"].sortDir", sortDirections[i]);
      }
    }
    return this.http.get<TableQueryResult<TenantBasic>>(`${this.options.apiEndpoint}/api/v${this.options.apiVersion}/tenants`,
      {params: params});
  }


  public getTenant(identifier: string) {
    return this.http.get<Tenant>(`${this.options.apiEndpoint}/api/v${this.options.apiVersion}/admin/${identifier}`);
  }

  public getSummary(identifier: string) {
    return this.http.get<TenantBasic>(`${this.options.apiEndpoint}/${identifier}/api/v${this.options.apiVersion}/operation`);
  }

  public createTenant(tenant: TenantCreate) {
    return this.http.post<TenantCreated>(`${this.options.apiEndpoint}/api/v${this.options.apiVersion}/admin`, tenant);
  }

  public updateTenant(id: string, tenant: TenantUpdate) {
    return this.http.put(`${this.options.apiEndpoint}/api/v${this.options.apiVersion}/admin/${id}`, tenant);
  }

  public deleteTenant(id: string) {
    return this.http.delete(`${this.options.apiEndpoint}/api/v${this.options.apiVersion}/admin/${id}`);
  }

  public activateTenant(id: string) {
    return this.http.post(`${this.options.apiEndpoint}/api/v${this.options.apiVersion}/admin/${id}/activate`, null);
  }

  public deactivateTenant(id: string) {
    return this.http.post(`${this.options.apiEndpoint}/api/v${this.options.apiVersion}/admin/${id}/deactivate`, null);
  }

  public reactivateTenant(id: string) {
    return this.http.post(`${this.options.apiEndpoint}/api/v${this.options.apiVersion}/admin/${id}/reactivate`, null);
  }

  public suspendTenant(id: string) {
    return this.http.post(`${this.options.apiEndpoint}/api/v${this.options.apiVersion}/admin/${id}/suspend`, null);
  }

  public approveTenant(id: string) {
    return this.http.post(`${this.options.apiEndpoint}/api/v${this.options.apiVersion}/admin/${id}/approve`, null);
  }
  public rejectTenant(id: string) {
    return this.http.post(`${this.options.apiEndpoint}/api/v${this.options.apiVersion}/admin/${id}/reject`, null);
  }

  public abandonTenant(id: string) {
    return this.http.put(`${this.options.apiEndpoint}/api/v${this.options.apiVersion}/admin/${id}/abandon`, null);
  }

  public getTenantSettings(id: string) {
    return this.http.get<TenantSetting[]>(`${this.options.apiEndpoint}/api/v${this.options.apiVersion}/admin/${id}/settings`);
  }

  public updateTenantSettings(id: string, settings: TenantSetting[]) {
    return this.http.put(`${this.options.apiEndpoint}/api/v${this.options.apiVersion}/admin/${id}/settings`, settings);
  }
}
