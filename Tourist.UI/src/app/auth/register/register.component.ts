
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
  loading = false;
  successMessage = '';
  errorMessage = '';

  constructor(private registerService: RegisterService) {}

  countries: any[] = [];
  states: any[] = [];
  cities: any[] = [];

  async onCountryChange(countryCode: string) {
    this.model.country = countryCode;
    const { State } = await import('country-state-city');
    this.states = State.getStatesOfCountry(countryCode);
    this.model.state = '';
    this.cities = [];
    this.model.city = '';
  }


  async onStateChange(stateCode: string) {
    this.model.state = stateCode;
    const { City } = await import('country-state-city');
    this.cities = City.getCitiesOfState(this.model.country, stateCode);
    this.model.city = '';
  }

  async ngOnInit() {
    const { Country } = await import('country-state-city');
    // Only load India
    this.countries = [Country.getCountryByCode('IN')].filter(Boolean);
    // Optionally, set default country to India
    this.model.country = 'IN';
    await this.onCountryChange('IN');
  }

  async onSubmit(form: any) {
    this.submitted = true;
    this.successMessage = '';
    this.errorMessage = '';
    this.loading = true;
    if (form.valid) {
      const { Country, State, City } = await import('country-state-city');
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
          this.loading = false;
        },
        error: (err) => {
          this.errorMessage = 'Registration failed. Please try again.';
          this.loading = false;
        }
      });
    } else {
      this.loading = false;
    }
  }
}
