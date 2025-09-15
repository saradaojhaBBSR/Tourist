import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { CookieService } from 'ngx-cookie-service';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(private router: Router, private cookieService: CookieService) {}

  canActivate(): boolean {
    // Check for JWT token in cookies (must start with 'Bearer ')
    const token = this.cookieService.get('Authorization');
    if (token && token.startsWith('Bearer ') && token.length > 10) {
      return true;
    }
    this.router.navigate(['/login']);
    return false;
  }
}
