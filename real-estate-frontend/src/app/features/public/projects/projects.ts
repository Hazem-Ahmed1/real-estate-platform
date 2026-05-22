import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { BreadcrumbComponent } from "../../../shared/components/breadcrumbs/breadcrumbs";
import { Filters } from "../../../shared/components/filters/filters";
import { ProjectsList } from "../../../shared/components/projects-list/projects-list";
import { IProject } from '../../../models/IProject';
import { PaginationComponent } from "../../../shared/components/pagination/pagination";
import { ContactSection } from '../../../shared/components/contact-section/contact-section';
import { ProjectService } from '../../../services/api/project.service';

@Component({
  selector: 'app-projects',
  imports: [BreadcrumbComponent, ProjectsList, PaginationComponent, ContactSection, Filters],
  templateUrl: './projects.html',
  styleUrl: './projects.css',
})
export class Projects implements OnInit, OnDestroy {
  private readonly projectService = inject(ProjectService);
  private readonly route = inject(ActivatedRoute);
  private sub?: Subscription;

  readonly projects = signal<IProject[]>([]);
  readonly currentPage = signal<number>(1);
  readonly totalPages = signal<number>(1);
  readonly isLoading = signal<boolean>(false);
  private statusFilters: string[] = ['Sale', 'Rent'];
  private advancedFilters: { minPrice?: number; maxPrice?: number; rooms?: number } = {};
  ngOnInit(): void {
    this.sub = this.route.queryParamMap.subscribe(params => {
      const raw = params.get('status');
      this.statusFilters = this.parseStatusParam(raw);
      this.loadProjects(1);
    });
  }

  onPageChange(page: number): void {
    this.loadProjects(page);
  }

  onFilterChange(event: { type: 'all' | 'sell' | 'rent'; advanced: any }): void {
    if (event.type === 'all') {
      this.statusFilters = this.parseStatusParam(this.route.snapshot.queryParamMap.get('status'));
    } else if (event.type === 'sell') {
      this.statusFilters = [this.baseStatusForIndex(0)];
    } else {
      this.statusFilters = [this.baseStatusForIndex(1)];
    }

    this.advancedFilters = {
      minPrice: this.parseNumber(event.advanced?.minPrice),
      maxPrice: this.parseNumber(event.advanced?.maxPrice),
      rooms: this.parseNumber(event.advanced?.rooms),
    };

    this.loadProjects(1);
  }

  private loadProjects(page: number): void {
    this.currentPage.set(page);
    this.isLoading.set(true);
    const params: Record<string, any> = { page, pageSize: 12 };
    if (this.statusFilters.length === 1) {
      params['status'] = this.statusFilters[0];
    }
    if (this.advancedFilters.minPrice) params['minPrice'] = this.advancedFilters.minPrice;
    if (this.advancedFilters.maxPrice) params['maxPrice'] = this.advancedFilters.maxPrice;
    if (this.advancedFilters.rooms) params['rooms'] = this.advancedFilters.rooms;

    this.projectService.getProjects(params).subscribe({
      next: (pageData) => {
        const items = (pageData.items ?? []).filter(p => this.statusFilters.includes(p.status));
        this.projects.set(items);
        this.totalPages.set(pageData.totalPages ?? 1);
        this.isLoading.set(false);
      },
      error: () => {
        this.projects.set([]);
        this.totalPages.set(1);
        this.isLoading.set(false);
      }
    });
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


  getEmptyStateMessage(): string {
    if (this.statusFilters.includes('Sold') || this.statusFilters.includes('Rented')) {
      if (this.statusFilters.includes('Sold') && !this.statusFilters.includes('Rented')) {
        return 'لا توجد مشاريع مباعة حالياً';
      }
      if (this.statusFilters.includes('Rented') && !this.statusFilters.includes('Sold')) {
        return 'لا توجد مشاريع مؤجرة حالياً';
      }
      return 'لا توجد مشاريع مباعة أو مؤجرة حالياً';
    }
    if (this.statusFilters.length === 1) {
      const status = this.statusFilters[0];
      if (status === 'Sale' || status === 'sell') {
        return 'لا توجد مشاريع معروضة للبيع حالياً';
      }
      if (status === 'Rent' || status === 'rent') {
        return 'لا توجد مشاريع معروضة للإيجار حالياً';
      }
    }
    return 'لا توجد مشاريع متاحة حالياً';
  }

  getFilterTitle(): string {
    if (this.statusFilters.includes('Sold') || this.statusFilters.includes('Rented')) {
      return 'المشـــــــاريع المباعـــة والمؤجـــرة';
    }
    return 'المشـــــــاريع الحـــالية';
  }

  getFilterSubTitle(): string {
    if (this.statusFilters.includes('Sold') || this.statusFilters.includes('Rented')) {
      return 'مشاريعنا';
    }
    return 'مشـــاريعنا';
  }

  getSellLabel(): string {
    if (this.statusFilters.includes('Sold') || this.statusFilters.includes('Rented')) {
      return 'مباع';
    }
    return 'للبيع';
  }

  getRentLabel(): string {
    if (this.statusFilters.includes('Sold') || this.statusFilters.includes('Rented')) {
      return 'مؤجر';
    }
    return 'للإيجار';
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
