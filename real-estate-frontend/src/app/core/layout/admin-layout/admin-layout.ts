import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AdminNavbar } from '../../../features/admin/layout/admin-navbar/admin-navbar';
import { AdminSidebar } from '../../../features/admin/layout/admin-sidebar/admin-sidebar';
import { Snackbar } from '../../../shared/components/snackbar/snackbar';

@Component({
  selector: 'app-admin-layout',
  imports: [RouterOutlet, AdminSidebar, AdminNavbar, Snackbar],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminLayout {}
