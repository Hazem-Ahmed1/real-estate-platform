import { Component, input, signal } from '@angular/core';
import { ProjectCard } from "../project-card/project-card";
import { IProject } from '../../../models/IProject';

@Component({
  selector: 'app-projects-list',
  imports: [ProjectCard],
  templateUrl: './projects-list.html',
  styleUrl: './projects-list.css',
})
export class ProjectsList {

  projects = input.required<IProject[]>()

}
