import { Routes } from '@angular/router';

export const adminRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('../../core/layout/admin-layout/admin-layout').then((m) => m.AdminLayout),
    children: [
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'dashboard',
      },
      {
        path: 'dashboard',
        loadComponent: () => import('./dashboard/dashboard').then((m) => m.AdminDashboard),
      },
      {
        path: 'blogs',
        loadComponent: () => import('./blogs/admin-blogs').then((m) => m.AdminBlogs),
      },
      {
        path: 'messages',
        loadComponent: () => import('./messages/admin-messages').then((m) => m.AdminMessages),
      },
    ],
  },
];
