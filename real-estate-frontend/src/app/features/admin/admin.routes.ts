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
      {
        path: 'features',
        loadComponent: () => import('./features/admin-features').then((m) => m.AdminFeatures),
      },
      {
        path: 'guarantees',
        loadComponent: () => import('./guarantees/admin-guarantees').then((m) => m.AdminGuarantees),
      },
      {
        path: 'projects',
        loadComponent: () => import('./projects/admin-projects').then((m) => m.AdminProjects),
      },
      {
        path: 'buildings',
        loadComponent: () => import('./buildings/admin-buildings').then((m) => m.AdminBuildings),
      },
      {
        path: 'units',
        loadComponent: () => import('./units/admin-units').then((m) => m.AdminUnits),
      },
    ],
  },
];
