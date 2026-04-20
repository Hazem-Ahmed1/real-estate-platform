import { Component, input } from '@angular/core';
import { IconFeatureGrid } from '../../../../../shared/components/icon-feature-grid/icon-feature-grid';

@Component({
  selector: 'app-unit-warranties',
  standalone: true,
  imports: [IconFeatureGrid],
  template: `<app-icon-feature-grid [items]="warranties()" [variant]="'warranties'" />`,
})
export class UnitWarranties {
  warranties = input.required<{ label: string; iconClass: string; subLabel: string }[]>();
}
