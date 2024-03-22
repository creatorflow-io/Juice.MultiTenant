import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { TenantsModule } from '@juice-js/tenants';
import { environment } from '../environments/environment';
import { HttpClientModule } from '@angular/common/http';
import { TranslateModule, MissingTranslationHandler } from '@ngx-translate/core';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MaterialModule } from './material.module';
import { OAuthModule } from 'angular-oauth2-oidc';
import { LayoutModule, UserProfileDialogModule } from '@juice-js/layout';
import { AuthModule } from '@juice-js/auth';

const {localize, auth, production, layout, tenantOptions} = environment;

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    AppRoutingModule,
    TenantsModule.forRoot(tenantOptions),
    LayoutModule.forRoot(production, layout),
    UserProfileDialogModule,
    AuthModule.forRoot(auth),
    TranslateModule.forRoot(),
    OAuthModule.forRoot(),
    BrowserAnimationsModule,
    MaterialModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }

