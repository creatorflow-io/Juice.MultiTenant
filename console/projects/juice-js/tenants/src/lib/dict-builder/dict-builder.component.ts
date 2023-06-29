import { Component, Input, Output, EventEmitter, OnInit , SimpleChange} from '@angular/core';

export class KeyValue {
  key: string;
  value: string;

  constructor(k: string, v: string) {
    this.key = k;
    this.value = v;
  }
}

export enum ModelType{
  StandardObject = 'standard',
  Configuration = 'configuration'
}

import { FormControl, FormGroupDirective, NgForm } from '@angular/forms';
import {ErrorStateMatcher} from '@angular/material/core';

/** Error when invalid control is dirty, touched, or submitted. */
export class KeyErrorStateMatcher implements ErrorStateMatcher {
  isErrorState(control: FormControl | null, form: FormGroupDirective | NgForm | null): boolean {
    return control!=null && (control.dirty || control.touched);
  }
}


@Component({
  selector: 'juice-dict-builder',
  templateUrl: './dict-builder.component.html',
  styleUrls: ['./dict-builder.component.css']
})

export class DictBuilderComponent implements OnInit{
  @Input() title = "Settings";
  @Input() model : any = {};

  @Input() type: ModelType = ModelType.StandardObject;

  @Output() onchange = new EventEmitter<SimpleChange>();
  
  @Output() saved = new EventEmitter<any>();
  
  @Output() cancelled = new EventEmitter();

  models: KeyValue[] = new Array<KeyValue>();
  
  editingKey: string = "";
  editKeyModel: string = "";

  matcher = new KeyErrorStateMatcher();

  ngOnInit(): void {
    this.models = this.objectToKeyValueArray(this.model);
  }

  objectFromKeyValueArray(arr: KeyValue[]) {
    var obj : any = {};
    arr.forEach(kv => obj[kv.key] = kv.value);
    return obj;
  }

  objectToKeyValueArray(obj: any) {
    return Object.keys(obj).map(k => { return { key: k, value: obj[k] } });
  }

  keyExists(key: string) {
    return key != this.editingKey && this.models.findIndex(m => m.key == key) > -1;
  }

  isEditing(key: string) {
    return key == this.editingKey;
  }

  getNewKey() {
    var i = 1;
    while (this.keyExists("newkey" + i)) {
      i++;
    }
    return "newkey" + i;
  }

  standardizePropertyKey(key: string) {
    return key.trim().replace(/\s+/g, "") // remove spaces
    .replace(/[^a-zA-Z0-9_]/g, "") // remove invalid chars
    .replace(/^[^a-zA-Z]+/g, "") // remove starting non-alpha chars
    .replace(/[^a-zA-Z0-9_]$/g, "") // remove ending non-alphanumeric chars
    ;
  }

  standardizeConfigurationKey(key: string) {
    return key.trim().replace(/\s+/g, "") // remove spaces
    .replace(/[^a-zA-Z0-9_:]/g, "") // remove invalid chars
    .replace(/(:[:0-9]+)/g, ":") // remove multiple colons or numbers after colon
    .replace(/^[^a-zA-Z]+/g, "") // remove starting non-alpha chars
    .replace(/[^a-zA-Z0-9]$/g, "") // remove ending non-alphanumeric chars
    ;
  }

  add(){
    if(this.editingKey != ""){
      this.updateValue(this.editingKey);
      this.updateKey(this.editingKey);
    }
    
    var newKey = this.getNewKey();
    this.editingKey = newKey;
    this.editKeyModel = newKey;
    this.models.push(new KeyValue(newKey, ""));
  }
  updateValue(key: string){
    var index = this.models.findIndex(m => m.key == key);
    this.model[key] = this.models[index].value;
    this.onchange.emit(new SimpleChange(new KeyValue(key, this.model[key]), new KeyValue(key, this.models[index].value), false));
  }

  clear(key: string){
    this.models.find(m => m.key == key)!.value = "";
  }

  remove(key: string){
    var value = this.model[key];
    var index = this.models.findIndex(m => m.key == key);
    this.models.splice(index, 1);
    delete this.model[key];
    this.onchange.emit(new SimpleChange(new KeyValue(key, value), null, false));
  }
  
  editKey(key: string){
    this.editingKey = key;
    this.editKeyModel = key;
  }
  cancelEditKey(){
    this.editingKey = "";
    this.editKeyModel = "";
  }
  updateKey(key: string){
    this.editKeyModel = this.type == ModelType.Configuration
      ? this.standardizeConfigurationKey(this.editKeyModel)
      : this.standardizePropertyKey(this.editKeyModel) ;

    if(this.keyExists(this.editKeyModel)){
      return;
    }
    var index = this.models.findIndex(m => m.key == key);
    this.models[index].key = this.editKeyModel;
    this.model[this.editKeyModel] = this.models[index].value;
    delete this.model[key];
    this.onchange.emit(new SimpleChange(new KeyValue(key, this.models[index].value),
       new KeyValue(this.editKeyModel, this.models[index].value), false));
    this.editingKey = "";
    this.editKeyModel = "";
  }

  save() {
    this.model = this.objectFromKeyValueArray(this.models);
    this.saved.emit(this.model);
  }
  
  cancel(){
    this.cancelled.emit();
  }
}
