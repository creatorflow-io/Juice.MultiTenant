import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TenantSettingsComponent } from './tenant-settings.component';
import { TranslateModule } from '@ngx-translate/core';
import { TenantsTestingModule } from '../shared/tenant-testing';
import { DictBuilderModule } from '@juice-js/dict-builder';

describe('TenantSettingsComponent', () => {
  let component: TenantSettingsComponent;
  let fixture: ComponentFixture<TenantSettingsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        TranslateModule.forRoot(),
        TenantsTestingModule.forTest({}),
        DictBuilderModule
      ],
      declarations: [ TenantSettingsComponent ]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(TenantSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
