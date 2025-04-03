import { NgModule } from '@angular/core';
import { DictBuilderComponent } from './dict-builder.component';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';
import { CommonModule } from '@angular/common';
import { MatInputModule } from '@angular/material/input';
import { NewKeyModalComponent } from './new-key-modal/new-key-modal.component';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';



@NgModule({
  declarations: [
    DictBuilderComponent,
    NewKeyModalComponent
  ],
  imports: [
    MatButtonModule,
    MatCardModule,
    MatProgressBarModule,
    MatFormFieldModule,
    MatIconModule,
    MatTooltipModule,
    MatInputModule,
    MatDialogModule,
    FormsModule,
    ReactiveFormsModule,
    CommonModule,
    TranslateModule
  ],
  exports: [
    DictBuilderComponent
  ],
  providers: [
    { provide: MatDialogRef, useValue: {} }
  ]
})
export class DictBuilderModule { }
