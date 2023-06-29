import { Component, OnInit, SimpleChange } from '@angular/core';
import { ModelType } from '@juice-js/tenants';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit{

  title = 'app';
  identifier = 'ts';
  id = 'ts';
  settings: any = { "key1": "value1", "key2": "value2" };
  public get ModelType(){
    return ModelType;
  }

  constructor(private translate: TranslateService) {
    translate.setDefaultLang('en');
    translate.use('en');
  }

  ngOnInit(): void {
    
  }

  tenantCreated(created: any) {
    console.log("tenant created", created);
    this.identifier='';
    this.identifier = created.identifier;
    this.id = created.id;

  }

  createCancelled() {
    console.log("create cancelled");
  }

  tenantUpdated(id: any) {
    this.id = "";
    this.id = id;
    console.log("tenant updated", id);
  }

  updateCancelled(id: any) {
    console.log("update cancelled");
  }

  settingsSaved(settings: any) {
    console.log("settings changed", settings);
    this.settings = settings;
  }

  settingsChanged(change: SimpleChange) {
    console.log("settings changing", change);
  }

  settingsCancelled() {
    console.log("settings cancelled");
  }
}

