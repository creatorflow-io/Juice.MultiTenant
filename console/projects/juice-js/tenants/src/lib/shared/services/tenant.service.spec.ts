import { TestBed } from '@angular/core/testing';

import { TenantAdminService } from './tenant-admin.service';
import { provideHttpClient, withInterceptorsFromDi, withXhr } from '@angular/common/http';
import { TenantConfiguration } from '../tenant-configuration';

describe('TenantsService', () => {
  let service: TenantAdminService;

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [],
    providers: [
        {
            provide: TenantConfiguration,
            useValue: {}
        },
        provideHttpClient(withXhr(), withInterceptorsFromDi())
    ]
});
    service = TestBed.inject(TenantAdminService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
