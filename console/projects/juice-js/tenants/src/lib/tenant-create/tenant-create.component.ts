import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';

import { TenantCreate, TenantCreated } from '../shared/models/tenant.create.model';
import { TenantAdminService } from '../../public-api';

import {
  FormControl,
  FormGroup,
  Validators
} from '@angular/forms';
import { CustomErrorStateMatcher } from '../shared/custom-error-state-matcher';

@Component({
  selector: 'juice-tenant-create',
  templateUrl: './tenant-create.component.html',
  styleUrls: ['./tenant-create.component.css']
})
export class TenantCreateComponent implements OnInit{
  model: TenantCreate = new TenantCreate();

  emailFormControl = new FormControl('', [Validators.email]);
  identifierFormControl = new FormControl('', [Validators.required]);
  nameFormControl = new FormControl('', [Validators.required]);

  createForm = new FormGroup({
    identifier: this.identifierFormControl,
    name: this.nameFormControl,
    email: this.emailFormControl
  });

  matcher = new CustomErrorStateMatcher();

  @Output() created = new EventEmitter<TenantCreated>();
  @Output() cancelled = new EventEmitter();

  constructor(private tenantService: TenantAdminService) { }

  ngOnInit(): void {

  }
  updateModel(){
    var value = this.createForm.value;
    this.model.identifier = value.identifier?? '';
    this.model.name = value.name??'';
    this.model.adminEmail = value.email??'';
  }

  submit(){
    if(this.createForm.valid){
      this.updateModel();
      this.tenantService.createTenant(this.model).subscribe({
          next:(result) => {
            if(this.created!=null){
              this.created.emit(result);
            }
            this.createForm.reset();
            this.model = new TenantCreate();
          },
          error:(err) => {

          }
        });
    }
  }
  cancel(){
    this.cancelled.emit();
  }
}
