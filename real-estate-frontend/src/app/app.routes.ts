import { Routes } from '@angular/router';
import { Home } from './features/public/home/home';
import { Projects } from './features/public/projects/projects';
import { SoldProjects } from './features/public/sold-projects/sold-projects';
import { Units } from './features/public/units/units';
import { Contact } from './features/public/contact/contact';

export const routes: Routes = [
  {
    path: "",
    pathMatch: "full",
    component: Home,
  },
  {
    path: "projects",
    component: Projects,
  },
  {
    path: "sold-projects",
    component: SoldProjects,
  },
  {
    path: "units",
    component: Units,
  }
  ,{
    path:"contact-us",
    component:Contact
  }


];
