import { Component } from '@angular/core';

@Component({
  selector: 'app-home',
  standalone: false,
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  // Developer section options
  showDevImage = true;
  devImageUrl: string | null = 'assets/dev-avatar.jpg';
  devName = 'Developer';
  devTitle = 'Full-Stack Developer • Angular & .NET & Azure';

  onImageError() {
    // Fallback to initials avatar if image fails to load
    this.showDevImage = false;
  }
}
