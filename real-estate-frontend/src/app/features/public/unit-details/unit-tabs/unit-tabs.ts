import { Component, computed, effect, input, signal } from '@angular/core';
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
  nearbyPlaces = input.required<{ name: string; distanceText: string; iconClass?: string }[]>();

  readonly activeTab = signal<UnitDetailsTab>('features');

  readonly hasFeatures = computed(() => (this.features()?.length ?? 0) > 0);
  readonly hasWarranties = computed(() => (this.warranties()?.length ?? 0) > 0);
  readonly hasDesigns = computed(() => (this.designs()?.length ?? 0) > 0);
  readonly hasNearby = computed(() => (this.nearbyPlaces()?.length ?? 0) > 0);

  constructor() {
    effect(() => {
      const firstAvailable = this.hasFeatures()
        ? 'features'
        : this.hasWarranties()
          ? 'warranties'
          : this.hasDesigns()
            ? 'designs'
            : this.hasNearby()
              ? 'nearby'
              : null;

      if (firstAvailable && !this.isTabAvailable(this.activeTab())) {
        this.activeTab.set(firstAvailable);
      }
    });
  }

  setTab(tab: UnitDetailsTab): void {
    if (!this.isTabAvailable(tab)) {
      return;
    }
    this.activeTab.set(tab);
  }

  private isTabAvailable(tab: UnitDetailsTab): boolean {
    switch (tab) {
      case 'features': return this.hasFeatures();
      case 'warranties': return this.hasWarranties();
      case 'designs': return this.hasDesigns();
      case 'nearby': return this.hasNearby();
    }
  }
}
