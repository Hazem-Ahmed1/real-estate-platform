import { Component, input, output, signal } from '@angular/core';
import { FilterType } from '../../../models/FilterType';

@Component({
  selector: 'app-filters',
  standalone: true,
  templateUrl: './filters.html',
  styleUrl: './filters.css',
})
export class Filters {
  subTitle = input.required<string>();
  title = input.required<string>();

  selectedFilter = signal<FilterType>('all');

  showFilter = signal(false);

  advancedFilters = signal({
    minPrice: null as number | null,
    maxPrice: null as number | null,
    rooms: null as number | null,
  });

  filterChange = output<{
    type: FilterType;
    advanced: any;
  }>();

  filterBy(type: FilterType) {
    this.selectedFilter.set(type);

    this.filterChange.emit({
      type,
      advanced: this.advancedFilters(),
    });
  }

  toggleFilter() {
    this.showFilter.update((v) => !v);
  }

  applyFilters() {
    this.filterChange.emit({
      type: this.selectedFilter(),
      advanced: this.advancedFilters(),
    });

    this.showFilter.set(false);
  }
}
