import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { TenantUpdate } from '../shared/models/tenant.update.model';
import { TenantAdminService } from '../../public-api';
import {
  FormControl,
  FormGroup,
  Validators
} from '@angular/forms';
import { CustomErrorStateMatcher } from '../shared/custom-error-state-matcher';


@Component({
  selector: 'juice-tenant-update',
  templateUrl: './tenant-update.component.html',
  styleUrls: ['./tenant-update.component.scss']
})
export class TenantUpdateComponent{

  @Input() id!: string;

  model: TenantUpdate = new TenantUpdate();

  identifierFormControl = new FormControl(this.model.identifier, [Validators.required]);
  nameFormControl = new FormControl(this.model.name, [Validators.required]);
  coneectionFormControl = new FormControl(this.model.connectionString, []);

  updateForm = new FormGroup({
    identifier: this.identifierFormControl,
    name: this.nameFormControl,
    connectionString: this.coneectionFormControl
  });

  matcher = new CustomErrorStateMatcher();

  @Output() updated = new EventEmitter<string>();
  @Output() cancelled = new EventEmitter<string>();

  constructor(private tenantService: TenantAdminService) { }

  ngOnChanges(): void {
    this.tenantService.getTenant(this.id).subscribe((result) => {
      this.updateForm.patchValue(result);
      this.updateModel();
      if(this.id == result.identifier){
        this.id = result.id;
      }
    });
  }

  updateModel(){
    var value = this.updateForm.value;
    this.model.identifier = value.identifier?? '';
    this.model.name = value.name??'';
    this.model.connectionString = value.connectionString??'';
  }

  // for modal control
  setId(value: string){
    this.id = value;
    this.ngOnChanges();
  }

  submit(){
    if(this.updateForm.valid){
      this.updateModel();
      this.tenantService.updateTenant(this.id, this.model).subscribe((_) => {
        this.updateForm.reset();
        this.model = new TenantUpdate();
        this.updated.emit(this.id);
      });
    }
  }

  cancel(){
    this.cancelled.emit(this.id);
  }
}
