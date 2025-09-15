import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CookieService } from 'ngx-cookie-service';

@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent {
  userEmail: string | null = null;

  constructor(private router: Router, private cookieService: CookieService) {}

  ngOnInit() {
    this.userEmail = localStorage.getItem('userEmail');
  }

  logout() {
    localStorage.removeItem('userEmail');
    this.cookieService.delete('Authorization', '/');
    this.router.navigate(['/login']);
  }
}
