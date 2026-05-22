import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Subscription, combineLatest } from 'rxjs';
import { BreadcrumbComponent } from "../../../shared/components/breadcrumbs/breadcrumbs";
import { Filters } from "../../../shared/components/filters/filters";
import { UnitsList } from "../../../shared/components/units-list/units-list";
import { PaginationComponent } from "../../../shared/components/pagination/pagination";
import { ContactSection } from "../../../shared/components/contact-section/contact-section";
import { UnitCardModel } from '../../../models/IUnit';
import { UnitService } from '../../../services/api/unit.service';

@Component({
  selector: 'app-units',
  imports: [BreadcrumbComponent, Filters, UnitsList, PaginationComponent, ContactSection],
  templateUrl: './units.html',
  styleUrl: './units.css',
})
export class Units implements OnInit, OnDestroy {
  private readonly unitService = inject(UnitService);
  private readonly route = inject(ActivatedRoute);
  private sub?: Subscription;
  private allowedStatuses = new Set(['Sale', 'Rent']);

  readonly currentPage = signal<number>(1);
  readonly totalPages = signal<number>(1);
  readonly units = signal<UnitCardModel[]>([]);
  readonly isLoading = signal<boolean>(false);
  private projectId?: number;
  private advancedFilters: { minPrice?: number; maxPrice?: number; rooms?: number } = {};
  ngOnInit(): void {
    this.sub = combineLatest([
      this.route.paramMap,
      this.route.queryParamMap
    ]).subscribe(([params, query]) => {
      const rawId = params.get('projectId');
      this.projectId = rawId ? Number(rawId) : undefined;
      this.allowedStatuses = new Set(this.parseStatusParam(query.get('status')));
      this.loadUnits(1);
    });
  }

  onPageChange(page: number): void {
    this.loadUnits(page);
  }

  onFilterChange(event: { type: 'all' | 'sell' | 'rent'; advanced: any }): void {
    if (event.type === 'all') {
      this.allowedStatuses = new Set(this.parseStatusParam(this.route.snapshot.queryParamMap.get('status')));
    } else if (event.type === 'sell') {
      this.allowedStatuses = new Set([this.baseStatusForIndex(0)]);
    } else {
      this.allowedStatuses = new Set([this.baseStatusForIndex(1)]);
    }

    this.advancedFilters = {
      minPrice: this.parseNumber(event.advanced?.minPrice),
      maxPrice: this.parseNumber(event.advanced?.maxPrice),
      rooms: this.parseNumber(event.advanced?.rooms),
    };

    this.loadUnits(1);
  }

  private loadUnits(page: number): void {
    this.currentPage.set(page);
    this.isLoading.set(true);
    const params: Record<string, any> = { page, pageSize: 12 };
    if (this.projectId) {
      params['projectId'] = this.projectId;
    }
    if (this.allowedStatuses.size === 1) {
      params['status'] = Array.from(this.allowedStatuses)[0];
    }
    if (this.advancedFilters.minPrice) params['minPrice'] = this.advancedFilters.minPrice;
    if (this.advancedFilters.maxPrice) params['maxPrice'] = this.advancedFilters.maxPrice;
    if (this.advancedFilters.rooms) params['rooms'] = this.advancedFilters.rooms;

    this.unitService.getUnits(params).subscribe({
      next: (pageData) => {
        const items = (pageData.items ?? []).filter(u => this.allowedStatuses.has(u.status));
        this.units.set(items);
        this.totalPages.set(pageData.totalPages ?? 1);
        this.isLoading.set(false);
      },
      error: () => {
        this.units.set([]);
        this.totalPages.set(1);
        this.isLoading.set(false);
      }
    });
  }

  getEmptyStateMessage(): string {
    if (this.allowedStatuses.size === 1) {
      const status = Array.from(this.allowedStatuses)[0];
      if (status === 'Sale' || status === 'sell') {
        return 'لا توجد وحدات معروضة للبيع حالياً';
      }
      if (status === 'Rent' || status === 'rent') {
        return 'لا توجد وحدات معروضة للإيجار حالياً';
      }
      if (status === 'Sold') {
        return 'لا توجد وحدات مباعة حالياً';
      }
      if (status === 'Rented') {
        return 'لا توجد وحدات مؤجرة حالياً';
      }
    }
    return 'لا توجد وحدات متاحة حالياً';
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  private parseStatusParam(raw: string | null): string[] {
    if (!raw) return ['Sale', 'Rent'];
    const list = raw.split(',').map(s => s.trim()).filter(s => s.length > 0);
    return list.length > 0 ? list : ['Sale', 'Rent'];
  }

  private baseStatusForIndex(index: number): string {
    const base = this.parseStatusParam(this.route.snapshot.queryParamMap.get('status'));
    if (base.length === 1) return base[0];
    return base[index] ?? base[0];
  }

  private parseNumber(value: any): number | undefined {
    const num = Number(value);
    return Number.isFinite(num) && num > 0 ? num : undefined;
  }

}
