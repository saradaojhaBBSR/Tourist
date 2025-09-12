import { Component } from '@angular/core';

import { RegisterService } from './services/register.service';
import { RegisterModel } from './Models/register.model';

@Component({
  selector: 'app-register',
  standalone: false,
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  model: RegisterModel = {
    firstName: '',
    middleName: '',
    lastName: '',
    country: '',
    state: '',
    city: '',
    email: '',
    PhoneNumber: '',
    password: '' ,
    confirmPassword: ''   
  };
  submitted = false;
  successMessage = '';
  errorMessage = '';

  constructor(private registerService: RegisterService) {}

  onSubmit(form: any) {
    this.submitted = true;
    this.successMessage = '';
    this.errorMessage = '';
    if (form.valid) {
      this.registerService.register(this.model).subscribe({
        next: (res) => {
          this.successMessage = typeof res === 'string' ? res : (res && res.message ? res.message : 'Registration successful!');
          form.resetForm();
          this.submitted = false;
        },
        error: (err) => {
          this.errorMessage = 'Registration failed. Please try again.';
        }
      });
    }
  }
}
