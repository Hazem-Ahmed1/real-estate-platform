import { Component, input } from '@angular/core';
import { ProjectStatCard } from '../../../../shared/components/project-stat-card/project-stat-card';

@Component({
  selector: 'app-project-hero',
  standalone: true,
  imports: [ProjectStatCard],
  templateUrl: './project-hero.html',
  styleUrl: './project-hero.css',
})
export class ProjectHero {
  // Data source: IProject.title
  title = input.required<string>();
  // Data source: IProject.location
  location = input.required<string>();
  // Static data: stats (passed from parent, not in IProject)
  stats = input.required<{ label: string; value: string | number; iconUrl?: string }[]>();
}
