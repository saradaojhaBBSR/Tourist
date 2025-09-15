import { ErrorHandler, Injectable, NgZone } from '@angular/core';
import { Router } from '@angular/router';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  constructor(private router: Router, private ngZone: NgZone) {}

  handleError(error: any): void {
    // Log error to console or send to server
    console.error('Global Error:', error);

    // Optionally, show a user-friendly message or redirect
    this.ngZone.run(() => {
      // Redirect to a global error page or show a toast
      // this.router.navigate(['/error']);
      alert('An unexpected error occurred. Please try again later.');
    });
  }
}
