import { Component, input } from '@angular/core';
import { SectionTitle } from '../../../../shared/components/section-title/section-title';
import { ProjectVideoBlock } from '../../../../shared/components/project-video-block/project-video-block';

@Component({
  selector: 'app-project-video-section',
  imports: [SectionTitle, ProjectVideoBlock],
  templateUrl: './project-video-section.html',
  styleUrl: './project-video-section.css',
})
export class ProjectVideoSection {
  videoUrl = input.required<string>();
}
