import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DictBuilderComponent } from './dict-builder.component';
import { MaterialModule } from '../shared/material.module';
import { TranslateModule } from '@ngx-translate/core';

describe('DictBuilderComponent', () => {
  let component: DictBuilderComponent;
  let fixture: ComponentFixture<DictBuilderComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ 
        MaterialModule,
        TranslateModule.forRoot()
      ],
      declarations: [ DictBuilderComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DictBuilderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('configuration key should be standardizing', () => {
    expect(component.standardizeConfigurationKey("abc::0 d ef:")).toBe('abc:def');
    expect(component.standardizeConfigurationKey("abc::0def:xyz")).toBe('abc:def:xyz');
    expect(component.standardizeConfigurationKey("abc:0:xyz")).toBe('abc:xyz');
    expect(component.standardizeConfigurationKey(":abc::0def:")).toBe('abc:def');
    expect(component.standardizeConfigurationKey(":0abc::0def:")).toBe('abc:def');
    expect(component.standardizeConfigurationKey("0abc::0def:")).toBe('abc:def');
  });

  it('property key should be standardizing', () => {
    expect(component.standardizePropertyKey("abc::0 d ef:")).toBe('abc0def');
    expect(component.standardizePropertyKey("abc::0def:xyz")).toBe('abc0defxyz');
    expect(component.standardizePropertyKey("abc:0:xyz")).toBe('abc0xyz');
    expect(component.standardizePropertyKey(":abc::0def:")).toBe('abc0def');
    expect(component.standardizePropertyKey(":0abc::0def:")).toBe('abc0def');
    expect(component.standardizePropertyKey("0abc::0def:")).toBe('abc0def');
  });
});
