import { Component, input, output, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FilterType } from '../../../models/FilterType';


@Component({
  selector: 'app-home-section-header-with-filters',
  imports: [RouterLink],
  templateUrl: './home-section-header-with-filters.html',
  styleUrl: './home-section-header-with-filters.css',
})
export class HomeSectionHeaderWithFilters {

  subTitle = input.required<string>();
  title = input.required<string>();
  url = input.required<string>();
  showAllQueryParams = input<Record<string, any>>({});

  sellLabel = input<string>('للبيع');
  rentLabel = input<string>('للإيجار');

  filterChange = output<FilterType>();
  selectedFilter = signal<FilterType>('all');

  FilterBy(type:FilterType){
    this.selectedFilter.set(type);
    this.filterChange.emit(type);
  }
}
