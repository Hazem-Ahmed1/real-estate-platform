import { Component, input } from '@angular/core';
import { ProjectGallery } from '../../../../../shared/components/project-gallery/project-gallery';

@Component({
  selector: 'app-unit-designs',
  standalone: true,
  imports: [ProjectGallery],
  template: `<app-project-gallery [images]="designs()" />`,
})
export class UnitDesigns {
  designs = input.required<string[]>();
}
