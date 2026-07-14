import { Component, ChangeDetectionStrategy } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';

@Component({
    selector: 'app-new-key-modal',
    templateUrl: './new-key-modal.component.html',
    styleUrls: ['./new-key-modal.component.css'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class NewKeyModalComponent {
  key: string = '';

  constructor(public dialogRef: MatDialogRef<NewKeyModalComponent>) {}

  onCancel(): void {
    this.dialogRef.close(null);
  }

  onSubmit(): void {
    this.dialogRef.close(this.key);
  }
}
