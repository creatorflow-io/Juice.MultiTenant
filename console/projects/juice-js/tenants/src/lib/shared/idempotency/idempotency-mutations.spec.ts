import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { IDEMPOTENCY_OPERATION, IdempotencyModule } from '@juice-js/core';

import { TenantAdminService } from '../services/tenant-admin.service';
import { TenantConfiguration } from '../tenant-configuration';
import { TenantUpdate } from '../models/tenant.update.model';
import { provideHttpClient, withInterceptorsFromDi, withXhr } from '@angular/common/http';

// US2 - Safe retries of tenant state changes.
describe('Idempotency: tenant state changes (US2)', () => {
  let service: TenantAdminService;
  let httpMock: HttpTestingController;
  const apiEndpoint = 'https://api.test';
  const base = `${apiEndpoint}/api/v2`;
  const id = 'acme';

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [IdempotencyModule],
    providers: [
        { provide: TenantConfiguration, useValue: { apiEndpoint, apiVersion: '2' } },
        provideHttpClient(withXhr(), withInterceptorsFromDi()),
        provideHttpClientTesting()
    ]
});
    service = TestBed.inject(TenantAdminService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  // [call, expected url, expected method, expected operation id]
  const cases: Array<[() => void, string, string, string]> = [
    [() => service.updateTenant(id, new TenantUpdate()).subscribe(), `${base}/admin/${id}`, 'PUT', `update-tenant:${id}`],
    [() => service.deleteTenant(id).subscribe(), `${base}/admin/${id}`, 'DELETE', `delete-tenant:${id}`],
    [() => service.activateTenant(id).subscribe(), `${base}/admin/${id}/activate`, 'POST', `activate-tenant:${id}`],
    [() => service.deactivateTenant(id).subscribe(), `${base}/admin/${id}/deactivate`, 'POST', `deactivate-tenant:${id}`],
    [() => service.reactivateTenant(id).subscribe(), `${base}/admin/${id}/reactivate`, 'POST', `reactivate-tenant:${id}`],
    [() => service.suspendTenant(id).subscribe(), `${base}/admin/${id}/suspend`, 'POST', `suspend-tenant:${id}`],
    [() => service.approveTenant(id).subscribe(), `${base}/admin/${id}/approve`, 'POST', `approve-tenant:${id}`],
    [() => service.rejectTenant(id).subscribe(), `${base}/admin/${id}/reject`, 'POST', `reject-tenant:${id}`],
    [() => service.abandonTenant(id).subscribe(), `${base}/admin/${id}/abandon`, 'PUT', `abandon-tenant:${id}`],
    [() => service.updateTenantSettings(id, []).subscribe(), `${base}/admin/${id}/settings`, 'PUT', `update-tenant-settings:${id}`],
    [() => service.updateTenantProperties(id, {}).subscribe(), `${base}/admin/${id}/properties`, 'PUT', `update-tenant-properties:${id}`],
    [() => service.updateRootSettings([]).subscribe(), `${base}/settings`, 'PUT', `update-root-settings`],
  ];

  cases.forEach(([call, url, method, operationId]) => {
    it(`attaches an Idempotency-Key + operation id for ${operationId} (FR-002)`, () => {
      call();
      const req = httpMock.expectOne(url);
      expect(req.request.method).toBe(method);
      expect(req.request.headers.has('Idempotency-Key')).toBeTrue();
      expect(req.request.context.get(IDEMPOTENCY_OPERATION)).toBe(operationId);
      req.flush({});
    });
  });

  it('gives two distinct targets distinct operation ids (FR-004)', () => {
    service.suspendTenant('acme').subscribe();
    service.suspendTenant('globex').subscribe();
    const a = httpMock.expectOne(`${base}/admin/acme/suspend`);
    const b = httpMock.expectOne(`${base}/admin/globex/suspend`);
    expect(a.request.context.get(IDEMPOTENCY_OPERATION))
      .not.toBe(b.request.context.get(IDEMPOTENCY_OPERATION));
    a.flush({});
    b.flush({});
  });
});
