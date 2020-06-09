import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HTTP_INTERCEPTORS } from '@angular/common/http';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { IdentityModule } from './components/identity/identity.module';
import { TokenInterceptor } from './core/token.interceptor';
import { AuthGuard } from './core/auth.guard';
import { AdminGuard } from './core/admin.guard';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    IdentityModule
  ],
  providers: [
    {
        provide: HTTP_INTERCEPTORS,
        useClass: TokenInterceptor,
        multi: true
    },
    AuthGuard, AdminGuard],
  bootstrap: [AppComponent]
})
export class AppModule { }
