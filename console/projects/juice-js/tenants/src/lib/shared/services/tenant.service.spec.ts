import { TestBed } from '@angular/core/testing';

import { TenantAdminService } from './tenant-admin.service';
import { HttpClientModule } from '@angular/common/http';
import { TenantConfiguration } from '../tenant-configuration';

describe('TenantsService', () => {
  let service: TenantAdminService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        HttpClientModule
      ],
      providers: [
        {
          provide: TenantConfiguration,
          useValue: {}
        }
      ]
    });
    service = TestBed.inject(TenantAdminService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
