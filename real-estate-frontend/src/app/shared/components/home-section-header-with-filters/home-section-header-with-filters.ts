import { Component, input, output } from '@angular/core';
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

  filterChange = output<FilterType>();

  FilterBy(type:FilterType){
    this.filterChange.emit(type);
  }



}
