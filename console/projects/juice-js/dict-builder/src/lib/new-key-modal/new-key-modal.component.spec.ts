import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NewKeyModalComponent } from './new-key-modal.component';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { FormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { TranslateModule } from '@ngx-translate/core';
import { MatIconModule } from '@angular/material/icon';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

describe('NewKeyModalComponent', () => {
  let component: NewKeyModalComponent;
  let fixture: ComponentFixture<NewKeyModalComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        NoopAnimationsModule,
        FormsModule,
        MatFormFieldModule,
        MatIconModule,
        MatInputModule,
        MatDialogModule,
        TranslateModule.forRoot()
      ],
      declarations: [NewKeyModalComponent],
      providers:[
        { provide: MatDialogRef, useValue: {} }
      ]
    });
    fixture = TestBed.createComponent(NewKeyModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
