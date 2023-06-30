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



@NgModule({
  declarations: [
    DictBuilderComponent
  ],
  imports: [
    MatButtonModule,
    MatCardModule,
    MatProgressBarModule,
    MatFormFieldModule,
    MatIconModule,
    MatTooltipModule,
    MatInputModule,
    FormsModule,
    ReactiveFormsModule,
    CommonModule,
    TranslateModule
  ],
  exports: [
    DictBuilderComponent
  ]
})
export class DictBuilderModule { }
