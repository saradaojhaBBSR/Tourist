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
  userRole: string | null = null;

  constructor(private router: Router, private cookieService: CookieService) {}

  ngOnInit() {
    this.userEmail = localStorage.getItem('userEmail');
    this.userRole = localStorage.getItem('user-role');
  }

  isAdmin(): boolean {
    return this.userRole === 'Admin';
  }

  logout() {
    localStorage.removeItem('userEmail');
    localStorage.removeItem('user-role');
    this.cookieService.delete('Authorization', '/');
    this.router.navigate(['/login']);
  }
}
