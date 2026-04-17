import { Component, input } from '@angular/core';
import { SoldProjectCard } from "../sold-project-card/sold-project-card";
import { ISoldProject } from '../../../models/ISoldProject';

@Component({
  selector: 'app-sold-projects-list',
  imports: [SoldProjectCard],
  templateUrl: './sold-projects-list.html',
  styleUrl: './sold-projects-list.css',
})
export class SoldProjectsList {
  projects = input.required<ISoldProject[]>();
}
