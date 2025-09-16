import { Component } from '@angular/core';
import { AdminService } from './services/admin.service';

@Component({
  selector: 'app-admin',
  standalone: false,
  templateUrl: './admin.component.html',
  styleUrl: './admin.component.css'
})
export class AdminComponent {
  users: any[] = [];
  editUserId: string | null = null;
  editUserData: any = {};
  loading = false;
  error = '';
  success = '';
  panelOpen = false;

  constructor(private adminService: AdminService) {}

  ngOnInit() {
    this.fetchUsers();
  }

  fetchUsers() {
    this.loading = true;
    this.adminService.getAll().subscribe({
      next: users => {
        this.users = users;
        this.loading = false;
      },
      error: err => {
        this.error = 'Failed to load users.';
        this.loading = false;
      }
    });
  }

  startEdit(user: any) {
    this.editUserId = user.id;
    this.editUserData = { ...user };
  }

  cancelEdit() {
    this.editUserId = null;
    this.editUserData = {};
  }

  saveEdit() {
    if (!this.editUserId) return;
    this.loading = true;
    this.success = '';
    this.error = '';
    this.adminService.patchUser(this.editUserId, this.editUserData).subscribe({
      next: () => {
        this.editUserId = null;
        this.editUserData = {};
        this.success = 'User updated successfully.';
        this.fetchUsers();
        setTimeout(() => { this.success = ''; }, 3000);
      },
      error: () => {
        this.error = 'Failed to update user.';
        this.loading = false;
      }
    });
  }

  deleteUser(id: string) {
    if (!confirm('Are you sure you want to delete this user?')) return;
    this.loading = true;
    this.adminService.deleteUser(id).subscribe({
      next: () => this.fetchUsers(),
      error: () => {
        this.error = 'Failed to delete user.';
        this.loading = false;
      }
    });
  }
}
