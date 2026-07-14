import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TenantSummaryComponent } from './tenant-summary.component';
import { TenantsTestingModule } from '../shared/tenant-testing';

describe('TenantSummaryComponent', () => {
  let component: TenantSummaryComponent;
  let fixture: ComponentFixture<TenantSummaryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports:[
        TenantsTestingModule.forTest({}),
      ],
      declarations: [ TenantSummaryComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TenantSummaryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
