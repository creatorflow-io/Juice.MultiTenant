import { Component, Input, Output, EventEmitter, ViewChild, AfterViewInit } from '@angular/core';
import { DictBuilderComponent, KeyValue, ModelType } from '@juice-js/dict-builder';

@Component({
  selector: 'juice-tenant-settings',
  templateUrl: './tenant-settings.component.html',
  styleUrls: ['./tenant-settings.component.scss']
})
export class TenantSettingsComponent{
  @Input() title = "Settings";
  @Output() saved = new EventEmitter<any>();
  
  @Output() cancelled = new EventEmitter();

  type: ModelType = ModelType.StandardObject;
  loading: boolean = false;
  @ViewChild('dict') dictBuilder!: DictBuilderComponent;

  setModel(model: KeyValue[]) {
    this.dictBuilder.models = model;
  }
  getModel():KeyValue[] {
    return this.dictBuilder.models;
  }
  save() {
    var model = this.dictBuilder.save();
    this.saved.emit(model);
  }
  
  cancel(){
    this.cancelled.emit();
  }
}
