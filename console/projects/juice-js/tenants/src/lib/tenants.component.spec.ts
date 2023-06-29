import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from "@angular/router/testing";
import { TenantsComponent } from './tenants.component';
import { TenantsTestingModule } from './shared/tenant-testing';
import { TranslateModule } from '@ngx-translate/core';

describe('TenantsComponent', () => {
  let component: TenantsComponent;
  let fixture: ComponentFixture<TenantsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        RouterTestingModule,
        TranslateModule.forRoot(),
        TenantsTestingModule.forTest({}),
      ],
      declarations: [ TenantsComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TenantsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
