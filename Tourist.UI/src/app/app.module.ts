import { NgModule } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { jwtRefreshInterceptorFn } from './core/interceptors/jwt-refresh.interceptor';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { RouterModule } from '@angular/router';
import { AppComponent } from './app.component';
import { HomeComponent } from './home/home.component';
import { RegisterComponent } from './auth/register/register.component';
import { LoginComponent } from './auth/login/login.component';
import { FormsModule } from '@angular/forms';
import { apiHttpInterceptorFn } from './core/interceptors/api-http-interceptor';
import { DashboardComponent } from './dashboard/dashboard.component';
import { AuthGuard } from './core/auth.guard';
import { CookieService } from 'ngx-cookie-service';
import { ErrorHandler } from '@angular/core';
import { GlobalErrorHandler } from './core/global-error-handler';

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    RegisterComponent,
    LoginComponent,   
    DashboardComponent
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
    { provide: ErrorHandler, useClass: GlobalErrorHandler }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
