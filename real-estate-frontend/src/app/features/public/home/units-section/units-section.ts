import { Component, OnDestroy, OnInit, inject, signal, computed } from '@angular/core';
import { Subscription } from 'rxjs';
import { UnitCardModel } from '../../../../models/IUnit';
import { UnitService } from '../../../../services/api/unit.service';
import { HomeSectionHeaderWithFilters } from '../../../../shared/components/home-section-header-with-filters/home-section-header-with-filters';
import { UnitsList } from '../../../../shared/components/units-list/units-list';

@Component({
  selector: 'app-units-section',
  imports: [HomeSectionHeaderWithFilters, UnitsList],
  templateUrl: './units-section.html',
  styleUrl: './units-section.css',
})
export class UnitsSection implements OnInit, OnDestroy {
  private readonly unitService = inject(UnitService);
  private sub?: Subscription;
  private readonly allowedStatuses = ['Sale', 'Rent'];

  // Signal — live data from GET /api/units?pageSize=6
  readonly allUnits = signal<UnitCardModel[]>([]);
  readonly hasLoaded = signal(false);

  // Active status filter ('all' | 'sell' | 'rent')
  readonly activeFilter = signal('all');

  // Filtered units signal (computed dynamically from allUnits and activeFilter)
  readonly units = computed(() => {
    const list = this.allUnits().filter(u => this.allowedStatuses.includes(u.status));
    const filterVal = this.activeFilter();
    if (filterVal === 'all') {
      return list;
    }
    const status = filterVal === 'sell' ? this.allowedStatuses[0] : this.allowedStatuses[1];
    return list.filter(u => u.status === status);
  });

  readonly emptyStateMessage = computed(() => {
    const filterVal = this.activeFilter();
    if (filterVal === 'sell') {
      return 'لا توجد وحدات معروضة للبيع حالياً';
    }
    if (filterVal === 'rent') {
      return 'لا توجد وحدات معروضة للإيجار حالياً';
    }
    return 'لا توجد وحدات متاحة حالياً';
  });

  ngOnInit(): void {
    this.loadUnits();
  }

  filter(val: string): void {
    this.activeFilter.set(val);
  }

  private loadUnits(): void {
    this.sub?.unsubscribe();
    this.sub = this.unitService
      .getUnits({ pageSize: 6 })
      .subscribe({
        next: (page) => {
          this.allUnits.set(page.items ?? []);
          this.hasLoaded.set(true);
        },
        error: () => {
          this.allUnits.set([]);
          this.hasLoaded.set(true);
        },
      });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
