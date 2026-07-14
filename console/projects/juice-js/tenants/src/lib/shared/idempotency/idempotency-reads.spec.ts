import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { IDEMPOTENCY_OPERATION, IdempotencyModule } from '@juice-js/core';

import { TenantAdminService } from '../services/tenant-admin.service';
import { TenantConfiguration } from '../tenant-configuration';

// US3 - Read operations remain unaffected + interceptor scoping.
describe('Idempotency: reads unaffected & scoping (US3)', () => {
  let service: TenantAdminService;
  let httpMock: HttpTestingController;
  const apiEndpoint = 'https://api.test';
  const base = `${apiEndpoint}/api/v2`;

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [IdempotencyModule],
    providers: [
        { provide: TenantConfiguration, useValue: { apiEndpoint, apiVersion: '2' } },
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting()
    ]
});
    service = TestBed.inject(TenantAdminService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  const reads: Array<[() => void, string]> = [
    [() => service.getTenants().subscribe(), `${base}/tenants?q=&page=1&pageSize=20`],
    [() => service.getTenant('acme').subscribe(), `${base}/admin/acme`],
    [() => service.getSummary('acme').subscribe(), `${apiEndpoint}/acme/api/v2/operation`],
    [() => service.getTenantSettings('acme').subscribe(), `${base}/admin/acme/settings`],
    [() => service.getRootSettings().subscribe(), `${base}/settings`],
  ];

  reads.forEach(([call, url]) => {
    it(`does not tag the read ${url} (FR-005)`, () => {
      call();
      const req = httpMock.expectOne(url);
      expect(req.request.method).toBe('GET');
      expect(req.request.headers.has('Idempotency-Key')).toBeFalse();
      expect(req.request.context.get(IDEMPOTENCY_OPERATION)).toBeNull();
      req.flush({});
    });
  });

  it('leaves non-opted-in (non-tenant) requests untouched (scoping, research R2)', () => {
    // Any request that does not opt in via withIdempotency never receives a key,
    // so the interceptor cannot affect non-tenant application traffic.
    const http = TestBed.inject(HttpClient);
    http.post('https://other.test/things', {}).subscribe();
    const req = httpMock.expectOne('https://other.test/things');
    expect(req.request.headers.has('Idempotency-Key')).toBeFalse();
    req.flush({});
  });
});

// Opt-out configuration (contract §1) — belongs to Polish but co-located for clarity.
describe('Idempotency: opt-out via enableIdempotency=false', () => {
  let service: TenantAdminService;
  let httpMock: HttpTestingController;
  const apiEndpoint = 'https://api.test';

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [IdempotencyModule],
    providers: [
        { provide: TenantConfiguration, useValue: { apiEndpoint, apiVersion: '2', enableIdempotency: false } },
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting()
    ]
});
    service = TestBed.inject(TenantAdminService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('does not opt mutating requests into idempotency when disabled', () => {
    service.suspendTenant('acme').subscribe();
    const req = httpMock.expectOne(`${apiEndpoint}/api/v2/admin/acme/suspend`);
    expect(req.request.headers.has('Idempotency-Key')).toBeFalse();
    expect(req.request.context.get(IDEMPOTENCY_OPERATION)).toBeNull();
    req.flush({});
  });
});
