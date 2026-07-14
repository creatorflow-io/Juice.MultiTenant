import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { IDEMPOTENCY_OPERATION, IdempotencyModule, IdempotencyKeyService } from '@juice-js/core';

import { TenantAdminService } from '../services/tenant-admin.service';
import { TenantConfiguration } from '../tenant-configuration';
import { TenantCreate } from '../models/tenant.create.model';
import { provideHttpClient, withInterceptorsFromDi, withXhr } from '@angular/common/http';

// US1 - Safe retries of tenant creation.
describe('Idempotency: tenant creation (US1)', () => {
  let service: TenantAdminService;
  let httpMock: HttpTestingController;
  const apiEndpoint = 'https://api.test';

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

  function newTenant(identifier: string): TenantCreate {
    const t = new TenantCreate();
    t.identifier = identifier;
    t.name = identifier;
    return t;
  }

  it('attaches an Idempotency-Key header to createTenant (FR-002)', () => {
    service.createTenant(newTenant('acme')).subscribe();
    const req = httpMock.expectOne(`${apiEndpoint}/api/v2/admin`);

    expect(req.request.method).toBe('POST');
    expect(req.request.headers.has('Idempotency-Key')).toBeTrue();
    expect(req.request.headers.get('Idempotency-Key')).toBeTruthy();

    req.flush({ id: '1', identifier: 'acme' });
  });

  it('opts in with a payload-scoped operation id', () => {
    service.createTenant(newTenant('acme')).subscribe();
    const req = httpMock.expectOne(`${apiEndpoint}/api/v2/admin`);

    expect(req.request.context.get(IDEMPOTENCY_OPERATION)).toBe('create-tenant:acme');

    req.flush({ id: '1', identifier: 'acme' });
  });

  it('reuses the same key across retries of one logical create (FR-003)', () => {
    // The service derives a stable operation id from the payload, so a retried
    // create resolves to the same key until the operation settles.
    const keys = TestBed.inject(IdempotencyKeyService);
    const first = keys.getKey('create-tenant:acme');
    const retried = keys.getKey('create-tenant:acme');
    expect(retried).toBe(first);
    keys.release('create-tenant:acme');
  });

  it('gives distinct creates distinct keys (FR-004)', () => {
    const keys = TestBed.inject(IdempotencyKeyService);
    const a = keys.getKey('create-tenant:acme');
    const b = keys.getKey('create-tenant:globex');
    expect(a).not.toBe(b);
    keys.release('create-tenant:acme');
    keys.release('create-tenant:globex');
  });
});
