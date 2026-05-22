import { Routes } from '@angular/router';
import { Home } from './features/public/home/home';
import { Projects } from './features/public/projects/projects';
import { SoldProjects } from './features/public/sold-projects/sold-projects';
import { Units } from './features/public/units/units';
import { Contact } from './features/public/contact/contact';

import { ProjectDetails } from './features/public/project-details/project-details';
import { UnitDetails } from './features/public/unit-details/unit-details';

import { UnitPage } from './features/public/unit-page/unit-page';
import { Login } from './features/public/auth/login/login';
import { Register } from './features/public/auth/register/register';
import { PublicLayout } from './core/layout/public-layout/public-layout';
import { adminAuthGuard } from './core/guards/admin-auth.guard';
import { Blog } from './features/public/blog/blog';
import { BlogWithSidebar } from './features/public/blog-with-sidebar/blog-with-sidebar';
import { AboutUs } from './features/public/about-us/about-us';

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
        path: 'units/:projectId',
        component: Units,
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
      {
        path: 'project-details/:id',
        component: ProjectDetails,
      },
      {
        path: 'project-details',
        component: ProjectDetails,
      },
      {
        path: 'unit-details/:id',
        component: UnitDetails,
      },
      {
        path: 'unit-details',
        component: UnitDetails,
      },
      {
        path: 'blog',
        component: Blog,
      },
      {
        path: 'blog/:id',
        component: BlogWithSidebar,
      },
      {
        path: 'about-us',
        component: AboutUs,
      },
    ],
  },
  {
    path: 'admin',
    canMatch: [adminAuthGuard],
    loadChildren: () =>
      import('./features/admin/admin.routes').then((m) => m.adminRoutes),
  },
];
