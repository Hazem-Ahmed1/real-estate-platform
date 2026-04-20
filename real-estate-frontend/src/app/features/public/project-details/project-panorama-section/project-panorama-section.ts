import { Component, input } from '@angular/core';
import { SectionTitle } from '../../../../shared/components/section-title/section-title';
import { PanoramaImage } from '../../../../shared/components/panorama-image/panorama-image';

@Component({
  selector: 'app-project-panorama',
  standalone: true,
  imports: [SectionTitle, PanoramaImage],
  templateUrl: './project-panorama-section.html',
  styleUrl: './project-panorama-section.css',
})
export class ProjectPanoramaSection {
  panoramaImage = input.required<string>();
}
