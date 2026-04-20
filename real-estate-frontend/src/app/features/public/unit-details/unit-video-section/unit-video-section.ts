import { Component, input } from '@angular/core';
import { SectionTitle } from '../../../../shared/components/section-title/section-title';
import { ProjectVideoBlock } from '../../../../shared/components/project-video-block/project-video-block';

@Component({
  selector:'app-unit-video',
  imports: [SectionTitle, ProjectVideoBlock],
  templateUrl: './unit-video-section.html',
  styleUrl: './unit-video-section.css',
})
export class UnitVideoSection {
  videoUrl = input.required<string>();
  videoPoster = input.required<string>();
}
