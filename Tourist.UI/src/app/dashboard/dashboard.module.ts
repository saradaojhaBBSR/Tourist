import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DashboardComponent } from './dashboard.component';
import { DashboardRoutingModule } from './dashboard-routing.module';
import { AdminComponent } from '../admin/admin.component';

@NgModule({
  declarations: [DashboardComponent, AdminComponent],
  imports: [CommonModule, FormsModule, DashboardRoutingModule]
})
export class DashboardModule {}
