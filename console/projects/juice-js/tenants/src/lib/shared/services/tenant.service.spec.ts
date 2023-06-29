import { TestBed } from '@angular/core/testing';

import { TenantService } from './tenant.service';
import { HttpClientModule } from '@angular/common/http';
import { ApiConfiguration } from '../api-configuration';

describe('TenantsService', () => {
  let service: TenantService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        HttpClientModule
      ],
      providers: [
        {
          provide: ApiConfiguration,
          useValue: {}
        }
      ]
    });
    service = TestBed.inject(TenantService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
