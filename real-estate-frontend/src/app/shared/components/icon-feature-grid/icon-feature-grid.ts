import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'app-icon-feature-grid',
  standalone: true,
  templateUrl: './icon-feature-grid.html',
  styleUrl: './icon-feature-grid.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IconFeatureGrid {
  items = input.required<{label: string, iconClass: string, subLabel?: string}[]>();
  variant = input<'features' | 'warranties'>('features');
}