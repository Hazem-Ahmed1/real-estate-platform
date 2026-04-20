import { Component, input, signal } from '@angular/core';
import { UnitFeatures } from './unit-features/unit-features';
import { UnitWarranties } from './unit-warranties/unit-warranties';
import { UnitDesigns } from './unit-designs/unit-designs';
import { UnitNearby } from './unit-nearby/unit-nearby';

type UnitDetailsTab = 'features' | 'warranties' | 'designs' | 'nearby';

@Component({
  selector: 'app-unit-tabs',
  standalone: true,
  imports: [UnitFeatures, UnitWarranties, UnitDesigns, UnitNearby],
  templateUrl: './unit-tabs.html',
  styleUrl: './unit-tabs.css',
})
export class UnitTabs {
  // Static data: all arrays passed from parent (none exist in UnitCardModel)
  features = input.required<{ label: string; iconClass: string }[]>();
  warranties = input.required<{ label: string; iconClass: string; subLabel: string }[]>();
  designs = input.required<string[]>();
  nearbyPlaces = input.required<{ name: string; distanceText: string }[]>();

  readonly activeTab = signal<UnitDetailsTab>('features');

  setTab(tab: UnitDetailsTab): void {
    this.activeTab.set(tab);
  }
}
