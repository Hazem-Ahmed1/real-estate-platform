import { Routes } from '@angular/router';
import { Home } from './features/public/home/home';
import { Projects } from './features/public/projects/projects';
import { SoldProjects } from './features/public/sold-projects/sold-projects';
import { Units } from './features/public/units/units';
import { Contact } from './features/public/contact/contact';
import { ProjectDetails } from './features/public/project-details/project-details';
import { UnitDetails } from './features/public/unit-details/unit-details';

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
  },
  {
    path: "project-details",
    component: ProjectDetails,
  },
  {
    path: "unit-details",
    component: UnitDetails,
  },
  
  {
    path:"contact-us",
    component:Contact
  }


];
