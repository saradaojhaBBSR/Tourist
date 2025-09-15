import { Component } from '@angular/core';
import { LoginService } from './services/login.service';
import { Router } from '@angular/router';
import { CookieService } from 'ngx-cookie-service';import { BehaviorSubject } from 'rxjs';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {

  model = {
    email: '',
    password: ''
  };
  errorMessage = '';
  loading = false;

  constructor(private loginService: LoginService, private router: Router,
    private cookieService: CookieService) { }

  onSubmit(form: any) {
    this.errorMessage = '';
    if (form.invalid) return;
    this.loading = true;
    this.loginService.login(this.model).subscribe({
      next: (res) => {
        // Store email in localStorage for dashboard display
        //jwt set
        this.cookieService.set('Authorization', `Bearer ${res.token}`, undefined, '/', undefined, true, 'Strict');
        localStorage.setItem('userEmail', this.model.email);
        this.loading = false;
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = 'Invalid email or password.';
      }
    });
  }
}
