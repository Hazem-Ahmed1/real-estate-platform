import { Routes } from '@angular/router';
import { Home } from './features/public/home/home';
import { Projects } from './features/public/projects/projects';
import { SoldProjects } from './features/public/sold-projects/sold-projects';
import { Units } from './features/public/units/units';

export const routes: Routes = [
  {
    path:"home",
    component:Home
  },
  {
    path:"projects",
    component:Projects
  },
  {
    path:"sold-projects",
    component:SoldProjects
  },
  {
    path:"units",
    component:Units
  }


];
