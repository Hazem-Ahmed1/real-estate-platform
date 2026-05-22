import { Component, OnInit, inject, signal } from '@angular/core';
import { SearchStateService } from '../../../../services/search-state.service';
import { SearchOptionsService } from '../../../../services/api/search-options.service';

type FilterKey = 'searchType' | 'city' | 'propertyType' | 'offerType' | 'rooms' | 'minPrice' | 'maxPrice';

interface FilterField {
  key: FilterKey;
  label: string;
  options: string[];
}

@Component({
  selector: 'app-search-filter',
  templateUrl: './search-filter.html',
  styleUrl: './search-filter.css',
})
export class SearchFilter implements OnInit {
  private readonly searchState = inject(SearchStateService);
  private readonly searchOptionsService = inject(SearchOptionsService);

  readonly defaultValues: Record<FilterKey, string> = {
    searchType: 'مشاريع',
    city: 'الكل',
    propertyType: 'الكل',
    offerType: 'الكل',
    rooms: 'الكل',
    minPrice: 'الكل',
    maxPrice: 'الكل',
  };

  readonly fields = signal<FilterField[]>(this.buildDefaultFields());

  active = signal<FilterKey | null>(null);
  values = signal<Record<FilterKey, string>>({ ...this.defaultValues });

  ngOnInit(): void {
    this.loadOptions();
  }

  toggleDropdown(field: FilterKey): void {
    this.active.set(this.active() === field ? null : field);
  }

  select(field: FilterKey, value: string, event: MouseEvent): void {
    event.stopPropagation();
    this.values.update(v => ({ ...v, [field]: value }));
    this.active.set(null);
  }

  doSearch(): void {
    const v = this.values();
    const params: Record<string, any> = {};

    if (v.city !== 'الكل') params['city'] = v.city;

    if (v.propertyType !== 'الكل') {
      const typeMap: Record<string, string> = {
        'شقة': 'Apartment', 'فيلا': 'Villa', 'دوبلكس': 'Duplex',
        'مكتب': 'Office'
      };
      params['type'] = typeMap[v.propertyType] || v.propertyType;
    }

    if (v.offerType !== 'الكل') {
      params['status'] = v.offerType === 'للبيع' ? 'Sale' : 'Rent';
    }

    if (v.rooms !== 'الكل') params['rooms'] = parseInt(v.rooms, 10);
    if (v.minPrice !== 'الكل') params['minPrice'] = parseInt(v.minPrice, 10);
    if (v.maxPrice !== 'الكل') params['maxPrice'] = parseInt(v.maxPrice, 10);

    const searchType = v.searchType === 'مشاريع' ? 'project' : 'unit';

    this.searchState.search(params, searchType, 1);
  }

  private loadOptions(): void {
    this.searchOptionsService.getSearchOptions().subscribe({
      next: (options) => {
        this.applyCityOptions(options.cities);
      },
      error: () => {
        this.fields.set(this.buildDefaultFields());
      }
    });
  }

  private buildDefaultFields(): FilterField[] {
    return [
      { key: 'searchType', label: 'البحث عن', options: ['مشاريع', 'وحدات'] },
      { key: 'city', label: 'المدينة', options: ['الكل', 'جدة', 'الرياض', 'الدمام'] },
      { key: 'propertyType', label: 'النوع', options: ['الكل', 'شقة', 'فيلا', 'دوبلكس', 'مكتب'] },
      { key: 'offerType', label: 'العرض', options: ['الكل', 'للبيع', 'للإيجار'] },
      { key: 'rooms', label: 'عدد الغرف', options: ['الكل', '2', '3', '4', '5'] },
      { key: 'minPrice', label: 'من سعر', options: ['الكل', '10000', '20000', '30000'] },
      { key: 'maxPrice', label: 'الى سعر', options: ['الكل', '100000', '150000', '200000'] },
    ];
  }

  private applyCityOptions(cities: string[]): void {
    const options = ['الكل', ...cities.filter(c => c && c.trim().length > 0)];
    this.fields.update(fields =>
      fields.map(f => f.key === 'city' ? { ...f, options } : f)
    );
  }
}
