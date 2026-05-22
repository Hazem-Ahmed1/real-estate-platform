import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { IProject } from '../../../models/IProject';

@Component({
  selector: 'app-project-card',
  imports: [RouterLink],
  templateUrl: './project-card.html',
  styleUrl: './project-card.css',
})
export class ProjectCard {
  project = input.required<IProject>();
}
