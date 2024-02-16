import { Component, Input, Output, EventEmitter, OnInit , SimpleChange} from '@angular/core';

export class KeyValue {
  key: string;
  value: string;

  private _originalKey: string;
  private _originalValue: string;
  private _inherited: boolean;
  private _overridden: boolean;
  private _keyEditing: boolean = false;
  private _lastKey: string;
  private _lastValue: string;

  get keyEditing(): boolean{
    return this._keyEditing || this.key != this._lastKey;
  }
  get keyChanged(): boolean{
    return this.key != this._originalKey;
  }
  
  get inherited(): boolean{
    return this._inherited && this.value == this._originalValue;
  }
  get override(): boolean{
    return this._overridden || (this._inherited && this.value != this._originalValue);
  }

  get valueEditing(): boolean{
    return this.value != this._lastValue;
  }

  get lastValue(): string{
    return this._lastValue;
  }

  public EditKey(){
    this._keyEditing = true;
  }

  public AcceptKey(){
    this._lastKey = this.key;
    this._keyEditing = false;
  }
  
  public AcceptValue(){
    this._lastValue = this.value;
  }

  public RestoreKey(){
    this.key = this._lastKey;
    this._keyEditing = false;
  }

  public RestoreValue(){
    this.value = this._lastValue;
  }

  constructor(k: string, v: string, i: boolean = false, o: boolean = false) {
    this.key = k;
    this._originalKey = k;
    this._lastKey = k;
    this.value = v;
    this._lastValue = v;
    this._originalValue = v;
    this._inherited = i;
    this._overridden = o;
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
  @Input() model : any = {};
  @Input() parent: any = {};

  @Input() type: ModelType = ModelType.StandardObject;

  @Output() onchange = new EventEmitter<SimpleChange>();

  models: KeyValue[] = new Array<KeyValue>();

  matcher = new KeyErrorStateMatcher();

  loading: boolean = false;

  ngOnInit(): void {
    this.models = this.objectToKeyValueArray(this.model, this.parent);
  }

  objectFromKeyValueArray(arr: KeyValue[]) {
    var obj : any = {};
    arr.forEach(kv => obj[kv.key] = kv.value);
    return obj;
  }

  objectToKeyValueArray(obj: any, parent: any) {
    var parentKeys = Object.keys(parent);
    return Object.keys(obj)
      .map(k => { return new KeyValue(k, obj[k], false) })
      .concat(parentKeys.filter(k => !obj.hasOwnProperty(k))
        .map(k => { return new KeyValue(k, parent[k], true) })
      );
  }

  keyDuplicated(key: string) {
    return this.models.filter(m => m.key == key).length > 1;
  }

  keyExists(key: string) {
    return this.models.findIndex(m => m.key == key) > -1;
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
    var newKey = this.getNewKey();
    this.models.push(new KeyValue(newKey, ""));
  }
  updateValue(key: string){
    var index = this.models.findIndex(m => m.key == key);
    let lastValue = this.models[index].lastValue;
    this.models[index].AcceptValue();
    // after accepting the value, the model is updated and the lastValue is updated to value
    this.model[key] = this.models[index].lastValue;
    this.onchange.emit(new SimpleChange(new KeyValue(key, lastValue), new KeyValue(key, this.model[key]), false));
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
  
  updateKey(key: string){

    var validKey = this.type == ModelType.Configuration
      ? this.standardizeConfigurationKey(key)
      : this.standardizePropertyKey(key) ;

    if(this.keyDuplicated(validKey) || (validKey != key && this.keyExists(validKey))){
      return;
    }

    var index = this.models.findIndex(m => m.key == key);
    this.models[index].key = validKey;
    this.models[index].AcceptKey();

    this.model[validKey] = this.models[index].value;
    if(validKey != key){
      delete this.model[key];
    }
    this.onchange.emit(new SimpleChange(new KeyValue(key, this.models[index].value),
       new KeyValue(validKey, this.models[index].value), false));
  }

  save(): any {
    this.model = this.objectFromKeyValueArray(this.models);
    return this.model;
  }
}
