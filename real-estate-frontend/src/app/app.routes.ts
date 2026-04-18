import { Routes } from '@angular/router';
import { Home } from './features/public/home/home';
import { Projects } from './features/public/projects/projects';
import { SoldProjects } from './features/public/sold-projects/sold-projects';
import { Units } from './features/public/units/units';
import { Contact } from './features/public/contact/contact';
import { UnitPage } from './features/public/unit-page/unit-page';
import { Login } from './features/public/auth/login/login';
import { Register } from './features/public/auth/register/register';
import { AdminDashboard } from './features/admin/dashboard/dashboard';
import { PublicLayout } from './core/layout/public-layout/public-layout';
import { AdminLayout } from './core/layout/admin-layout/admin-layout';

export const routes: Routes = [
  {
    path: '',
    component: PublicLayout,
    children: [
      {
        path: '',
        pathMatch: 'full',
        component: Home,
      },
      {
        path: 'projects',
        component: Projects,
      },
      {
        path: 'sold-projects',
        component: SoldProjects,
      },
      {
        path: 'units',
        component: Units,
      },
      {
        path: 'contact-us',
        component: Contact,
      },
      {
        path: 'unit',
        component: UnitPage,
      },
      {
        path: 'login',
        component: Login,
      },
      {
        path: 'register',
        component: Register,
      },
    ],
  },
  {
    path: 'admin',
    component: AdminLayout,
    children: [
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'dashboard',
      },
      {
        path: 'dashboard',
        component: AdminDashboard,
      },
    ],
  },



];
