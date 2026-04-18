import { Component, input } from '@angular/core';

@Component({
  selector: 'app-project-stat-card',
  standalone: true,
  templateUrl: './project-stat-card.html',
  styleUrl: './project-stat-card.css',
})
export class ProjectStatCard {
  label = input.required<string>();
  value = input.required<string | number>();
  iconUrl = input.required<string | undefined>();
}