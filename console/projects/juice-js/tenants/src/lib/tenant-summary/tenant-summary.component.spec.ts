import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TenantSummaryComponent } from './tenant-summary.component';
import { TenantsTestingModule } from '../shared/tenant-testing';
import { TranslateModule } from '@ngx-translate/core';

describe('TenantSummaryComponent', () => {
  let component: TenantSummaryComponent;
  let fixture: ComponentFixture<TenantSummaryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports:[
        TranslateModule.forRoot(),
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
