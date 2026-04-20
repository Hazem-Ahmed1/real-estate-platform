import { Component, input } from '@angular/core';
import { IconFeatureGrid } from '../../../../../shared/components/icon-feature-grid/icon-feature-grid';

@Component({
  selector: 'app-unit-features',
  standalone: true,
  imports: [IconFeatureGrid],
  template: `<app-icon-feature-grid [items]="features()" [variant]="'features'" />`,
})
export class UnitFeatures {
  features = input.required<{ label: string; iconClass: string }[]>();
}
