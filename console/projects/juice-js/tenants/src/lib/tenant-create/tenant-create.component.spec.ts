import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TenantCreateComponent } from './tenant-create.component';
import {} from '@angular/common/http';
import { TenantConfiguration } from '../shared/tenant-configuration';
import { MaterialModule } from '../shared/material.module';
import { TenantsTestingModule } from '../shared/tenant-testing';

describe('TenantCreateComponent', () => {
  let component: TenantCreateComponent;
  let fixture: ComponentFixture<TenantCreateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        TenantsTestingModule.forTest({}),
      ],
      declarations: [ TenantCreateComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TenantCreateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
