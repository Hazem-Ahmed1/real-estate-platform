import { Component, OnChanges, OnDestroy, OnInit, input, output, signal, inject } from '@angular/core';
import { FilterType } from '../../../models/FilterType';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-filters',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './filters.html',
  styleUrl: './filters.css',
})
export class Filters implements OnInit, OnChanges, OnDestroy {
  private readonly fb = inject(FormBuilder);

  subTitle = input.required<string>();
  title = input.required<string>();
  sellLabel = input<string>('للبيع');
  rentLabel = input<string>('للإيجار');

  selectedFilter = signal<FilterType>('all');

  showFilter = signal(false);

  filterForm: FormGroup = this.fb.group({
    minPrice: [null as number | null, [Validators.min(0)]],
    maxPrice: [null as number | null, [Validators.min(0)]],
    rooms: [null as number | null, [Validators.min(0), Validators.pattern('^[0-9]*$')]]
  }, { validators: this.priceRangeValidator });

  filterChange = output<{
    type: FilterType;
    advanced: any;
  }>();

  private defaultEmitted = false;
  private destroyed = false;
  private userInteracted = false;

  priceRangeValidator(group: FormGroup) {
    const min = group.get('minPrice')?.value;
    const max = group.get('maxPrice')?.value;
    if (min !== null && max !== null && min !== '' && max !== '' && Number(min) > Number(max)) {
      return { priceRangeInvalid: true };
    }
    return null;
  }

  ngOnInit(): void {
    this.emitDefault();
  }

  ngOnChanges(): void {
    this.emitDefault();
  }

  ngOnDestroy(): void {
    this.destroyed = true;
  }

  filterBy(type: FilterType) {
    this.userInteracted = true;
    this.selectedFilter.set(type);

    this.filterChange.emit({
      type,
      advanced: this.filterForm.value,
    });
  }

  toggleFilter() {
    this.showFilter.update((v) => !v);
  }

  applyFilters() {
    if (this.filterForm.invalid) return;

    this.userInteracted = true;
    this.filterChange.emit({
      type: this.selectedFilter(),
      advanced: this.filterForm.value,
    });

    this.showFilter.set(false);
  }

  private emitDefault(): void {
    if (this.defaultEmitted || this.destroyed || this.userInteracted) return;

    queueMicrotask(() => {
      if (this.defaultEmitted || this.destroyed || this.userInteracted) return;
      this.defaultEmitted = true;
      this.filterBy('all');
    });
  }
}
