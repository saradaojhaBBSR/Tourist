

  import { Component } from '@angular/core';

import { RegisterService } from './services/register.service';
import { RegisterModel } from './Models/register.model';
import { Country, State, City } from 'country-state-city';

@Component({
  selector: 'app-register',
  standalone: false,
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  getSelectValue(event: Event): string {
    return (event.target && (event.target as HTMLSelectElement).value) || '';
  }
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

  countries = Country.getAllCountries();
  states: any[] = [];
  cities: any[] = [];
  onCountryChange(countryCode: string) {
    this.model.country = countryCode;
    this.states = State.getStatesOfCountry(countryCode);
    this.model.state = '';
    this.cities = [];
    this.model.city = '';
  }

  onStateChange(stateCode: string) {
    this.model.state = stateCode;
    this.cities = City.getCitiesOfState(this.model.country, stateCode);
    this.model.city = '';
  }

  onSubmit(form: any) {
    this.submitted = true;
    this.successMessage = '';
    this.errorMessage = '';
    if (form.valid) {
      // Prepare a copy of the model with names instead of codes
      const countryObj = this.countries.find(c => c.isoCode === this.model.country);
      const stateObj = this.states.find(s => s.isoCode === this.model.state);
      const cityObj = this.cities.find(c => c.name === this.model.city);
      const payload = {
        ...this.model,
        country: countryObj ? countryObj.name : '',
        state: stateObj ? stateObj.name : '',
        city: cityObj ? cityObj.name : ''
      };
      this.registerService.register(payload).subscribe({
        next: (res) => {
          this.successMessage = typeof res === 'string' ? res : (res && res.message ? res.message : 'Registration successful!');
          form.resetForm();
          // Reset dropdowns to default
          this.model.country = '';
          this.model.state = '';
          this.model.city = '';
          this.states = [];
          this.cities = [];
          this.submitted = false;
        },
        error: (err) => {
          this.errorMessage = 'Registration failed. Please try again.';
        }
      });
    }
  }
}
