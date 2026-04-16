import { Component } from '@angular/core';

type FilterKey = 'location' | 'propertyType' | 'offerType' | 'rooms' | 'minPrice' | 'maxPrice';

interface FilterField {
  key: FilterKey;
  label: string;
  options: string[];
}

@Component({
  selector: 'app-search-filter',
  imports: [],
  templateUrl: './search-filter.html',
  styleUrl: './search-filter.css',
})
export class SearchFilter {
  readonly defaultValues: Record<FilterKey, string> = {
    location: 'جدة',
    propertyType: 'شقة',
    offerType: 'للبيع',
    rooms: '04 غرف',
    minPrice: '$10.000',
    maxPrice: '$100.000',
  };

  readonly fields: FilterField[] = [
    { key: 'location', label: 'الموقع الحالي', options: ['جدة', 'الرياض', 'الدمام'] },
    { key: 'propertyType', label: 'النوع', options: ['شقة', 'فيلا', 'تاون هاوس'] },
    { key: 'offerType', label: 'العرض', options: ['للبيع', 'للإيجار'] },
    { key: 'rooms', label: 'عدد الغرف', options: ['01 غرفة', '02 غرف', '03 غرف', '04 غرف', '05 غرف'] },
    { key: 'minPrice', label: 'من سعر', options: ['$10.000', '$20.000', '$30.000'] },
    { key: 'maxPrice', label: 'الى سعر', options: ['$100.000', '$150.000', '$200.000'] },
  ];

  active: FilterKey | null = null;
  values: Record<FilterKey, string> = { ...this.defaultValues };

  toggleDropdown(field: FilterKey): void {
    this.active = this.active === field ? null : field;
  }

  select(field: FilterKey, value: string, event: MouseEvent): void {
    event.stopPropagation();
    this.values[field] = value;
    this.active = null;
  }

  resetFilters(): void {
    this.values = { ...this.defaultValues };
    this.active = null;
  }
}
