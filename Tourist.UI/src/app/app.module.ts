import { NgModule, LOCALE_ID } from '@angular/core';
import { registerLocaleData } from '@angular/common';
import localeEnIn from '@angular/common/locales/en-IN';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { jwtRefreshInterceptorFn } from './core/interceptors/jwt-refresh.interceptor';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { RouterModule } from '@angular/router';
import { AppComponent } from './app.component';
import { FormsModule } from '@angular/forms';
import { apiHttpInterceptorFn } from './core/interceptors/api-http-interceptor';
import { AuthGuard } from './core/auth.guard';
import { CookieService } from 'ngx-cookie-service';
import { ErrorHandler } from '@angular/core';
import { GlobalErrorHandler } from './core/global-error-handler';

@NgModule({
  declarations: [
    AppComponent,
    
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    RouterModule,
    FormsModule
  ],
  providers: [
    provideHttpClient(withInterceptors([
      apiHttpInterceptorFn,
      jwtRefreshInterceptorFn
    ])),
    AuthGuard,
    CookieService,
    { provide: ErrorHandler, useClass: GlobalErrorHandler },
    { provide: LOCALE_ID, useValue: 'en-IN' }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }

// Register locale data for Indian numbering system (₹, lakhs/crores)
registerLocaleData(localeEnIn);
