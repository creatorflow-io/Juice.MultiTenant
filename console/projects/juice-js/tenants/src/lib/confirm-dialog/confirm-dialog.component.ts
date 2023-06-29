import { Component, Output, EventEmitter, Inject } from '@angular/core';
import {MAT_DIALOG_DATA} from '@angular/material/dialog';

@Component({
  selector: 'juice-confirm-dialog',
  templateUrl: './confirm-dialog.component.html',
  styleUrls: ['./confirm-dialog.component.scss']
})
export class ConfirmDialogComponent {

  @Output() confirmed = new EventEmitter();
  @Output() cancelled = new EventEmitter();

  confirmMessage: string = "Are you sure?";
  confirmMessageClasses: string = "";
  confirmTitle: string = "Action required";
  confirmButtonText: string = "Confirm";
  confirmButtonColor: string = "primary";

  constructor(@Inject(MAT_DIALOG_DATA) public data: {
      title: string, message: string, buttonText: string, buttonColor: string,
      messageClasses: string
    }) { 

    if(data.title){
      this.confirmTitle = data.title;
    }
    if(data.message){
      this.confirmMessage = data.message;
    }
    if(data.buttonText){
      this.confirmButtonText = data.buttonText;
    }
    if(data.buttonColor){
      this.confirmButtonColor = data.buttonColor;
    }
    if(data.messageClasses){
      this.confirmMessageClasses = data.messageClasses;
    }
  }

  confirm(){
    this.confirmed.emit();
  }

  cancel(){
    this.cancelled.emit();
  }
}
